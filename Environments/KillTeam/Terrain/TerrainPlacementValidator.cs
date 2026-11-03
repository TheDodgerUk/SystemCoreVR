
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
public class TerrainPlacementValidator : MonoBehaviour
{
    [Header("Line Settings")]
    public float lineWidth = 0.005f;
    public float maxDistance = 5f;



    [Header("Base")]
    public int edgeSamples = 16;

    [Header("Surface")]
    public LayerMask placementLayers  = ~0;
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
    private Vector3 startPosition;
    private Quaternion startPositionRotation;

    private LineRenderer line;
    KillTeamGameManager.LayerType mLayer;

    private void Awake()
    {
        grab = GetComponent<Grabbable>();
        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;


        GameObject child = new GameObject("VerticalLine");
        child.transform.SetParent(transform, false);
        line = child.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;


    }
    public void SetMode(KillTeamGameManager.LayerType layer)
    {
        mLayer = layer;
    }
    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        grab.enabled = false;
        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainPlacement:
                if (mLayer == KillTeamGameManager.LayerType.Terrain)
                {
                    grab.enabled = true;
                }
                if (mLayer == KillTeamGameManager.LayerType.Barracade)
                {
                    grab.enabled = true;
                }
                break;
            case KillTeamGameManager.GameState.BarracadePlacement:
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                break;
            case KillTeamGameManager.GameState.ReadyToStart:
                break;
            case KillTeamGameManager.GameState.Playing:
                break;
            case KillTeamGameManager.GameState.Finished:
                break;
            default:
                break;
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
        startPosition = this.transform.position;
        startPositionRotation = this.transform.rotation;
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

        canPlace = CheckPlacement(
                transform.position,
                out placementPosition);


        if (canPlace)
        {
            transform.position = placementPosition;
            GenericSoundPlayer.Instance.PlaySound(GenericSoundPlayer.SoundType.PassSound, transform.position);
        }
        else
        {
            transform.position = startPosition;
            transform.rotation = startPositionRotation;
            GenericSoundPlayer.Instance.PlaySound(GenericSoundPlayer.SoundType.FailSound, transform.position);
        }
    }

    private void LateUpdate()
    {
        line.enabled = isGrabbed;
        if (!isGrabbed)
            return;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;


        canPlace = CheckPlacement(
            transform.position,
            out placementPosition);


        currentColor =
            canPlace ? validColor : invalidColor;


        Vector3 start = transform.position;



        line.startColor = currentColor;
        line.endColor = currentColor;
        line.SetPosition(0, start);
        if(canPlace)
        {
            line.SetPosition(1, placementPosition);
        }
        else
        {
            line.SetPosition(1, new Vector3(placementPosition.x, 0, placementPosition.z));
        }


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

        // Use the placement collider.
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
                box.transform.TransformPoint(localPoint) + positionOffset;

            lowestColliderY = Mathf.Min(
                lowestColliderY,
                worldPoint.y
            );

            bool valid = CastDown(
                worldPoint,
                out RaycastHit hit
            );

#if UNITY_EDITOR
        DrawCheckLine(worldPoint, valid);
#endif

            if (!valid)
                return false;

            highestSurfaceY = Mathf.Max(
                highestSurfaceY,
                hit.point.y
            );

            lowestSurfaceY = Mathf.Min(
                lowestSurfaceY,
                hit.point.y
            );
        }

        // Reject surfaces with too much height difference.
        if (highestSurfaceY - lowestSurfaceY > heightTolerance)
            return false;

        // Raise the object so its lowest collider point
        // rests on the highest detected surface.
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


    private bool CastDown(Vector3 position, out RaycastHit hit)
    {
        hit = default(RaycastHit);

        Vector3 origin =
            position + Vector3.up * rayStartHeight;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            rayDistance,
            placementLayers,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = float.MaxValue;
        bool hasHit = false;

        int requiredLayer = -1;

        switch (mLayer)
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

            // Ignore this GameObject and all child colliders
            if (h.collider.transform == transform ||
                h.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            // Only accept the required placement layer
            if (requiredLayer < 0 ||
                h.collider.gameObject.layer != requiredLayer)
            {
                continue;
            }

            // Find closest valid surface
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
        Vector3.down * (hasHit ? hit.distance : rayDistance),
        hasHit ? Color.green : Color.red
    );

    if (hasHit)
    {
        Debug.Log(
            $"VALID HIT: {hit.collider.name} | " +
            $"Position: {hit.point} | " +
            $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | " +
            $"Distance: {hit.distance:F3}"
        );
    }
    else
    {
        Debug.LogWarning(
            $"NO VALID HIT | " +
            $"Origin: {origin} | " +
            $"Required Layer: {requiredLayer} | " +
            $"Ray Distance: {rayDistance}"
        );
    }
#endif

        return hasHit;
    }

    private void OnDestroy()
    {
        KillTeamGameManager.Instance.OnStateChanged -= KillTeamGameManager_OnStateChanged;
    }

}
