using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private int keyAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (KeyManager.Instance != null)
            KeyManager.Instance.AddKey(keyAmount);

        Destroy(gameObject);
    }
}