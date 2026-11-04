
using Autohand;
using UnityEngine;

// Rigidbody
// Collider
// Grabbable (AutoHand)
// MiniatureMovementLine
// MiniatureVerticalLine

[RequireComponent(typeof(Grabbable))]
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
            gameObject.ForceComponent<MiniatureMovementLine>();

        miniatureVerticalLine =
            gameObject.ForceComponent<MiniatureVerticalLine>();

        miniatureStarPositionVerticalLine = gameObject.ForceComponent<MiniatureStarPositionVerticalLine>();
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
