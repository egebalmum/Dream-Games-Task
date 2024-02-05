using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Item
{
    public override void NearBlastBehaviour(HashSet<Item> markedItems)
    {
        if (IsMarked(markedItems))
        {
            return;
        }
        base.NearBlastBehaviour(markedItems);
        
        GetDamage();
    }

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
