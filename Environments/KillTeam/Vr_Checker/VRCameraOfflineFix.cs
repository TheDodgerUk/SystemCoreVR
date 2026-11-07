
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VRCameraOfflineFix : MonoBehaviour
{
    [Header("VR Camera")]
    public Camera vrCamera;

    [Header("Offline Camera Settings")]
    public Vector3 offlineLocalPosition = Vector3.zero;
    public Vector3 offlineLocalRotation = Vector3.zero;

    [Header("Settings")]
    public bool autoFix = true;
    public bool requireTracking = false;
    public bool logStatusChanges = true;

    [Header("Debug")]
    public bool vrConnected;
    public bool vrTracking;
    public bool xrRunning;
    public bool vrReady;

    // Previous VR states used to detect changes.
    private bool previousVRConnected;
    private bool previousVRTracking;
    private bool previousXRRunning;
    private bool previousVRReady;

    // Ensures the initial status is logged once.
    private bool statusInitialized;

    private void LateUpdate()
    {
        UpdateVRStatus();

        if (!autoFix || vrCamera == null)
            return;

        if (!vrReady)
            ResetCameraOffsets();
    }

    // Check whether a headset is connected.
    public static bool IsVRConnected()
    {
        InputDevice headset =
            InputDevices.GetDeviceAtXRNode(XRNode.Head);

        return headset.isValid;
    }

    // Check whether the headset is tracking.
    public static bool IsVRTracking()
    {
        InputDevice headset =
            InputDevices.GetDeviceAtXRNode(XRNode.Head);

        return headset.isValid &&
               headset.TryGetFeatureValue(
                   CommonUsages.isTracked,
                   out bool tracked) &&
               tracked;
    }

    // Check whether XR has an active loader.
    public static bool IsXRRunning()
    {
        return XRGeneralSettings.Instance != null &&
               XRGeneralSettings.Instance.Manager != null &&
               XRGeneralSettings.Instance.Manager.activeLoader != null;
    }

    // Update VR status and detect changes.
    public void UpdateVRStatus()
    {
        vrConnected = IsVRConnected();
        vrTracking = IsVRTracking();
        xrRunning = IsXRRunning();

        vrReady = xrRunning &&
                  vrConnected &&
                  (!requireTracking || vrTracking);

        bool statusChanged =
            !statusInitialized ||
            vrConnected != previousVRConnected ||
            vrTracking != previousVRTracking ||
            xrRunning != previousXRRunning ||
            vrReady != previousVRReady;

        if (statusChanged)
        {
            if (logStatusChanges)
                PrintVRStatus();

            previousVRConnected = vrConnected;
            previousVRTracking = vrTracking;
            previousXRRunning = xrRunning;
            previousVRReady = vrReady;

            statusInitialized = true;
        }
    }

    // Reset unwanted camera offsets.
    public void ResetCameraOffsets()
    {
        if (vrCamera == null)
            return;

        Transform cam = vrCamera.transform;

        cam.localPosition = offlineLocalPosition;
        cam.localRotation =
            Quaternion.Euler(offlineLocalRotation);
    }

    // Print current values without updating them.
    private void PrintVRStatus()
    {
        Debug.Log(
            $"VR Status Changed:\n" +
            $"VR Connected: {vrConnected}\n" +
            $"VR Tracking: {vrTracking}\n" +
            $"XR Running: {xrRunning}\n" +
            $"VR Ready: {vrReady}\n" +
            $"Camera Local Position: " +
            $"{vrCamera?.transform.localPosition}\n" +
            $"Camera World Position: " +
            $"{vrCamera?.transform.position}"
        );
    }

    // Manual status logging.
    public void LogVRStatus()
    {
        UpdateVRStatus();
        PrintVRStatus();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(VRCameraOfflineFix))]
public class VRCameraOfflineFixEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VRCameraOfflineFix script =
            (VRCameraOfflineFix)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Reset Camera Offsets"))
        {
            script.ResetCameraOffsets();
        }

        if (GUILayout.Button("Check VR Status"))
        {
            script.LogVRStatus();
        }
    }
}
#endif
