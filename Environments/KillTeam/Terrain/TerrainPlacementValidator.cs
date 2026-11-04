
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(TerrainVerticalLineRenderer))]
public class TerrainPlacementValidator : MonoBehaviour
{
    protected Grabbable grab;
    protected TerrainVerticalLineRenderer verticalLine;

    private bool isGrabbed;

    private Vector3 startPosition;
    private Quaternion startPositionRotation;

    protected virtual KillTeamGameManager.LayerType mLayer =>
        KillTeamGameManager.LayerType.Terrain;

    private void Awake()
    {
        grab = GetComponent<Grabbable>();
        verticalLine = GetComponent<TerrainVerticalLineRenderer>();

        KillTeamGameManager.Instance.OnStateChanged +=
            KillTeamGameManager_OnStateChanged;
    }

    public KillTeamGameManager.LayerType GetPlacementLayer()
    {
        return mLayer;
    }

    protected virtual void KillTeamGameManager_OnStateChanged(
        KillTeamGameManager.GameState state)
    {
        grab.enabled = false;

        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;

            case KillTeamGameManager.GameState.BoardSetup:
                break;

            case KillTeamGameManager.GameState.TerrainPlacement:
                grab.enabled = true;
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
        }
    }

    private void OnEnable()
    {
        if (grab == null)
            grab = GetComponent<Grabbable>();

        grab.OnGrabEvent += OnGrab;
        grab.OnReleaseEvent += OnRelease;
    }

    private void OnDisable()
    {
        if (grab != null)
        {
            grab.OnGrabEvent -= OnGrab;
            grab.OnReleaseEvent -= OnRelease;
        }

        isGrabbed = false;

        if (verticalLine != null)
            verticalLine.SetActive(false);
    }

    private void OnGrab(Hand hand, Grabbable grabbable)
    {
        startPosition = transform.position;
        startPositionRotation = transform.rotation;

        isGrabbed = true;

        verticalLine.SetActive(true);
    }

    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        isGrabbed = false;

        verticalLine.SetActive(false);

        Vector3 rotation = transform.eulerAngles;

        transform.rotation =
            Quaternion.Euler(0f, rotation.y, 0f);

        bool canPlace = verticalLine.CheckPlacement(
            transform.position,
            out Vector3 placementPosition);

        if (canPlace)
        {
            transform.position = placementPosition;

            GenericSoundPlayer.Instance.PlaySound(
                GenericSoundPlayer.SoundType.PassSound,
                transform.position);
        }
        else
        {
            transform.position = startPosition;
            transform.rotation = startPositionRotation;

            GenericSoundPlayer.Instance.PlaySound(
                GenericSoundPlayer.SoundType.FailSound,
                transform.position);
        }
    }

    public bool IsValidPlacement()
    {
        return verticalLine.IsValidPlacement();
    }

    public Color GetColour()
    {
        return verticalLine.GetColour();
    }

    public bool CheckPlacement(
        Vector3 position,
        out Vector3 validPosition)
    {
        return verticalLine.CheckPlacement(
            position,
            out validPosition);
    }

    private void OnDestroy()
    {
        if (KillTeamGameManager.Instance != null)
        {
            KillTeamGameManager.Instance.OnStateChanged -=
                KillTeamGameManager_OnStateChanged;
        }
    }
}
