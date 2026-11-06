
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
    [Serializable]
    public class UnityGameObjectPointerEvent
        : UnityEvent<Vector3, GameObject>
    { }

    [RequireComponent(typeof(LineRenderer))]
    public class HandGameObjectPointer : MonoBehaviour
    {
        [Header("References")]
        public GameObject hitPointMarker;

        [Header("Ray Settings")]
        public float raycastLength = 8f;
        public bool autoShowTarget = true;
        public LayerMask selectableLayers = ~0;

        public QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Collide;

        [Header("Events")]
        public UnityGameObjectPointerEvent StartSelect =
            new UnityGameObjectPointerEvent();

        public UnityGameObjectPointerEvent StopSelect =
            new UnityGameObjectPointerEvent();

        public UnityGameObjectPointerEvent StartPoint =
            new UnityGameObjectPointerEvent();

        public UnityGameObjectPointerEvent StopPoint =
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
                StopPoint?.Invoke(lastHit.point, currTarget);

            currTarget = null;
            lastHit = default(RaycastHit);

            ShowRay(false);
        }

        private void Update()
        {
            UpdatePointer();
        }

        // -------------------------
        // TARGET DETECTION
        // -------------------------

        public bool HasValidTarget()
        {
            if (!isActiveAndEnabled)
                return false;

            return Physics.Raycast(
                transform.position,
                transform.forward,
                raycastLength,
                selectableLayers,
                triggerInteraction);
        }

        public void UpdatePointer()
        {
            Ray ray = new Ray(
                transform.position,
                transform.forward);

            bool hasHit = Physics.Raycast(
                ray,
                out RaycastHit hit,
                raycastLength,
                selectableLayers,
                triggerInteraction);

            GameObject newTarget =
                hasHit ? hit.collider.gameObject : null;

            Vector3 endPosition = hasHit
                ? hit.point
                : ray.GetPoint(raycastLength);

            if (newTarget != currTarget)
            {
                if (currTarget != null)
                    StopPoint?.Invoke(lastHit.point, currTarget);

                currTarget = newTarget;

                if (currTarget != null)
                    StartPoint?.Invoke(hit.point, currTarget);
            }

            lastHit = hit;

            if (autoShowTarget)
                ShowRay(hasHit);

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPosition);

            if (hitPointMarker != null && hasHit)
            {
                hitPointMarker.transform.position = hit.point;

                if (hit.normal.sqrMagnitude > 0f)
                {
                    hitPointMarker.transform.rotation =
                        Quaternion.LookRotation(hit.normal);
                }
            }
        }

        // -------------------------
        // PRESS / RELEASE
        // -------------------------

        public void Press()
        {
            if (IsSelecting)
                return;

            UpdatePointer();

            IsSelecting = true;
            selectedTarget = currTarget;
            selectedPoint = lastHit.point;

            if (selectedTarget != null)
                StartSelect?.Invoke(selectedPoint, selectedTarget);
        }

        public void Release()
        {
            if (!IsSelecting)
                return;

            IsSelecting = false;

            if (selectedTarget != null)
                StopSelect?.Invoke(selectedPoint, selectedTarget);

            selectedTarget = null;
        }

        // -------------------------
        // VISUALS
        // -------------------------

        public void ShowRay(bool show)
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            if (lineRenderer != null)
                lineRenderer.enabled = show;

            if (hitPointMarker != null)
                hitPointMarker.SetActive(show);
        }

        public GameObject GetTarget()
        {
            return currTarget;
        }
    }
}
