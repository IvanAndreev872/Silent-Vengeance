using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Панели")]
    public GameObject menuPanel;
    public GameObject settingsPanel;

    [Header("Кнопки")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button closeSettingsButton; // кнопка "Закрыть" в SettingsPanel

    [Header("Сцена")]
    public string gameSceneName = "SampleScene";

    void Start()
    {
        startButton.onClick.AddListener(OnStart);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(OnExit);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // Начальное состояние
        ShowMenu();
    }

    void ShowMenu()
    {
        menuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void OpenSettings()
    {
        menuPanel.SetActive(false);   // скрыть меню
        settingsPanel.SetActive(true); // показать настройки
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false); // скрыть настройки
        menuPanel.SetActive(true);      // вернуть меню
    }

    void OnStart()
    {
        SceneManager.LoadScene(gameSceneName);
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