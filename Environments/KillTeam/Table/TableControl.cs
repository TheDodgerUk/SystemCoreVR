
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
        public float scaleSensitivity = 1f;
        public float rotationSpeed = 90f; // Degrees per metre of relative vertical hand movement

        [Header("Inspector Testing")]
        public float debugScale = 1f;
        public float debugRotation = 45f;
        public Vector3 debugPosition = Vector3.zero;

        private bool twoHanded;
        private float startHorizontalDistance;
        private float startScale;
        private float startHeightDifference;
        private Vector3 startRigPosition;
        private Quaternion startTwoHandRotation;

        private ControllerVr left;
        private ControllerVr right;

        private ColliderStorage colliderStorage =
            new ColliderStorage();

        private Vector3 offSet;

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
            if (Player == null || leftHand == null || rightHand == null ||
                left == null || right == null)
            {
                EndTwoHand();
                return;
            }

            // Both GRIPS must be held. Face buttons are not used.
            bool bothGrips = left.Grip > 0.9f && right.Grip > 0.9f;
            if (!bothGrips)
            {
                EndTwoHand();
                return;
            }

            if (!twoHanded)
            {
                BeginTwoHand();
                twoHanded = true;
                colliderStorage.DisableAll();
            }

            UpdateTwoHand();
        }

        private void EndTwoHand()
        {
            if (!twoHanded) return;
            twoHanded = false;
            colliderStorage.Restore();
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

            twoHanded = false;

            colliderStorage.Restore();
        }

        // ==========================================
        // TWO GRIPS: HORIZONTAL = SCALE, VERTICAL = ROTATION
        // ==========================================

        private void BeginTwoHand()
        {
            Vector3 separation = rightHand.position - leftHand.position;
            Vector3 horizontal = Vector3.ProjectOnPlane(separation, TableUp);

            startHorizontalDistance = horizontal.magnitude;
            startHeightDifference = Vector3.Dot(separation, TableUp);
            startScale = Player.lossyScale.x;
            startRigPosition = Player.position;
            startTwoHandRotation = Player.rotation;

            offSet = Player.transform.position - Camera.main.transform.position;
        }

        private void UpdateTwoHand()
        {
            Vector3 separation = rightHand.position - leftHand.position;
            float horizontalDistance =
                Vector3.ProjectOnPlane(separation, TableUp).magnitude;

            // Hands apart = bigger; hands together = smaller.
            float distanceChange = horizontalDistance - startHorizontalDistance;
            float newScale = Mathf.Clamp(
                startScale * (1f + distanceChange * scaleSensitivity),
                minScale, maxScale);

            // Difference in hand heights controls yaw around the table.
            float heightDifference = Vector3.Dot(separation, TableUp);
            float heightChange = heightDifference - startHeightDifference;
            float degrees = heightChange * rotationSpeed;
            Quaternion deltaRotation = Quaternion.AngleAxis(degrees, TableUp);

            SetScale(newScale);

            // Preserve player position on scaling; only rotation orbits the table.
            Vector3 rotatedOffset = deltaRotation * (startRigPosition - TableCentre);
            SetPlayerTransform(TableCentre + rotatedOffset,
                deltaRotation * startTwoHandRotation);
        }

        private void OnDisable()
        {
            twoHanded = false;
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
