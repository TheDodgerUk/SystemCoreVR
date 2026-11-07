
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

////AutoHandPlayer
//// └── TrackingContainer
////      ├── LeftHand
////      │    └── VRPalmUpDetector
////      └── RightHand
////           └── VRPalmUpDetector

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
