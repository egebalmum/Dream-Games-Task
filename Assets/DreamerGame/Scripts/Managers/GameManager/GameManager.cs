using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [HideInInspector]public GameState state;
    [HideInInspector] public int persistLevel;
    [HideInInspector] public int upgrade;
    [HideInInspector] public GameSettings gameSettings;
    
    private void Awake()
    {
        InitializeSingleton();
        Initialize();
    }


    private void Initialize()
    {
        if (Instance != this)
        {
            return;
        }
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            LoadMenuScene();
            return;
        }
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this);
        gameSettings = Resources.Load<GameSettings>("GameSettings");
        GetPersist();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void GetPersist()
    {
        bool hasKey = PlayerPrefs.HasKey("Level");
        if (!hasKey)
        {
            PlayerPrefs.SetInt("Level", 1);
            PlayerPrefs.SetInt("Upgrade",0);
            PlayerPrefs.Save();
        }
        persistLevel = PlayerPrefs.GetInt("Level");
        upgrade = PlayerPrefs.GetInt("Upgrade");
    }


    public void LoadGameScene()
    {
        state = GameState.InGame;
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadMenuScene()
    {
        state = GameState.InMenu;
        SceneManager.LoadScene("MainScene");
    }

    public void LevelCompleted()
    {
        persistLevel += 1;
        upgrade = 1;
        PlayerPrefs.SetInt("Level", persistLevel);
        PlayerPrefs.SetInt("Upgrade", upgrade);
        PlayerPrefs.Save();
    }

    public void UpgradeExecuted()
    {
        upgrade = 0;
        PlayerPrefs.SetInt("Upgrade", upgrade);
        PlayerPrefs.Save();
    }
    
}
