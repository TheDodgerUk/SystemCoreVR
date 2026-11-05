
using UnityEngine;
using Autohand;

public class MiniatureVerticalLine : MonoBehaviour
{
    [Header("Line Settings")]
    public float lineWidth = 0.005f;
    public float maxDistance = 5f;

    [Header("Base")]
    public int edgeSamples = 16;

    [Header("Placement Layers")]
    [SerializeField]
    protected LayerMask placementLayers =
        (1 << (int)KillTeamGameManager.LayerType.TableBoard) |
        (1 << (int)KillTeamGameManager.LayerType.Terrain);

    public float rayStartHeight = 2f;
    public float rayDistance = 5f;
    public float heightTolerance = 10f;

    [Header("Visual")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public Color originalColor = Color.white;
    public Color currentColor = Color.red;

    [Header("Placement")]
    public bool canPlace;

    private LineRenderer line;
    private Grabbable grabbable;
    private CapsuleCollider capsule;

    [SerializeField]
    private bool isGrabbed;
    [SerializeField]
    private Vector3 placementPosition;

    public Vector3 PlacementPosition => placementPosition;


    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        capsule = this.GetComponentInChildren<CapsuleCollider>();

        GameObject child = new GameObject(GetType().Name);
        child.transform.SetParent(transform, false);

        line = child.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.material = new Material(
            Shader.Find("Universal Render Pipeline/Unlit"));

        line.material.SetColor("_BaseColor", validColor);

        line.startColor = validColor;
        line.endColor = validColor;
        line.enabled = false;
    }

    private void OnEnable()
    {
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        grabbable.OnGrabEvent += OnGrab;
        grabbable.OnReleaseEvent += OnRelease;
        line.enabled = true;
    }

    private void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.OnGrabEvent -= OnGrab;
            grabbable.OnReleaseEvent -= OnRelease;
        }

        isGrabbed = false;

        if (line != null)
            line.enabled = false;
    }

    private void OnDestroy()
    {
        if (KillTeamGameManager.Instance != null)
        {
        }

        if (line != null && line.material != null)
            Destroy(line.material);
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

        Vector3 rotation = transform.eulerAngles;

        transform.rotation =
            Quaternion.Euler(0f, rotation.y, 0f);

        canPlace = CheckPlacement(
            transform.position,
            out placementPosition);

        currentColor = originalColor;
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
            return;

        // Check the miniature placement.
        canPlace = CheckPlacement(
            transform.position,
            out placementPosition);

        currentColor = canPlace
            ? validColor
            : invalidColor;

        // Update line appearance.
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.startColor = currentColor;
        line.endColor = currentColor;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * maxDistance;

        // Find the surface below the miniature.
        if (Physics.Raycast(
            start,
            Vector3.down,
            out RaycastHit hit,
            maxDistance,
            placementLayers,
            QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    public bool IsValidPlacement()
    {
        return canPlace;
    }

    public Color GetColour()
    {
        return currentColor;
    }

  
    public bool CheckPlacement(
        Vector3 position,
        out Vector3 validPosition)
    {
        validPosition = position;

        if (capsule == null)
            return false;

        // CapsuleCollider radius in world space.
        // Assumes a vertical capsule (Direction Y).
        Vector3 scale = capsule.transform.lossyScale;

        float radius = capsule.radius *
            Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.z));

        // Account for the collider centre offset.
        Vector3 centerOffset =
            capsule.transform.TransformVector(capsule.center);

        Vector3 centerPosition =
            position + centerOffset;

        // Check the centre of the miniature.
        bool centerValid = CastDown(
            centerPosition,
            out RaycastHit centerHit);

#if UNITY_EDITOR
        DrawCheckLine(centerPosition, centerValid);
#endif

        if (!centerValid)
            return false;

        Collider targetCollider = centerHit.collider;
        float surfaceY = centerHit.point.y;

        // Check the edges around the capsule.
        int samples = Mathf.Max(3, edgeSamples);

        for (int i = 0; i < samples; i++)
        {
            float angle =
                i * Mathf.PI * 2f / samples;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius);

            Vector3 samplePosition =
                centerPosition + offset;

            bool valid =
                CastDown(
                    samplePosition,
                    out RaycastHit edgeHit) &&
                edgeHit.collider == targetCollider &&
                Mathf.Abs(edgeHit.point.y - surfaceY)
                    <= heightTolerance;

#if UNITY_EDITOR
            DrawCheckLine(samplePosition, valid);
#endif

            if (!valid)
                return false;
        }

        // Preserve the pivot-to-surface offset.
        validPosition.y =
            surfaceY - centerOffset.y;

        return true;
    }

    private bool CastDown(
        Vector3 position,
        out RaycastHit hit)
    {
        hit = default(RaycastHit);

        Vector3 origin =
            position + Vector3.up * rayStartHeight;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            rayDistance,
            placementLayers,
            QueryTriggerInteraction.Ignore);

        float closestDistance = float.MaxValue;
        bool hasHit = false;

        foreach (RaycastHit h in hits)
        {
            if (h.collider == null)
                continue;

            // Ignore the miniature's own colliders.
            if (h.collider.transform == transform ||
                h.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            // Check whether the layer is included
            // in the placement layer mask.
            int layerBit =
                1 << h.collider.gameObject.layer;

            if ((placementLayers.value & layerBit) == 0)
                continue;

            // Find the closest valid surface.
            if (h.distance < closestDistance)
            {
                closestDistance = h.distance;
                hit = h;
                hasHit = true;
            }
        }

#if UNITY_EDITOR
        Debug.DrawRay(
            origin,
            Vector3.down *
                (hasHit ? hit.distance : rayDistance),
            hasHit ? Color.green : Color.red);
#endif

        return hasHit;
    }

#if UNITY_EDITOR
    private void DrawCheckLine(
        Vector3 position,
        bool valid)
    {
        Debug.DrawLine(
            position + Vector3.up * 0.1f,
            position + Vector3.down * 0.5f,
            valid ? Color.green : Color.red);
    }
#endif
}
