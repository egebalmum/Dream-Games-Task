using TMPro;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    public GameObject goalLayout;
    public TextMeshProUGUI moveTestMesh;
    public GameObject GoalEntityUIPrefab;
    
    public void SetMoveCount(int value)
    {
        moveTestMesh.text = value.ToString();
    }

    public void DecrementMoveCount()
    {
        int currentAmount = int.Parse(moveTestMesh.text);
        moveTestMesh.text = (currentAmount - 1).ToString();
    }
    
    public GoalEntityView CreateGoalEntityUI((ItemType type, ColorType color) identity)
    {
        var goalEntityUI = Instantiate(GoalEntityUIPrefab, Vector3.zero, Quaternion.identity).GetComponent<GoalEntityView>();
        goalEntityUI.SetGoalEntityUI(ItemFactory.Instance.GetItem(identity.type).GetComponent<Item>().itemSprite.sprites[0].sprite, 0);
        goalEntityUI.transform.SetParent(goalLayout.transform);
        ResetRectTransform(goalEntityUI.GetComponent<RectTransform>());
        return goalEntityUI;
    }
    private void ResetRectTransform(RectTransform rectTransform)
    {
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(100, 100);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
    }
}
