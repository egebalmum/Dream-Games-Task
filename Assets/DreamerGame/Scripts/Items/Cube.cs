using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cube : Item
{
    [Header("Type Specific Attributes")]
    [SerializeField] private int tntBonusRule;
    public override void TouchBehaviour(HashSet<Item> markedItems)
    {
        var items = Board.Instance.CheckMatches(pos.x, pos.y);
        if (items.Count < minMatchCount)
        {
            return;
        }
        
        foreach (var item in items)
        {
            item.BlastBehaviour(markedItems);
        }

        if (items.Count >= tntBonusRule)
        {
            Board.Instance.CreateNewItem(ItemType.TNT, pos);
        }
    }

    public override void BlastBehaviour(HashSet<Item> markedItems)
    {
        if (IsMarked(markedItems))
        {
            return;
        }
        base.BlastBehaviour(markedItems);
        
        GetDamage();
        var aroundItems = Board.Instance.AroundItems(pos);
        foreach (var aroundItem in aroundItems)
        {
            if (aroundItem.type == type && aroundItem.color == color)
            {
                continue;
            }
            aroundItem.NearBlastBehaviour(markedItems);
        }
    }

    public override void ExplosionBehavior(HashSet<Item> markedItems)
    {
        if (IsMarked(markedItems))
        {
            return;
        }
        base.ExplosionBehavior(markedItems);
        
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
