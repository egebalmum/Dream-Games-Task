using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : Item
{
    public override void TouchBehaviour()
    {
        ReleaseReservedCell();
        Board.Instance.items[pos.y * Board.Instance.size.x + pos.x] = null;
        Destroy(gameObject);
    }
}
