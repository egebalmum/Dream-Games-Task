using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Item
{
    public override void NearBlastBehaviour()
    {
        DestroyItem();
    }

    public override void ExplosionBehavior()
    {
        DestroyItem();
    }
}
