using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemFactory : MonoBehaviour
{
    public static ItemFactory Instance;
    private ItemFactorySettings _itemFactorySettings;
    
    //private ParticleFactorySettings _particleFactorySettings;

    private void Awake()
    {
        InitializeSingleton();
        _itemFactorySettings = Resources.Load<ItemFactorySettings>("ItemFactory");
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

    public GameObject GetItem(ItemType type, ColorType color = ColorType.Random)
    {
        GameObject prefab =  _itemFactorySettings.itemEntities.First(item => item.type == type).prefab;
        prefab.GetComponent<Item>().SetColor(color);
        return prefab;
    }

    public ColorVariance GetColorVariance(ItemType type, ColorType color = ColorType.Random)
    {
        ColorVariance[] colorVariances = _itemFactorySettings.itemEntities.FirstOrDefault(item => item.type == type).colorVariances;
        if (colorVariances == null)
        {
            Debug.LogError("Could not found colorVariances");
            return null;
        }

        ColorVariance variance;
        
        if (color == ColorType.Random)
        {
            int index = Random.Range(0, colorVariances.Length);
            variance = colorVariances[index];
        }
        else
        {
            variance = colorVariances.FirstOrDefault(variance => variance.color == color);
        }
        

        if (variance == null)
        {
            Debug.LogError("Could not found colorVariance");
            return null;
        }

        return variance;
    }
    
}
