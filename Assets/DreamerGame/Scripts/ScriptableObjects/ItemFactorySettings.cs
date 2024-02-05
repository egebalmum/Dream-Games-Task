using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[CreateAssetMenu(menuName= "ScriptableObjects/ItemFactorySettings")]
public class ItemFactorySettings : ScriptableObject
{
    [Serializable]
    public class ItemASD
    {
        public ItemType type;
        public GameObject prefab;
    }

    public ItemASD[] itemAsds;
    
}
