using DG.Tweening;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject playButtonObject;
    [SerializeField] private GameObject taskParent;
    private GameObject[] _taskObjects;
    private bool _nextLevelExist;
    private bool _upgraded;
    private Button playButton;
    private void Start()
    {
        _upgraded = GameManager.Instance.upgrade == 1;
        playButton = playButtonObject.GetComponent<Button>();
        playButton.interactable = !_upgraded;
        Initialize();
        CheckNextLevel();
        SetTasks();
    }

    private void Initialize()
    {
        int childCount = taskParent.transform.childCount;
        _taskObjects = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            var obj = taskParent.transform.GetChild(i);
            obj.gameObject.SetActive(false);
            _taskObjects[i] = obj.gameObject;
        }
    }

    private void CheckNextLevel()
    {
        var levelReader = new LevelReader();
        _nextLevelExist = levelReader.Exist($"level_" + GameManager.Instance.persistLevel.ToString("D2"));
    }
    private void SetTasks()
    {
        if (!_upgraded)
        {
            SetButtonText();
        }
        int wonLevelCount = GameManager.Instance.persistLevel-1;
        for (int i = 0; i < wonLevelCount; i++)
        {
            if (i == wonLevelCount - 1)
            {
                if (_upgraded)
                {
                    GameManager.Instance.UpgradeExecuted();
                    _taskObjects[i].transform.localScale = new Vector3(0, 0, 1);
                    _taskObjects[i].SetActive(true);
                    _taskObjects[i].transform.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutElastic).OnComplete(
                        () =>
                        {
                            playButton.transform.DOPunchRotation(new Vector3(0, 0, 30), 0.5f, 10, 0.5f).OnComplete(() =>
                            {
                                playButton.interactable = true;
                            });
                            SetButtonText();
                        });
                }
                else
                {
                    _taskObjects[i].SetActive(true);
                }
            }
            _taskObjects[i].SetActive(true);
        }
    }
    
    private void SetButtonText()
    {
        if (!_nextLevelExist)
        {
            playButton.GetComponentInChildren<TextMeshProUGUI>().text = "Finished";
            return;
        }
        playButton.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + GameManager.Instance.persistLevel;
    }
    public void LoadGameScene()
    {
        playButton.interactable = false;
        playButton.transform.DOPunchRotation(new Vector3(0, 0, 30), 0.5f, 10, 0.5f).OnComplete(() =>
        {
            if (!_nextLevelExist)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                GameManager.Instance.persistLevel = 1;
                GameManager.Instance.LoadMenuScene();
                return;
            }
            GameManager.Instance.LoadGameScene();
        });
    }
}
