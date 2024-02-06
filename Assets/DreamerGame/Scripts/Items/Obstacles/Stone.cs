using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Item
{
    public override void ExplosionBehavior(ItemTracker tracker)
    {
        if (IsMarked(tracker))
        {
            return;
        }
        base.ExplosionBehavior(tracker);
        
        GetDamage();
    }
}
