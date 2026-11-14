
/*
 * ============================================================
 * VR PALM UP DETECTOR - HOW TO USE
 * ============================================================
 *
 * PURPOSE:
 * Detects when a VR player's palm faces upwards or downwards.
 * Works with AutoHand, Quest 2, Rift S and Unity XR.
 *
 * ------------------------------------------------------------
 * 1. SETUP IN UNITY
 * ------------------------------------------------------------
 *
 * Add VRPalmUpDetector to each hand.
 *
 * AutoHandPlayer
 *  └── TrackingContainer
 *       ├── LeftHand
 *       │    └── VRPalmUpDetector
 *       └── RightHand
 *            └── VRPalmUpDetector
 *
 * Inspector:
 *
 * Palm              = Hand Transform (or palm Transform)
 * Palm Up Axis      = (0, 1, 0)
 * Palm Up Threshold = 0.7
 * Palm Exit Threshold = 0.5
 *
 * IMPORTANT:
 * If detection is backwards, change Palm Up Axis.
 *
 * Examples:
 *   (0, 1, 0)  = Local Y+
 *   (0,-1, 0)  = Local Y-
 *   (0, 0, 1)  = Local Z+
 *   (0, 0,-1)  = Local Z-
 *
 * ------------------------------------------------------------
 * 2. USING UNITY INSPECTOR EVENTS
 * ------------------------------------------------------------
 *
 * OnPalmUp:
 *   Called ONCE when the palm turns upwards.
 *
 * OnPalmDown:
 *   Called ONCE when the palm stops facing upwards.
 *
 * Drag a GameObject into the event and select its method.
 *
 * ------------------------------------------------------------
 * 3. USING C# EVENTS
 * ------------------------------------------------------------
 *
 * public VRPalmUpDetector leftPalm;
 *
 * private void Start()
 * {
 *     leftPalm.OnPalmUp.AddListener(OnPalmUp);
 *     leftPalm.OnPalmDown.AddListener(OnPalmDown);
 * }
 *
 * private void OnDestroy()
 * {
 *     leftPalm.OnPalmUp.RemoveListener(OnPalmUp);
 *     leftPalm.OnPalmDown.RemoveListener(OnPalmDown);
 * }
 *
 * private void OnPalmUp()
 * {
 *     Debug.Log("Palm facing UP");
 * }
 *
 * private void OnPalmDown()
 * {
 *     Debug.Log("Palm facing DOWN");
 * }
 *
 * ------------------------------------------------------------
 * 4. CHECK CURRENT STATE
 * ------------------------------------------------------------
 *
 * if (leftPalm.IsPalmUp)
 * {
 *     Debug.Log("Palm is currently facing upwards");
 * }
 *
 * ------------------------------------------------------------
 * 5. MANUAL CHECK
 * ------------------------------------------------------------
 *
 * leftPalm.ForceCheck();
 *
 * Or press "Check Palm" in the Inspector.
 *
 * ------------------------------------------------------------
 * 6. DEBUGGING
 * ------------------------------------------------------------
 *
 * Show Debug Ray = true
 *
 * Green = Palm facing up
 * Red   = Palm not facing up
 *
 * The Inspector also displays the current palm state.
 *
 * ============================================================
 */

using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VRPalmUpDetector : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform representing the palm of the hand.")]
    public Transform palm;

    [Header("Palm Detection")]
    [Tooltip("Local axis pointing out of the palm.")]
    public Vector3 palmUpAxis = Vector3.up;

    [Range(0f, 1f)]
    [Tooltip("How closely the palm must face upwards.")]
    public float palmUpThreshold = 0.7f;

    [Range(0f, 1f)]
    [Tooltip("Threshold for leaving the palm-up state.")]
    public float palmExitThreshold = 0.5f;

    [Header("Events")]
    public UnityEvent OnPalmUp = new UnityEvent();
    public UnityEvent OnPalmDown = new UnityEvent();

    private bool isPalmUpLast = false;

    [Header("Debug")]
    [SerializeField] private bool isPalmUp;
    [SerializeField] private float palmDot;

    public bool showDebugRay = true;

    public bool IsPalmUp => isPalmUp;

    private void Awake()
    {
        if (palm == null)
            palm = transform;
    }

    private void Update()
    {
        CheckPalm();
    }

    private void CheckPalm()
    {
        if (palm == null)
            return;

        // Convert the palm's local direction to world space.
        Vector3 worldPalmDirection =
            palm.TransformDirection(palmUpAxis.normalized);

        // Compare against world up.
        palmDot = Vector3.Dot(
            worldPalmDirection,
            Vector3.up);

        // Enter palm-up state.
        if (!isPalmUp && palmDot >= palmUpThreshold)
        {
            isPalmUp = true;
            OnPalmUp.Invoke();
        }

        // Leave palm-up state.
        else if (isPalmUp && palmDot <= palmExitThreshold)
        {
            isPalmUp = false;
            OnPalmDown.Invoke();
        }

        if (showDebugRay)
        {
            Debug.DrawRay(
                palm.position,
                worldPalmDirection * 0.2f,
                isPalmUp ? Color.green : Color.red);
        }

        if (isPalmUpLast != isPalmUp)
        {
            Debug.Log($"isPalmUp {isPalmUp}");
            isPalmUpLast = isPalmUp;
        }
    }

    public void ForceCheck()
    {
        CheckPalm();
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(VRPalmUpDetector))]
    private class VRPalmUpDetectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (VRPalmUpDetector)target;

            GUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Palm Debug",
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "Palm Up",
                script.IsPalmUp ? "YES" : "NO");

            if (GUILayout.Button("Check Palm"))
            {
                script.ForceCheck();
            }
        }
    }

#endif
}
