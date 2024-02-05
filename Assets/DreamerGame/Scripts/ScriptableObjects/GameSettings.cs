using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName= "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public float acceleration = 9.81f;
    public float speedLimit = 50f;
    public float fallStopThreshold = 0.02f;
    public SpeedTransferType speedTransferType = SpeedTransferType.TopToBottom;
}
