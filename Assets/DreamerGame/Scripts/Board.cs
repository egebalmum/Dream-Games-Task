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
    private float _shadedFaceLength;
    private SpriteRenderer _borderSprite;
    
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
        AdjustBorder();
        InitializeCells();
        InitializeItems();
        InitializeQueuedItemCount();
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
        InitializeItemDimensions();
    }
    private void InitializeItemDimensions()
    {
        Item defaultItem = defaultItemPrefab.GetComponent<Item>();
        cellSize = defaultItem.boxCollider.size;
        _shadedFaceLength = defaultItem.spriteRenderer.size.y - cellSize.y;
    }
    private void AdjustBorder()
    {
        float desiredRatio = boardBorderRect.rect.width / boardBorderRect.rect.height;
        Vector2 midPoint = new Vector2(boardBorderRect.transform.position.x, boardBorderRect.transform.position.y);
        float left = Mathf.Abs(boardBorderRect.offsetMin.x);
        float right = Mathf.Abs(boardBorderRect.offsetMax.x);
        float top = Mathf.Abs(boardBorderRect.offsetMax.y);
        float bottom = Mathf.Abs(boardBorderRect.offsetMin.y);
        float ratioX = boardBorderRect.rect.width / (boardBorderRect.rect.width + left + right);
        float ratioY = boardBorderRect.rect.height / (boardBorderRect.rect.height + top + bottom);
        _borderSprite.size = new Vector2(cellSize.x * size.x + borderPadding, cellSize.y * size.y + borderPadding + _shadedFaceLength);
        float currentRatio = _borderSprite.size.x / _borderSprite.size.y;
        float oldSize = activeCamera.orthographicSize;
        if (currentRatio < desiredRatio)
        {
            activeCamera.orthographicSize = (cellSize.y * size.y + borderPadding) / ratioY / 2;
        }
        else
        {
            activeCamera.orthographicSize = ((cellSize.x * size.x + borderPadding) / ratioX) / (2 * activeCamera.aspect);
        }
        float sizeMultiplier = activeCamera.orthographicSize / oldSize;
        transform.position = midPoint;
        Vector2 adjustedPosition = transform.position;
        adjustedPosition.y *= sizeMultiplier;
        transform.position = adjustedPosition;
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
    private void InitializeItems() //UPDATE
    {
        items = new Item[size.x * size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Item item = Instantiate(testItemPrefabs[Random.Range(0,testItemPrefabs.Length)], cells[size.x*y + x].transform.position, quaternion.identity, itemParent.transform).GetComponent<Item>();
                item.InitializeItemInBoard(new Vector2Int(x,y));
            }
        }
    }

    private void InitializeQueuedItemCount()
    {
        columnQueues = new ColumnQueue[size.x];
        for (int x = 0; x < size.x; x++)
        {
            columnQueues[x] = new ColumnQueue(cells[x].transform.position + (Vector3.up * cellSize.y));
        }
    }

    private void CreateNewItems()
    {
        List<Item> items = new List<Item>();
        for (int x = 0; x < size.x; x++)
        {
            int emptyCellCount = GetEmptyCellCountInColumn(x);
            for (int index = 0; index < emptyCellCount; index++)
            {
                items.Add(CreateNewItem(x));
            }
        }
        FallNewItems(items);
    }

    private void FallNewItems(List<Item> items)
    {
        foreach (Item item in items)
        {
            item.FallIntoBoard();
        }
    }

    private Item CreateNewItem(int x)
    {
        Item item = Instantiate(testItemPrefabs[Random.Range(0,testItemPrefabs.Length)], columnQueues[x].GetNextPosition(), quaternion.identity, itemParent.transform).GetComponent<Item>();
        item.InitializeItemAboveBoard(x);
        columnQueues[x].Enqueue(item);
        return item;
    }

    private int GetEmptyCellCountInColumn(int x)
    {
        int count = 0;
        for (int y = 0; y < size.y; y++)
        {
            Item item = items[y * size.x + x];
            if (item == null)
            {
                count += 1;
            }
            else if (!item.fallable)
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

    public void FindMatches(int x, int y, ItemColor color, List<Item> matches)
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
}
