using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Панели")]
    public GameObject menuPanel;
    public GameObject settingsPanel;
    public GameObject levelSelectPanel;

    [Header("Кнопки")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button closeSettingsButton;
    public Button level1Button;
    public Button level2Button;
    public Button backFromLevelsButton;

    [Header("Сцены")]
    public string level1SceneName = "Level1";
    public string level2SceneName = "Level2";

    void Start()
    {
        startButton.onClick.AddListener(OpenLevelSelect);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(OnExit);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        if (level1Button != null)
            level1Button.onClick.AddListener(LoadLevel1);

        if (level2Button != null)
            level2Button.onClick.AddListener(LoadLevel2);

        if (backFromLevelsButton != null)
            backFromLevelsButton.onClick.AddListener(BackToMenu);

        ShowMenu();
    }

    void ShowMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    void OpenSettings()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    void OpenLevelSelect()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    void BackToMenu()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    void LoadLevel1()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    void LoadLevel2()
    {
        SceneManager.LoadScene(level2SceneName);
    }

    void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}