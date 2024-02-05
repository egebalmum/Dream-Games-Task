using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName= "ScriptableObjects/PoolSettings")]
public class PoolSettings : ScriptableObject
{
    [Serializable]
    public class PoolableObject
    {
        [SerializeField] public ItemType type;
        [SerializeField] public int amount;
    }

    [SerializeField]
    public PoolableObject[] poolableObjects;
}
