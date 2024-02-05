using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : Item
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

    protected override void GetDamage()
    {
        lives -= 1;
        activeState += 1;
        if (lives == 0)
        {
            base.GetDamage();
            lives = 2;
        }
        else
        {
            spriteRenderer.sprite = itemSprite.sprites[activeState].sprite;
        }
    }
}
