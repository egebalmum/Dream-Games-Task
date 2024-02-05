using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class GoalEntityUI : MonoBehaviour
{
    public (ItemType type, ColorType color) goalIdentity;
    public Image image;
    public Image checkImage;
    public TextMeshProUGUI textMesh;
    
    public void SetGoalEntityUI((ItemType, ColorType) identity, Sprite sprite, int amount)
    {
        goalIdentity = identity;
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
