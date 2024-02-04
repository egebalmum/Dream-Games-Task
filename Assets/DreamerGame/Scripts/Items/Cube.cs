using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cube : Item
{
    [Header("Type Specific Attributes")]
    [SerializeField] private int tntBonusRule;
    public override void TouchBehaviour()
    {
        var items = Board.Instance.CheckMatches(pos.x, pos.y);
        if (items.Count < minMatchCount)
        {
            return;
        }

        List<Item> nearBlastItems = new List<Item>();
        foreach (var item in items)
        {
            item.BlastBehaviour(nearBlastItems);
        }

        foreach (var nearItem in nearBlastItems)
        {
            nearItem.NearBlastBehaviour();
        }

        if (items.Count >= tntBonusRule)
        {
            Board.Instance.CreateNewItem(ItemType.TNT, pos);
        }
    }

    public override void BlastBehaviour(List<Item> items = null)
    {
        DamageBehaviour();
        if (items == null)
        {
            return;
        }
        var aroundItems = Board.Instance.AroundItems(pos);
        foreach (var aroundItem in aroundItems)
        {
            if (!items.Contains(aroundItem))
            {
                items.Add(aroundItem);
            }
        }
    }

    public override void ExplosionBehavior()
    {
        DamageBehaviour();
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
