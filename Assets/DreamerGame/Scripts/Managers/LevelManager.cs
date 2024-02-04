using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [Header("Game Settings")] 
    public string currentLevel;
    public float acceleration = 9.81f;
    public float speedLimit = 50f;
    public float fallStopThreshold = 0.02f;
    public SpeedTransferType speedTransferType = SpeedTransferType.TopToBottom;
    [HideInInspector] public LevelData levelData;

    [Header("Level Settings")] public Goal[] goals;
    [HideInInspector] public LevelState state = LevelState.Loading;
    
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


    private void LoadLevelData()
    {
        levelData = LevelReader.LoadLevel(currentLevel);
        ArrangeArray(levelData.grid);
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
