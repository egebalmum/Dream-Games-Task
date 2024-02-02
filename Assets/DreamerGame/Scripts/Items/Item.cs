using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
   public Vector2Int reservedCell;
   public float speed;
   
   private void OnMouseDown()
   {
      if (!touchable)
      {
         //Play feedback animation in coroutine
         return;
      }
      Board.Instance.TouchItem(this);
   }
   
   public void InitializeItemInBoard(Vector2Int posInBoard)
   {
      pos = posInBoard;
      Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = this;
      reservedCell = new Vector2Int(-1, -1);
   }

   public void InitializeItemAboveBoard(int x)
   {
       pos = new Vector2Int(x, -1);
       reservedCell = new Vector2Int(-1, -1);
   }
   
    public bool Fall()
    {
        int destinationY = GetBottomY();
        if (destinationY == -1)
        {
            return false;
        }
        StartFall();
        StartCoroutine(FallCoroutine(destinationY));
        return true;
    }

    private void StartFall()
    {
        falling = true;
    }

    private void StopFall()
    {
        ReleaseReservedCell();
        falling = false;
        speed = 0;
        //CheckMatches();
    }

    public List<Item> CheckMatches()
    {
        List<Item> matchedItems = Board.Instance.CheckMatches(pos.x, pos.y);
        return matchedItems;
    }
    public void FallIntoBoard()
    {
        falling = true;
        StartCoroutine(FallIntoBoardCoroutine());
    }

    IEnumerator FallCoroutine(int destinationY)
    {
        ReserveCell(pos.x, destinationY);
        while (true)
        {
            int destinationIndex = CalculateIndex(pos.x, destinationY);
            int currentIndex = CalculateIndex(pos.x, pos.y);

            Item bottomItem = Board.Instance.items[destinationIndex];
            if (bottomItem != null)
            {
                float distanceYBetween = transform.position.y - bottomItem.transform.position.y;
                if (distanceYBetween <= Board.Instance.cellSize.y)
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
            }
            if (IsReachedDestination(destinationIndex))
            {
                if (!UpdatePosition(destinationIndex, currentIndex, destinationY))
                {
                    speed = Board.Instance.items[destinationIndex].speed;
                    yield return new WaitWhile(() => Board.Instance.items[destinationIndex] != null);
                    UpdatePosition(destinationIndex, currentIndex, destinationY);
                }
                destinationY = GetBottomY();
                if (destinationY == -1)
                {
                    break;
                }
                ReserveCell(pos.x, destinationY);
                Board.Instance.FallItemsInColumn(pos.x);
            }
            speed = Mathf.Min(speed + GameManager.Instance.acceleration * Time.deltaTime, GameManager.Instance.speedLimit);
            transform.position += -Vector3.up * (speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        StopFall();
        bool IsReachedDestination(int destinationIndex)
        {
            float distanceY = transform.position.y - Board.Instance.cells[destinationIndex].transform.position.y;
            return distanceY <= GameManager.Instance.fallStopThreshold;
        }

        bool UpdatePosition(int destinationIndex, int currentIndex, int destinationY)
        {
            if (Board.Instance.items[destinationIndex] == null)
            {
                Board.Instance.items[currentIndex] = null;
                Board.Instance.items[destinationIndex] = this;
                pos.y = destinationY;
                transform.position = Board.Instance.cells[destinationIndex].transform.position;
                ReleaseReservedCell();
                return true;
            }
            return false;
        }
    }

    IEnumerator FallIntoBoardCoroutine()
    {
        while (true)
        {
            int destinationIndex = CalculateIndex(pos.x, 0);
            if (IsReachedDestination(destinationIndex))
            {
                if (!UpdatePosition(destinationIndex, 0))
                {
                    speed = Board.Instance.items[destinationIndex].speed;
                    yield return new WaitWhile(() => Board.Instance.items[destinationIndex] != null);
                    UpdatePosition(destinationIndex, 0);
                }
                break;
            }
            speed = Mathf.Min(speed + GameManager.Instance.acceleration * Time.deltaTime, GameManager.Instance.speedLimit);
            transform.position += -Vector3.up * (speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        Board.Instance.columnQueues[pos.x].Dequeue(this);
        if (!Fall())
        {
            StopFall();
        }
        bool IsReachedDestination(int destinationIndex)
        {
            float distanceY = transform.position.y - Board.Instance.cells[destinationIndex].transform.position.y;
            return distanceY <= GameManager.Instance.fallStopThreshold;
        }

        bool UpdatePosition(int destinationIndex, int destinationY)
        {
            if (Board.Instance.items[destinationIndex] == null)
            {
                Board.Instance.items[destinationIndex] = this;
                pos.y = destinationY;
                transform.position = Board.Instance.cells[destinationIndex].transform.position;
                return true;
            }
            return false;
        }
    }

    private int GetBottomY()
    {
        int nextY = pos.y + 1;
        if (nextY >= Board.Instance.size.y) return -1;
    
        int bottomIndex = CalculateIndex(pos.x, nextY);
        Item bottomItem = Board.Instance.items[bottomIndex];

        if ((bottomItem == null || bottomItem.falling) && !Board.Instance.cells[bottomIndex].reserved)
            return nextY;
        return -1;
    }

    public void ReleaseReservedCell()
    {
        if (reservedCell != new Vector2Int(-1,-1))
        {
            Board.Instance.cells[CalculateIndex(reservedCell.x, reservedCell.y)].reserved = false;
            reservedCell = new Vector2Int(-1,-1);
        }
    }

    public void ReserveCell(int x, int y)
    {
        ReleaseReservedCell();
        Board.Instance.cells[CalculateIndex(x, y)].reserved = true;
        reservedCell = new Vector2Int(x, y);
    }

    private int CalculateIndex(int x, int y)
    {
        return y * Board.Instance.size.x + x;
    }

   public abstract void TouchBehaviour();
}