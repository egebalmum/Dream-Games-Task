using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
   public SpriteRenderer spriteRenderer;
   public BoxCollider2D boxCollider;
   public bool fallable;
   public bool touchable;
   public bool matchable;
   public ItemColor color;
   public bool falling;
   public Vector2Int pos;
   public Vector2Int destinationPos;
   public float speed;
   protected readonly Vector2Int invalidPos = new Vector2Int(-1, -1);
   
   private void OnMouseDown()
   {
      if (!touchable)
      {
         //Play feedback animation in coroutine
         return;
      }
      Board.Instance.TouchItem(this);
   }
   
   public void InitializeItem(Vector2Int initialPos)
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
        StartCoroutine(FallCoroutine(nextStop));
        return true;
    }

    private void StopFall()
    {
        SetDestinationPos(invalidPos);
        falling = false;
        speed = 0;
        //CheckMatches();
    }
    
    public List<Item> CheckMatches()
    {
        List<Item> matchedItems = Board.Instance.CheckMatches(pos.x, pos.y);
        return matchedItems;
    }
    
    IEnumerator FallCoroutine(Vector2Int nextStop)
    {
        SetDestinationPos(nextStop);
        while (true)
        {
            int currentIndex = CalculateIndex(pos.x, pos.y);
            int destinationIndex = CalculateIndex(destinationPos.x, destinationPos.y);
            Item bottomItem = Board.Instance.items[destinationIndex];
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
            if (GameManager.Instance.speedTransferType == 1) //slow down above object
            {
                speed = bottomItem.speed;
                transform.position = bottomItem.transform.position + (Vector3.up * Board.Instance.cellSize.y);
            }
            else if (GameManager.Instance.speedTransferType == 2) //speed up bottom object
            {
                bottomItem.speed = speed;
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
        this.destinationPos = newDestinationPos;
    }

    private int CalculateIndex(int x, int y)
    {
        return y * Board.Instance.size.x + x;
    }
   public abstract void TouchBehaviour();
}