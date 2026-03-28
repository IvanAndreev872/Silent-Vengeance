using UnityEngine;

public class VinePuzzle : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private Vine[] vines;
    [SerializeField] private int[] correctOrder;

    [Header("Chest")]
    [SerializeField] private SpriteRenderer chestRenderer;
    [SerializeField] private Sprite closedChestSprite;
    [SerializeField] private Sprite openedChestSprite;

    [Header("Key Drop")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform keySpawnPoint;

    private int[] playerOrder;
    private int currentStep = 0;
    private bool isSolved = false;

    private void Awake()
    {
        playerOrder = new int[correctOrder.Length];

        if (chestRenderer != null && closedChestSprite != null)
            chestRenderer.sprite = closedChestSprite;
    }

    public void OnVinePulled(int index)
    {
        if (isSolved) return;

        if (currentStep >= playerOrder.Length) return;

        playerOrder[currentStep] = index;
        currentStep++;

        Debug.Log(name + " -> OnVinePulled: " + index);

        if (currentStep >= correctOrder.Length)
            CheckOrder();
    }

    private void CheckOrder()
    {
        for (int i = 0; i < correctOrder.Length; i++)
        {
            if (playerOrder[i] != correctOrder[i])
            {
                Debug.Log(name + " -> wrong order");
                Invoke(nameof(ResetAll), 0.5f);
                return;
            }
        }

        Solve();
    }

    private void Solve()
    {
        isSolved = true;
        Debug.Log(name + " -> solved");

        if (chestRenderer != null && openedChestSprite != null)
            chestRenderer.sprite = openedChestSprite;

        SpawnKey();
    }

    private void SpawnKey()
    {
        if (keyPrefab == null) return;

        Vector3 spawnPos = keySpawnPoint != null ? keySpawnPoint.position : transform.position;

        GameObject key = Instantiate(keyPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = key.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), 2f);
    }

    private void ResetAll()
    {
        currentStep = 0;

        for (int i = 0; i < vines.Length; i++)
        {
            if (vines[i] != null)
                vines[i].ResetVine();
        }

        Debug.Log(name + " -> reset");
    }
}