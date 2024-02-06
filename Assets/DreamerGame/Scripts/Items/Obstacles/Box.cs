using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Item
{
    public override void NearBlastBehaviour(ItemTracker tracker)
    {
        if (IsMarked(tracker))
        {
            return;
        }
        base.NearBlastBehaviour(tracker);
        
        GetDamage();
    }

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
