
using Autohand;
using UnityEngine;
using UnityEngine.UIElements;

public class MiniaturePlacementValidator : MonoBehaviour
{
    [Header("Movement")]
    public float movementInches = 6f;
    public float metresPerInch = 0.0254f;

    [Header("Visuals")]
    public Renderer baseRenderer;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private Vector3 startPosition;
    private bool isMoving;

    private Material baseMaterial;
    private Color originalColor;

    private Grabbable grabbable;
    private MiniatureMovementLine miniatureMovementLine;
    private MiniatureVerticalLine miniatureVerticalLine;
    protected MiniatureStarPositionVerticalLine miniatureStarPositionVerticalLine;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();

        if (baseRenderer != null)
        {
            baseMaterial = baseRenderer.material;
            originalColor = baseMaterial.color;
        }

        miniatureMovementLine =
            gameObject.AddComponent<MiniatureMovementLine>();

        miniatureVerticalLine =
            gameObject.AddComponent<MiniatureVerticalLine>();

        miniatureStarPositionVerticalLine = gameObject.AddComponent<MiniatureStarPositionVerticalLine>();
        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
    }


    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        miniatureMovementLine.SetActive(true);
        miniatureVerticalLine.SetActive(true);
        miniatureStarPositionVerticalLine.SetActive(false);
        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainPlacement:
                break;
            case KillTeamGameManager.GameState.BarracadePlacement:
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                miniatureVerticalLine.SetActive(false);
                miniatureStarPositionVerticalLine.SetActive(true);

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
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        grabbable.OnGrabEvent += OnGrab;
        grabbable.OnReleaseEvent += OnRelease;
    }

    private void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.OnGrabEvent -= OnGrab;
            grabbable.OnReleaseEvent -= OnRelease;
        }

        isMoving = false;

        if (baseMaterial != null)
            baseMaterial.color = originalColor;
    }

    private void OnGrab(Hand hand, Grabbable grab)
    {
        startPosition = transform.position;
        isMoving = true;
    }

    private void LateUpdate()
    {
        if (!isMoving)
            return;

        bool valid = IsValidPosition(transform.position);

        if (baseMaterial != null)
        {
            baseMaterial.color =
                valid ? validColor : invalidColor;
        }
    }

    private void OnRelease(Hand hand, Grabbable grab)
    {
        if (!isMoving)
            return;

        bool validDistance =
            miniatureMovementLine.IsValidDistance();

        bool validPlacement =
            miniatureVerticalLine.IsValidPlacement();

        bool ValidStartPlace = miniatureStarPositionVerticalLine.IsValidPlacement();

        bool isStartPlacement =
            KillTeamGameManager.Instance.CurrentState ==
            KillTeamGameManager.GameState.SetCharacterToStartPlacement;

        bool valid;

        if (isStartPlacement)
        {
            valid = ValidStartPlace;
        }
        else
        {
            valid = validDistance && validPlacement;
        }

        if (!valid)
        {
            transform.position = startPosition;

            GenericSoundPlayer.Instance.PlaySound(
                GenericSoundPlayer.SoundType.FailSound,
                transform.position);
        }
        else
        {
            transform.position =  miniatureVerticalLine.PlacementPosition;
            GenericSoundPlayer.Instance.PlaySound(
                GenericSoundPlayer.SoundType.PassSound,
                transform.position);
        }

        isMoving = false;

        if (baseMaterial != null)
            baseMaterial.color = originalColor;

        Debug.Log(
            $"Miniature moved to {transform.position}. " +
            $"Valid: {valid}");

        Debug.Log(
            $"Distance moved: " +
            $"{Vector3.Distance(startPosition, transform.position)} metres. " +
            $"Max allowed: {movementInches * metresPerInch} metres.");
    }

    private bool IsValidPosition(Vector3 position)
    {
        float distance = Vector3.Distance(
            startPosition,
            position);

        float maxDistance =
            movementInches * metresPerInch;

        return distance <= maxDistance;
    }
}
