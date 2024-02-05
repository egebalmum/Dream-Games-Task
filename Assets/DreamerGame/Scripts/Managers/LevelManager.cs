using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    public string currentLevel;
    [HideInInspector] public LevelData levelData;
    [HideInInspector] public LevelState state = LevelState.Loading;
    private HashSet<Item> _markedItems = new HashSet<Item>();
    private Goal[] _goals;
    private LevelReader _levelReader;
    
    private void Awake()
    {
        InitializeSingleton();
        LoadLevelData();
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
        if (touchedItem.falling)
        {
            return;
        }
        TouchEventCoroutine(touchedItem);
        //StartCoroutine(TouchEventCoroutine(touchedItem));
    }

    public void TouchEventCoroutine(Item touchedItem)
    {
        touchedItem.TouchBehaviour(_markedItems);
        _markedItems.Clear();
        Board.Instance.AfterTouchLoop();
    }
    
    private void LoadLevelData()
    {
        _levelReader = new LevelReader();
        levelData = _levelReader.LoadLevel(currentLevel);
        ArrangeArray(levelData.grid);
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
}
