using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName= "ScriptableObjects/ItemFactorySettings")]
public class ItemFactorySettings : ScriptableObject
{
    [Serializable]
    public class ItemEntity
    {
        public ItemType type;
        public GameObject prefab;
        public ColorVariance[] colorVariances;
    }

    public ItemEntity[] itemEntities;
    
}
