using UnityEngine;

public class NodePlacer : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Область генерации")]
    [SerializeField] private Vector2 areaMin = new Vector2(-14, -3);
    [SerializeField] private Vector2 areaMax = new Vector2(14, 4);
    [SerializeField] private float nodeSpacing = 1f;

    [Header("Проверка")]
    [SerializeField] private float groundCheckDistance = 1.5f;

    [ContextMenu("Generate Nodes")]
    public void GenerateNodes()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        int count = 0;

        for (float x = areaMin.x; x <= areaMax.x; x += nodeSpacing)
        {
            for (float y = areaMin.y; y <= areaMax.y; y += nodeSpacing)
            {
                Vector2 pos = new Vector2(transform.position.x + x, transform.position.y + y);

                Collider2D obstacle = Physics2D.OverlapCircle(
                    pos, 0.3f, obstacleLayer | groundLayer
                );
                if (obstacle != null) continue;

                RaycastHit2D groundHit = Physics2D.Raycast(
                    pos, Vector2.down, groundCheckDistance, groundLayer
                );

                if (groundHit.collider != null)
                {
                    GameObject nodeObj = Instantiate(
                        nodePrefab, pos,
                        Quaternion.identity, transform
                    );
                    nodeObj.name = $"Node_{x}_{y}";
                    count++;
                }
            }
        }

        Debug.Log($"Создано {count} нодов");
    }

    [ContextMenu("Clear Nodes")]
    public void ClearNodes()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        Debug.Log("Все ноды удалены");
    }
}