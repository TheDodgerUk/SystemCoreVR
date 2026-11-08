
using UnityEngine;

namespace Autohand
{
    public class HandGameObjectPickupPointer : MonoBehaviour
    {
        [Header("Pickup Settings")]
        public bool followRotation = true;
        public bool preserveGrabOffset = true;

        [Header("Debug")]
        [SerializeField] protected bool isHeld;

        protected Transform pointerTransform;
        protected float holdDistance;
        protected Vector3 grabOffset;
        protected Quaternion rotationOffset;

        public bool IsHeld => isHeld;

        // Called when the laser trigger is pressed.
        public virtual void Pickup(
            Transform pointer,
            Vector3 hitPoint)
        {
            if (isHeld || pointer == null)
                return;

            pointerTransform = pointer;

            holdDistance = Vector3.Distance(
                pointer.position,
                hitPoint);

            Vector3 targetPosition =
                pointer.position +
                pointer.forward * holdDistance;

            grabOffset = preserveGrabOffset
                ? transform.position - targetPosition
                : Vector3.zero;

            rotationOffset =
                Quaternion.Inverse(pointer.rotation) *
                transform.rotation;

            isHeld = true;
        }

        // Called while the object is being held.
        public virtual void Move()
        {
            if (!isHeld || pointerTransform == null)
                return;

            Vector3 targetPosition =
                pointerTransform.position +
                pointerTransform.forward * holdDistance;

            transform.position =
                targetPosition + grabOffset;

            if (followRotation)
            {
                transform.rotation =
                    pointerTransform.rotation * rotationOffset;
            }
        }

        // Called when the laser trigger is released.
        public virtual void Release()
        {
            isHeld = false;
            pointerTransform = null;
        }

        protected virtual void LateUpdate()
        {
            Move();
        }

        protected virtual void OnDisable()
        {
            Release();
        }
    }
}
