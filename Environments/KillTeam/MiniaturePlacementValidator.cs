
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(CapsuleCollider))]
public class MiniaturePlacementValidator : MonoBehaviour
{
    [Header("Base")]
    public int edgeSamples = 16;

    [Header("Surface")]
    public LayerMask placementLayers = ~0;
    public float rayStartHeight = 2f;
    public float rayDistance = 5f;
    public float heightTolerance = 10;

    [Header("Visual")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    public Color currentColor = Color.red;

    private Grabbable grab;
    private CapsuleCollider capsule;
    public Color originalColor;

    private bool isGrabbed;
    public bool canPlace;
    private Vector3 placementPosition;

    private void Awake()
    {
        grab = GetComponent<Grabbable>();
        capsule = GetComponent<CapsuleCollider>();

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

    public bool IsValidPlacement()
    {
        return canPlace;
    }

    public Color GetColour()
    {
        return currentColor;
    }

    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        isGrabbed = false;

        Vector3 rotation = transform.eulerAngles;
        transform.rotation =
            Quaternion.Euler(0f, rotation.y, 0f);

        currentColor =  originalColor;
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
            return;

        canPlace = CheckPlacement(
            transform.position,
            out placementPosition);


            currentColor =
                canPlace ? validColor : invalidColor;
    }

    public bool CheckPlacement(
        Vector3 position,
        out Vector3 validPosition)
    {
        validPosition = position;

        // CapsuleCollider radius in world space.
        // Assumes the capsule is vertical (Direction Y).
        Vector3 scale = capsule.transform.lossyScale;

        float radius = capsule.radius *
            Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));

        // Account for the collider's centre offset.
        Vector3 centerOffset =
            capsule.transform.TransformVector(capsule.center);

        Vector3 centerPosition = position + centerOffset;

        // Check centre.
        bool centerValid = CastDown(
            centerPosition, out RaycastHit centerHit);

#if UNITY_EDITOR
        DrawCheckLine(centerPosition, centerValid);
#endif

        if (!centerValid)
            return false;

        Collider targetCollider = centerHit.collider;
        float surfaceY = centerHit.point.y;

        // Check edges around the capsule width.
        int samples = Mathf.Max(3, edgeSamples);

        for (int i = 0; i < samples; i++)
        {
            float angle = i * Mathf.PI * 2f / samples;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius);

            Vector3 samplePosition = centerPosition + offset;

            bool valid =
                CastDown(samplePosition, out RaycastHit edgeHit) &&
                edgeHit.collider == targetCollider &&
                Mathf.Abs(edgeHit.point.y - surfaceY) <= heightTolerance;

#if UNITY_EDITOR
            DrawCheckLine(samplePosition, valid);
#endif

            if (!valid)
                return false;
        }

        // Preserve the original pivot-to-surface offset.
        validPosition.y = surfaceY - centerOffset.y;

        return true;
    }

#if UNITY_EDITOR
    private void DrawCheckLine(Vector3 position, bool valid)
    {
        Debug.DrawLine(
            position + Vector3.up * 0.1f,
            position + Vector3.down * 0.5f,
            valid ? Color.green : Color.red);
    }
#endif

    private bool CastDown(
        Vector3 position,
        out RaycastHit hit)
    {
        Vector3 origin =
            position + Vector3.up * rayStartHeight;

        return Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            rayDistance,
            placementLayers,
            QueryTriggerInteraction.Ignore);
    }
}
