
using UnityEngine;
using Autohand;

namespace KillTeam
{
    public class TableControl : MonoBehaviour
    {
        [Header("Quest Controller Anchors")]
        public Transform leftHand;
        public Transform rightHand;

        [Header("Settings")]
        public float gripThreshold = 0.5f;
        public float minScale = 0.2f;
        public float maxScale = 5f;

        private Transform activeHand;

        // Single hand movement
        private Vector3 positionOffset;
        private float startHandYaw;
        private float startBoardYaw;
        private Vector3 startHandPosition;
        private Vector3 startBoardPosition;

        // Two hand movement
        private bool twoHanded;
        private float startDistance;
        private Vector3 startScale;
        private float startTwoHandYaw;
        private Vector3 startMidpoint;
        private Vector3 startBoardOffset;
        private Vector3 startDirection;


        private void Awake()
        {
            leftHand = CameraControllerVR.Instance.HandLeftRef.transform;
            rightHand = CameraControllerVR.Instance.HandRightRef.transform;
        }

        private void Update()
        {
            if (leftHand == null || rightHand == null)
                return;

            bool leftGrip = OVRInput.GetDown(
                                OVRInput.Button.One,
                                OVRInput.Controller.LTouch);

            bool rightGrip = OVRInput.GetDown(
                                OVRInput.Button.One,
                                OVRInput.Controller.RTouch);

            // Both grips held
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
            startBoardPosition = transform.position;

            startHandYaw = activeHand.eulerAngles.y;
            startBoardYaw = transform.eulerAngles.y;

            positionOffset =
                transform.position - activeHand.position;
        }

        private void UpdateSingleHand()
        {
            // Move board with hand
            Vector3 movement =
                activeHand.position - startHandPosition;

            // Rotate around Y axis only
            float yawChange = Mathf.DeltaAngle(
                startHandYaw,
                activeHand.eulerAngles.y);

            Quaternion rotation =
                Quaternion.Euler(0f, yawChange, 0f);

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    startBoardYaw + yawChange,
                    0f);

            // Move relative to original hand position
            transform.position =
                startBoardPosition + movement;
        }

        private void BeginTwoHand()
        {
            startDistance = Vector3.Distance(
                leftHand.position,
                rightHand.position);

            startScale = transform.localScale;
            startTwoHandYaw = transform.eulerAngles.y;

            startMidpoint =
                (leftHand.position + rightHand.position) * 0.5f;

            startBoardOffset =
                transform.position - startMidpoint;

            startDirection =
                rightHand.position - leftHand.position;
        }

        private void UpdateTwoHand()
        {
            Vector3 midpoint =
                (leftHand.position + rightHand.position) * 0.5f;

            Vector3 direction =
                rightHand.position - leftHand.position;

            float distance = direction.magnitude;

            if (startDistance < 0.001f || distance < 0.001f)
                return;

            // Zoom in and out
            float scaleFactor = distance / startDistance;

            float originalScale = Mathf.Max(
                Mathf.Abs(startScale.x),
                Mathf.Abs(startScale.y),
                Mathf.Abs(startScale.z));

            float newScale = Mathf.Clamp(
                originalScale * scaleFactor,
                minScale,
                maxScale);

            scaleFactor = originalScale > 0.0001f
                ? newScale / originalScale
                : 1f;

            transform.localScale =
                startScale * scaleFactor;

            // Rotate horizontally only
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

            Quaternion rotation =
                Quaternion.Euler(0f, yawChange, 0f);

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    startTwoHandYaw + yawChange,
                    0f);

            // Move, rotate and scale around hand midpoint
            transform.position =
                midpoint +
                rotation * (startBoardOffset * scaleFactor);
        }
    }
}
