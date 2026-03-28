using UnityEngine;

public class VinePuzzle : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private Vine[] vines;
    [SerializeField] private int[] correctOrder; // например: {2, 0, 1}

    [Header("Chest")]
    [SerializeField] private SpriteRenderer chestRenderer;
    [SerializeField] private Sprite closedChestSprite;
    [SerializeField] private Sprite openedChestSprite;

    [Header("Key Drop")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform keySpawnPoint;

    private int[] _playerOrder;
    private int _currentStep = 0;
    private bool _isSolved = false;

    private void Awake()
    {
        _playerOrder = new int[correctOrder.Length];

        if (chestRenderer != null && closedChestSprite != null)
            chestRenderer.sprite = closedChestSprite;
    }

    public void OnVinePulled(int index)
    {
        if (_isSolved) return;

        _playerOrder[_currentStep] = index;
        _currentStep++;

        if (_currentStep >= correctOrder.Length)
            CheckOrder();
    }

    private void CheckOrder()
    {
        for (int i = 0; i < correctOrder.Length; i++)
        {
            if (_playerOrder[i] != correctOrder[i])
            {
                Invoke(nameof(ResetAll), 0.5f);
                return;
            }
        }

        Solve();
    }

    private void Solve()
    {
        _isSolved = true;
        Debug.Log("Головоломка решена!");

        if (chestRenderer != null && openedChestSprite != null)
            chestRenderer.sprite = openedChestSprite;
        
        SpawnKey();
    }

    private void SpawnKey()
    {
        if (keyPrefab == null) return;

        Vector3 spawnPos = keySpawnPoint != null
            ? keySpawnPoint.position
            : transform.position;

        GameObject key = Instantiate(keyPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = key.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), 2f);
        }
    }

    private void ResetAll()
    {
        _currentStep = 0;

        foreach (var vine in vines)
            vine.ResetVine();
    }
}