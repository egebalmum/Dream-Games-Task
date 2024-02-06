using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationManager
{
    public static IEnumerator WinPopupAnimation(Transform transformStar, Transform transformText, ParticleSystem particle)
    {
        var tween = transformStar.DOMove(Vector3.zero, 0.85f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            transformText.DOScale(new Vector3(1, 1, 1), 0.25f).SetEase(Ease.OutCubic);
            particle.Play();
        });
        yield return tween;
    }

    public static IEnumerator FailPopupAnimation(Transform transform)
    {
        var tween = transform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.OutCirc);
        yield return tween;
    }
   
    
    public static IEnumerator BonusMatchAnimation(List<Item> items, Vector2 targetPosition, float duration)
    {
        foreach (var item in items)
        {
            item.spriteRenderer.sortingOrder = 100;
            item.transform.DOScale(1.25f, duration / 3).OnComplete(() =>
            {
                item.transform.DOMove(targetPosition, duration / 3).OnComplete(() =>
                {
                    item.transform.DOScale(Vector3.one, duration / 3).OnComplete(() =>
                    {
                        
                    });
                });
            });
        }
        yield return new WaitForSeconds(duration);
    }

    public static IEnumerator TNTAnimation(Item tntItem, float duration)
    {
        tntItem.spriteRenderer.sortingOrder = 100;
        tntItem.transform.DOScale(1.25f, duration / 2).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            tntItem.transform.DOScale(Vector3.zero, duration / 2).SetEase(Ease.InQuad);
        });
        yield return new WaitForSeconds(duration);
        Camera.main.DOShakePosition(0.25f, 1.5f, 5, 30);
    }

    public static IEnumerator TNTComboAnimation(List<Item> items, Item touchedItem,float duration)
    {
        touchedItem.spriteRenderer.sortingOrder = 200;
        foreach (var item in items)
        {
            item.spriteRenderer.sortingOrder = 100;
            item.transform.DOMove(touchedItem.transform.position, duration / 2).SetEase(Ease.OutQuad);
        }
        yield return new WaitForSeconds(duration/2);
        touchedItem.transform.DOScale(new Vector3(2, 2, 2), duration/2).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(duration / 2);
        Camera.main.DOShakePosition(0.25f, 1.5f, 5, 30);

    }

    public static IEnumerator UntouchableAnimation(Transform itemTransform, float duration)
    {
        itemTransform.DOKill();
        itemTransform.eulerAngles = Vector3.zero;
        itemTransform.DOPunchRotation(new Vector3(0,0,20), duration, 25, 0.5f);
        yield break;
    }
    
    public static IEnumerator DamagedAnimation(Transform itemTransform, float duration)
    {
        itemTransform.DOKill();
        itemTransform.eulerAngles = Vector3.zero;
        itemTransform.DOPunchRotation(new Vector3(0,0,20), duration, 10, 0.5f);
        yield break;
    }
    
}
