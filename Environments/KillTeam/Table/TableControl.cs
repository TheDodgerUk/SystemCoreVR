
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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

        [Header("Rotation")]
        public Vector3 rotationPivot = Vector3.zero;

        [Header("Inspector Testing")]
        public float debugScale = 1f;
        public float debugRotation = 45f;
        public Vector3 debugPosition = Vector3.zero;

        private Transform activeHand;
        private bool twoHanded;

        // Single-hand starting values.
        private Vector3 startHandPosition;
        private Vector3 startRigPosition;
        private float startHandYaw;
        private Quaternion startRigRotation;

        // Two-hand starting values.
        private float startDistance;
        private float startScale;
        private Vector3 startMidpoint;
        private Vector3 startDirection;
        private Quaternion startTwoHandRotation;

        private ControllerVr left;
        private ControllerVr right;

        private ColliderStorage colliderStorage =
            new ColliderStorage();

        private void Awake()
        {
            var controller = CameraControllerVR.Instance;

            if (controller == null)
            {
                Debug.LogError(
                    "TableControl: CameraControllerVR not found!");

                enabled = false;
                return;
            }

            if (controller.HandLeftRef != null)
                leftHand = controller.HandLeftRef.transform;

            if (controller.HandRightRef != null)
                rightHand = controller.HandRightRef.transform;

            if (head == null && Camera.main != null)
                head = Camera.main.transform;

            left = controller.vRControllerSetup.LeftController;
            right = controller.vRControllerSetup.RightController;

            rigRoot = controller.RigRoot;

            controller.vRControllerSetup.SetLeftLaserMode(true);
            controller.vRControllerSetup.SetRightLaserMode(true);

            colliderStorage.Store(gameObject);
        }

        private void Update()
        {
            if (rigRoot == null ||
                leftHand == null ||
                rightHand == null ||
                left == null ||
                right == null)
                return;

            bool leftGrip = left.PrimaryButton;
            bool rightGrip = right.PrimaryButton;

            if (!leftGrip && !rightGrip)
            {
                activeHand = null;
                twoHanded = false;

                colliderStorage.Restore();
                return;
            }

            colliderStorage.DisableAll();

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

                Transform hand =
                    leftGrip ? leftHand : rightHand;

                if (hand != activeHand)
                {
                    activeHand = hand;
                    BeginSingleHand();
                }

                UpdateSingleHand();
            }
        }

        // ==========================================
        // SHARED PLAYER CONTROL METHODS
        // ==========================================

        // Set the player's absolute world scale.
        // All scaling must go through CameraControllerVR.
        public void SetScale(float scale)
        {
            if (CameraControllerVR.Instance == null)
                return;

            scale = Mathf.Clamp(
                scale,
                minScale,
                maxScale);

            CameraControllerVR.Instance.SetScale(scale);
        }

        // Set the player's absolute world position.
        public void SetPosition(Vector3 position)
        {
            if (rigRoot == null)
                return;

            rigRoot.position = position;
        }

        // Apply a relative Y rotation around rotationPivot.
        public void SetRotation(float degrees)
        {
            if (rigRoot == null)
                return;

            Quaternion rotation =
                Quaternion.Euler(0f, degrees, 0f);

            Vector3 offset =
                rigRoot.position - rotationPivot;

            SetPosition(
                rotationPivot + rotation * offset);

            rigRoot.rotation =
                rotation * rigRoot.rotation;
        }

        // Apply an absolute rotation and position.
        // Used by VR gesture calculations.
        public void SetTransform(
            Vector3 position,
            Quaternion rotation)
        {
            if (rigRoot == null)
                return;

            SetPosition(position);
            rigRoot.rotation = rotation;
        }

        // Reset the player using the shared methods.
        public void ResetRig()
        {
            if (rigRoot == null)
                return;

            SetScale(1f);
            SetPosition(Vector3.zero);

            rigRoot.rotation = Quaternion.identity;

            activeHand = null;
            twoHanded = false;

            colliderStorage.Restore();
        }

        // ==========================================
        // SINGLE HAND
        // ==========================================

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

            Vector3 position =
                startRigPosition -
                movement * movementSpeed;

            Vector3 offset =
                position - rotationPivot;

            Vector3 finalPosition =
                rotationPivot + rotation * offset;

            Quaternion finalRotation =
                rotation * startRigRotation;

            SetTransform(
                finalPosition,
                finalRotation);
        }

        // ==========================================
        // TWO HANDS
        // ==========================================

        private void BeginTwoHand()
        {
            startDistance = Vector3.Distance(
                leftHand.position,
                rightHand.position);

            // Read the world scale, not local scale.
            startScale = rigRoot.lossyScale.x;

            startRigPosition = rigRoot.position;
            startTwoHandRotation = rigRoot.rotation;

            startMidpoint =
                (leftHand.position + rightHand.position) * 0.5f;

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

            if (startDistance < 0.001f ||
                distance < 0.001f)
                return;

            // ======================================
            // SCALE
            // ======================================

            float handFactor =
                distance / startDistance;

            float newScale = Mathf.Clamp(
                startScale / handFactor,
                minScale,
                maxScale);

            float scaleFactor =
                newScale / Mathf.Max(startScale, 0.0001f);

            // Use the actual player scaling system.
            SetScale(newScale);

            // ======================================
            // ROTATION
            // ======================================

            Vector3 startFlat =
                Vector3.ProjectOnPlane(
                    startDirection,
                    Vector3.up);

            Vector3 currentFlat =
                Vector3.ProjectOnPlane(
                    direction,
                    Vector3.up);

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

            Quaternion finalRotation =
                rotation * startTwoHandRotation;

            // ======================================
            // POSITION
            // ======================================

            // Preserve relative distance from the
            // world rotation pivot when scaling.
            Vector3 offset =
                startRigPosition - rotationPivot;

            offset *= scaleFactor;
            offset = rotation * offset;

            Vector3 handMovement =
                midpoint - startMidpoint;

            Vector3 finalPosition =
                rotationPivot +
                offset -
                handMovement * movementSpeed;

            SetTransform(
                finalPosition,
                finalRotation);
        }

        private void OnDisable()
        {
            colliderStorage.Restore();
        }

        private void OnDestroy()
        {
            colliderStorage.Restore();
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(TableControl))]
    public class TableControlEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

            TableControl table =
                (TableControl)target;

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField(
                "Quick Testing",
                EditorStyles.boldLabel);

            bool canControl =
                table.rigRoot != null &&
                CameraControllerVR.Instance != null;

            EditorGUI.BeginDisabledGroup(!canControl);

            if (GUILayout.Button(
                "Apply Scale",
                GUILayout.Height(30)))
            {
                Undo.RecordObject(
                    table.rigRoot,
                    "Apply Player Scale");

                table.SetScale(table.debugScale);
            }

            if (GUILayout.Button(
                "Apply Rotation",
                GUILayout.Height(30)))
            {
                Undo.RecordObject(
                    table.rigRoot,
                    "Apply Player Rotation");

                table.SetRotation(table.debugRotation);
            }

            if (GUILayout.Button(
                "Apply Position",
                GUILayout.Height(30)))
            {
                Undo.RecordObject(
                    table.rigRoot,
                    "Apply Player Position");

                table.SetPosition(table.debugPosition);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button(
                "RESET RIG",
                GUILayout.Height(40)))
            {
                Undo.RecordObject(
                    table.rigRoot,
                    "Reset Player Rig");

                table.ResetRig();
            }

            EditorGUI.EndDisabledGroup();

            if (!canControl)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to test player controls.",
                    MessageType.Info);
            }
        }
    }

#endif
}
