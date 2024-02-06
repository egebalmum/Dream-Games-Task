using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : Item
{
    [Header("Type Specific Attributes")] [SerializeField]
    private int lives = 2;
    public override void ExplosionBehavior(ItemTracker tracker)
    {
        if (IsMarked(tracker))
        {
            return;
        }
        base.ExplosionBehavior(tracker);
        if (lives == 1)
        {
            GetDamage();
        }
        else
        {
            StartCoroutine(DamageEffect(tracker));
        }
    }

    public override void NearBlastBehaviour(ItemTracker tracker)
    {
        if (IsMarked(tracker))
        {
            return;
        }
        base.NearBlastBehaviour(tracker);
        
        if (lives == 1)
        {
            GetDamage();
        }
        else
        {
            StartCoroutine(DamageEffect(tracker));
        }
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

    private IEnumerator DamageEffect(ItemTracker tracker)
    {
        tracker.coroutineCount += 1;
        GetDamage();
        yield return AnimationManager.DamagedAnimation(transform, 0.5f);
        yield return new WaitForEndOfFrame();
        tracker.coroutineCount -= 1;
    }
}
