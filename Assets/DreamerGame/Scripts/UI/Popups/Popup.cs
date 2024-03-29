using UnityEngine;

public class Popup : MonoBehaviour
{
    public GameObject closeButton;
    public bool showCloseButton = true;

    private void Start()
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        if (closeButton != null)
        {
            closeButton.SetActive(showCloseButton);
        }
    }
    
    public virtual void OnCloseButtonPressed()
    {
        Hide();
    }
}
