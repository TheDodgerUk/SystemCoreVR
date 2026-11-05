using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class VRInputDebug : MonoBehaviour
{
    private void Start()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            Debug.LogError(
                $"Camera: {cam.name} | " +
                $"Enabled: {cam.enabled} | " +
                $"Active: {cam.gameObject.activeInHierarchy} | " +
                $"Stereo: {cam.stereoTargetEye}");
        }



        bool hmdPresent = OVRManager.isHmdPresent;

        bool xrRunning =
            XRGeneralSettings.Instance != null &&
            XRGeneralSettings.Instance.Manager != null &&
            XRGeneralSettings.Instance.Manager.activeLoader != null;

        bool xrDeviceActive = XRSettings.isDeviceActive;

        Debug.LogError(
            $"VR Debug:\n" +
            $"OVRManager.isHmdPresent: {hmdPresent}\n" +
            $"XR Running: {xrRunning}\n" +
            $"XR Device Active: {xrDeviceActive}\n" +
            $"XR Device Name: {XRSettings.loadedDeviceName}\n" +
            $"XR Enabled: {XRSettings.enabled}");
    }


    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
            Debug.Log("A button pressed");

        if (OVRInput.GetDown(OVRInput.Button.Three))
            Debug.Log("X button pressed");

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5f)
            Debug.Log("Left grip pressed");

        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5f)
            Debug.Log("Right grip pressed");

        DataOut();
    }


    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.LogError($"VR Application Focus: {hasFocus}");
    }

    private void OnApplicationPause(bool paused)
    {
        Debug.LogError($"VR Application Paused: {paused}");
    }


    private void DataOut()
    {
        if (Time.frameCount % 120 != 0)
            return;

        InputDevice headset =
            InputDevices.GetDeviceAtXRNode(XRNode.Head);

        bool hasPosition = headset.TryGetFeatureValue(
            CommonUsages.devicePosition,
            out Vector3 position);

        bool hasRotation = headset.TryGetFeatureValue(
            CommonUsages.deviceRotation,
            out Quaternion rotation);

        bool hasTracking = headset.TryGetFeatureValue(
            CommonUsages.isTracked,
            out bool isTracked);

        Debug.Log(
            $"Device: {headset.name}\n" +
            $"Valid: {headset.isValid}\n" +
            $"Tracking Available: {hasTracking}\n" +
            $"Is Tracked: {isTracked}\n" +
            $"Position Available: {hasPosition}\n" +
            $"Rotation Available: {hasRotation}\n" +
            $"Position: {position}\n" +
            $"Rotation: {rotation.eulerAngles}\n" +
            $"CenterEyeAnchor Position: " +
            $"{OVRManager.instance?.GetComponentInChildren<Camera>()?.transform.position}"
        );

        Debug.LogError(
            $"OVR HMD Present: {OVRManager.isHmdPresent}\n" +
            $"Unity Focus: {Application.isFocused}\n" +
            $"OVR Input Focus: {OVRManager.hasInputFocus}\n" +
            $"OVR VR Focus: {OVRManager.hasVrFocus}");
            }
}