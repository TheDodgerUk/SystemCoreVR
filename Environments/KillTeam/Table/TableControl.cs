
using UnityEngine;

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
        public float movementSpeed = 0.01f;
        public float rotationSpeed = 0.01f;

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

        // Actual AutoHand player.
        private Transform Player
        {
            get
            {
                CameraControllerVR controller =
                    CameraControllerVR.Instance;

                if (controller == null ||
                    controller.AutoHandPlayerRef == null)
                    return null;

                return controller.AutoHandPlayerRef.transform;
            }
        }

        private Vector3 TableCentre => transform.position;
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

            if (controller.vRControllerSetup == null)
            {
                Debug.LogError(
                    "TableControl: VR controller setup not found!");

                enabled = false;
                return;
            }

            left = controller.vRControllerSetup.LeftController;
            right = controller.vRControllerSetup.RightController;

            rigRoot = Player;

            controller.vRControllerSetup.SetLeftLaserMode(true);
            controller.vRControllerSetup.SetRightLaserMode(true);

            colliderStorage.Store(gameObject);

            Transform table = transform.Find("Table");

            if (table != null)
            {
                foreach (Collider item in
                    table.GetComponentsInChildren<Collider>())
                {
                    item.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning(
                    "TableControl: Child object 'Table' not found.");
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
        // PLAYER CONTROL
        // ==========================================

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

        // Local position relative to table.
        public void SetPosition(Vector3 localPosition)
        {
            if (Player == null)
                return;

            Player.position =
                transform.TransformPoint(localPosition);
        }

        private void SetWorldPosition(Vector3 worldPosition)
        {
            if (Player == null)
                return;

            Player.position = worldPosition;
        }

        // Rotate player around table centre.
        public void SetRotation(float degrees)
        {
            if (Player == null)
                return;

            Player.RotateAround(
                TableCentre,
                TableUp,
                degrees);
        }

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
            if (activeHand == null || Player == null)
                return;

            startHandPosition = activeHand.position;
            startRigPosition = Player.position;
            startHandYaw = activeHand.eulerAngles.y;
            startRigRotation = Player.rotation;
        }

        private void UpdateSingleHand()
        {
            if (activeHand == null || Player == null)
                return;

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
            if (Player == null)
                return;

            startDistance = Vector3.Distance(
                leftHand.position,
                rightHand.position);

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
            if (Player == null)
                return;

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

            Vector3 offset =
                startRigPosition - TableCentre;

            offset *= scaleFactor;
            offset = rotation * offset;

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
        private bool showTransformPositions = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

            TableControl table =
                (TableControl)target;

            // ======================================
            // LIVE TRANSFORM POSITIONS
            // ======================================

            EditorGUILayout.Space(15);

            showTransformPositions =
                EditorGUILayout.Foldout(
                    showTransformPositions,
                    "VR Transform Positions",
                    true);

            if (showTransformPositions)
            {
                EditorGUI.indentLevel++;

                DrawTransformPositions(table);

                EditorGUI.indentLevel--;
            }

            // ======================================
            // QUICK TESTING
            // ======================================

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField(
                "Quick Testing",
                EditorStyles.boldLabel);

            CameraControllerVR controller =
                CameraControllerVR.Instance;

            bool canControl =
                controller != null &&
                controller.AutoHandPlayerRef != null;

            EditorGUI.BeginDisabledGroup(!canControl);

            if (GUILayout.Button(
                "Apply Scale",
                GUILayout.Height(30)))
            {
                Transform player =
                    controller.AutoHandPlayerRef.transform;

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
                    controller.AutoHandPlayerRef.transform;

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
                    controller.AutoHandPlayerRef.transform;

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
                    controller.AutoHandPlayerRef.transform;

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

            // Refresh inspector while playing.
            if (Application.isPlaying)
                Repaint();
        }

        // ==========================================
        // TRANSFORM DISPLAY
        // ==========================================

        private void DrawTransformPositions(
            TableControl table)
        {
            DrawPosition("Rig Root", table.rigRoot);
            DrawPosition("Head", table.head);
            DrawPosition("Left Hand", table.leftHand);
            DrawPosition("Right Hand", table.rightHand);
        }

        private void DrawPosition(
            string label,
            Transform targetTransform)
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                label,
                EditorStyles.boldLabel);

            if (targetTransform == null)
            {
                EditorGUILayout.HelpBox(
                    label + " is not assigned.",
                    MessageType.Warning);

                return;
            }

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.ObjectField(
                "Transform",
                targetTransform,
                typeof(Transform),
                true);

            EditorGUILayout.Vector3Field(
                "World Position",
                targetTransform.position);

            EditorGUILayout.Vector3Field(
                "Local Position",
                targetTransform.localPosition);

            EditorGUILayout.Vector3Field(
                "World Rotation",
                targetTransform.eulerAngles);

            EditorGUILayout.Vector3Field(
                "Local Rotation",
                targetTransform.localEulerAngles);

            EditorGUI.EndDisabledGroup();
        }
    }

#endif
}
