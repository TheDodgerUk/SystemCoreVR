
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
public class MiniaturePlacementValidator : MonoBehaviour
{
    [Header("Base")]
    public float baseDiameter = 0.032f;
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
    public Color originalColor;

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

    public bool IsValidPlacement()
    {
        return canPlace;
    }
    public Color GetColour()
    {
        return originalColor;
    }
    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        isGrabbed = false;

            Vector3 rotation = transform.eulerAngles;
            transform.rotation =
                Quaternion.Euler(0f, rotation.y, 0f);
 

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

        // Check centre
        bool centerValid = CastDown(position, out RaycastHit centerHit);

#if UNITY_EDITOR
        DrawCheckLine(position, centerValid);
#endif

        if (!centerValid)
            return false;

        Collider targetCollider = centerHit.collider;
        float surfaceY = centerHit.point.y;

        // Check edges
        for (int i = 0; i < edgeSamples; i++)
        {
            float angle = i * Mathf.PI * 2f / edgeSamples;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius);

            Vector3 samplePosition = position + offset;

            bool valid = CastDown(samplePosition, out RaycastHit edgeHit) &&
                         edgeHit.collider == targetCollider &&
                         Mathf.Abs(edgeHit.point.y - surfaceY) <= heightTolerance;

#if UNITY_EDITOR
            DrawCheckLine(samplePosition, valid);
#endif

            if (!valid)
                return false;
        }

        validPosition.y = surfaceY;
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
        Vector3 origin = position +
            Vector3.up * rayStartHeight;

        return Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            rayDistance,
            placementLayers,
            QueryTriggerInteraction.Ignore);


    }
}
