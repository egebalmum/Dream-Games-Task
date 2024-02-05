using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Game Settings")]
    public float acceleration = 9.81f;
    public float speedLimit = 50f;
    public float fallStopThreshold = 0.02f;
    public SpeedTransferType speedTransferType = SpeedTransferType.TopToBottom;
    private void Awake()
    {
        InitializeSingleton();
        Application.targetFrameRate = 60;
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
}
