
using UnityEngine;

public class TerrainVerticalLineRenderer : MonoBehaviour
{
    [Header("Line Settings")]
    public float lineWidth = 0.005f;
    public float maxDistance = 5f;

    [Header("Base")]
    public int edgeSamples = 16;

    [Header("Surface")]
    public LayerMask placementLayers = ~0;
    public float rayStartHeight = 2f;
    public float rayDistance = 5f;
    public float heightTolerance = 10f;

    [Header("Visual")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    public Color currentColor = Color.red;
    public bool canPlace;

    private LineRenderer line;
    private TerrainPlacementValidator validator;

    private bool isActive;
    private Vector3 placementPosition;

    public Vector3 PlacementPosition => placementPosition;

    private void Awake()
    {
        validator = GetComponent<TerrainPlacementValidator>();

        GameObject child = new GameObject("VerticalLine");
        child.transform.SetParent(transform, false);

        line = child.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.enabled = false;
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (line != null)
            line.enabled = active;
    }

    private void LateUpdate()
    {
        if (!isActive)
            return;

        canPlace = CheckPlacement(
            transform.position,
            out placementPosition);

        currentColor = canPlace
            ? validColor
            : invalidColor;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.startColor = currentColor;
        line.endColor = currentColor;

        line.SetPosition(0, transform.position);

        if (canPlace)
        {
            line.SetPosition(1, placementPosition);
        }
        else
        {
            line.SetPosition(
                1,
                new Vector3(
                    placementPosition.x,
                    0f,
                    placementPosition.z));
        }
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

        BoxCollider[] boxColliders =
            GetComponentsInChildren<BoxCollider>();

        if (boxColliders.Length == 0)
        {
            Debug.LogWarning("No BoxColliders found.");
            return false;
        }

        BoxCollider box = boxColliders[0];

        Vector3 halfSize = box.size * 0.5f;

        Vector3[] localPoints =
        {
            box.center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
            box.center + new Vector3( halfSize.x, -halfSize.y, -halfSize.z),
            box.center + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z),
            box.center + new Vector3( halfSize.x, -halfSize.y,  halfSize.z),
            box.center + new Vector3(0f, -halfSize.y, 0f)
        };

        float highestSurfaceY = float.NegativeInfinity;
        float lowestSurfaceY = float.PositiveInfinity;
        float lowestColliderY = float.PositiveInfinity;

        Vector3 positionOffset = position - transform.position;

        foreach (Vector3 localPoint in localPoints)
        {
            Vector3 worldPoint =
                box.transform.TransformPoint(localPoint) +
                positionOffset;

            lowestColliderY = Mathf.Min(
                lowestColliderY,
                worldPoint.y);

            bool valid = CastDown(
                worldPoint,
                out RaycastHit hit);

#if UNITY_EDITOR
            DrawCheckLine(worldPoint, valid);
#endif

            if (!valid)
                return false;

            highestSurfaceY = Mathf.Max(
                highestSurfaceY,
                hit.point.y);

            lowestSurfaceY = Mathf.Min(
                lowestSurfaceY,
                hit.point.y);
        }

        if (highestSurfaceY - lowestSurfaceY > heightTolerance)
            return false;

        float heightAdjustment =
            highestSurfaceY - lowestColliderY;

        validPosition = position +
            Vector3.up * heightAdjustment;

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

        int requiredLayer = -1;

        KillTeamGameManager.LayerType layer =
            validator != null
                ? validator.GetPlacementLayer()
                : KillTeamGameManager.LayerType.Terrain;

        switch (layer)
        {
            case KillTeamGameManager.LayerType.Terrain:
                requiredLayer =
                    (int)KillTeamGameManager.LayerType.TableBoard;
                break;

            case KillTeamGameManager.LayerType.Barracade:
                requiredLayer =
                    (int)KillTeamGameManager.LayerType.PlayerSetupStrip;
                break;
        }

        foreach (RaycastHit h in hits)
        {
            if (h.collider == null)
                continue;

            if (h.collider.transform == transform ||
                h.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (requiredLayer < 0 ||
                h.collider.gameObject.layer != requiredLayer)
            {
                continue;
            }

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

        if (hasHit)
        {
            Debug.Log(
                $"VALID HIT: {hit.collider.name} | " +
                $"Position: {hit.point} | " +
                $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | " +
                $"Distance: {hit.distance:F3}");
        }
        else
        {
            Debug.LogWarning(
                $"NO VALID HIT | " +
                $"Origin: {origin} | " +
                $"Required Layer: {requiredLayer} | " +
                $"Ray Distance: {rayDistance}");
        }
#endif

        return hasHit;
    }

    private void OnDisable()
    {
        SetActive(false);
    }
}
