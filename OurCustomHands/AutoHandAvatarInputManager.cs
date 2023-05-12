#if VR_INTERACTION
using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.XR;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif



/// this is  stright copy of SampleInputManager aprt from parts below which i will mark


/* This is an example class for how to send input and IK transforms to the sdk from any source
 * InputTrackingDelegate and InputControlDelegate are set on BodyTracking.
 */
public class AutoHandAvatarInputManager : OvrAvatarInputManager
{
    private const string logScope = "sampleInput";

    [SerializeField]
    [Tooltip("Optional. If added, it will use input directly from OVRCameraRig instead of doing its own calculations.")]
    private OVRCameraRig _ovrCameraRig = null;
    private bool _useOvrCameraRig;


    private OurOvrInputTrackingDelegate m_OffsetInputTrackingDelegate;

    public bool m_UseDebugHands = false;


    // Only used in editor, produces warnings when packaging
#pragma warning disable CS0414 // is assigned but its value is never used
    [SerializeField] private bool _debugDrawTrackingLocations = false;
#pragma warning restore CS0414 // is assigned but its value is never used

    protected void Awake()
    {
        _useOvrCameraRig = _ovrCameraRig != null;

        // Debug Drawing
#if UNITY_EDITOR
#if UNITY_2019_3_OR_NEWER
        SceneView.duringSceneGui += OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
#endif
    }

    private void Start()
    {
        // If OVRCameraRig doesn't exist, we should set tracking origin ourselves
        if (!_useOvrCameraRig)
        {
            if (OVRManager.instance == null)
            {
                OvrAvatarLog.LogDebug("Creating OVRManager, as one doesn't exist yet.", logScope, this);
                var go = new GameObject("OVRManager");
                var manager = go.AddComponent<OVRManager>();
                manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }
            else
            {
                OVRManager.instance.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }

            OvrAvatarLog.LogInfo("Setting Tracking Origin to FloorLevel", logScope, this);

            var instances = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(instances);
            foreach (var instance in instances)
            {
                instance.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            }
        }

        if (BodyTracking != null)
        {          
            m_OffsetInputTrackingDelegate = new OurOvrInputTrackingDelegate(_ovrCameraRig, this); //this has been added
            BodyTracking.InputTrackingDelegate = m_OffsetInputTrackingDelegate;
            BodyTracking.InputControlDelegate = new OurOvrInputControlDelegate();
        }
    }




    protected override void OnDestroyCalled()
    {
#if UNITY_EDITOR
#if UNITY_2019_3_OR_NEWER
        SceneView.duringSceneGui -= OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
#endif

        base.OnDestroyCalled();
    }

#if UNITY_EDITOR
#region Debug Drawing

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_debugDrawTrackingLocations)
        {
            DrawTrackingLocations();
        }
    }

    private void DrawTrackingLocations()
    {
        var inputTrackingState = BodyTracking.InputTrackingState;

        float radius = 0.2f;
        Quaternion orientation;
        float outerRadius() => radius + 0.25f;
        Vector3 forward() => orientation * Vector3.forward;

        Handles.color = Color.blue;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.headset.position, radius);

        orientation = inputTrackingState.headset.orientation;
        Handles.DrawLine((Vector3)inputTrackingState.headset.position + forward() * radius,
            (Vector3)inputTrackingState.headset.position + forward() * outerRadius());

        radius = 0.1f;
        Handles.color = Color.yellow;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.leftController.position, radius);

        orientation = inputTrackingState.leftController.orientation;
        Handles.DrawLine((Vector3)inputTrackingState.leftController.position + forward() * radius,
            (Vector3)inputTrackingState.leftController.position + forward() * outerRadius());

        Handles.color = Color.yellow;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.rightController.position, radius);

        orientation = inputTrackingState.rightController.orientation;
        Handles.DrawLine((Vector3)inputTrackingState.rightController.position + forward() * radius,
            (Vector3)inputTrackingState.rightController.position + forward() * outerRadius());
    }

#endregion
#endif // UNITY_EDITOR
}
#endif