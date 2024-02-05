using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [HideInInspector]public GameState state;
    [HideInInspector] public int persistLevel;
    [Header("Game Settings")]
    public float acceleration = 9.81f;
    public float speedLimit = 50f;
    public float fallStopThreshold = 0.02f;
    public SpeedTransferType speedTransferType = SpeedTransferType.TopToBottom;
    private void Awake()
    {
        InitializeSingleton();
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this);
        GetPersist();
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

    private void GetPersist()
    {
        bool hasKey = PlayerPrefs.HasKey("Level");
        if (!hasKey)
        {
            PlayerPrefs.SetInt("Level", 1);
        }
        persistLevel = PlayerPrefs.GetInt("Level");
    }


    public void LoadGameScene()
    {
        state = GameState.InGame;
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadMenuScene()
    {
        state = GameState.InMenu;
        SceneManager.LoadScene("MainScene");
    }

    public void LevelCompleted()
    {
        persistLevel += 1;
        PlayerPrefs.SetInt("Level", persistLevel);
    }
    
}
