using System.Collections.Generic;
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
    [SerializeField] private float heightAboveGround = 0.5f;

    [ContextMenu("Generate Nodes")]
    public void GenerateNodes()
    {
        // Удаляем старые ноды
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        HashSet<Vector2Int> placedKeys = new HashSet<Vector2Int>();
        int count = 0;

        for (float x = areaMin.x; x <= areaMax.x; x += nodeSpacing)
        {
            for (float y = areaMin.y; y <= areaMax.y; y += nodeSpacing)
            {
                Vector2 scanPos = new Vector2(
                    transform.position.x + x,
                    transform.position.y + y
                );

                RaycastHit2D groundHit = Physics2D.Raycast(
                    scanPos, Vector2.down, groundCheckDistance, groundLayer
                );
                if (groundHit.collider == null) continue;

                Vector2 nodePos = new Vector2(
                    Mathf.Round(scanPos.x * 2f) / 2f,
                    Mathf.Round((groundHit.point.y + heightAboveGround) * 2f) / 2f
                );
                Vector2Int key = new Vector2Int(
                    Mathf.RoundToInt(nodePos.x * 2f),
                    Mathf.RoundToInt(nodePos.y * 2f)
                );

                if (placedKeys.Contains(key)) continue;

                Collider2D blockAtNode = Physics2D.OverlapCircle(
                    nodePos, 0.15f, obstacleLayer | groundLayer
                );
                if (blockAtNode != null) continue;

                placedKeys.Add(key);

                GameObject nodeObj = Instantiate(
                    nodePrefab, nodePos,
                    Quaternion.identity, transform
                );
                nodeObj.name = $"Node_{nodePos.x:F1}_{nodePos.y:F1}";
                count++;
            }
        }

        Debug.Log($"Создано {count} нодов (высота над землёй: {heightAboveGround})");
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
