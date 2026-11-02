
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
public class MiniaturePlacementValidator : MonoBehaviour
{
    [Header("Base")]
    public int edgeSamples = 16;

    [Header("Surface")]
    public LayerMask placementLayers = (1 << (int)KillTeamGameManager.LayerType.TableBoard) | (1 << (int)KillTeamGameManager.LayerType.Terrain);
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


    public void SetLayerMode(bool SetCharacterToStartPosition)
    {
        if (SetCharacterToStartPosition)
        {
            placementLayers = (1 << (int)KillTeamGameManager.LayerType.PlayerSetupStrip);
        }
        else
        {
            placementLayers = (1 << (int)KillTeamGameManager.LayerType.TableBoard) | (1 << (int)KillTeamGameManager.LayerType.Terrain);
        }
    }
    private void Awake()
    {
        grab = GetComponent<Grabbable>();
        capsule = GetComponent<CapsuleCollider>();
        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;      
    }

    private void OnDestroy()
    {
        KillTeamGameManager.Instance.OnStateChanged -= KillTeamGameManager_OnStateChanged;
    }
    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        SetLayerMode(false);
        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainSetup:
                break;
            case KillTeamGameManager.GameState.BarracadeSetup:
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPosition:
                SetLayerMode(true);
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
            if (placementLayers < 0 ||
                h.collider.gameObject.layer != placementLayers)
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
            $"Required Layer: {placementLayers} | " +
            $"Ray Distance: {rayDistance}"
        );
    }
#endif

        return hasHit;
    }
}
