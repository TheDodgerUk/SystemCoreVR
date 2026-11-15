
using UnityEngine;
using VLB_Samples;

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
        public float rotationSpeed = 90f;

        private ControllerVr left;
        private ControllerVr right;

        private readonly ColliderStorage colliders =
            new ColliderStorage();

        private bool manipulating;

        private float startScale;
        private float startHorizontal;
        private float startVertical;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private CameraControllerVR Controller => CameraControllerVR.Instance;

        private Transform AutoHandTransform => Controller.AutoHandPlayerRef.transform;

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

            rigRoot = Controller.AutoHandPlayerRef.transform;

            if (Controller.AutoHandPlayerRef.handLeft != null)
            {
                leftHand =
                    Controller.AutoHandPlayerRef.handLeft.transform;
            }

            if (Controller.AutoHandPlayerRef.handRight != null)
            {
                rightHand =
                    Controller.AutoHandPlayerRef.handRight.transform;
            }

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
            Transform rig = CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer;

            scale = Mathf.Clamp(
                scale,
                minScale,
                maxScale);
            rig.localScale = Vector3.one * scale;
        }

        public void SetRotation(float degrees)
        {
            var player = CameraControllerVR.Instance.AutoHandPlayerRef;

            if (player == null)
                return;

            Transform rig = player.trackingContainer;

            if (rig == null)
                return;

            Quaternion rotation =
                Quaternion.AngleAxis(degrees, Vector3.up);

            // Orbit player around world origin.
            Vector3 position =
                rotation * player.transform.position;

            // Move the player using AutoHand.
            player.SetPosition(position, player.transform.rotation);

            // Rotate the VR tracking space.
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

            startScale = AutoHandTransform.lossyScale.x;
            startPosition = AutoHandTransform.position;
            startRotation = AutoHandTransform.rotation;

            colliders.DisableAll();
        }

        private void ApplyManipulation(
            float horizontal,
            float vertical)
        {
            if (!manipulating || AutoHandTransform == null)
                return;

            float horizontalChange =
                horizontal - startHorizontal;

            float verticalChange =
                vertical - startVertical;

            float scale =
                startScale *
                (1f + horizontalChange * scaleSensitivity);

            float angle =
                verticalChange * rotationSpeed;

            // Apply scaling using the shared method.
            SetScale(scale);

            // Restore the starting transform so rotation
            // does not accumulate every frame.
            AutoHandTransform.SetPositionAndRotation(
                startPosition,
                startRotation);

            // Apply rotation using the shared method.
            SetRotation(angle);
        }

        // -----------------------------------------
        // MANIPULATION CLEANUP
        // -----------------------------------------

        private void EndManipulation()
        {
            if (!manipulating)
                return;

            manipulating = false;

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
