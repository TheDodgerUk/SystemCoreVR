
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
    [Serializable]
    public class PointerObjectEvent
        : UnityEvent<GameObject>
    { }

    [Serializable]
    public class PointerHitEvent
        : UnityEvent<Vector3, GameObject>
    { }

    public abstract class HandGameObjectPointerBase : MonoBehaviour
    {
        [Header("Pointer Events")]
        public PointerObjectEvent OnClick = new PointerObjectEvent();
        public PointerObjectEvent OnRelease = new PointerObjectEvent();

        [Header("Debug")]
        [SerializeField] protected bool isPressed;

        public bool IsPressed => isPressed;

        public virtual void Click(
            Transform pointer,
            Vector3 hitPoint,
            GameObject clickedObject)
        {
            if (isPressed)
                return;

            isPressed = true;

            OnClick.Invoke(clickedObject);
        }

        public virtual void Release(
            GameObject clickedObject)
        {
            if (!isPressed)
                return;

            isPressed = false;

            OnRelease.Invoke(clickedObject);
        }

        protected virtual void OnDisable()
        {
            isPressed = false;
        }
    }
}
