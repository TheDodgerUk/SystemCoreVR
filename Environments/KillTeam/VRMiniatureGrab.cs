
using UnityEngine;
using Autohand;
using Org.BouncyCastle.Asn1.Mozilla;

// add to the miniatures

public class VRMiniatureGrab : MonoBehaviour
{
    private Grabbable grab;
    private MiniatureMovement movement;
    private MiniaturePlacementValidator miniaturePlacementValidator;
    private Vector3 startPosition;

    bool isGabbedEnabled = true;
    private bool isGrabbed;
    private float lockedHeight;

    private void Awake()
    {
        grab = GetComponent<Grabbable>();
        movement = GetComponent<MiniatureMovement>();
        this.WaitFor(1, () => 
        { 
            miniaturePlacementValidator = GetComponent<MiniaturePlacementValidator>(); 
        });

        grab.OnGrabEvent += OnGrab;
        grab.OnReleaseEvent += OnRelease;
    }
    public void SetGrabEnabled(bool enabled)
    {
        isGabbedEnabled = enabled;
        grab.enabled = enabled;
    }

    private void OnGrab(Hand hand, Grabbable grabbable)
    {
        startPosition = transform.position;
        lockedHeight = transform.position.y;
        movement.BeginMove();
        isGrabbed = true;
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
            return;

        Vector3 position = transform.position;
        position.y = lockedHeight;

        movement.UpdateMove(position);
    }

    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        isGrabbed = false;

        // Reset height to original Y
        Vector3 position = transform.position;
        position.y = lockedHeight;
        transform.position = position;

        // Make miniature upright, but preserve Y rotation
        Vector3 rotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);


        if(miniaturePlacementValidator.canPlace == false)
        {
            transform.position = startPosition;
        }
        movement.EndMove();
    }

    private void OnDestroy()
    {
        grab.OnGrabEvent -= OnGrab;
        grab.OnReleaseEvent -= OnRelease;
    }
}
