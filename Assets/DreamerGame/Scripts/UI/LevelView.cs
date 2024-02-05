using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    public GameObject goalLayout;
    public TextMeshProUGUI moveTestMesh;
    
    public void SetMoveCount(int value)
    {
        moveTestMesh.text = value.ToString();
    }

    public void DecrementMoveCount()
    {
        int currentAmount = int.Parse(moveTestMesh.text);
        moveTestMesh.text = (currentAmount - 1).ToString();
    }
}
