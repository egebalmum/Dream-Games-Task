using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Obstacle
{
    public override void NearBlastBehaviour(HashSet<Item> markedItems)
    {
        GetDamage();
    }

    public override void ExplosionBehavior(HashSet<Item> markedItems)
    {
        GetDamage();
    }
}
