
using UnityEngine;

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


            var l = CameraControllerVR.Instance.ExtraControllerInteractionsRef.LeftController;
            var r = CameraControllerVR.Instance.ExtraControllerInteractionsRef.RightController;
            SetControllers(l, r);

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
            if (Player == null || leftHand == null || rightHand == null ||
                left == null || right == null)
            {
                EndManipulation();
                Debug.LogError("Found werid error");
                return;
            }

            bool bothGrips = left.Grip > 0.9f && right.Grip > 0.9f;
            if (!bothGrips)
            {
                EndManipulation();
                return;
            }

            Vector3 separation = rightHand.position - leftHand.position;
            float horizontal = Vector3.ProjectOnPlane(separation, Up).magnitude;
            float vertical = Vector3.Dot(separation, Up);

            if (!manipulating)
                BeginManipulation(horizontal, vertical);

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

            EndManipulation();

            SetScale(1f);
            SetPosition(Vector3.zero);

            Player.rotation = transform.rotation;
        }

        // -----------------------------------------
        // SHARED TWO-HAND MANIPULATION
        // -----------------------------------------

        private void BeginManipulation(
            float horizontal,
            float vertical)
        {
            if (Player == null)
                return;

            EndManipulation();

            manipulating = true;

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
}
