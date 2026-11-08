
using UnityEngine;

namespace Autohand
{
    public class HandGameObjectVerticalPickupPointer
        : HandGameObjectPickupPointer
    {
        [Header("Vertical Pickup Settings")]
        public bool allowYRotation = true;

        private float initialYaw;
        private float pointerYawAtPickup;

        public override void Click(
            Transform pointer,
            Vector3 hitPoint,
            GameObject clickedObject)
        {
            if (isPressed || pointer == null)
                return;

            base.Click(pointer, hitPoint, clickedObject);

            initialYaw = transform.eulerAngles.y;
            pointerYawAtPickup = pointer.eulerAngles.y;
        }

        protected override void Move()
        {
            Vector3 targetPosition =
                pointerTransform.position +
                pointerTransform.forward * holdDistance;

            transform.position = targetPosition + grabOffset;

            float yaw = initialYaw;

            if (allowYRotation)
            {
                yaw += Mathf.DeltaAngle(
                    pointerYawAtPickup,
                    pointerTransform.eulerAngles.y);
            }

            transform.rotation =
                Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
