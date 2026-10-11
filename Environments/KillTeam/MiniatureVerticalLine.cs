
using UnityEngine;
using Autohand;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Grabbable))]
public class MiniatureVerticalLine : MonoBehaviour
{
    [Header("Line Settings")]
    public float lineWidth = 0.005f;
    public float maxDistance = 5f;
    public LayerMask groundLayer = ~0;
    public Color lineColor = Color.green;

    private LineRenderer line;
    private Grabbable grabbable;
    private bool isGrabbed;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        grabbable = GetComponent<Grabbable>();

        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.material = new Material(
            Shader.Find("Sprites/Default"));

        line.startColor = lineColor;
        line.endColor = lineColor;
        line.enabled = false;
    }

    private void OnEnable()
    {
        grabbable.OnGrabEvent += OnGrab;
        grabbable.OnReleaseEvent += OnRelease;
    }

    private void OnDisable()
    {
        grabbable.OnGrabEvent -= OnGrab;
        grabbable.OnReleaseEvent -= OnRelease;
    }

    private void OnGrab(Hand hand, Grabbable grab)
    {
        isGrabbed = true;
        line.enabled = true;
    }

    private void OnRelease(Hand hand, Grabbable grab)
    {
        isGrabbed = false;
        line.enabled = false;
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
            return;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * maxDistance;

        if (Physics.Raycast(
            start,
            Vector3.down,
            out RaycastHit hit,
            maxDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
