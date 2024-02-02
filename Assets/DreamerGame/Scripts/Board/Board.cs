using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
public class Board : MonoBehaviour
{
    public static Board Instance;
    [SerializeField] public Vector2Int size; //Going to pull value from json
    [SerializeField] public float borderPadding;
    [SerializeField] private GameObject cellParent;
    [SerializeField] private GameObject itemParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Camera activeCamera;
    [SerializeField] private GameObject defaultItemPrefab;
    [SerializeField] private RectTransform boardBorderRect;
    [SerializeField] private GameObject[] testItemPrefabs;
    [HideInInspector] public Cell[] cells;
    [HideInInspector] public Item[] items;
    [HideInInspector] public ColumnQueue[] columnQueues;
    [HideInInspector] public Vector2 cellSize;
    private SpriteRenderer _borderSprite;
    private BorderAdjuster _borderAdjuster;
    
    public void Awake()
    {
        InitializeSingleton();
    }

    public void Start()
    {
        InitializeComponents();
        InitializeLevel();
    }
    public void TouchItem(Item item)
    {
        if (item.falling)
        {
            return;
        }
        item.TouchBehaviour();
        FallItems();
        CreateNewItems();
    }
    private void FallItems()
    {
        for (int x = 0; x < size.x; x++)
        {
            FallItemsInColumn(x);
        }
    }
    public void FallItemsInColumn(int x)
    {
        for (int y = size.y - 2; y >= 0; y--)
        {
            Item item = items[y * size.x + x];
            if (item != null && item.fallable && !item.falling)
            {
                Item bottomItem = items[(y + 1) * size.x + x];
                if (bottomItem == null || bottomItem.falling)
                {
                    item.Fall();
                }
            }
        }
    }
    
    private void InitializeLevel()
    {
        AdjustBorder();
        InitializeCells();
        InitializeItems();
        InitializeColumnQueues();
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
    private void InitializeComponents()
    {
        _borderSprite = GetComponent<SpriteRenderer>();
        Item defaultItem = defaultItemPrefab.GetComponent<Item>();
        cellSize = defaultItem.boxCollider.size;
    }
    private void AdjustBorder()
    {
        _borderAdjuster = new BorderAdjuster(transform, size, boardBorderRect, _borderSprite, borderPadding,
            defaultItemPrefab, cellSize, activeCamera);
        _borderAdjuster.Adjust();
    }
    private void InitializeCells()
    {
        cells = new Cell[size.x * size.y];
        Vector2 midPoint = transform.position;
        Vector2 initialOffset = new Vector2((cellSize.x * size.x / 2) - (cellSize.x/2), (cellSize.y * size.y / 2) - (cellSize.y/2));
        Vector2 addedOffset = Vector2.zero;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Cell cell = Instantiate(cellPrefab, midPoint + initialOffset + addedOffset,quaternion.identity, cellParent.transform).GetComponent<Cell>();
                cell.gameObject.name = "Cell" + "[" + x + "]" + "[" + y + "]";
                cells[size.x * y + x] = cell;
                addedOffset.x -= cellSize.x;
            }
            addedOffset.x = 0;
            addedOffset.y -= cellSize.y;
        }
    }
    
    private void InitializeColumnQueues()
    {
        columnQueues = new ColumnQueue[size.x];
        for (int x = 0; x < size.x; x++)
        {
            columnQueues[x] = new ColumnQueue(cells[x].transform.position + (Vector3.up * cellSize.y));
        }
    }
    
    private void InitializeItems()
    {
        items = new Item[size.x * size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = size.y-1; y >= 0; y--)
            {
                CreateNewItem(testItemPrefabs[Random.Range(0,testItemPrefabs.Length)], new Vector2Int(x,y));
            }
        }
    }

    private void CreateNewItems()
    {
        List<Item> createdItems = new List<Item>();
        for (int x = 0; x < size.x; x++)
        {
            int fallableCellCount = GetEmptyCellCountInColumn(x);
            for (int index = 0; index < fallableCellCount; index++)
            {
                createdItems.Add(CreateNewItem(testItemPrefabs[Random.Range(0,testItemPrefabs.Length)], new Vector2Int(x,-1)));
            }
        }
        FallNewItems(createdItems);
    }

    private void FallNewItems(List<Item> newItems)
    {
        foreach (Item item in newItems)
        {
            item.Fall();
        }
    }

    private Item CreateNewItem(GameObject itemObject, Vector2Int pos)
    {
        Item item;
        if (IsInBoard(pos))
        {
            item = Instantiate(itemObject, cells[pos.y * size.x + pos.x].transform.position, quaternion.identity, itemParent.transform).GetComponent<Item>();
            item.InitializeItem(pos);
        }
        else
        {
            item = Instantiate(itemObject, columnQueues[pos.x].GetNextPosition(), quaternion.identity, itemParent.transform).GetComponent<Item>();
            item.InitializeItem(pos);
        }
        
        return item;
    }

    private int GetEmptyCellCountInColumn(int x, bool fromAbove = true)
    {
        int count = 0;
        for (int y = 0; y < size.y; y++)
        {
            Item item = items[y * size.x + x];
            if (item == null)
            {
                count += 1;
            }
            else if (!item.fallable && fromAbove)
            {
                break;
            }
        }
        count -= columnQueues[x].Count();
        return count;
    }

    public List<Item> CheckMatches(int startX, int startY)
    {
        List<Item> matches = new List<Item>();
        FindMatches(startX, startY, items[startY* size.x + startX].color, matches);
        return matches;
    }

    private void FindMatches(int x, int y, ColorType color, List<Item> matches)
    {
        if (x < 0 || x >= size.x || y < 0 || y >= size.y)
        {
            return;
        }
        Item item = items[y * size.x + x];
        if (item == null)
        {
            return;
        }

        if (item.falling || !item.matchable || matches.Contains(item) || item.color != color)
        {
            return;
        }
        
        matches.Add(item);
        FindMatches(x+1, y, color, matches);
        FindMatches(x-1, y, color, matches);
        FindMatches(x, y+1, color, matches);
        FindMatches(x, y-1, color, matches);
    }

    public bool IsInBoard(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= size.x || pos.y < 0 || pos.y >= size.y)
        {
            return false;
        }

        return true;
    }
}
