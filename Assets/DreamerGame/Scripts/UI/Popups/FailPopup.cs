using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FailPopup : Popup
{
    [SerializeField] private GameObject playAgainButton;
    public override void Show()
    {
        transform.localScale = new Vector3(0, 0, 0);
        base.Show();
        StartCoroutine(AnimationManager.FailPopupAnimation(transform));
    }

    public override void OnCloseButtonPressed()
    {
        GameManager.Instance.LoadMenuScene();
    }

    public void OnPlayAgainButtonPressed()
    {
        playAgainButton.transform.DOPunchRotation(new Vector3(0, 0, 30), 0.5f, 10, 0.5f).OnComplete(() =>
        {
            GameManager.Instance.LoadGameScene();
        });

    }
    
    
}
