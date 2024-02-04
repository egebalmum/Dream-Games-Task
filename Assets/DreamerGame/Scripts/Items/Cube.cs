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
        base.TouchBehaviour(markedItems);
        
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
        base.BlastBehaviour(markedItems);
        GetDamage();
        var aroundItems = Board.Instance.AroundItems(pos);
        foreach (var aroundItem in aroundItems)
        {
            aroundItem.NearBlastBehaviour(markedItems);
        }
    }

    public override void ExplosionBehavior(HashSet<Item> markedItems)
    {
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
            spriteRenderer.sprite = specialSpriteContainers.First(state => state.name.Equals("BombHint")).sprite;
        }
        else
        {
            spriteRenderer.sprite = spriteContainers[0].sprite;
        }
    }
}
