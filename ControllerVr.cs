
using UnityEngine;
using UnityEngine.XR;

public class ControllerVr
{
    public XRNode Node { get; private set; }

    private InputDevice device;

    public ControllerVr(XRNode node)
    {
        Node = node;
        RefreshDevice();
    }

    private void RefreshDevice()
    {
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(Node);
    }

    public bool IsConnected
    {
        get
        {
            RefreshDevice();
            return device.isValid;
        }
    }

    // -------------------------
    // BUTTONS
    // -------------------------

    public bool PrimaryButton =>
        GetButton(CommonUsages.primaryButton);

    public bool SecondaryButton =>
        GetButton(CommonUsages.secondaryButton);

    public bool TriggerButton =>
        GetButton(CommonUsages.triggerButton);

    public bool GripButton =>
        GetButton(CommonUsages.gripButton);

    public bool JoystickButton =>
        GetButton(CommonUsages.primary2DAxisClick);

    public bool MenuButton =>
        GetButton(CommonUsages.menuButton);

    // -------------------------
    // ANALOGUE INPUT
    // -------------------------

    public float Trigger =>
        GetFloat(CommonUsages.trigger);

    public float Grip =>
        GetFloat(CommonUsages.grip);

    public Vector2 Joystick =>
        GetVector2(CommonUsages.primary2DAxis);

    // -------------------------
    // PRESSED STATES
    // -------------------------

    public float ButtonThreshold { get; set; } = 0.5f;

    public bool TriggerPressed =>
        TriggerButton || Trigger > ButtonThreshold;

    public bool GripPressed =>
        GripButton || Grip > ButtonThreshold;

    // -------------------------
    // TRACKING
    // -------------------------

    public Vector3 Position =>
        GetVector3(CommonUsages.devicePosition);

    public Quaternion Rotation =>
        GetQuaternion(CommonUsages.deviceRotation);

    public bool IsTracked =>
        GetButton(CommonUsages.isTracked);

    // -------------------------
    // INTERNAL HELPERS
    // -------------------------

    private bool GetButton(InputFeatureUsage<bool> usage)
    {
        RefreshDevice();

        return device.isValid &&
               device.TryGetFeatureValue(
                   usage, out bool value) && value;
    }

    private float GetFloat(InputFeatureUsage<float> usage)
    {
        RefreshDevice();

        return device.isValid &&
               device.TryGetFeatureValue(
                   usage, out float value)
                   ? value : 0f;
    }

    private Vector2 GetVector2(
        InputFeatureUsage<Vector2> usage)
    {
        RefreshDevice();

        return device.isValid &&
               device.TryGetFeatureValue(
                   usage, out Vector2 value)
                   ? value : Vector2.zero;
    }

    private Vector3 GetVector3(
        InputFeatureUsage<Vector3> usage)
    {
        RefreshDevice();

        return device.isValid &&
               device.TryGetFeatureValue(
                   usage, out Vector3 value)
                   ? value : Vector3.zero;
    }

    private Quaternion GetQuaternion(
        InputFeatureUsage<Quaternion> usage)
    {
        RefreshDevice();

        return device.isValid &&
               device.TryGetFeatureValue(
                   usage, out Quaternion value)
                   ? value : Quaternion.identity;
    }
}
