using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    public List<GoalEntityUI> goalEntityUis;
    public GameObject goalLayout;
    public TextMeshProUGUI moveTextMesh;
    
    public void SetMoveCount(int value)
    {
        moveTextMesh.text = value.ToString();
    }

    public void DecrementMoveCount()
    {
        int currentAmount = int.Parse(moveTextMesh.text);
        moveTextMesh.text = (currentAmount - 1).ToString();
    }
}
