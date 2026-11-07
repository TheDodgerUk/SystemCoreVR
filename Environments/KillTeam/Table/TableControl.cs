
using UnityEngine;
using Antlr4.Runtime.Tree;


#if UNITY_EDITOR
using UnityEditor;
#endif

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

        // Cached reference to the actual AutoHand player.
        private Transform Player =>
            CameraControllerVR.Instance != null &&
            CameraControllerVR.Instance.AutoHandPlayerRef != null
                ? CameraControllerVR.Instance.AutoHandPlayerRef.transform
                : null;

        // Table centre in world space.
        private Vector3 TableCentre => transform.position;

        // Table up direction.
        private Vector3 TableUp => transform.up;

        private void Awake()
        {
            CameraControllerVR controller =
                CameraControllerVR.Instance;

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

            // Use the actual AutoHand player.
            rigRoot = Player;

            controller.vRControllerSetup.SetLeftLaserMode(true);
            controller.vRControllerSetup.SetRightLaserMode(true);

            colliderStorage.Store(gameObject);

            foreach (var item in transform.Find("Table").GetComponentsInChildren<Collider>())
            {
                item.enabled = false;
            }
        }

        private void Update()
        {
            if (Player == null ||
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

        // Set the player's absolute scale.
        // Always use CameraControllerVR.
        public void SetScale(float scale)
        {
            if (Player == null)
                return;

            scale = Mathf.Clamp(
                scale,
                minScale,
                maxScale);

            CameraControllerVR.Instance.SetScale(scale);
        }

        // Position the player relative to the table.
        // Vector3.zero means the table centre.
        public void SetPosition(Vector3 localPosition)
        {
            if (Player == null)
                return;

            Player.position =
                transform.TransformPoint(localPosition);
        }

        // Set an absolute world position.
        // Used internally by VR gesture calculations.
        private void SetWorldPosition(Vector3 worldPosition)
        {
            if (Player == null)
                return;

            Player.position = worldPosition;
        }

        // Rotate the player around the table centre.
        // The table itself does not move.
        public void SetRotation(float degrees)
        {
            if (Player == null)
                return;

            Player.RotateAround(
                TableCentre,
                TableUp,
                degrees);
        }

        // Set the player's world position and rotation.
        // Used by VR gesture calculations.
        private void SetPlayerTransform(
            Vector3 worldPosition,
            Quaternion worldRotation)
        {
            if (Player == null)
                return;

            Player.SetPositionAndRotation(
                worldPosition,
                worldRotation);
        }

        // Reset player using the same real methods.
        public void ResetRig()
        {
            if (Player == null)
                return;

            SetScale(1f);
            SetPosition(Vector3.zero);

            Player.rotation = transform.rotation;

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
            startRigPosition = Player.position;
            startHandYaw = activeHand.eulerAngles.y;
            startRigRotation = Player.rotation;
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

            // Rotate the player position around the table.
            Vector3 offset =
                position - TableCentre;

            Vector3 finalPosition =
                TableCentre + rotation * offset;

            Quaternion finalRotation =
                rotation * startRigRotation;

            SetPlayerTransform(
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

            // Get the actual player's world scale.
            startScale = Player.lossyScale.x;

            startRigPosition = Player.position;
            startTwoHandRotation = Player.rotation;

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

            // Scale the real AutoHand player.
            SetScale(newScale);

            // ======================================
            // ROTATION
            // ======================================

            Vector3 startFlat =
                Vector3.ProjectOnPlane(
                    startDirection,
                    TableUp);

            Vector3 currentFlat =
                Vector3.ProjectOnPlane(
                    direction,
                    TableUp);

            float yawChange = 0f;

            if (startFlat.sqrMagnitude > 0.0001f &&
                currentFlat.sqrMagnitude > 0.0001f)
            {
                yawChange = Vector3.SignedAngle(
                    startFlat,
                    currentFlat,
                    TableUp);
            }

            Quaternion rotation =
                Quaternion.AngleAxis(
                    -yawChange * rotationSpeed,
                    TableUp);

            Quaternion finalRotation =
                rotation * startTwoHandRotation;

            // ======================================
            // POSITION
            // ======================================

            // Player distance from table centre.
            Vector3 offset =
                startRigPosition - TableCentre;

            // Preserve relative distance when scaling.
            offset *= scaleFactor;

            // Rotate the offset around the table.
            offset = rotation * offset;

            // Move in the opposite direction
            // to the controller movement.
            Vector3 handMovement =
                midpoint - startMidpoint;

            Vector3 finalPosition =
                TableCentre +
                offset -
                handMovement * movementSpeed;

            SetPlayerTransform(
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
                CameraControllerVR.Instance != null &&
                CameraControllerVR.Instance.AutoHandPlayerRef != null;

            EditorGUI.BeginDisabledGroup(!canControl);

            if (GUILayout.Button(
                "Apply Scale",
                GUILayout.Height(30)))
            {
                Transform player =
                    CameraControllerVR.Instance
                        .AutoHandPlayerRef.transform;

                Undo.RecordObject(
                    player,
                    "Apply Player Scale");

                table.SetScale(table.debugScale);
            }

            if (GUILayout.Button(
                "Apply Rotation",
                GUILayout.Height(30)))
            {
                Transform player =
                    CameraControllerVR.Instance
                        .AutoHandPlayerRef.transform;

                Undo.RecordObject(
                    player,
                    "Apply Player Rotation");

                table.SetRotation(table.debugRotation);
            }

            if (GUILayout.Button(
                "Apply Position",
                GUILayout.Height(30)))
            {
                Transform player =
                    CameraControllerVR.Instance
                        .AutoHandPlayerRef.transform;

                Undo.RecordObject(
                    player,
                    "Apply Player Position");

                table.SetPosition(table.debugPosition);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button(
                "RESET RIG",
                GUILayout.Height(40)))
            {
                Transform player =
                    CameraControllerVR.Instance
                        .AutoHandPlayerRef.transform;

                Undo.RecordObject(
                    player,
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
