using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button closeSettingsButton;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(LoadMainMenu);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);

        ShowGameplayState();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void ShowGameplayState()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}