using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cube : Item
{
    [Header("Type Specific Attributes")]
    [SerializeField] private int tntBonusRule;
    public override void TouchBehaviour(ItemTracker tracker)
    {
        var items = Board.Instance.CheckMatches(pos.x, pos.y);
        if (items.Count < minMatchCount)
        {
            return;
        }

        if (items.Count >= tntBonusRule) //BONUS BEHAVIOUR
        {
            StartCoroutine(BonusBehaviour(items, tracker));
        }
        else//NORMAL BEHAVIOUR
        {
            foreach (var item in items)
            {
                item.BlastBehaviour(tracker);
            }
        }
    }

    public override void BlastBehaviour(ItemTracker tracker)
    {
        if (IsMarked(tracker))
        {
            return;
        }
        base.BlastBehaviour(tracker);
        
        GetDamage();
        var aroundItems = Board.Instance.AroundItems(pos);
        foreach (var aroundItem in aroundItems)
        {
            if (aroundItem.type == type && aroundItem.color == color)
            {
                continue;
            }
            aroundItem.NearBlastBehaviour(tracker);
        }
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
    
    private IEnumerator BonusBehaviour(List<Item> matchedItems, ItemTracker tracker)
    {
        tracker.coroutineCount += 1;

        foreach (var matchedItem in matchedItems)
        {
            var aroundItems = Board.Instance.AroundItems(matchedItem.pos);
            foreach (var aroundItem in aroundItems)
            {
                if (aroundItem.type == type && aroundItem.color == color)
                {
                    continue;
                }
                aroundItem.NearBlastBehaviour(tracker);
            }
        }
        yield return AnimationManager.BonusMatchAnimation(matchedItems, transform.position, 0.5f);

        foreach (var matchedItem in matchedItems)
        {
            matchedItem.BlastBehaviour(tracker);
        }

        Board.Instance.CreateNewItem(ItemType.TNT, pos);

        yield return new WaitForEndOfFrame();
        tracker.coroutineCount -= 1;
    }
    
    public override void InitializeItemInBoard(Vector2Int initialPos)
    {
        base.InitializeItemInBoard(initialPos);
        OnMatchCountUpdated.AddListener(UpdateSprite);
    }

    private void UpdateSprite()
    {
        if (matchCount >= tntBonusRule)
        {
            spriteRenderer.sprite = itemSprite.specialSprites.First(container => container.name.Equals("BombHint")).sprite;
        }
        else
        {
            spriteRenderer.sprite = itemSprite.sprites[activeState].sprite;
        }
    }
}
