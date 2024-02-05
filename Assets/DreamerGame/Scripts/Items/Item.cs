using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public abstract class Item : MonoBehaviour
{
    [Header("Components")]
   public SpriteRenderer spriteRenderer;
   public BoxCollider2D boxCollider;
   
   [Header("Attributes")]
   public bool fallable;
   public bool matchable;
   public int minMatchCount;
   public ColorType color;
   public ItemType type;
   [HideInInspector] public bool interactable;
   [SerializeField] public ItemSprite.SpecialSpriteContainer[] specialSpriteContainers;
   [SerializeField] public ItemSprite.SpriteContainer[] spriteContainers;
   [HideInInspector] public Vector2Int pos;
   [HideInInspector] public Vector2Int destinationPos;
   [HideInInspector] public float speed;
   [HideInInspector] public bool falling;
   [HideInInspector] public int matchCount = 1;
   [HideInInspector] public int activeState = 0;
   [HideInInspector] public UnityEvent OnMatchCountUpdated;
   [HideInInspector] public ColorStorage[] colorStorages;
   private Coroutine fallCoroutine;
   
   
   private readonly Vector2Int invalidPos = new Vector2Int(-1, -1);
   
   private void OnMouseDown()
   {
      if (!interactable)
      {
         //Play feedback animation in coroutine
         return;
      }
      LevelManager.Instance.TouchEvent(this);
   }

   public void InitializeItem()
   {
       colorStorages = GetComponents<ColorStorage>();
   }
   public virtual void InitializeItemInBoard(Vector2Int initialPos)
   {
       if (Board.Instance.IsInBoard(initialPos))
       {
           Board.Instance.items[initialPos.y * Board.Instance.size.x + initialPos.x] = this;
       }
       else
       {
           Board.Instance.columnQueues[initialPos.x].Enqueue(this);
       }
       pos = initialPos;
       destinationPos = invalidPos;
   }
   
    public bool Fall()
    {
        Vector2Int nextStop = GetNextStop();
        if (nextStop == invalidPos)
        {
            return false;
        }
        
        falling = true;
        Board.Instance.RegisterFallingObject(1);
        
        if (matchable && Board.Instance.IsInBoard(pos))
        {
            UpdateMatchCount(1);
            var items = Board.Instance.AroundItems(pos);
            items.ForEach(item => item.UpdateMatches());
        }
        fallCoroutine = StartCoroutine(FallCoroutine(nextStop));
        return true;
    }

    private void StopFall()
    {
        SetDestinationPos(invalidPos);
        
        falling = false;
        speed = 0;
        UpdateMatches();
        Board.Instance.RegisterFallingObject(-1);
    }
    
    IEnumerator FallCoroutine(Vector2Int nextStop)
    {
        SetDestinationPos(nextStop);
        while (falling)
        {
            int currentIndex = CalculateIndex(pos.x, pos.y);
            int destinationIndex = CalculateIndex(destinationPos.x, destinationPos.y);
            
            Item bottomItem;
            if (!Board.Instance.IsInBoard(pos))
            {
                bottomItem = Board.Instance.columnQueues[pos.x].GetFrontItem(this);
                if (bottomItem == null)
                {
                    bottomItem = Board.Instance.items[destinationIndex];
                }
            }
            else
            {
                bottomItem = Board.Instance.items[destinationIndex];
            }
            if (bottomItem != null)
            {
                float distanceYBetween = transform.position.y - bottomItem.transform.position.y;
                if (distanceYBetween <= Board.Instance.cellSize.y)
                {
                    AdjustItemOnHit(bottomItem);
                }
            }
            if (IsReachedDestination(destinationIndex))
            {
                if (!UpdatePosition(destinationIndex, currentIndex))
                {
                    speed = Board.Instance.items[destinationIndex].speed;
                    yield return new WaitWhile(() => Board.Instance.items[destinationIndex] != null);
                    UpdatePosition(destinationIndex, currentIndex);
                }
                nextStop = GetNextStop();
                if (nextStop == invalidPos)
                {
                    break;
                }
                SetDestinationPos(nextStop);
                Board.Instance.FallItemsInColumn(pos.x);
            }
            speed = Mathf.Min(speed + GameManager.Instance.acceleration * Time.deltaTime, GameManager.Instance.speedLimit);
            transform.position += -Vector3.up * (speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        StopFall();

        void AdjustItemOnHit(Item bottomItem)
        {
            if (GameManager.Instance.speedTransferType == SpeedTransferType.TopToBottom)
            {
                speed = bottomItem.speed;
                transform.position = bottomItem.transform.position + (Vector3.up * Board.Instance.cellSize.y);
            }
            else if (GameManager.Instance.speedTransferType == SpeedTransferType.BottomToTop)
            {
                bottomItem.speed = speed;
                transform.position = bottomItem.transform.position + (Vector3.up * Board.Instance.cellSize.y);
            }
        }
        bool IsReachedDestination(int destinationIndex)
        {
            float distanceY = transform.position.y - Board.Instance.cells[destinationIndex].transform.position.y;
            return distanceY <= GameManager.Instance.fallStopThreshold;
        }
        bool UpdatePosition(int destinationIndex, int currentIndex)
        {
            if (Board.Instance.items[destinationIndex] == null)
            {
                if (currentIndex >= 0)
                {
                    Board.Instance.items[currentIndex] = null;
                }
                else
                {
                    Board.Instance.columnQueues[pos.x].Dequeue(this);
                }
                Board.Instance.items[destinationIndex] = this;
                transform.position = Board.Instance.cells[destinationIndex].transform.position;
                pos = destinationPos;
                SetDestinationPos(invalidPos);
                return true;
            }
            return false;
        }
    }
    
    private Vector2Int GetNextStop()
    {
        Vector2Int nextStop = new Vector2Int(pos.x, pos.y + 1);
        if (!Board.Instance.IsInBoard(nextStop)) 
            return invalidPos;
    
        int bottomIndex = CalculateIndex(nextStop.x, nextStop.y);
        Item bottomItem = Board.Instance.items[bottomIndex];

        if (bottomItem == null || (bottomItem.falling && bottomItem.destinationPos != bottomItem.pos))
            return nextStop;
        return invalidPos;
    }

    public void SetDestinationPos(Vector2Int newDestinationPos)
    {
        destinationPos = newDestinationPos;
    }

    private int CalculateIndex(int x, int y)
    {
        return Board.Instance.CalculateIndex(x, y);
    }
    private void DestroyItem()
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine);
        }
        Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = null;
        if (falling)
        {
            speed = 0;
            falling = false;
            Board.Instance.RegisterFallingObject(-1);
        }
        SetDestinationPos(invalidPos);
        UpdateMatchCount(1);
        var items = Board.Instance.AroundItems(pos);
        items.ForEach(item => item.UpdateMatches());
        LevelManager.Instance.DecrementGoal((type, color));
        ObjectPool.Instance.DestroyItem(this);
        spriteRenderer.sprite = spriteContainers[0].sprite;
    }


    public void UpdateMatchCount(int value)
    {
        matchCount = value;
        if (matchCount >= minMatchCount)
        {
            interactable = true;
        }
        else
        {
            interactable = false;
        }
        OnMatchCountUpdated?.Invoke();
    }
    public void UpdateMatches()
    {
        if (matchable && Board.Instance.IsInBoard(pos))
        {
            List<Item> items = Board.Instance.CheckMatches(pos.x, pos.y);
            if (items.Count == 0)
            {
                UpdateMatchCount(1);
                return;
            }

            int matchCount = items.Count;
            items.ForEach(item => item.UpdateMatchCount(matchCount));
        }
    }

    public void SetColor(ColorType newColor)
    {
        if (colorStorages.Length == 0)
        {
            Debug.Log("Item is not colored");
            return;
        }
        
        if (newColor == ColorType.Random)
        {
            var colors = Enum.GetValues(typeof(ColorType));
            int selectedIndex = Random.Range(2, colors.Length);
            newColor = (ColorType) colors.GetValue(selectedIndex);
            
        }
        var storage = colorStorages.First(storage => storage.color == newColor);
        color = storage.color;
        specialSpriteContainers = storage.specialStates;
        spriteContainers = storage.states;
        spriteRenderer.sprite = spriteContainers[activeState].sprite;
    }

    public virtual void GetDamage()
    {
        DestroyItem();
    }

    public bool IsMarked(HashSet<Item> markedItems)
    {
        if (markedItems.Contains(this))
        {
            return true;
        }

        return false;
    }
    public virtual void TouchBehaviour(HashSet<Item> markedItems)
    {
        markedItems.Add(this);
        //
    }

    public virtual void ExplosionBehavior(HashSet<Item> markedItems)
    {
        markedItems.Add(this);
        //
    }

    public virtual void BlastBehaviour(HashSet<Item> markedItems)
    {
        markedItems.Add(this);
        //
    }

    public virtual void NearBlastBehaviour(HashSet<Item> markedItems)
    {
        markedItems.Add(this);
        //
    }
}