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
    private int _activeCoroutines;
    private List<Goal> _goals = new List<Goal>();
    private GoalSettings _goalSettings;
    private int _moveCount;
    private LevelReader _levelReader;
    [SerializeField] private LevelView view;
    [SerializeField] private Popup failPopup;
    [SerializeField] private Popup winPopup;
    private UnityEvent OnMove = new UnityEvent();
    private void Awake()
    {
        InitializeSingleton();
        LoadLevelData();
        _goalSettings = Resources.Load<GoalSettings>("GoalItems");
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
        if (state == LevelState.Idle && _moveCount != 0 && GameManager.Instance.state == GameState.InGame)
        {
            if (touchedItem.falling)
            {
                return;
            }
            TouchEventCoroutine(touchedItem);
        }
    }

    private void TouchEventCoroutine(Item touchedItem)
    {
        state = LevelState.Animating;
        _moveCount -= 1;
        OnMove?.Invoke();
        
        
        
        touchedItem.TouchBehaviour(_markedItems);



        
        _activeCoroutines = 0;
        _markedItems.Clear();
        Board.Instance.AfterTouchLoop();
        if (_moveCount == 0 && GameManager.Instance.state != GameState.Win)
        {
            GameManager.Instance.state = GameState.Fail;
            StartCoroutine(FailCoroutine());
        }

        if (GameManager.Instance.state == GameState.InGame && state == LevelState.Animating)
        {
            state = LevelState.Idle;
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
        foreach (var item in items)
        {
            if (_goalSettings.goalEntities.Any(goalEntity => goalEntity.type == item.type && goalEntity.color == item.color))
            {
                Goal goal;
                if (_goals.Any(goalInList => goalInList.goalIdentity == (item.type, item.color)))
                {
                    goal = _goals.First(goalInList => goalInList.goalIdentity == (item.type, item.color));
                }
                else
                {
                    goal = new Goal((item.type, item.color));
                    _goals.Add(goal);
                    var goalEntityUI = view.CreateGoalEntityUI((item.type, item.color));
                    goal.onAmountDecrement.AddListener(goalEntityUI.DecrementAmount);
                    goal.onFinished.AddListener(goalEntityUI.SetFinish);
                    goal.onFinished.AddListener(CheckGoals);
                    goal.onAmountIncrement.AddListener(goalEntityUI.IncrementAmount);
                }
                goal.IncrementAmount();
            }
        }
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

    private void CheckGoals()
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
        GameManager.Instance.LevelCompleted();
        yield return new WaitForSeconds(0.5f);
        winPopup.Show();
        yield return new WaitForSeconds(2.5f);
        GameManager.Instance.LoadMenuScene();
    }

    private IEnumerator FailCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        failPopup.Show();
    }
}
