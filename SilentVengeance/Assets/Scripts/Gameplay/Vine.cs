using UnityEngine;
using UnityEngine.InputSystem;

public class Vine : MonoBehaviour
{
    [SerializeField] public int vineIndex;
    [SerializeField] private float dropDistance = 1f;

    private Vector3 _startPosition;
    private bool _isDropped = false;
    private bool _playerNearby = false;
    private SpriteRenderer _sr;

    private VinePuzzle _puzzle;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
        _puzzle = FindObjectOfType<VinePuzzle>();
    }

    private void Update()
    {
        if (_playerNearby && !_isDropped && Keyboard.current.eKey.wasPressedThisFrame)
            Drop();
    }

    private void Drop()
    {
        _isDropped = true;
        transform.position = _startPosition + Vector3.down * dropDistance;
        _sr.color = Color.green;
        _puzzle.OnVinePulled(vineIndex);
    }

    public void ResetVine()
    {
        _isDropped = false;
        transform.position = _startPosition;
        _sr.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player")) _playerNearby = false;
    }
}