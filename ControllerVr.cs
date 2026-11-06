
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

public class ControllerVr
{
    public XRNode Node { get; private set; }

    private InputDevice device;

    // -------------------------
    // AUTOHAND
    // -------------------------

    private Autohand.Hand hand;
    private HandDistanceGrabber laserPointer;
    private Behaviour grabInputComponent;

    private bool originalGrabInputEnabled;
    private bool originalLaserEnabled;

    public bool IsLaserMode { get; private set; }

    public void SetAutoHand(
        Autohand.Hand autoHand,
        HandDistanceGrabber distanceGrabber,
        Behaviour grabInput)
    {
        hand = autoHand;
        laserPointer = distanceGrabber;
        grabInputComponent = grabInput;

        originalGrabInputEnabled =
            grabInputComponent != null &&
            grabInputComponent.enabled;

        originalLaserEnabled =
            laserPointer != null &&
            laserPointer.enabled;

        SetLaserMode(false);
    }

    public void SetLaserMode(bool enabled)
    {
        IsLaserMode = enabled;

        // Disable normal grabbing input
        if (grabInputComponent != null)
        {
            grabInputComponent.enabled =
                enabled ? false : originalGrabInputEnabled;
        }

        // Enable AutoHand distance laser
        if (laserPointer != null)
            laserPointer.enabled = enabled;

        previousLaserTrigger = TriggerPressed;

        if (previousHoverObject != null)
        {
            previousHoverObject = null;
            OnLaserHover?.Invoke(null);
        }

        Debug.Log(
            $"{Node}: " +
            (enabled
                ? "Laser ON - Normal grabbing OFF"
                : "Laser OFF - Normal grabbing ON"));
    }

    public void ToggleLaserMode()
    {
        SetLaserMode(!IsLaserMode);
    }

    // -------------------------
    // LASER CALLBACKS
    // -------------------------

    public event Action<GameObject> OnLaserClick;
    public event Action<GameObject> OnLaserHover;

    public Transform LaserOrigin { get; set; }

    public float LaserDistance { get; set; } = 5f;

    public LayerMask LaserLayers { get; set; } = ~0;

    private bool previousLaserTrigger;
    private GameObject previousHoverObject;

    public GameObject LaserTarget
    {
        get
        {
            return TryGetLaserHit(out RaycastHit hit)
                ? hit.collider.gameObject
                : null;
        }
    }

    public bool TryGetLaserHit(out RaycastHit hit)
    {
        hit = default(RaycastHit);

        if (!IsLaserMode || LaserOrigin == null)
            return false;

        return Physics.Raycast(
            LaserOrigin.position,
            LaserOrigin.forward,
            out hit,
            LaserDistance,
            LaserLayers,
            QueryTriggerInteraction.Collide);
    }

    public void UpdateLaser()
    {
        if (!IsLaserMode)
            return;

        GameObject target = LaserTarget;

        // Hover callback
        if (target != previousHoverObject)
        {
            previousHoverObject = target;
            OnLaserHover?.Invoke(target);
        }

        // Click callback
        bool trigger = TriggerPressed;

        if (trigger && !previousLaserTrigger)
        {
            if (target != null)
                OnLaserClick?.Invoke(target);
        }

        previousLaserTrigger = trigger;
    }

    // -------------------------
    // CONSTRUCTOR
    // -------------------------

    public ControllerVr(XRNode node)
    {
        Node = node;
        RefreshDevice();
    }

    // -------------------------
    // XR DEVICE
    // -------------------------

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

    // -------------------------
    // CLEANUP
    // -------------------------

    public void Dispose()
    {
        if (grabInputComponent != null)
            grabInputComponent.enabled =
                originalGrabInputEnabled;

        if (laserPointer != null)
            laserPointer.enabled =
                originalLaserEnabled;

        OnLaserClick = null;
        OnLaserHover = null;
    }
}
