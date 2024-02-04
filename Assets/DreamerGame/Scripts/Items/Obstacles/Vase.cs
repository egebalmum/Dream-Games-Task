using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : Obstacle
{
    [Header("Type Specific Attributes")] [SerializeField]
    private int lives = 2;
    public override void ExplosionBehavior()
    {
        DamageBehaviour();
    }

    public override void NearBlastBehaviour()
    {
        DamageBehaviour();
    }

    public override void DamageBehaviour()
    {
        lives -= 1;
        if (lives == 0)
        {
            base.DamageBehaviour();
            lives = 2;
        }
        else
        {
            spriteRenderer.sprite = spriteContainers[spriteContainers.Length-lives].sprite;
        }
    }
}
