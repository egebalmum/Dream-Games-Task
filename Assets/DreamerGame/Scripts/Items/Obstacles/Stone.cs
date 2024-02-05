using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Item
{
    public override void ExplosionBehavior(HashSet<Item> markedItems)
    {
        if (IsMarked(markedItems))
        {
            return;
        }
        base.ExplosionBehavior(markedItems);
        
        GetDamage();
    }
}
