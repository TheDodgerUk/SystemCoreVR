
using UnityEngine;

namespace Autohand
{
    public class HandGameObjectPickupPointer : HandGameObjectPointer
    {
        [Header("Pickup Settings")]
        public bool followRotation = true;
        public bool preserveGrabOffset = true;

        [Header("Debug")]
        [SerializeField] protected GameObject heldObject;
        [SerializeField] protected bool isHolding;

        protected Transform heldTransform;
        protected float grabDistance;
        protected Vector3 grabOffset;
        protected Quaternion rotationOffset;

        protected virtual void LateUpdate()
        {
            if (!isHolding || heldTransform == null)
                return;

            UpdateHeldObject();
        }

        public override void Press()
        {
            base.Press();

            if (heldTransform != null)
                return;

            GameObject target = GetTarget();

            if (target == null)
                return;

            heldObject = target;
            heldTransform = target.transform;
            isHolding = true;

            // Preserve the original distance from the controller.
            grabDistance = Vector3.Distance(
                transform.position,
                lastHit.point);

            Vector3 laserPosition =
                transform.position +
                transform.forward * grabDistance;

            // Preserve the offset between the hit point
            // and the object's pivot.
            grabOffset = preserveGrabOffset
                ? heldTransform.position - laserPosition
                : Vector3.zero;

            // Preserve the object's rotation relative
            // to the controller.
            rotationOffset =
                Quaternion.Inverse(transform.rotation) *
                heldTransform.rotation;
        }

        public override void Release()
        {
            base.Release();

            isHolding = false;
            heldObject = null;
            heldTransform = null;
        }

        protected virtual void UpdateHeldObject()
        {
            Vector3 laserPosition =
                transform.position +
                transform.forward * grabDistance;

            heldTransform.position =
                laserPosition + grabOffset;

            if (followRotation)
            {
                heldTransform.rotation =
                    transform.rotation * rotationOffset;
            }
        }
    }
}
