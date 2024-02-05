using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [HideInInspector] public LevelData levelData;
    [HideInInspector] public LevelState state = LevelState.Loading;
    private HashSet<Item> _markedItems = new HashSet<Item>();
    private List<Goal> _goals = new List<Goal>();
    private int _moveCount;
    private LevelReader _levelReader;
    [SerializeField] private LevelView view;
    [SerializeField] private Popup failPopup;
    [SerializeField] private ParticleSystem winAnimation;
    [SerializeField] private GameObject goalEntityUIPrefab;
    private UnityEvent OnMove = new UnityEvent();
    private void Awake()
    {
        InitializeSingleton();
        LoadLevelData();
        OnMove.AddListener(view.DecrementMoveCount);
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TouchEvent(Item touchedItem)
    {
        if (state == LevelState.Idle && _moveCount != 0)
        {
            if (touchedItem.falling)
            {
                return;
            }
            TouchEventCoroutine(touchedItem);
            //StartCoroutine(TouchEventCoroutine(touchedItem));
        }
    }

    public void TouchEventCoroutine(Item touchedItem)
    {
        state = LevelState.Animating;
        _moveCount -= 1;
        OnMove?.Invoke();
        
        
        
        touchedItem.TouchBehaviour(_markedItems);
        _markedItems.Clear();
        Board.Instance.AfterTouchLoop();

        
        
        
        if (state == LevelState.Animating)
        {
            state = LevelState.Idle;
        }

        if (_moveCount == 0 && GameManager.Instance.state != GameState.Win)
        {
            GameManager.Instance.state = GameState.Fail;
            StartCoroutine(FailCoroutine());
        }
    }
    
    private void LoadLevelData()
    {
        _levelReader = new LevelReader();
        levelData = _levelReader.LoadLevel($"level_" + GameManager.Instance.persistLevel.ToString("D2"));
        ArrangeArray(levelData.grid);
        _moveCount = levelData.move_count;
        view.SetMoveCount(_moveCount);
    }

    public JsonTypeConverter.JsonType.JsonOutput GetTypes(int index)
    {
        return _levelReader.GetTypes(levelData.grid[index]);
    }

    private void ArrangeArray(string[] array)
    {
        int pointer1 = 0;
        int pointer2 = array.Length-1;
        while (pointer1 < pointer2)
        {
            string tmp = array[pointer1];
            array[pointer1] = array[pointer2];
            array[pointer2] = tmp;
            pointer1 += 1;
            pointer2 -= 1;
        }
        for (int y = 0; y < levelData.grid_height; y++)
        {
            pointer1 = 0;
            pointer2 = levelData.grid_width - 1;
            while (pointer1 < pointer2)
            {
                int leftIndex = y * levelData.grid_width + pointer1;
                int rightIndex = y * levelData.grid_width + pointer2;
                string tmp = array[leftIndex];
                array[leftIndex] = array[rightIndex];
                array[rightIndex] = tmp;
                pointer1 += 1;
                pointer2 -= 1;
            }
        }
    }

    public void InitializeGoals(Item[] items)
    {
        DemoGoalSystem(items);
    }

    public void DemoGoalSystem(Item[] items)
    {
        foreach (var item in items)
        {
            if (item.type == ItemType.Box || item.type == ItemType.Stone || item.type == ItemType.Vase)
            {
                if (_goals.Any(goal => goal.goalIdentity == (item.type, item.color)))
                {
                    var goal = _goals.First(goal => goal.goalIdentity == (item.type, item.color));
                    goal.IncrementAmount();
                }
                else
                {
                    Goal goal = new Goal((item.type, item.color));
                    _goals.Add(goal);
                    var goalEntityUI = CreateGoalEntityUI((item.type, item.color));
                    goal.onAmountDecrement.AddListener(goalEntityUI.DecrementAmount);
                    goal.onFinished.AddListener(goalEntityUI.SetFinish);
                    goal.onFinished.AddListener(CheckGoals);
                    goal.onAmountIncrement.AddListener(goalEntityUI.IncrementAmount);
                    goal.IncrementAmount();
                }
            }
        }
    }

    public GoalEntityView CreateGoalEntityUI((ItemType type, ColorType color) identity)
    {
        var goalEntityUI = Instantiate(goalEntityUIPrefab, Vector3.zero, quaternion.identity).GetComponent<GoalEntityView>();
        goalEntityUI.SetGoalEntityUI(ItemFactory.Instance.GetItem(identity.type).GetComponent<Item>().itemSprite.sprites[0].sprite, 0);
        goalEntityUI.transform.SetParent(view.goalLayout.transform);
        ResetRectTransform(goalEntityUI.GetComponent<RectTransform>());
        return goalEntityUI;
    }
    
    public void ResetRectTransform(RectTransform rectTransform)
    {
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(100, 100);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
    }
    public void DecrementGoal((ItemType type, ColorType color) identity)
    {
        var goal = _goals.FirstOrDefault(goal => goal.goalIdentity == identity);
        if (goal == null)
        {
            return;
        }
        goal.DecrementAmount();
    }

    public void CheckGoals()
    {
        bool goalsNotFinished = _goals.Any(goal => goal.amount != 0);
        if (!goalsNotFinished)
        {
            GameManager.Instance.state = GameState.Win;
            StartCoroutine(WinCoroutine());
        }
    }

    private IEnumerator WinCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        winAnimation.Play();
        yield return new WaitForSeconds(1);
        GameManager.Instance.LevelCompleted();
        GameManager.Instance.LoadMenuScene();
    }

    private IEnumerator FailCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        failPopup.Show();
    }
}
