using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Obstacle
{
    public override void ExplosionBehavior(HashSet<Item> markedItems)
    {
        GetDamage();
    }
}
