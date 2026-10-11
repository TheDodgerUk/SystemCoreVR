
using UnityEngine;
using Autohand;

namespace KillTeam
{
    public class TableControl : MonoBehaviour
    {
        [Header("References")]
        public Transform tableRoot;
        public Transform leftHand;
        public Transform rightHand;

        [Header("Movement")]
        public float moveSensitivity = 1f;

        [Header("Scaling")]
        public float minScale = 0.4f;
        public float maxScale = 3f;
        public float scaleSensitivity = 1f;

        [Header("Rotation")]
        public float rotationSensitivity = 1f;

        [Header("Tilt")]
        public bool allowTilt = true;
        public float tiltSensitivity = 1f;
        public float maxTilt = 45f;

        private enum GrabMode
        {
            None,
            Left,
            Right,
            Both
        }

        private GrabMode currentMode = GrabMode.None;

        private Vector3 previousHandPosition;
        private Vector3 previousMidpoint;
        private Vector3 previousHandVector;

        public void Initialise()
        {
            tableRoot = this.gameObject.SearchComponent<Transform>("Table");
            foreach (Hand hand in FindObjectsOfType<Hand>())
            {
                if (hand.left)
                    leftHand = hand.transform;
                else
                    rightHand = hand.transform;
            }
        }

        private void Update()
        {
            if (tableRoot == null ||
                leftHand == null ||
                rightHand == null)
                return;

            bool leftGrip = OVRInput.Get(
                OVRInput.Button.PrimaryHandTrigger,
                OVRInput.Controller.LTouch);

            bool rightGrip = OVRInput.Get(
                OVRInput.Button.PrimaryHandTrigger,
                OVRInput.Controller.RTouch);

            GrabMode newMode = GrabMode.None;

            if (leftGrip && rightGrip)
                newMode = GrabMode.Both;
            else if (leftGrip)
                newMode = GrabMode.Left;
            else if (rightGrip)
                newMode = GrabMode.Right;

            // Reset reference positions whenever grip mode changes.
            if (newMode != currentMode)
            {
                currentMode = newMode;
                CaptureHands();
                return;
            }

            switch (currentMode)
            {
                case GrabMode.Left:
                    MoveWithHand(leftHand);
                    break;

                case GrabMode.Right:
                    MoveWithHand(rightHand);
                    break;

                case GrabMode.Both:
                    MoveWithBothHands();
                    break;
            }
        }

        private void CaptureHands()
        {
            if (currentMode == GrabMode.Left)
                previousHandPosition = leftHand.position;

            if (currentMode == GrabMode.Right)
                previousHandPosition = rightHand.position;

            if (currentMode == GrabMode.Both)
            {
                previousMidpoint =
                    (leftHand.position + rightHand.position) * 0.5f;

                previousHandVector =
                    rightHand.position - leftHand.position;
            }
        }

        private void MoveWithHand(Transform hand)
        {
            Vector3 delta =
                hand.position - previousHandPosition;

            tableRoot.position += delta * moveSensitivity;

            previousHandPosition = hand.position;
        }

        private void MoveWithBothHands()
        {
            Vector3 midpoint =
                (leftHand.position + rightHand.position) * 0.5f;

            Vector3 handVector =
                rightHand.position - leftHand.position;

            // 1. Move table
            Vector3 delta = midpoint - previousMidpoint;
            tableRoot.position += delta * moveSensitivity;

            // 2. Zoom table
            float oldDistance = previousHandVector.magnitude;
            float newDistance = handVector.magnitude;

            if (oldDistance > 0.001f &&
                newDistance > 0.001f)
            {
                float ratio = newDistance / oldDistance;

                float scaleFactor =
                    Mathf.Pow(ratio, scaleSensitivity);

                float targetScale = Mathf.Clamp(
                    tableRoot.localScale.x * scaleFactor,
                    minScale,
                    maxScale);

                float appliedFactor =
                    targetScale / tableRoot.localScale.x;

                // Scale around the midpoint between hands.
                Vector3 offset = tableRoot.position - midpoint;

                tableRoot.position =
                    midpoint + offset * appliedFactor;

                tableRoot.localScale *= appliedFactor;
            }

            // 3. Rotate table
            Vector3 oldFlat = Vector3.ProjectOnPlane(
                previousHandVector, Vector3.up);

            Vector3 newFlat = Vector3.ProjectOnPlane(
                handVector, Vector3.up);

            if (oldFlat.sqrMagnitude > 0.0001f &&
                newFlat.sqrMagnitude > 0.0001f)
            {
                float angle = Vector3.SignedAngle(
                    oldFlat,
                    newFlat,
                    Vector3.up);

                tableRoot.RotateAround(
                    midpoint,
                    Vector3.up,
                    -angle * rotationSensitivity);
            }

            // 4. Tilt table
            if (allowTilt)
            {
                float oldPitch = Mathf.Atan2(
                    previousHandVector.y,
                    oldFlat.magnitude) * Mathf.Rad2Deg;

                float newPitch = Mathf.Atan2(
                    handVector.y,
                    newFlat.magnitude) * Mathf.Rad2Deg;

                float pitchDelta =
                    (newPitch - oldPitch) * tiltSensitivity;

                // Tilt around the table's local right axis.
                Vector3 axis = tableRoot.right;

                float currentTilt = Vector3.SignedAngle(
                    Vector3.up,
                    tableRoot.up,
                    axis);

                float targetTilt = Mathf.Clamp(
                    currentTilt + pitchDelta,
                    -maxTilt,
                    maxTilt);

                float appliedTilt = targetTilt - currentTilt;

                tableRoot.RotateAround(
                    midpoint,
                    axis,
                    appliedTilt);
            }

            previousMidpoint = midpoint;
            previousHandVector = handVector;
        }
    }
}
