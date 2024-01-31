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
    [SerializeField] public GameObject cellParent;
    [SerializeField] public GameObject itemParent;
    [SerializeField] public GameObject cellPrefab;
    [SerializeField] public Camera activeCamera;
    [SerializeField] private GameObject defaultItemPrefab;
    [SerializeField] private RectTransform boardBorderRect;
    [SerializeField] private GameObject[] testItemPrefabs;
    
    [HideInInspector] public Cell[] cells;
    [HideInInspector] public Item[] items;
    
    
    private Vector2 _cellSize;
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
    }

    public void FallItems()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = size.y-2; y >= 0; y--)
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
    }
    private void InitializeLevel()
    {
        AdjustBorder();
        InitializeCells();
        InitializeItems();
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
        _cellSize = defaultItem.boxCollider.size;
        _shadedFaceLength = defaultItem.spriteRenderer.size.y - _cellSize.y;
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
        _borderSprite.size = new Vector2(_cellSize.x * size.x + borderPadding, _cellSize.y * size.y + borderPadding + _shadedFaceLength);
        float currentRatio = _borderSprite.size.x / _borderSprite.size.y;
        float oldSize = activeCamera.orthographicSize;
        if (currentRatio < desiredRatio)
        {
            activeCamera.orthographicSize = (_cellSize.y * size.y + borderPadding) / ratioY / 2;
        }
        else
        {
            activeCamera.orthographicSize = ((_cellSize.x * size.x + borderPadding) / ratioX) / (2 * activeCamera.aspect);
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
        Vector2 initialOffset = new Vector2((_cellSize.x * size.x / 2) - (_cellSize.x/2), (_cellSize.y * size.y / 2) - (_cellSize.y/2));
        Vector2 addedOffset = Vector2.zero;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Cell cell = Instantiate(cellPrefab, midPoint + initialOffset + addedOffset,quaternion.identity, cellParent.transform).GetComponent<Cell>();
                cell.gameObject.name = "Cell" + "[" + x + "]" + "[" + y + "]";
                cells[size.x * y + x] = cell;
                addedOffset.x -= _cellSize.x;
            }
            addedOffset.x = 0;
            addedOffset.y -= _cellSize.y;
        }
    }
    private void InitializeItems()
    {
        items = new Item[size.x * size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Item item = Instantiate(testItemPrefabs[Random.Range(0,testItemPrefabs.Length)], cells[size.x*y + x].transform.position, quaternion.identity, itemParent.transform).GetComponent<Item>();
                item.pos = new Vector2Int(x, y);
                items[size.x * y + x] = item;
                
            }
        }
    }
}
