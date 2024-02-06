using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TNT : Item
{
    [Header("Type Specific Attributes")]
    [SerializeField] private int diameter = 5;

    [SerializeField] private int comboDiameter = 7;
    public override void TouchBehaviour(ItemTracker tracker)
    {
        base.TouchBehaviour(tracker);
        
        GetDamage();
        var aroundItems = Board.Instance.AroundItems(pos);
        bool comboCondition = aroundItems.Any(item => item.type == ItemType.TNT);
        int usedDiameter;
        if (comboCondition)
        {
            usedDiameter = comboDiameter;
        }
        else
        {
            usedDiameter = diameter;
        }
        for (int x = pos.x - (usedDiameter-1)/2; x < pos.x + (usedDiameter-1)/2 + 1; x++)
        {
            for (int y = pos.y - (usedDiameter-1)/2; y < pos.y + (usedDiameter-1)/2 + 1; y++)
            {
                if (x >= 0 && x < Board.Instance.size.x && y >= 0 && y < Board.Instance.size.y)
                {
                    Item item = Board.Instance.items[y * Board.Instance.size.x + x];
                    if (item != null)
                    {
                        item.ExplosionBehavior(tracker);
                    }
                }
            }
        }
    }

    public override void ExplosionBehavior(ItemTracker tracker)
    {
        if (IsMarked(tracker))
        {
            return;
        }
        base.ExplosionBehavior(tracker);
        
        GetDamage();
        for (int x = pos.x - (diameter-1)/2; x < pos.x + (diameter-1)/2 + 1; x++)
        {
            for (int y = pos.y - (diameter-1)/2; y < pos.y + (diameter-1)/2 + 1; y++)
            {
                if (x >= 0 && x < Board.Instance.size.x && y >= 0 && y < Board.Instance.size.y)
                {
                    Item item = Board.Instance.items[y * Board.Instance.size.x + x];
                    if (item != null)
                    {
                        item.ExplosionBehavior(tracker);
                    }
                }
            }
        }
    }
}
