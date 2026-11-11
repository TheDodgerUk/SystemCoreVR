
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
    [Serializable]
    public class UnityGameObjectPointerEvent : UnityEvent<Vector3, GameObject>
    {
    }

    // Attach this script to the VR hand.
    [RequireComponent(typeof(LineRenderer))]
    public class HandGameObjectPointer_OnVrHands : MonoBehaviour
    {
        [Header("References")]
        public GameObject hitPointMarker;

        [Header("Ray Settings")]
        public float raycastLength = 8f;
        public bool autoShowTarget = true;
        public LayerMask selectableLayers = ~0;

        public QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Collide;

        [Header("Target Settings")]
        public bool requirePointerComponent = true;

        [Header("Events")]
        public UnityGameObjectPointerEvent OnLaserClick =
            new UnityGameObjectPointerEvent();

        public UnityGameObjectPointerEvent OnLaserRelease =
            new UnityGameObjectPointerEvent();

        public UnityGameObjectPointerEvent OnLaserHoverEnter =
            new UnityGameObjectPointerEvent();

        public UnityGameObjectPointerEvent OnLaserHoverExit =
            new UnityGameObjectPointerEvent();

        public GameObject currTarget { get; private set; }

        public RaycastHit lastHit { get; private set; }

        public bool IsHovering => currTarget != null;

        public bool IsSelecting { get; private set; }

        private LineRenderer lineRenderer;

        private GameObject selectedTarget;
        private Vector3 selectedPoint;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;

            ShowRay(false);
        }

        private void OnEnable()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            ShowRay(false);
        }

        private void OnDisable()
        {
            if (IsSelecting)
                Release();

            if (currTarget != null)
                OnLaserHoverExit?.Invoke(lastHit.point, currTarget);

            currTarget = null;
            lastHit = default(RaycastHit);

            ShowRay(false);
        }

        private void Update()
        {
            UpdatePointer();
        }

        // ----------------------------------
        // TARGET VALIDATION
        // ----------------------------------

        private GameObject FindValidTarget(Collider collider)
        {
            if (collider == null)
                return null;

            GameObject obj = collider.gameObject;

            if (!obj.activeInHierarchy)
                return null;

            if (!requirePointerComponent)
                return obj;

            // Find the pointer component on the object
            // or one of its parents.
            HandGameObjectPointerBase pointer =
                collider.GetComponentInParent<HandGameObjectPointerBase>();

            if (pointer == null || !pointer.isActiveAndEnabled)
                return null;

            return pointer.gameObject;
        }

        public bool HasValidTarget()
        {
            if (!isActiveAndEnabled)
                return false;

            Ray ray = new Ray(
                transform.position,
                transform.forward);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    raycastLength,
                    selectableLayers,
                    triggerInteraction))
            {
                return false;
            }

            return FindValidTarget(hit.collider) != null;
        }

        // ----------------------------------
        // POINTER UPDATE
        // ----------------------------------

        public void UpdatePointer()
        {
            if (lineRenderer == null)
                return;

            Ray ray = new Ray(
                transform.position,
                transform.forward);

            bool hasHit = Physics.Raycast(
                ray,
                out RaycastHit hit,
                raycastLength,
                selectableLayers,
                triggerInteraction);

            GameObject newTarget = hasHit
                ? FindValidTarget(hit.collider)
                : null;

            bool validHit = newTarget != null;

            Vector3 endPosition = hasHit
                ? hit.point
                : ray.GetPoint(raycastLength);

            // Hover changed
            if (newTarget != currTarget)
            {
                if (currTarget != null)
                {
                    OnLaserHoverExit?.Invoke(
                        lastHit.point,
                        currTarget);
                }

                currTarget = newTarget;

                if (currTarget != null)
                {
                    OnLaserHoverEnter?.Invoke(
                        hit.point,
                        currTarget);
                }
            }

            lastHit = hit;

            // Update laser positions
            lineRenderer.SetPosition(
                0,
                transform.position);

            lineRenderer.SetPosition(
                1,
                endPosition);

            // Show laser only for valid targets
            if (autoShowTarget)
                ShowRay(validHit);

            // Update hit marker
            if (hitPointMarker != null && validHit)
            {
                hitPointMarker.transform.position =
                    hit.point;

                if (hit.normal.sqrMagnitude > 0.001f)
                {
                    hitPointMarker.transform.rotation =
                        Quaternion.LookRotation(hit.normal);
                }
            }
        }

        // ----------------------------------
        // PRESS
        // ----------------------------------

        public virtual void Press()
        {
            if (!isActiveAndEnabled || IsSelecting)
                return;

            UpdatePointer();

            if (currTarget == null)
                return;

            IsSelecting = true;

            selectedTarget = currTarget;
            selectedPoint = lastHit.point;

            OnLaserClick?.Invoke(
                selectedPoint,
                selectedTarget);
        }

        // ----------------------------------
        // RELEASE
        // ----------------------------------

        public virtual void Release()
        {
            if (!IsSelecting)
                return;

            IsSelecting = false;

            if (selectedTarget != null)
            {
                OnLaserRelease?.Invoke(
                    selectedPoint,
                    selectedTarget);
            }

            selectedTarget = null;
        }

        // ----------------------------------
        // VISUALS
        // ----------------------------------

        public void ShowRay(bool show)
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            if (lineRenderer != null)
                lineRenderer.enabled = show;

            if (hitPointMarker != null)
                hitPointMarker.SetActive(show);
        }

        // ----------------------------------
        // PUBLIC ACCESS
        // ----------------------------------

        public GameObject GetTarget()
        {
            return currTarget;
        }

        public Vector3 GetHitPoint()
        {
            return lastHit.point;
        }

        public bool IsPointingAt(GameObject obj)
        {
            return currTarget == obj;
        }
    }
}
