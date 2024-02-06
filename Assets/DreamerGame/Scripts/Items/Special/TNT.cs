using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class TNT : Item
{
    [Header("Type Specific Attributes")]
    [SerializeField] private int diameter = 5;

    [SerializeField] private int comboDiameter = 7;
    public override void TouchBehaviour(ItemTracker tracker)
    {
        base.TouchBehaviour(tracker);
        var connectedTNTs = Board.Instance.CheckMatches(pos.x, pos.y);
        connectedTNTs.Remove(this);
        bool comboCondition = connectedTNTs.Count != 0;
        if (comboCondition)
        {
            StartCoroutine(ComboTouchBehaviour(connectedTNTs, tracker));
        }
        else
        {
            StartCoroutine(NormalTouchBehaviour(connectedTNTs, tracker));
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

    private IEnumerator NormalTouchBehaviour(List<Item> connectedTNTs, ItemTracker tracker)
    {
        tracker.coroutineCount += 1;
        
        yield return AnimationManager.TNTAnimation(this, 0.5f);
        
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
        GetDamage();
        yield return new WaitForEndOfFrame();
        tracker.coroutineCount -= 1;
    }

    private IEnumerator ComboTouchBehaviour(List<Item> connectedTNTs, ItemTracker tracker)
    {
        tracker.coroutineCount += 1;
        var tntItems = connectedTNTs.Where(item => item.type == ItemType.TNT).ToList();
        yield return AnimationManager.TNTComboAnimation(tntItems, this, 1f);
        
        for (int x = pos.x - (comboDiameter-1)/2; x < pos.x + (comboDiameter-1)/2 + 1; x++)
        {
            for (int y = pos.y - (comboDiameter-1)/2; y < pos.y + (comboDiameter-1)/2 + 1; y++)
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
        GetDamage();
        yield return new WaitForEndOfFrame();
        tracker.coroutineCount -= 1;
    }
    
}
