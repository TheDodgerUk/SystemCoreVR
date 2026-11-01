
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
public class TerrainPlacementValidator : MonoBehaviour
{
    [Header("Base")]
    public int edgeSamples = 16;

    [Header("Surface")]
    public LayerMask placementLayers = (1 << (int)KillTeamGameManager.LayerType.Board);
    public float rayStartHeight = 2f;
    public float rayDistance = 5f;
    public float heightTolerance = 10;

    [Header("Visual")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    public Color currentColor = Color.red;

    private Grabbable grab;
    public Color originalColor;

    private bool isGrabbed;
    public bool canPlace;
    private Vector3 placementPosition;


    public void SetLayerMode(bool setup)
    {
        if (setup)
        {
            placementLayers = (1 << (int)KillTeamGameManager.LayerType.Start);
        }
        else
        {
            placementLayers = (1 << (int)KillTeamGameManager.LayerType.Board) | (1 << (int)KillTeamGameManager.LayerType.Terrain);
        }
    }
    private void Awake()
    {
        grab = GetComponent<Grabbable>();
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

        currentColor = originalColor;
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

        BoxCollider[] boxColliders =
            GetComponentsInChildren<BoxCollider>();

        if (boxColliders.Length == 0)
        {
            Debug.LogWarning("No BoxColliders found.");
            return false;
        }

        float surfaceY = 0f;
        bool firstHit = true;

        foreach (BoxCollider box in boxColliders)
        {
            Vector3 halfSize = box.size * 0.5f;

            // Bottom corners in local collider space.
            Vector3[] localPoints =
            {
            box.center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
            box.center + new Vector3( halfSize.x, -halfSize.y, -halfSize.z),
            box.center + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z),
            box.center + new Vector3( halfSize.x, -halfSize.y,  halfSize.z),
            box.center + new Vector3(0f, -halfSize.y, 0f)
        };

            foreach (Vector3 localPoint in localPoints)
            {
                Vector3 worldPoint =
                    box.transform.TransformPoint(localPoint);

                // Account for proposed position.
                worldPoint += position - transform.position;

                bool valid = CastDown(
                    worldPoint,
                    out RaycastHit hit);

                if (valid)
                {
                    if (firstHit)
                    {
                        surfaceY = hit.point.y;
                        firstHit = false;
                    }
                    else
                    {
                        valid =
                            Mathf.Abs(hit.point.y - surfaceY)
                            <= heightTolerance;
                    }
                }

#if UNITY_EDITOR
            DrawCheckLine(worldPoint, valid);
#endif

                if (!valid)
                    return false;
            }
        }

        if (firstHit)
            return false;

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
