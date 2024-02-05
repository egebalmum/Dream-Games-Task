using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject[] tasks;

    private void Start()
    {
        playButton.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + GameManager.Instance.persistLevel;

        for (int i = 0; i < tasks.Length; i++)
        {
            if (GameManager.Instance.persistLevel - 1 > i)
            {
                tasks[i].SetActive(true);
            }
            else
            {
                tasks[i].SetActive(false);
            }
        }
    }
    public void LoadGameScene()
    {
        GameManager.Instance.LoadGameScene();
    }
}
