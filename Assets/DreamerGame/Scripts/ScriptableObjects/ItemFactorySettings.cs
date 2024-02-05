using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
[CreateAssetMenu(menuName= "ScriptableObjects/ItemFactorySettings")]
public class ItemFactorySettings : ScriptableObject
{
    [Serializable]
    public class ItemEntity
    {
        public ItemType type;
        public GameObject prefab;
    }

    public ItemEntity[] itemEntities;
    
}
