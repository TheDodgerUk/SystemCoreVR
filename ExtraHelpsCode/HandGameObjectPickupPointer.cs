
using UnityEngine;

namespace Autohand
{
    public class HandGameObjectPickupPointer
        : HandGameObjectPointerBase
    {
        [Header("Pickup Settings")]
        public bool followRotation = true;
        public bool preserveGrabOffset = true;

        protected Transform pointerTransform;
        protected float holdDistance;
        protected Vector3 grabOffset;
        protected Quaternion rotationOffset;

        public override void Click(
            Transform pointer,
            Vector3 hitPoint,
            GameObject clickedObject)
        {
            if (isPressed || pointer == null)
                return;

            base.Click(pointer, hitPoint, clickedObject);

            pointerTransform = pointer;

            holdDistance = Vector3.Distance(
                pointer.position, hitPoint);

            Vector3 targetPosition =
                pointer.position +
                pointer.forward * holdDistance;

            grabOffset = preserveGrabOffset
                ? transform.position - targetPosition
                : Vector3.zero;

            rotationOffset =
                Quaternion.Inverse(pointer.rotation) *
                transform.rotation;
        }

        protected virtual void LateUpdate()
        {
            if (!isPressed || pointerTransform == null)
                return;

            Move();
        }

        protected virtual void Move()
        {
            Vector3 targetPosition =
                pointerTransform.position +
                pointerTransform.forward * holdDistance;

            transform.position = targetPosition + grabOffset;

            if (followRotation)
                transform.rotation =
                    pointerTransform.rotation * rotationOffset;
        }

        public override void Release(GameObject clickedObject)
        {
            base.Release(clickedObject);
            pointerTransform = null;
        }
    }
}
