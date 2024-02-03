using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : Item
{
    [Header("Type Specific Attributes")]
    [SerializeField] private int diameter = 3;
    public override void TouchBehaviour()
    {
        DestroyItem();
        for (int x = pos.x - (diameter-1)/2; x < pos.x + (diameter-1)/2 + 1; x++)
        {
            for (int y = pos.y - (diameter-1)/2; y < pos.y + (diameter-1)/2 + 1; y++)
            {
                if (x >= 0 && x < Board.Instance.size.x && y >= 0 && y < Board.Instance.size.y)
                {
                    Item item = Board.Instance.items[y * Board.Instance.size.x + x];
                    if (item != null)
                    {
                        item.ExplosionBehavior();
                    }
                }
            }
        }
    }

    public override void ExplosionBehavior()
    {
        DestroyItem();
        for (int x = pos.x - (diameter-1)/2; x < pos.x + (diameter-1)/2 + 1; x++)
        {
            for (int y = pos.y - (diameter-1)/2; y < pos.y + (diameter-1)/2 + 1; y++)
            {
                if (x >= 0 && x < Board.Instance.size.x && y >= 0 && y < Board.Instance.size.y)
                {
                    Item item = Board.Instance.items[y * Board.Instance.size.x + x];
                    if (item != null)
                    {
                        item.ExplosionBehavior();
                    }
                }
            }
        }
    }
}
