using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class GoalEntityView : MonoBehaviour
{
    public Image image;
    public Image checkImage;
    public TextMeshProUGUI textMesh;
    
    public void SetGoalEntityUI(Sprite sprite, int amount)
    {
        image.sprite = sprite;
        textMesh.text = amount.ToString();
    }

    public void DecrementAmount()
    {
        int currentAmount = int.Parse(textMesh.text);
        textMesh.text = (currentAmount - 1).ToString();
    }

    public void IncrementAmount()
    {
        int currentAmount = int.Parse(textMesh.text);
        textMesh.text = (currentAmount + 1).ToString();
    }

    public void SetFinish()
    {
        textMesh.gameObject.SetActive(false);
        checkImage.gameObject.SetActive(true);
    }
}
