using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    private List<Item> itemPool = new List<Item>();
    private List<DreamParticle> particlePool = new List<DreamParticle>();
    private PoolSettings _poolSettings;
    private Transform _parent;
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
        _parent = parent;
        foreach (var poolableObject in _poolSettings.poolableObjects)
        {
            int amount = poolableObject.amount;
            for (int i = 0; i < amount; i++)
            {
                Item item = Instantiate(ItemFactory.Instance.GetItem(poolableObject.type), Vector3.up * 1000, quaternion.identity).GetComponent<Item>();
                item.transform.parent = _parent;
                itemPool.Add(item);
                DreamParticle particle = item.GetComponentInChildren<DreamParticle>();
                particle.type = item.type;
                particle.transform.parent = _parent;
                particlePool.Add(particle);
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
            DreamParticle particle = item.GetComponentInChildren<DreamParticle>();
            particle.type = item.type;
            particle.transform.parent = _parent;
            particlePool.Add(particle);
        }
        else
        {
            item.SetColor(color);
        }
        item.transform.position = position;
        itemPool.Remove(item);
        return item;
    }

    public void CreateParticle(ItemType type, Vector3 position, ColorType color)
    {
        var particle = particlePool.FirstOrDefault(particle => particle.type == type);
        if (particle == null)
        {
            Debug.LogError("Particle not Found in the Pool!");
            return;
        }
        particle.SetParticleColor(color);
        StartCoroutine(CreateAndDestroyParticle(particle, position));
    }

    public void DestroyItem(Item item)
    {
        item.transform.position = Vector3.up * 1000;
        itemPool.Add(item);
        item.activeState = 0;
        item.spriteRenderer.sprite = item.itemSprite.sprites[0].sprite;
        item.spriteRenderer.sortingOrder = 2;
        item.transform.localScale = new Vector3(1, 1, 1);
    }

    private IEnumerator CreateAndDestroyParticle(DreamParticle particle, Vector3 position)
    {
        particle.transform.position = position;
        particlePool.Remove(particle);
        particle.particle.Play();

        yield return new WaitForSeconds(0.5f);
        particle.transform.position = Vector3.up * 1000;
        particlePool.Add(particle);
    }
}
