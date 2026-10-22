
using UnityEngine;


//Rigidbody
//Collider
//Grabbable(AutoHand)
// MiniatureMovement
// VRMiniatureGrab
// add to the miniatures

public class MiniatureMovement : MonoBehaviour
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
    private MiniatureMovementLine miniatureMovementLine;
    private MiniaturePlacementValidator miniaturePlacementValidator;

    void Awake()
    {
        if (baseRenderer != null)
        {
            baseMaterial = baseRenderer.material;
            originalColor = baseMaterial.color;
        }
        miniatureMovementLine = GetComponent<MiniatureMovementLine>();
        miniaturePlacementValidator = GetComponent<MiniaturePlacementValidator>();
    }

    public void BeginMove()
    {
        startPosition = transform.position;
        isMoving = true;
    }

    public void UpdateMove(Vector3 targetPosition)
    {
        if (!isMoving)
            return;


        transform.position = targetPosition;

        bool valid = IsValidPosition(targetPosition);

        if (baseMaterial != null)
            baseMaterial.color = valid ? validColor : invalidColor;
    }

    public void EndMove()
    {
        if (!isMoving)
            return;

        if(miniatureMovementLine.IsValidPosition() == false || miniaturePlacementValidator.IsValidPlacement() == false)
        {
            Debug.Log($"miniatureMovementLine: {miniatureMovementLine.IsValidPosition()}"); ;
            Debug.Log($"miniaturePlacementValidator: {miniaturePlacementValidator.IsValidPlacement()}"); ;
            transform.position = startPosition;
        }

        isMoving = false;

        if (baseMaterial != null)
            baseMaterial.color = originalColor;

        Debug.Log($"Miniature moved to {transform.position}. Valid: {IsValidPosition(transform.position)}");
        Debug.Log($"Distance moved: {Vector3.Distance(startPosition, transform.position)} metres. Max allowed: {movementInches * metresPerInch} metres.");
    }

    private bool IsValidPosition(Vector3 position)
    {
        float distance = Vector3.Distance(
            startPosition,
            position
        );

        float maxDistance = movementInches * metresPerInch;

        return distance <= maxDistance;
    }
}
