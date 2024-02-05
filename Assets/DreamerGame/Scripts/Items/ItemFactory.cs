using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemFactory : MonoBehaviour
{
    public static ItemFactory Instance;
    private ItemFactorySettings _factorySettings;

    private void Awake()
    {
        InitializeSingleton();
        _factorySettings = Resources.Load<ItemFactorySettings>("ItemFactorySettings");
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

    public GameObject GetItem(ItemType type)
    {
        GameObject prefab =  _factorySettings.itemAsds.First(item => item.type == type).prefab;
        return prefab;
    }
    
}
