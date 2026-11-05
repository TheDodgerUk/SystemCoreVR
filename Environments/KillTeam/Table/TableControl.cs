
using UnityEngine;

namespace KillTeam
{
    public class TableControl : MonoBehaviour
    {
        [Header("VR Rig")]
        public Transform rigRoot;
        public Transform head;

        [Header("Controller Anchors")]
        public Transform leftHand;
        public Transform rightHand;

        [Header("Settings")]
        public float minScale = 0.2f;
        public float maxScale = 5f;
        public float movementSpeed = 1f;
        public float rotationSpeed = 1f;

        private Transform activeHand;
        private bool twoHanded;

        // Single hand
        private Vector3 startHandPosition;
        private Vector3 startRigPosition;
        private float startHandYaw;
        private Quaternion startRigRotation;

        // Two hands
        private float startDistance;
        private float startScale;
        private Vector3 startMidpoint;
        private Vector3 startDirection;
        private Quaternion startTwoHandRotation;

        // World-space pivot on the board
        private Vector3 startPivot;

        private void Awake()
        {
            if (CameraControllerVR.Instance != null)
            {
                leftHand =
                    CameraControllerVR.Instance.HandLeftRef.transform;

                rightHand =
                    CameraControllerVR.Instance.HandRightRef.transform;
            }

            if (head == null && Camera.main != null)
                head = Camera.main.transform;
        }

        private void Update()
        {
            if (rigRoot == null ||
                leftHand == null ||
                rightHand == null)
                return;

            // Use Get, not GetDown, to detect held buttons.
            bool leftGrip = OVRInput.Get(
                OVRInput.Button.One,
                OVRInput.Controller.LTouch);

            bool rightGrip = OVRInput.Get(
                OVRInput.Button.One,
                OVRInput.Controller.RTouch);

            if (leftGrip && rightGrip)
            {
                if (!twoHanded)
                {
                    BeginTwoHand();
                    twoHanded = true;
                    activeHand = null;
                }

                UpdateTwoHand();
            }
            else
            {
                if (twoHanded)
                {
                    twoHanded = false;
                    activeHand = null;
                }

                Transform hand = leftGrip ? leftHand :
                                 rightGrip ? rightHand : null;

                if (hand != activeHand)
                {
                    activeHand = hand;

                    if (activeHand != null)
                        BeginSingleHand();
                }

                if (activeHand != null)
                    UpdateSingleHand();
            }
        }

        private void BeginSingleHand()
        {
            startHandPosition = activeHand.position;
            startRigPosition = rigRoot.position;

            startHandYaw = activeHand.eulerAngles.y;
            startRigRotation = rigRoot.rotation;
        }

        private void UpdateSingleHand()
        {
            Vector3 movement =
                activeHand.position - startHandPosition;

            float yawChange = Mathf.DeltaAngle(
                startHandYaw,
                activeHand.eulerAngles.y);

            Quaternion rotation = Quaternion.Euler(
                0f,
                -yawChange * rotationSpeed,
                0f);

            // Inverse hand movement:
            // dragging the world right moves the rig left.
            rigRoot.position =
                startRigPosition - movement * movementSpeed;

            rigRoot.rotation =
                rotation * startRigRotation;
        }

        private void BeginTwoHand()
        {
            startDistance = Vector3.Distance(
                leftHand.position,
                rightHand.position);

            startScale = rigRoot.localScale.x;
            startRigPosition = rigRoot.position;
            startTwoHandRotation = rigRoot.rotation;

            startMidpoint =
                (leftHand.position + rightHand.position) * 0.5f;

            startDirection =
                rightHand.position - leftHand.position;

            // Pivot is fixed in world space.
            startPivot = startMidpoint;
        }

        private void UpdateTwoHand()
        {
            Vector3 midpoint =
                (leftHand.position + rightHand.position) * 0.5f;

            Vector3 direction =
                rightHand.position - leftHand.position;

            float distance = direction.magnitude;

            if (startDistance < 0.001f ||
                distance < 0.001f)
                return;

            // Inverse scale:
            // hands apart -> player becomes smaller -> zoom out.
            // hands together -> player becomes bigger -> zoom in.
            float handFactor = distance / startDistance;

            float newScale = Mathf.Clamp(
                startScale / handFactor,
                minScale,
                maxScale);

            float scaleFactor =
                newScale / Mathf.Max(startScale, 0.0001f);

            rigRoot.localScale =
                Vector3.one * newScale;

            // Horizontal rotation
            Vector3 startFlat =
                Vector3.ProjectOnPlane(startDirection, Vector3.up);

            Vector3 currentFlat =
                Vector3.ProjectOnPlane(direction, Vector3.up);

            float yawChange = 0f;

            if (startFlat.sqrMagnitude > 0.0001f &&
                currentFlat.sqrMagnitude > 0.0001f)
            {
                yawChange = Vector3.SignedAngle(
                    startFlat,
                    currentFlat,
                    Vector3.up);
            }

            Quaternion rotation = Quaternion.Euler(
                0f,
                -yawChange * rotationSpeed,
                0f);

            rigRoot.rotation =
                rotation * startTwoHandRotation;

            // Keep the original world pivot stationary
            // relative to the scaled/rotated rig.
            Vector3 offset = startRigPosition - startPivot;

            rigRoot.position =
                startPivot +
                rotation * (offset * scaleFactor)
                - (midpoint - startMidpoint);
        }
    }
}
