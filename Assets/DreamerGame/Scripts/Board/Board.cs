using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        int createdItemCount = CreateNewItems();
        if (createdItemCount == 0 && _fallingObjectCount == 0)
        {
            TryShuffle();
        }
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
        size = new Vector2Int(LevelManager.Instance.levelData.grid_width, LevelManager.Instance.levelData.grid_height); 
        AdjustBorder();
        InitializePool();
        InitializeCells();
        InitializeItems();
        InitializeGoals();
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

    private void InitializeGoals()
    {
        LevelManager.Instance.InitializeGoals(items);
    }

    private int CreateNewItems()
    {
        List<Item> createdItems = new List<Item>();
        for (int x = 0; x < size.x; x++)
        {
            int fallableCellCount = GetEmptyCellCountInColumn(x);
            for (int index = 0; index < fallableCellCount; index++)
            {
                createdItems.Add(CreateNewItem(ItemType.Cube, new Vector2Int(x,-1)));
            }
        }
        FallNewItems(createdItems);
        return createdItems.Count;
    }

    private void FallNewItems(List<Item> newItems)
    {
        foreach (Item item in newItems)
        {
            item.Fall();
        }
    }

    public Item CreateNewItem(ItemType type, Vector2Int pos, ColorType color = ColorType.Random)
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
        FindMatches(startX, startY, items[startY* size.x + startX].type , items[startY* size.x + startX].color, matches, items[startY* size.x + startX].matchable);
        return matches;
    }
    
    private void FindMatches(int x, int y, ItemType type, ColorType color, List<Item> matches, bool isMatchable)
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

        if (item.falling || (isMatchable && !item.matchable) || matches.Contains(item) || item.color != color || item.type != type)
        {
            return;
        }
        
        matches.Add(item);
        FindMatches(x+1, y, type, color, matches, isMatchable);
        FindMatches(x-1, y, type, color, matches, isMatchable);
        FindMatches(x, y+1, type, color, matches, isMatchable);
        FindMatches(x, y-1, type, color, matches, isMatchable);
    }

    public List<Item> AroundItems(Vector2Int itemPos)
    {
        List<Item> itemList = new List<Item>();
        Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(-1, 0), 
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int direction in directions)
        {
            int neighborIndex = CalculateIndex(itemPos.x + direction.x, itemPos.y + direction.y);
            if (IsInBoard(neighborIndex))
            {
                Item item = items[neighborIndex];
                if (item != null)
                {
                    itemList.Add(item);
                }
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

    public Vector2Int CalculateVector(int index)
    {
        return new Vector2Int(index % size.x, index / size.x);
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
        int depth = 6;
        while (depth > 0)
        {
            int n = items.Length;
            while (n > 1)
            {
                n--;
                if (items[n] == null || !items[n].matchable)
                    continue;

                int k = Random.Range(0, n + 1);
                if (items[k] == null || !items[k].matchable)
                    continue;

                Item temp = items[n];
                items[n] = items[k];
                items[k] = temp;
            }
            
            for (int index = 0; index < items.Length; index++)
            {
                if (items[index] == null)
                {
                    continue;
                }

                items[index].pos = CalculateVector(index);
            }
            CheckBoardMatches();
            if (DoesMatchExist())
            {
                for (int index = 0; index < items.Length; index++)
                {
                    if (items[index] == null)
                    {
                        continue;
                    }

                    items[index].transform.DOMove(cells[index].transform.position, 0.25f).SetEase(Ease.InOutCubic);
                }
                yield return new WaitForSeconds(0.25f);
                Debug.Log("Board shuffled");
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
