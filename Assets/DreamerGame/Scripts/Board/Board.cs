using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public static Board Instance;
    [SerializeField] public float borderPadding;
    [SerializeField] private GameObject cellParent;
    [SerializeField] private GameObject itemParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Camera activeCamera;
    [SerializeField] private RectTransform boardBorderRect;
    [HideInInspector] public Cell[] cells;
    [HideInInspector] public Item[] items;
    [HideInInspector] public ColumnQueue[] columnQueues;
    [HideInInspector] public Vector2Int size;
    [HideInInspector] public Vector2 cellSize;
    private SpriteRenderer _borderSprite;
    private BorderAdjuster _borderAdjuster;
    private int _fallingObjectCount = 0;
    
    public void Awake()
    {
        InitializeSingleton();
    }

    public void Start()
    {
        InitializeComponents();
        InitializeLevel();
    }

    public void AfterTouchLoop()
    {
        //CheckBoardMatches();
        FallItems();
        CreateNewItems();
    }
    public void FallItems()
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
        size = new Vector2Int(LevelManager.Instance.levelData.grid_width, LevelManager.Instance.levelData.grid_height); 
        AdjustBorder();
        InitializePool();
        InitializeCells();
        InitializeItems();
        InitializeColumnQueues();
        CheckBoardMatches();
        bool shuffleNeeded = TryShuffle();
        if (!shuffleNeeded)
        {
            LevelManager.Instance.state = LevelState.Idle;
        }
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
        cellSize = ItemFactory.Instance.GetItem(ItemType.Cube).GetComponent<Item>().boxCollider.size;
    }

    private void InitializePool()
    {
        ObjectPool.Instance.CreatePool(itemParent.transform);
    }
    
    private void AdjustBorder()
    {
        _borderAdjuster = new BorderAdjuster(transform, size, boardBorderRect, _borderSprite, borderPadding, cellSize, activeCamera);
        _borderAdjuster.Adjust();
    }
    private void InitializeCells()
    {
        cells = new Cell[size.x * size.y];
        Vector2 midPoint = transform.position;
        Vector2 initialOffset = new Vector2(-(cellSize.x * size.x / 2 - cellSize.x/2), (cellSize.y * size.y / 2) - (cellSize.y/2));
        Vector2 addedOffset = Vector2.zero;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Cell cell = Instantiate(cellPrefab, midPoint + initialOffset + addedOffset,quaternion.identity, cellParent.transform).GetComponent<Cell>();
                cell.gameObject.name = "Cell" + "[" + x + "]" + "[" + y + "]";
                cells[size.x * y + x] = cell;
                addedOffset.x += cellSize.x;
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
                int index = CalculateIndex(x, y);
                var output = LevelManager.Instance.GetTypes(index);
                CreateNewItem(output.type, new Vector2Int(x, y), output.color);
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
                createdItems.Add(CreateNewItem(ItemType.Cube, new Vector2Int(x,-1), ColorType.Random));
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

    public Item CreateNewItem(ItemType type, Vector2Int pos, ColorType color = ColorType.NoColor)
    {
        Item item;
        if (IsInBoard(pos))
        {
            item = ObjectPool.Instance.CreateItem(type, cells[pos.y * size.x + pos.x].transform.position, color);
            item.transform.SetParent(itemParent.transform);
            item.InitializeItemInBoard(pos);
        }
        else
        {
            item = ObjectPool.Instance.CreateItem(type, columnQueues[pos.x].GetNextPosition(), color);
            item.transform.SetParent(itemParent.transform);
            item.InitializeItemInBoard(pos);
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
    
    public void CheckBoardMatches()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = size.y-1; y >= 0; y--)
            {
                Item item = items[y * size.x + x];
                if (item == null)
                {
                    continue;
                }
                if (item.matchable)
                {
                    List<Item> matchedItems = CheckMatches(item.pos.x, item.pos.y);
                    int matchCount = matchedItems.Count;
                    matchedItems.ForEach(theItem => theItem.UpdateMatchCount(matchCount));
                }
            }
        }
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

    public List<Item> AroundItems(Vector2Int itemPos)
    {
        List<Item> itemList = new List<Item>();
        int leftIndex = CalculateIndex(itemPos.x - 1, itemPos.y);
        int rightIndex = CalculateIndex(itemPos.x + 1, itemPos.y);
        int topIndex = CalculateIndex(itemPos.x, itemPos.y + 1);
        int bottomIndex = CalculateIndex(itemPos.x, itemPos.y - 1);

        Item item;
        if (IsInBoard(leftIndex))
        {
            item = items[leftIndex];
            if (item != null)
            {
                itemList.Add(item);
            }
        }

        if (IsInBoard(rightIndex))
        {
            item = items[rightIndex];
            if (item != null)
            {
                itemList.Add(item);
            }
        }

        if (IsInBoard(topIndex))
        {
            item = items[topIndex];
            if (item != null)
            {
                itemList.Add(item);
            }
        }

        if (IsInBoard(bottomIndex))
        {
            item = items[bottomIndex];
            if (item != null)
            {
                itemList.Add(item);
            }
        }
        return itemList;
    }
    
    public int CalculateIndex(int x, int y)
    {
        var pos = new Vector2Int(x, y);
        if (!IsInBoard(pos))
        {
            return -1;
        }
        return y * size.x + x;
    }

    public bool IsInBoard(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= size.x || pos.y < 0 || pos.y >= size.y)
        {
            return false;
        }
        return true;
    }

    private bool IsInBoard(int index)
    {
        if (index < 0 || index >= size.x * size.y)
        {
            return false;
        }

        return true;
    }


    private bool DoesMatchExist()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = size.y - 1; y >= 0; y--)
            {
                Item item = items[CalculateIndex(x, y)];
                if (item != null && items[CalculateIndex(x, y)].interactable)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private IEnumerator ShuffleBoardCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        int depth = 5;
        while (depth > 0)
        {
            int n = items.Length;
            while (n > 1)
            {
                n--;
                if (items[n] == null || items[n] is Obstacle)
                    continue;

                int k = Random.Range(0, n + 1);
                if (items[k] == null || items[k] is Obstacle)
                    continue;

                Item temp = items[n];
                items[n] = items[k];
                items[k] = temp;
            }
            
            for (int x = 0; x < size.x; x++)
            {
                for (int y = size.y - 1; y >= 0; y--)
                {
                    if (items[CalculateIndex(x, y)] == null)
                    {
                        continue;
                    }
                    items[CalculateIndex(x, y)].transform.position = cells[CalculateIndex(x, y)].transform.position;
                    items[CalculateIndex(x, y)].pos = new Vector2Int(x, y);
                }
            }
            CheckBoardMatches();
            if (DoesMatchExist())
            {
                print("Board shuffled");
                break;
            }
            depth -= 1;
        }
        if (depth == 0)
        {
            Debug.LogError("CANNOT GENERATE MATCH");
        }

        LevelManager.Instance.state = LevelState.Idle;

    }

    public void RegisterFallingObject(int value)
    {
        _fallingObjectCount += value;

        if (_fallingObjectCount == 0)
        {
            TryShuffle();
        }
    }

    public bool TryShuffle()
    {
        if (!DoesMatchExist() && LevelManager.Instance.state != LevelState.Shuffling)
        {
            LevelManager.Instance.state = LevelState.Shuffling;
            StartCoroutine(ShuffleBoardCoroutine());
            return true;
        }

        return false;
    }
}
