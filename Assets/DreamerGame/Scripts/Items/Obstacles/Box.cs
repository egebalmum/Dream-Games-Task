using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Obstacle
{
    public override void NearBlastBehaviour()
    {
        DamageBehaviour();
    }

    public override void ExplosionBehavior()
    {
        DamageBehaviour();
    }
}
