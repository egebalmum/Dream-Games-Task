using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float acceleration = 9.81f;
    public float speedLimit = 50f;
    public float fallStopThreshold = 0.02f;
    public int speedTransferType = 1;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        InitializeSingleton();
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
