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
    private PoolSettings _poolSettings;
    private void Awake()
    {
        InitializeSingleton();
        _poolSettings = Resources.Load<PoolSettings>("PoolItems");
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
        foreach (var poolableObject in _poolSettings.poolableObjects)
        {
            int amount = poolableObject.amount;
            for (int i = 0; i < amount; i++)
            {
                Item item = Instantiate(ItemFactory.Instance.GetItem(poolableObject.type), Vector3.up * 1000, quaternion.identity).GetComponent<Item>();
                item.transform.parent = parent;
                itemPool.Add(item); 
            }
        }
    }
    
    public Item CreateItem(ItemType type, Vector3 position, ColorType color)
    {
        Item item = itemPool.FirstOrDefault(item => item.type == type);
        if (item == null)
        {
            Debug.Log("Instantiated Object");
            var poolableObject= _poolSettings.poolableObjects.First(poolableObject => poolableObject.type == type);
            item = Instantiate(ItemFactory.Instance.GetItem(poolableObject.type, color), Vector3.up * 1000, quaternion.identity).GetComponent<Item>();
        }
        else
        {
            item.SetColor(color);
        }
        item.transform.position = position;
        itemPool.Remove(item);
        return item;
    }

    public void DestroyItem(Item item)
    {
        item.transform.position = Vector3.up * 1000;
        itemPool.Add(item);
        item.activeState = 0;
        item.spriteRenderer.sprite = item.itemSprite.sprites[0].sprite;
        item.transform.localScale = new Vector3(1, 1, 1);
    }
}
