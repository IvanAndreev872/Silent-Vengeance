using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Optional visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openedSprite;

    private InputAction _interactAction;
    private bool _playerNearby;
    private bool _isOpened;

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Player/Interact");
    }

    private void Update()
    {
        if (_isOpened || !_playerNearby)
            return;

        if (_interactAction != null && _interactAction.WasPressedThisFrame())
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        _isOpened = true;

        if (openedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = openedSprite;

        if (keyPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up * 0.5f;
            Instantiate(keyPrefab, pos, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _playerNearby = false;
    }
}