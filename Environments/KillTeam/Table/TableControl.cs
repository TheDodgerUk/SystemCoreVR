
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KillTeam
{
    public class TableControl : MonoBehaviour
    {
        [Header("VR References")]
        public Transform rigRoot;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        [Header("Settings")]
        public float minScale = 0.2f;
        public float maxScale = 5f;
        public float scaleSensitivity = 1f;
        public float rotationSpeed = 90f;

        [Header("Quick Testing")]
        public float debugScale = 1f;
        public float debugRotation = 45f;
        public Vector3 debugPosition;

        [Header("Hand Simulation")]
        [Range(0.5f, 5f)] public float editorHorizontal;
        [Range(-5f, 5f)] public float editorVertical;

        [SerializeField, HideInInspector]
        private bool editorBothHeld;

        private ControllerVr left;
        private ControllerVr right;

        private readonly ColliderStorage colliders =
            new ColliderStorage();

        private bool manipulating;
        private bool simulated;

        private float startScale;
        private float startHorizontal;
        private float startVertical;

        private Vector3 startPosition;
        private Quaternion startRotation;

        public bool EditorBothHeld => editorBothHeld;

        private CameraControllerVR Controller =>
            CameraControllerVR.Instance;

        private Transform Player =>
            rigRoot != null
                ? rigRoot
                : Controller != null &&
                  Controller.AutoHandPlayerRef != null
                    ? Controller.AutoHandPlayerRef.transform
                    : null;

        private Vector3 Centre => transform.position;
        private Vector3 Up => transform.up;

        private void Awake()
        {
            
            if (Controller == null ||
                Controller.AutoHandPlayerRef == null)
            {
                Debug.LogError(
                    "TableControl: VR controller not found.");
                enabled = false;
                return;
            }

            rigRoot = Controller.AutoHandPlayerRef.transform;

            if (Controller.AutoHandPlayerRef.handLeft != null)
                leftHand = Controller.AutoHandPlayerRef.handLeft.transform;

            if (Controller.AutoHandPlayerRef.handRight != null)
                rightHand = Controller.AutoHandPlayerRef.handRight.transform;

            if (head == null && Camera.main != null)
                head = Camera.main.transform;

            colliders.Store(gameObject);

            Transform table = transform.Find("Table");

            ////if (table != null)
            ////{
            ////    foreach (Collider collider in
            ////        table.GetComponentsInChildren<Collider>())
            ////    {
            ////        collider.enabled = false;
            ////    }
            ////}
        }

        // Supply your existing controller wrappers.
        public void SetControllers(
            ControllerVr leftController,
            ControllerVr rightController)
        {
            left = leftController;
            right = rightController;
        }

        private void Update()
        {
            if (editorBothHeld)
            {
                UpdateEditorHands();
                return;
            }

            if (Player == null ||
                leftHand == null ||
                rightHand == null ||
                left == null ||
                right == null)
            {
                EndManipulation();
                return;
            }

            bool bothGrips =
                left.Grip > 0.9f &&
                right.Grip > 0.9f;

            if (!bothGrips)
            {
                EndManipulation();
                return;
            }

            Vector3 separation =
                rightHand.position - leftHand.position;

            float horizontal =
                Vector3.ProjectOnPlane(
                    separation, Up).magnitude;

            float vertical =
                Vector3.Dot(separation, Up);

            if (!manipulating)
                BeginManipulation(horizontal, vertical, false);

            ApplyManipulation(horizontal, vertical);
        }

        // -----------------------------------------
        // PLAYER TRANSFORMS
        // -----------------------------------------

        public void SetScale(float scale)
        {
            if (Player == null || Controller == null)
                return;

            scale = Mathf.Clamp(
                scale, minScale, maxScale);

            Controller.SetScale(scale);
        }

        public void SetPosition(Vector3 localPosition)
        {
            if (Player == null)
                return;

            Player.position =
                transform.TransformPoint(localPosition);
        }

        public void SetRotation(float degrees)
        {
            if (Player == null)
                return;

            Player.RotateAround(Centre, Up, degrees);
        }

        public void ResetRig()
        {
            if (Player == null)
                return;

            ReleaseBoth();

            SetScale(1f);
            SetPosition(Vector3.zero);

            Player.rotation = transform.rotation;
        }

        // -----------------------------------------
        // SHARED TWO-HAND MANIPULATION
        // -----------------------------------------

        private void BeginManipulation(
            float horizontal,
            float vertical,
            bool isSimulation)
        {
            if (Player == null)
                return;

            EndManipulation();

            manipulating = true;
            simulated = isSimulation;

            startHorizontal = horizontal;
            startVertical = vertical;

            startScale = Player.lossyScale.x;
            startPosition = Player.position;
            startRotation = Player.rotation;

            colliders.DisableAll();
        }

        private void ApplyManipulation(
            float horizontal,
            float vertical)
        {
            if (!manipulating || Player == null)
                return;

            float horizontalChange =
                horizontal - startHorizontal;

            float verticalChange =
                vertical - startVertical;

            float scale = startScale *
                (1f + horizontalChange * scaleSensitivity);

            float angle =
                verticalChange * rotationSpeed;

            Quaternion rotation =
                Quaternion.AngleAxis(angle, Up);

            SetScale(scale);

            Vector3 position =
                Centre + rotation * (startPosition - Centre);

            Player.SetPositionAndRotation(
                position,
                rotation * startRotation);
        }

        private void EndManipulation()
        {
            if (!manipulating)
                return;

            manipulating = false;
            simulated = false;

            colliders.Restore();
        }

        // -----------------------------------------
        // EDITOR HAND SIMULATION
        // -----------------------------------------

        public void BothHold()
        {
            if (Player == null || editorBothHeld)
                return;

            editorBothHeld = true;

            BeginManipulation(
                editorHorizontal,
                editorVertical,
                true);
        }

        public void ReleaseBoth()
        {
            editorBothHeld = false;
            EndManipulation();
        }

        public void UpdateEditorHands()
        {
            if (!editorBothHeld || !simulated)
                return;

            ApplyManipulation(
                editorHorizontal,
                editorVertical);
        }

        private void OnDisable()
        {
            ReleaseBoth();
            colliders.Restore();
        }

        private void OnDestroy()
        {
            ReleaseBoth();
            colliders.Restore();
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(TableControl))]
    public class TableControlEditor : Editor
    {
        private bool showTransforms = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TableControl table = (TableControl)target;

            DrawSimulation(table);
            DrawTesting(table);
            DrawTransforms(table);
            if (Application.isPlaying)
                Repaint();
        }

        private void DrawTransforms(TableControl table)
        {
            EditorGUILayout.Space(10);

            showTransforms = EditorGUILayout.Foldout(
                showTransforms,
                "VR Transform Positions",
                true);

            if (!showTransforms)
                return;

            DrawTransform("Rig Root", table.rigRoot);
            DrawTransform("Head", table.head);
            DrawTransform("Left Hand", table.leftHand);
            DrawTransform("Right Hand", table.rightHand);
        }

        private void DrawTransform(
            string label,
            Transform target)
        {
            if (target == null)
                return;

            EditorGUILayout.LabelField(
                label, EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    target, typeof(Transform), true);

                EditorGUILayout.Vector3Field(
                    "World Position", target.position);

                EditorGUILayout.Vector3Field(
                    "Local Position", target.localPosition);

                EditorGUILayout.Vector3Field(
                    "Rotation", target.eulerAngles);
            }
        }

        private void DrawSimulation(TableControl table)
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Hand Simulation",
                EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            float horizontal = EditorGUILayout.Slider(
                "Horizontal",
                table.editorHorizontal, 0.1f, 5f);

            float vertical = EditorGUILayout.Slider(
                "Vertical",
                table.editorVertical, -5f, 5f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(
                    table, "Simulate VR Hands");

                table.editorHorizontal = horizontal;
                table.editorVertical = vertical;

                table.UpdateEditorHands();

                EditorUtility.SetDirty(table);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("BothHold"))
                table.BothHold();

            if (GUILayout.Button("ReleaseBoth"))
                table.ReleaseBoth();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                "Both Held",
                table.EditorBothHeld ? "TRUE" : "FALSE");
        }

        private void DrawTesting(TableControl table)
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Quick Testing",
                EditorStyles.boldLabel);

            if (GUILayout.Button("Apply Scale"))
                table.SetScale(table.debugScale);

            if (GUILayout.Button("Apply Rotation"))
                table.SetRotation(table.debugRotation);

            if (GUILayout.Button("Apply Position"))
                table.SetPosition(table.debugPosition);

            if (GUILayout.Button("RESET RIG"))
                table.ResetRig();
        }
    }

#endif
}
