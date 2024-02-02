using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : Item
{
    public override void TouchBehaviour()
    {
        var items = CheckMatches();
        if (items == null || items.Count < 2)
        {
            return;
        }
        items.Remove(this);
        foreach (var item in items)
        {
            item.SetDestinationPos(invalidPos);
            Board.Instance.items[item.pos.y * Board.Instance.size.x + item.pos.x] = null;
            Destroy(item.gameObject);
        }
        SetDestinationPos(invalidPos);
        Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = null;
        Destroy(gameObject);
    }
}
