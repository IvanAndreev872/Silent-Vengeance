using UnityEngine;
using TMPro;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    [Header("Keys")]
    [SerializeField] private int currentKeys = 0;
    [SerializeField] private int totalKeys = 5;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI keyText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        UpdateUI();
    }

    public void AddKey(int amount = 1)
    {
        currentKeys += amount;

        if (currentKeys > totalKeys)
            currentKeys = totalKeys;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (keyText != null)
            keyText.text = currentKeys + "/" + totalKeys;
    }

    public int GetCurrentKeys() => currentKeys;
    public int GetTotalKeys() => totalKeys;
}