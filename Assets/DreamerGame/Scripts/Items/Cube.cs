using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : Item
{
    public override void TouchBehaviour()
    {
        var items = Board.Instance.CheckMatches(pos.x, pos.y);
        if (items == null || items.Count < 2)
        {
            return;
        }
        items.Remove(this);
        foreach (var item in items)
        {
            item.SetDestinationPos(invalidPos);
            Board.Instance.items[CalculateIndex(item.pos.x, item.pos.y)] = null;
            Destroy(item.gameObject);
        }
        SetDestinationPos(invalidPos);
        Board.Instance.items[CalculateIndex(pos.x, pos.y)] = null;
        Destroy(gameObject);
    }
}
