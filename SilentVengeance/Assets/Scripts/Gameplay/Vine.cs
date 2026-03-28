using UnityEngine;
using UnityEngine.InputSystem;

public class Vine : MonoBehaviour
{
    [SerializeField] private int vineIndex;
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private VinePuzzle puzzle;

    private Vector3 startPosition;
    private bool isDropped = false;
    private bool playerNearby = false;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (playerNearby && !isDropped && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Drop();
    }

    private void Drop()
    {
        isDropped = true;
        transform.position = startPosition + Vector3.down * dropDistance;

        if (sr != null)
            sr.color = Color.green;

        Debug.Log(name + " -> dropped, index = " + vineIndex);

        if (puzzle != null)
            puzzle.OnVinePulled(vineIndex);
        else
            Debug.LogWarning(name + " -> puzzle is NULL", this);
    }

    public void ResetVine()
    {
        isDropped = false;
        transform.position = startPosition;

        if (sr != null)
            sr.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerNearby = false;
    }
}