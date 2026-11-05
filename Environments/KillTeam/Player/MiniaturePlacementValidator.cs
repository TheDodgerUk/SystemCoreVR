
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

    private Vector3 startPosition;
    private bool isMoving;


    private Grabbable grabbable;
    private MiniatureMovementLine miniatureMovementLine;
    private MiniatureVerticalLine miniatureVerticalLine;
    protected MiniatureStarPositionVerticalLine miniatureStarPositionVerticalLine;

    [SerializeField]
    private bool isValid = false;

    public bool IsValidPlacement() => isValid;
    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();

        miniatureMovementLine = gameObject.AddComponent<MiniatureMovementLine>();
        miniatureVerticalLine = gameObject.AddComponent<MiniatureVerticalLine>();
        miniatureStarPositionVerticalLine = gameObject.AddComponent<MiniatureStarPositionVerticalLine>();

        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
        KillTeamGameManager_OnStateChanged(KillTeamGameManager.Instance.CurrentState);
        Debug.Log($"MiniaturePlacementValidator {this.gameObject.name}  state {KillTeamGameManager.Instance.CurrentState} ");
    }
    private void OnDestroy()
    {
        KillTeamGameManager.Instance.OnStateChanged -= KillTeamGameManager_OnStateChanged;
    }


    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        miniatureMovementLine.SetActive(true);
        miniatureVerticalLine.SetActive(true);
        miniatureStarPositionVerticalLine.SetActive(false);
        switch (state)
        {
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
            case KillTeamGameManager.GameState.DEBUG_SlowPlayerCreation:
                Debug.LogError("MiniaturePlacementValidator  SetCharacterToStartPlacement");
                miniatureVerticalLine.SetActive(false);
                miniatureMovementLine.SetActive(false);
                miniatureStarPositionVerticalLine.SetActive(true);
                grabbable.enabled = true;
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
;
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

        isValid = IsValidPosition(transform.position);

    }

    private void OnRelease(Hand hand, Grabbable grab)
    {
        if (!isMoving)
            return;

        bool validDistance = !miniatureMovementLine.gameObject.activeInHierarchy || miniatureMovementLine.IsValidDistance();
        bool validPlacement = !miniatureVerticalLine.gameObject.activeInHierarchy || miniatureVerticalLine.IsValidPlacement();
        bool ValidStartPlace = !miniatureStarPositionVerticalLine.gameObject.activeInHierarchy || miniatureStarPositionVerticalLine.IsValidPlacement();

        isValid = validDistance && validPlacement && ValidStartPlace;


        if (!isValid)
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

        Debug.Log(
            $"Miniature moved to {transform.position}. " +
            $"Valid: {isValid}");

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
