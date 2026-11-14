
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
        KillTeamGameManager_OnStateChanged(KillTeamGameManager.Instance.CurrentGameState);
        Debug.Log($"MiniaturePlacementValidator {this.gameObject.name}  state {KillTeamGameManager.Instance.CurrentGameState} ");
    }
    private void OnDestroy()
    {
        KillTeamGameManager.Instance.OnStateChanged -= KillTeamGameManager_OnStateChanged;
    }


    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        miniatureMovementLine.enabled = true;
        miniatureVerticalLine.enabled = true;
        miniatureStarPositionVerticalLine.enabled = false;
        switch (state)
        {
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
            case KillTeamGameManager.GameState.DEBUG_SlowPlayerCreation:
                Debug.LogError("MiniaturePlacementValidator  SetCharacterToStartPlacement");
                miniatureVerticalLine.enabled = false;
                miniatureMovementLine.enabled = false;
                miniatureStarPositionVerticalLine.enabled = true;
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

    }

    private void OnGrab(Hand hand, Grabbable grab)
    {
        startPosition = transform.position;
    }

    private void LateUpdate()
    {
        bool validDistance = !miniatureMovementLine.enabled || miniatureMovementLine.IsValidDistance();
        bool validPlacement = !miniatureVerticalLine.enabled || miniatureVerticalLine.IsValidPlacement();
        bool ValidStartPlace = !miniatureStarPositionVerticalLine.enabled || miniatureStarPositionVerticalLine.IsValidPlacement();

        isValid = validDistance && validPlacement && ValidStartPlace;
    }

    private void OnRelease(Hand hand, Grabbable grab)
    {

        bool validDistance = !miniatureMovementLine.enabled               || miniatureMovementLine.IsValidDistance();
        bool validPlacement = !miniatureVerticalLine.enabled              || miniatureVerticalLine.IsValidPlacement();
        bool ValidStartPlace = !miniatureStarPositionVerticalLine.enabled || miniatureStarPositionVerticalLine.IsValidPlacement();

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
            if(miniatureVerticalLine.enabled == true)
            {
                transform.position = miniatureVerticalLine.PlacementPosition;
            }
            else
            {
                transform.position = miniatureStarPositionVerticalLine.PlacementPosition;
            }

            GenericSoundPlayer.Instance.PlaySound(
                GenericSoundPlayer.SoundType.PassSound,
                transform.position);
        }


        Debug.Log(
            $"Miniature moved to {transform.position}. " +
            $"Valid: {isValid}");

        Debug.Log(
            $"Distance moved: " +
            $"{Vector3.Distance(startPosition, transform.position)} metres. " +
            $"Max allowed: {movementInches * metresPerInch} metres.");
    }
}
