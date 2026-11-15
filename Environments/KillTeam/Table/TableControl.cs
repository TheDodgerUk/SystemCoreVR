
using UnityEngine;

namespace KillTeam
{
    public class TableControl : MonoBehaviour
    {
        [Header("VR Debug")]
        public float debug_scale = 1.0f;
        public float debug_rotation = 45f;

        [Header("VR References")]
        public Transform rigRoot;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        [Header("Settings")]
        public float minScale = 0.2f;
        public float maxScale = 5f;
        public float scaleSensitivity = 1f;
        public float rotationSpeed = -190f;

        private ControllerVr left;
        private ControllerVr right;

        private readonly ColliderStorage colliders =
            new ColliderStorage();

        private bool manipulating;

        private float startScale;
        private float startHorizontal;
        private float startVertical;
        private float lastAngle;

        private CameraControllerVR Controller =>
            CameraControllerVR.Instance;

        private Transform AutoHandTransform =>
            Controller != null &&
            Controller.AutoHandPlayerRef != null
                ? Controller.AutoHandPlayerRef.transform
                : null;

        private Vector3 Up => transform.up;

        // -----------------------------------------
        // INITIALISATION
        // -----------------------------------------

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

            var player = Controller.AutoHandPlayerRef;

            rigRoot = player.transform;

            if (player.handLeft != null)
                leftHand = player.handLeft.transform;

            if (player.handRight != null)
                rightHand = player.handRight.transform;

            if (head == null && Camera.main != null)
                head = Camera.main.transform;

            colliders.Store(gameObject);

            var interactions =
                Controller.ExtraControllerInteractionsRef;

            if (interactions != null)
            {
                SetControllers(
                    interactions.LeftController,
                    interactions.RightController);
            }
        }

        // -----------------------------------------
        // CONTROLLER SETUP
        // -----------------------------------------

        public void SetControllers(
            ControllerVr leftController,
            ControllerVr rightController)
        {
            left = leftController;
            right = rightController;
        }

        // -----------------------------------------
        // UPDATE
        // -----------------------------------------

        private void Update()
        {
            if (AutoHandTransform == null ||
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
            {
                BeginManipulation(
                    horizontal,
                    vertical);
            }

            ApplyManipulation(
                horizontal,
                vertical);
        }

        // -----------------------------------------
        // PLAYER TRANSFORMS
        // -----------------------------------------

        public void SetScale(float scale)
        {
            if (Controller == null ||
                Controller.AutoHandPlayerRef == null)
                return;

            Transform rig =
                Controller.AutoHandPlayerRef.trackingContainer;

            if (rig == null)
                return;

            scale = Mathf.Clamp(
                scale,
                minScale,
                maxScale);

            rig.localScale = Vector3.one * scale;
        }

        public void SetRotation(float degrees)
        {
            if (Controller == null ||
                Controller.AutoHandPlayerRef == null)
                return;

            var player = Controller.AutoHandPlayerRef;

            Transform rig = player.trackingContainer;

            if (rig == null)
                return;

            if (Mathf.Abs(degrees) < 0.0001f)
                return;

            Quaternion rotation =
                Quaternion.AngleAxis(
                    degrees,
                    Vector3.up);

            // Orbit player around world origin.
            Vector3 position =
                rotation * player.transform.position;

            // Move player through AutoHand.
            player.SetPosition(
                position,
                player.transform.rotation);

            // Rotate tracking space incrementally.
            rig.rotation = rotation * rig.rotation;
        }

        // -----------------------------------------
        // DEBUG METHODS
        // -----------------------------------------

        [ContextMenu("Debug Set Scale")]
        public void DebugSetScale()
        {
            SetScale(debug_scale);
        }

        [ContextMenu("Debug Set Rotation")]
        public void DebugSetRotation()
        {
            SetRotation(debug_rotation);
        }

        // -----------------------------------------
        // TWO-HAND MANIPULATION
        // -----------------------------------------

        private void BeginManipulation(
            float horizontal,
            float vertical)
        {
            if (AutoHandTransform == null)
                return;

            EndManipulation();

            manipulating = true;

            startHorizontal = horizontal;
            startVertical = vertical;

            Transform rig =
                Controller.AutoHandPlayerRef.trackingContainer;

            startScale = rig != null
                ? rig.localScale.x
                : 1f;

            // Reset only the gesture's accumulated angle.
            // Do not reset the actual player rotation.
            lastAngle = 0f;

            colliders.DisableAll();
        }

        private void ApplyManipulation(
            float horizontal,
            float vertical)
        {
            if (!manipulating ||
                AutoHandTransform == null)
                return;

            float horizontalChange =
                horizontal - startHorizontal;

            float verticalChange =
                vertical - startVertical;

            // Scale relative to the start of this gesture.
            float scale =
                startScale +
                (horizontalChange * scaleSensitivity);

            SetScale(scale);

            // Total angle requested by this gesture.
            float angle =
                verticalChange * rotationSpeed;

            // Apply only the change since the last frame.
            float deltaAngle =
                angle - lastAngle;

            if (Mathf.Abs(deltaAngle) > 0.001f)
            {
                SetRotation(deltaAngle);
            }

            // Store the last applied angle.
            lastAngle = angle;
        }

        // -----------------------------------------
        // MANIPULATION CLEANUP
        // -----------------------------------------

        private void EndManipulation()
        {
            if (!manipulating)
                return;

            manipulating = false;

            lastAngle = 0f;

            colliders.Restore();
        }

        private void OnDisable()
        {
            EndManipulation();
            colliders.Restore();
        }

        private void OnDestroy()
        {
            EndManipulation();
            colliders.Restore();
        }
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(TableControl))]
    public class TableControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TableControl table =
                (TableControl)target;

            UnityEditor.EditorGUILayout.Space(10);

            UnityEditor.EditorGUILayout.LabelField(
                "VR Debug Controls",
                UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Debug Set Scale"))
            {
                table.DebugSetScale();
            }

            if (GUILayout.Button("Debug Set Rotation"))
            {
                table.DebugSetRotation();
            }
        }
    }

#endif
}
