
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class VRInputDebug : MonoBehaviour
{
    [Header("Debug Intervals")]
    [Min(1)] public int headsetInterval = 10;
    [Min(1)] public int controllerInterval = 10;
    [Min(1)] public int xrStatusInterval = 120;

    [Header("Movement Tolerance")]
    public float positionTolerance = 0.001f;
    public float rotationTolerance = 0.1f;
    public float gripTolerance = 0.05f;

    [Header("Debug Options")]
    public bool debugHeadset = true;
    public bool debugControllers = true;
    public bool debugButtons = true;
    public bool debugXRStatus = true;
    public bool debugCamera = true;

    private class DeviceData
    {
        public bool valid;
        public bool hasPosition;
        public bool hasRotation;
        public bool hasTracking;
        public bool isTracked;

        public Vector3 position;
        public Quaternion rotation;
    }

    private readonly Dictionary<XRNode, DeviceData> previousDevices =
        new Dictionary<XRNode, DeviceData>();

    private readonly Dictionary<string, string> previousValues =
        new Dictionary<string, string>();

    private Camera vrCamera;

    private void Start()
    {
        FindVRCamera();
        CheckCameras();
        CheckXRStatus();
    }

    private void Update()
    {
        if (debugButtons)
            CheckButtons();

        DataOut();

        if (debugXRStatus &&
            Time.frameCount % Mathf.Max(1, xrStatusInterval) == 0)
        {
            CheckXRStatus();
        }

        if (debugCamera &&
            Time.frameCount % Mathf.Max(1, headsetInterval) == 0)
        {
            CheckCamera();
        }
    }

    // --------------------------------------------------
    // LOG ONLY WHEN VALUE CHANGES
    // --------------------------------------------------

    private void LogChanged(string key, object value)
    {
        string current = value != null
            ? value.ToString()
            : "null";

        if (previousValues.TryGetValue(key, out string previous))
        {
            if (previous == current)
                return;

            Debug.Log(
                $"[VR CHANGE] {key}: {previous} -> {current}");
        }
        else
        {
            Debug.Log($"[VR INITIAL] {key}: {current}");
        }

        previousValues[key] = current;
    }

    private void LogChangedPosition(string key, Vector3 position)
    {
        // Store actual Vector3 values rather than rounded strings.
        if (previousPositions.TryGetValue(key, out Vector3 previous))
        {
            if (Vector3.Distance(previous, position) <= positionTolerance)
                return;

            Debug.Log(
                $"[VR POSITION] {key}: " +
                $"{previous.ToString("F4")} -> " +
                $"{position.ToString("F4")}");
        }
        else
        {
            Debug.Log(
                $"[VR INITIAL] {key}: {position.ToString("F4")}");
        }

        previousPositions[key] = position;
    }

    private void LogChangedRotation(string key, Quaternion rotation)
    {
        if (previousRotations.TryGetValue(key, out Quaternion previous))
        {
            if (Quaternion.Angle(previous, rotation) <= rotationTolerance)
                return;

            Debug.Log(
                $"[VR ROTATION] {key}: " +
                $"{previous.eulerAngles.ToString("F2")} -> " +
                $"{rotation.eulerAngles.ToString("F2")}");
        }
        else
        {
            Debug.Log(
                $"[VR INITIAL] {key}: " +
                rotation.eulerAngles.ToString("F2"));
        }

        previousRotations[key] = rotation;
    }

    private readonly Dictionary<string, Vector3> previousPositions =
        new Dictionary<string, Vector3>();

    private readonly Dictionary<string, Quaternion> previousRotations =
        new Dictionary<string, Quaternion>();

    // --------------------------------------------------
    // XR STATUS
    // --------------------------------------------------

    private void CheckXRStatus()
    {
        bool hmdPresent = OVRManager.isHmdPresent;

        bool xrRunning =
            XRGeneralSettings.Instance != null &&
            XRGeneralSettings.Instance.Manager != null &&
            XRGeneralSettings.Instance.Manager.activeLoader != null;

        LogChanged("OVRManager.isHmdPresent", hmdPresent);
        LogChanged("XR Running", xrRunning);
        LogChanged("XR Device Active", XRSettings.isDeviceActive);
        LogChanged("XR Device Name", XRSettings.loadedDeviceName);
        LogChanged("XR Enabled", XRSettings.enabled);
    }

    // --------------------------------------------------
    // CAMERAS
    // --------------------------------------------------

    private void FindVRCamera()
    {
        if (OVRManager.instance != null)
        {
            Camera[] cameras =
                OVRManager.instance.GetComponentsInChildren<Camera>(true);

            foreach (Camera cam in cameras)
            {
                if (cam.name == "CenterEyeAnchor")
                {
                    vrCamera = cam;
                    break;
                }
            }

            if (vrCamera == null && cameras.Length > 0)
                vrCamera = cameras[0];
        }

        if (vrCamera == null)
        {
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.stereoTargetEye != StereoTargetEyeMask.None)
                {
                    vrCamera = cam;
                    break;
                }
            }
        }

        LogChanged(
            "VR Camera Found",
            vrCamera != null ? vrCamera.name : "NONE");
    }

    private void CheckCameras()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            string key = $"Camera [{cam.GetInstanceID()}] {cam.name}";

            LogChanged(key + " Enabled", cam.enabled);
            LogChanged(key + " Active", cam.gameObject.activeInHierarchy);
            LogChanged(key + " Stereo", cam.stereoTargetEye);
        }
    }

    private void CheckCamera()
    {
        if (vrCamera == null)
        {
            FindVRCamera();
            return;
        }

        LogChanged("VR Camera Enabled", vrCamera.enabled);
        LogChanged("VR Camera Active", vrCamera.gameObject.activeInHierarchy);
        LogChanged("VR Camera Stereo", vrCamera.stereoTargetEye);

        LogChangedPosition(
            "CenterEyeAnchor World Position",
            vrCamera.transform.position);

        LogChangedRotation(
            "CenterEyeAnchor World Rotation",
            vrCamera.transform.rotation);

        LogChangedPosition(
            "CenterEyeAnchor Local Position",
            vrCamera.transform.localPosition);

        LogChangedRotation(
            "CenterEyeAnchor Local Rotation",
            vrCamera.transform.localRotation);
    }

    // --------------------------------------------------
    // BUTTONS
    // --------------------------------------------------

    private void CheckButtons()
    {
        LogChanged(
            "A Button",
            OVRInput.Get(OVRInput.Button.One));

        LogChanged(
            "X Button",
            OVRInput.Get(OVRInput.Button.Three));

        float leftGrip =
            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);

        float rightGrip =
            OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);

        CheckGrip("Left Grip", leftGrip);
        CheckGrip("Right Grip", rightGrip);
    }

    private readonly Dictionary<string, float> previousGrips =
        new Dictionary<string, float>();

    private void CheckGrip(string key, float value)
    {
        if (previousGrips.TryGetValue(key, out float previous))
        {
            if (Mathf.Abs(previous - value) <= gripTolerance)
                return;

            Debug.Log(
                $"[VR GRIP] {key}: {previous:F2} -> {value:F2}");
        }
        else
        {
            Debug.Log($"[VR INITIAL] {key}: {value:F2}");
        }

        previousGrips[key] = value;
    }

    // --------------------------------------------------
    // XR DEVICES
    // --------------------------------------------------

    private void DataOut()
    {
        if (debugHeadset &&
            Time.frameCount % Mathf.Max(1, headsetInterval) == 0)
        {
            OutPutDevice(XRNode.Head);
        }

        if (debugControllers &&
            Time.frameCount % Mathf.Max(1, controllerInterval) == 0)
        {
            OutPutDevice(XRNode.LeftHand);
            OutPutDevice(XRNode.RightHand);
        }
    }

    private void OutPutDevice(XRNode node)
    {
        InputDevice device =
            InputDevices.GetDeviceAtXRNode(node);

        DeviceData current = new DeviceData
        {
            valid = device.isValid,
            rotation = Quaternion.identity
        };

        if (device.isValid)
        {
            current.hasPosition = device.TryGetFeatureValue(
                CommonUsages.devicePosition,
                out current.position);

            current.hasRotation = device.TryGetFeatureValue(
                CommonUsages.deviceRotation,
                out current.rotation);

            current.hasTracking = device.TryGetFeatureValue(
                CommonUsages.isTracked,
                out current.isTracked);
        }

        string prefix = $"XR {node}";

        LogChanged(prefix + " Valid", current.valid);
        LogChanged(prefix + " Device Name",
            device.isValid ? device.name : "NONE");

        LogChanged(prefix + " Tracking Available", current.hasTracking);
        LogChanged(prefix + " Is Tracked", current.isTracked);
        LogChanged(prefix + " Position Available", current.hasPosition);
        LogChanged(prefix + " Rotation Available", current.hasRotation);

        if (current.hasPosition)
        {
            LogChangedPosition(
                prefix + " Position",
                current.position);
        }
        else
        {
            previousPositions.Remove(prefix + " Position");
        }

        if (current.hasRotation)
        {
            LogChangedRotation(
                prefix + " Rotation",
                current.rotation);
        }
        else
        {
            previousRotations.Remove(prefix + " Rotation");
        }

        previousDevices[node] = current;
    }

    // --------------------------------------------------
    // APPLICATION FOCUS
    // --------------------------------------------------

    private void OnApplicationFocus(bool hasFocus)
    {
        LogChanged("Application Focus", hasFocus);
    }

    private void OnApplicationPause(bool paused)
    {
        LogChanged("Application Paused", paused);
    }
}
