using TMPro;
using UnityEngine;

public class WinPopup : Popup
{
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private GameObject starObject;
    [SerializeField] private TextMeshProUGUI text;

    public override void Show()
    {
        base.Show();
        StartCoroutine(AnimationManager.WinPopupAnimation(starObject.transform, text.transform, particle)); 
    }
}
