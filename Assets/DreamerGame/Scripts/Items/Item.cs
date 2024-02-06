using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Item : MonoBehaviour
{
    [Header("Components")]
   public SpriteRenderer spriteRenderer;
   public BoxCollider2D boxCollider;
   
   [Header("Attributes")]
   public bool fallable;
   public bool matchable;
   public int minMatchCount;
   public ItemType type;
   public ColorType color;
   [HideInInspector] public bool interactable;
   [HideInInspector] public ItemSprite itemSprite;
   [HideInInspector] public Vector2Int pos;
   [HideInInspector] public Vector2Int destinationPos;
   [HideInInspector] public float speed;
   [HideInInspector] public bool falling;
   [HideInInspector] public int matchCount = 1;
   [HideInInspector] public int activeState;
   [HideInInspector] public UnityEvent OnMatchCountUpdated;
   private readonly Vector2Int _invalidPos = new Vector2Int(-1, -1);
   
   private void OnMouseDown()
   {
      if (!interactable)
      {
         //Play feedback animation in coroutine
         return;
      }
      LevelManager.Instance.TouchEvent(this);
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
       destinationPos = _invalidPos;
   }
   
    public bool Fall()
    {
        Vector2Int nextStop = GetNextStop();
        if (nextStop == _invalidPos)
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
        StartCoroutine(FallCoroutine(nextStop));
        return true;
    }

    private void StopFall()
    {
        SetDestinationPos(_invalidPos);
        
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
                if (nextStop == _invalidPos)
                {
                    break;
                }
                SetDestinationPos(nextStop);
                Board.Instance.FallItemsInColumn(pos.x);
            }
            speed = Mathf.Min(speed + GameManager.Instance.gameSettings.acceleration * Time.deltaTime, GameManager.Instance.gameSettings.speedLimit);
            transform.position += -Vector3.up * (speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        StopFall();

        void AdjustItemOnHit(Item bottomItem)
        {
            if (GameManager.Instance.gameSettings.speedTransferType == SpeedTransferType.TopToBottom)
            {
                speed = bottomItem.speed;
                transform.position = bottomItem.transform.position + (Vector3.up * Board.Instance.cellSize.y);
            }
            else if (GameManager.Instance.gameSettings.speedTransferType == SpeedTransferType.BottomToTop)
            {
                bottomItem.speed = speed;
                transform.position = bottomItem.transform.position + (Vector3.up * Board.Instance.cellSize.y);
            }
        }
        bool IsReachedDestination(int destinationIndex)
        {
            float distanceY = transform.position.y - Board.Instance.cells[destinationIndex].transform.position.y;
            return distanceY <= GameManager.Instance.gameSettings.fallStopThreshold;
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
                SetDestinationPos(_invalidPos);
                return true;
            }
            return false;
        }
    }
    
    private Vector2Int GetNextStop()
    {
        Vector2Int nextStop = new Vector2Int(pos.x, pos.y + 1);
        if (!Board.Instance.IsInBoard(nextStop)) 
            return _invalidPos;
    
        int bottomIndex = CalculateIndex(nextStop.x, nextStop.y);
        Item bottomItem = Board.Instance.items[bottomIndex];

        if (bottomItem == null || (bottomItem.falling && bottomItem.destinationPos != bottomItem.pos))
            return nextStop;
        return _invalidPos;
    }

    private void SetDestinationPos(Vector2Int newDestinationPos)
    {
        destinationPos = newDestinationPos;
    }

    private int CalculateIndex(int x, int y)
    {
        return Board.Instance.CalculateIndex(x, y);
    }
    private void DestroyItem()
    {
        StopAllCoroutines();
        Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = null;
        if (falling)
        {
            speed = 0;
            falling = false;
            Board.Instance.RegisterFallingObject(-1);
        }
        SetDestinationPos(_invalidPos);
        UpdateMatchCount(1);
        var items = Board.Instance.AroundItems(pos);
        items.ForEach(item => item.UpdateMatches());
        LevelManager.Instance.DecrementGoal((type, color));
        ObjectPool.Instance.DestroyItem(this);
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
    private void UpdateMatches()
    {
        if (matchable && Board.Instance.IsInBoard(pos))
        {
            List<Item> items = Board.Instance.CheckMatches(pos.x, pos.y);
            if (items.Count == 0)
            {
                UpdateMatchCount(1);
                return;
            }

            int matchedItemCount = items.Count;
            items.ForEach(item => item.UpdateMatchCount(matchedItemCount));
        }
    }

    public void SetColor(ColorType newColor)
    {
        ColorVariance variance = ItemFactory.Instance.GetColorVariance(type, newColor);
        itemSprite = variance.itemSprite;
        color = variance.color;
        spriteRenderer.sprite = itemSprite.sprites[activeState].sprite;
    }

    protected virtual void GetDamage()
    {
        DestroyItem();
    }

    protected bool IsMarked(ItemTracker tracker)
    {
        if (tracker.markedItems.Contains(this))
        {
            return true;
        }

        return false;
    }
    public virtual void TouchBehaviour(ItemTracker tracker)
    {
        tracker.markedItems.Add(this);
        //
    }

    public virtual void ExplosionBehavior(ItemTracker tracker)
    {
        tracker.markedItems.Add(this);
        //
    }

    public virtual void BlastBehaviour(ItemTracker tracker)
    {
        tracker.markedItems.Add(this);
        //
    }

    public virtual void NearBlastBehaviour(ItemTracker tracker)
    {
        tracker.markedItems.Add(this);
        //
    }
}