using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : Obstacle
{
    [Header("Type Specific Attributes")] [SerializeField]
    private int lives = 2;
    public override void ExplosionBehavior(HashSet<Item> markedItems)
    {
        if (IsMarked(markedItems))
        {
            return;
        }
        base.ExplosionBehavior(markedItems);
        
        GetDamage();
    }

    public override void NearBlastBehaviour(HashSet<Item> markedItems)
    {
        if (IsMarked(markedItems))
        {
            return;
        }
        base.NearBlastBehaviour(markedItems);
        
        GetDamage();
    }

    public override void GetDamage()
    {
        lives -= 1;
        if (lives == 0)
        {
            base.GetDamage();
            lives = 2;
        }
        else
        {
            spriteRenderer.sprite = spriteContainers[spriteContainers.Length-lives].sprite;
        }
    }
}
