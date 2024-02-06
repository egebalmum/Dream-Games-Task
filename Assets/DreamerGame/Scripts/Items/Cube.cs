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
        
        foreach (var item in items)
        {
            item.BlastBehaviour(tracker);
        }

        if (items.Count >= tntBonusRule)
        {
            Board.Instance.CreateNewItem(ItemType.TNT, pos);
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
