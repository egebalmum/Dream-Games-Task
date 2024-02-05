using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    private List<Item> itemPool = new List<Item>();
    private PoolSettings.PoolableObject[] _poolableObjects;
    private void Awake()
    {
        InitializeSingleton();
        _poolableObjects = Resources.Load<PoolSettings>("PoolSettings").poolableObjects;
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
    public void CreatePool(Transform parent)
    {
        foreach (var poolableObject in _poolableObjects)
        {
            int amount = poolableObject.amount;
            for (int i = 0; i < amount; i++)
            {
                Item item = Instantiate(ItemFactory.Instance.GetItem(poolableObject.type), Vector3.up * 1000, quaternion.identity).GetComponent<Item>();
                item.transform.parent = parent;
                item.InitializeItem();
                itemPool.Add(item); 
            }
        }
    }
    
    public Item CreateItem(ItemType type, Vector3 position, ColorType color)
    {
        Item item = itemPool.FirstOrDefault(item => item.type == type);
        if (item == null)
        {
            print("Instantiated Object");
            var poolableObject= _poolableObjects.First(poolableObject => poolableObject.type == type);
            item = Instantiate(ItemFactory.Instance.GetItem(poolableObject.type), Vector3.up * 1000, quaternion.identity).GetComponent<Item>();
            item.InitializeItem();
        }
        item.transform.position = position;
        item.SetColor(color);
        itemPool.Remove(item);
        return item;
    }

    public void DestroyItem(Item item)
    {
        item.transform.position = Vector3.up * 1000;
        itemPool.Add(item);
    }
}
