
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

        public override void Pickup(
            Transform pointer,
            Vector3 hitPoint)
        {
            if (isHeld || pointer == null)
                return;

            base.Pickup(pointer, hitPoint);

            initialYaw = transform.eulerAngles.y;
            pointerYawAtPickup = pointer.eulerAngles.y;
        }

        public override void Move()
        {
            if (!isHeld || pointerTransform == null)
                return;

            Vector3 targetPosition =
                pointerTransform.position +
                pointerTransform.forward * holdDistance;

            transform.position =
                targetPosition + grabOffset;

            // Keep the object upright.
            float yaw = initialYaw;

            if (allowYRotation)
            {
                float deltaYaw = Mathf.DeltaAngle(
                    pointerYawAtPickup,
                    pointerTransform.eulerAngles.y);

                yaw += deltaYaw;
            }

            transform.rotation =
                Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
