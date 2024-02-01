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
   public bool falling;
   public Vector2Int pos;
   public Vector2Int reservedCell;
   
   private void OnMouseDown()
   {
      if (!touchable)
      {
         //Play feedback animation in coroutine
         return;
      }
      Board.Instance.TouchItem(this);
   }
   
   public void InitializeItem(Vector2Int posInBoard)
   {
      pos = posInBoard;
      Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = this;
      reservedCell = new Vector2Int(-1, -1);
   }
   
   public void Fall()
    {
        int destinationY = GetBottomY();
        if (destinationY == -1)
        {
            return;
        }
        falling = true;
        StartCoroutine(FallCoroutine(destinationY));
    }

    IEnumerator FallCoroutine(int destinationY)
    {
        ReserveCell(pos.x, destinationY);
        float speed = 0;

        while (true)
        {
            int destinationIndex = CalculateIndex(pos.x, destinationY);
            int currentIndex = CalculateIndex(pos.x, pos.y);

            if (IsReachedDestination(destinationIndex))
            {
                UpdatePosition(destinationIndex, currentIndex, ref destinationY);
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
        ReleaseReservedCell();
        falling = false;
        
        bool IsReachedDestination(int destinationIndex)
        {
            float distanceY = transform.position.y - Board.Instance.cells[destinationIndex].transform.position.y;
            return distanceY <= GameManager.Instance.fallStopThreshold;
        }

        void UpdatePosition(int destinationIndex, int currentIndex, ref int destinationY)
        {
            if (Board.Instance.items[currentIndex] == this)
                Board.Instance.items[currentIndex] = null;
    
            Board.Instance.items[destinationIndex] = this;
            pos.y = destinationY;
            transform.position = Board.Instance.cells[destinationIndex].transform.position;
            Board.Instance.cells[destinationIndex].reserved = false;
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