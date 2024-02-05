using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailPopup : Popup
{
    public override void OnCloseButtonPressed()
    {
        base.OnCloseButtonPressed();
        GameManager.Instance.LoadMenuScene();
    }

    public void OnPlayAgainButtonPressed()
    {
        GameManager.Instance.LoadGameScene();
    }
    
    
}
