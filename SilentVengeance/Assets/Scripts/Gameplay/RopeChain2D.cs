using UnityEngine;

public class RopeChain2D : MonoBehaviour
{
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private int segmentCount = 10;
    [SerializeField] private float segmentSpacing = 0.2f;

    private Rigidbody2D _topBody;
    private GameObject _lastSegment;

    private void Awake()
    {
        _topBody = GetComponent<Rigidbody2D>();
        if (_topBody == null)
        {
            _topBody = gameObject.AddComponent<Rigidbody2D>();
            _topBody.bodyType = RigidbodyType2D.Static; // точка крепления
        }

        GenerateRope();
    }

    private void GenerateRope()
    {
        Rigidbody2D prevBody = _topBody;
        Vector3 pos = transform.position;

        for (int i = 0; i < segmentCount; i++)
        {
            pos.y -= segmentSpacing;

            GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);
            Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
            HingeJoint2D joint = seg.GetComponent<HingeJoint2D>();

            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = prevBody;

            joint.anchor = new Vector2(0f, segmentSpacing * 0.5f);
            joint.connectedAnchor = new Vector2(0f, -segmentSpacing * 0.5f);

            prevBody = rb;
            _lastSegment = seg;
        }
    }

    public GameObject LastSegment => _lastSegment;
}