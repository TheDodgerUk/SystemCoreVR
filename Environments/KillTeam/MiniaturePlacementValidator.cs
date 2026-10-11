
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
public class MiniaturePlacementValidator : MonoBehaviour
{
    [Header("Base")]
    public float baseDiameter = 0.5f;
    public int edgeSamples = 16;

    [Header("Surface")]
    public LayerMask placementLayers = ~0;
    public float rayStartHeight = 2f;
    public float rayDistance = 5f;
    public float heightTolerance = 0.01f;

    [Header("Visual")]
    public Renderer baseRenderer;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private Grabbable grab;
    private Material baseMaterial;
    private Color originalColor;

    private bool isGrabbed;
    public bool canPlace;
    private Vector3 placementPosition;

    private void Awake()
    {
        grab = GetComponent<Grabbable>();

        if (baseRenderer != null)
        {
            baseMaterial = baseRenderer.material;
            originalColor = baseMaterial.color;
        }
    }

    private void OnEnable()
    {
        grab.OnGrabEvent += OnGrab;
        grab.OnReleaseEvent += OnRelease;
    }

    private void OnDisable()
    {
        grab.OnGrabEvent -= OnGrab;
        grab.OnReleaseEvent -= OnRelease;
    }

    private void OnGrab(Hand hand, Grabbable grabbable)
    {
        isGrabbed = true;
    }

    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        isGrabbed = false;

        if (canPlace)
        {
            transform.position = placementPosition;

            Vector3 rotation = transform.eulerAngles;
            transform.rotation =
                Quaternion.Euler(0f, rotation.y, 0f);
        }

        if (baseMaterial != null)
            baseMaterial.color = originalColor;
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
            return;

        canPlace = CheckPlacement(
            transform.position,
            out placementPosition);

        if (baseMaterial != null)
        {
            baseMaterial.color =
                canPlace ? validColor : invalidColor;
        }
    }

    public bool CheckPlacement(
        Vector3 position,
        out Vector3 validPosition)
    {
        validPosition = position;

        float radius = baseDiameter * 0.5f;

        // Find the central supporting collider.
        if (!CastDown(position, out RaycastHit centerHit))
            return false;

        Collider targetCollider = centerHit.collider;
        float surfaceY = centerHit.point.y;

        // Check points around the circular base.
        for (int i = 0; i < edgeSamples; i++)
        {
            float angle =
                i * Mathf.PI * 2f / edgeSamples;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius);

            Vector3 samplePosition = position + offset;

            if (!CastDown(
                samplePosition,
                out RaycastHit edgeHit))
                return false;

            // Every sample must hit the SAME collider.
            if (edgeHit.collider != targetCollider)
                return false;

            // All points must be on the same height.
            if (Mathf.Abs(
                edgeHit.point.y - surfaceY) > heightTolerance)
                return false;
        }

        // Assumes miniature pivot is at bottom of its base.
        validPosition.y = surfaceY;

        return true;
    }

    private bool CastDown(
        Vector3 position,
        out RaycastHit hit)
    {
        Vector3 origin = position +
            Vector3.up * rayStartHeight;

        return Physics.Raycast(
            origin,
            out hit,
            rayDistance,
            placementLayers,
            QueryTriggerInteraction.Ignore);
    }
}
