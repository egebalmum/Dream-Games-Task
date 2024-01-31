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

   private void OnMouseDown()
   {
      if (!touchable)
      {
         //Play feedback animation in coroutine
         return;
      }
      Board.Instance.TouchItem(this);
   }

   public void Fall()
   {
      int destinationY = GetDestinationY();
      falling = true;
      Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = null;
      Board.Instance.items[destinationY * Board.Instance.size.x + pos.x] = this;
      pos.y = destinationY;
      transform.DOMoveY(Board.Instance.cells[pos.y * Board.Instance.size.x + pos.x].transform.position.y, 0.25f).OnComplete(() =>falling=false);
   }

   private int GetDestinationY()
   {
      int destinationY = -1;
      for (int y = pos.y + 1; y < Board.Instance.size.y; y++)
      {
         if (Board.Instance.items[y * Board.Instance.size.x + pos.x] == null)
         {
            destinationY = y;
            continue;
         }
         break;
      }
      return destinationY;
   }
   public abstract void TouchBehaviour();
}