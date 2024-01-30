using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : Item
{
    public override void TouchBehaviour()
    {
        
        for (int x = pos.x - 1; x < pos.x + 2; x++)
        {
            for (int y = pos.y - 1; y < pos.y + 2; y++)
            {
                if (x >= 0 && x < Board.Instance.size.x && y >= 0 && y < Board.Instance.size.y)
                {
                    Item item = Board.Instance.items[y * Board.Instance.size.x + x];
                    if (item != null)
                    {
                        Board.Instance.items[y * Board.Instance.size.x + x] = null;
                        Destroy(item.gameObject);
                    }
                }
            }
        }
    }
}
