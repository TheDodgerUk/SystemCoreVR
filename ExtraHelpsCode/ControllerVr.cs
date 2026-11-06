
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

public class ControllerVr : IDisposable
{
    public XRNode Node { get; private set; }

    private InputDevice device;
    private Autohand.Hand hand;
    private HandGameObjectPointer pointer;
    private Behaviour grabInput;

    private bool originalGrabInputEnabled;
    private bool originalPointerEnabled;
    private bool laserSelectionEnabled;

    public bool IsLaserMode { get; private set; }
    public bool AutoSwitchLaser { get; set; } = true;
    public float ButtonThreshold { get; set; } = 0.5f;

    public event Action<GameObject> OnLaserClick;
    public event Action<GameObject> OnLaserHover;
    public event Action<bool, GameObject> OnLaserModeChanged;

    private bool previousPrimary;
    private bool previousSecondary;
    private bool previousTrigger;
    private bool previousGrip;
    private bool previousJoystick;

    public bool PrimaryButtonDown { get; private set; }
    public bool SecondaryButtonDown { get; private set; }
    public bool TriggerDown { get; private set; }
    public bool GripDown { get; private set; }
    public bool JoystickButtonDown { get; private set; }

    public bool PrimaryButtonUp { get; private set; }
    public bool SecondaryButtonUp { get; private set; }
    public bool TriggerUp { get; private set; }
    public bool GripUp { get; private set; }
    public bool JoystickButtonUp { get; private set; }

    public ControllerVr(XRNode node)
    {
        if (node != XRNode.LeftHand &&
            node != XRNode.RightHand)
        {
            throw new ArgumentException(
                "ControllerVr requires LeftHand or RightHand.");
        }

        Node = node;
        RefreshDevice();
    }

    // -------------------------
    // AUTOHAND SETUP
    // -------------------------

    public void SetAutoHand(
        Autohand.Hand autoHand,
        HandGameObjectPointer laserPointer,
        Behaviour grabInputComponent)
    {
        if (pointer != null)
        {
            pointer.StartSelect.RemoveListener(HandlePointerClick);
            pointer.StartPoint.RemoveListener(HandlePointerHover);
        }

        hand = autoHand;
        pointer = laserPointer;
        grabInput = grabInputComponent;

        originalGrabInputEnabled =
            grabInput != null && grabInput.enabled;

        originalPointerEnabled =
            pointer != null && pointer.enabled;

        if (pointer != null)
        {
            pointer.StartSelect.AddListener(HandlePointerClick);
            pointer.StartPoint.AddListener(HandlePointerHover);
            pointer.enabled = false;
        }

        laserSelectionEnabled = false;
        IsLaserMode = false;
    }

    // -------------------------
    // LASER MODE
    // -------------------------

    public void SetLaserMode(bool enabled)
    {
        laserSelectionEnabled = enabled;

        if (!enabled)
        {
            if (pointer != null && pointer.IsSelecting)
                pointer.Release();

            ApplyLaserMode(false);
            UpdatePointerEnabled();
            return;
        }

        UpdatePointerEnabled();

        if (pointer != null)
            pointer.UpdatePointer();

        bool useLaser =
            !AutoSwitchLaser ||
            (pointer != null && pointer.HasValidTarget());

        ApplyLaserMode(useLaser);
    }

    public void ToggleLaserMode()
    {
        SetLaserMode(!laserSelectionEnabled);
    }

    private void ApplyLaserMode(bool enabled)
    {
        if (IsLaserMode == enabled)
            return;

        IsLaserMode = enabled;

        if (grabInput != null)
        {
            grabInput.enabled =
                enabled ? false : originalGrabInputEnabled;
        }

        if (!enabled && pointer != null && pointer.IsSelecting)
            pointer.Release();

        GameObject target =
            enabled && pointer != null
                ? pointer.currTarget
                : null;

        OnLaserModeChanged?.Invoke(enabled, target);

        Debug.Log(
            $"{Node}: " +
            (enabled
                ? "LASER MODE"
                : "HAND MODE"));
    }

    private void UpdatePointerEnabled()
    {
        if (pointer != null)
            pointer.enabled = laserSelectionEnabled;
    }

    public GameObject LaserTarget =>
        IsLaserMode && pointer != null
            ? pointer.currTarget
            : null;

    // -------------------------
    // POINTER CALLBACKS
    // -------------------------

    private void HandlePointerClick(
        Vector3 position,
        GameObject target)
    {
        if (IsLaserMode && target != null)
            OnLaserClick?.Invoke(target);
    }

    private void HandlePointerHover(
        Vector3 position,
        GameObject target)
    {
        if (IsLaserMode)
            OnLaserHover?.Invoke(target);
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
    // FRAME UPDATE
    // -------------------------

    public void Update()
    {
        bool primary = PrimaryButton;
        bool secondary = SecondaryButton;
        bool trigger = TriggerPressed;
        bool grip = GripPressed;
        bool joystick = JoystickButton;

        PrimaryButtonDown = primary && !previousPrimary;
        SecondaryButtonDown = secondary && !previousSecondary;
        TriggerDown = trigger && !previousTrigger;
        GripDown = grip && !previousGrip;
        JoystickButtonDown = joystick && !previousJoystick;

        PrimaryButtonUp = !primary && previousPrimary;
        SecondaryButtonUp = !secondary && previousSecondary;
        TriggerUp = !trigger && previousTrigger;
        GripUp = !grip && previousGrip;
        JoystickButtonUp = !joystick && previousJoystick;

        previousPrimary = primary;
        previousSecondary = secondary;
        previousTrigger = trigger;
        previousGrip = grip;
        previousJoystick = joystick;

        // Automatic laser detection
        if (laserSelectionEnabled && pointer != null)
        {
            pointer.UpdatePointer();

            bool hasTarget = pointer.HasValidTarget();
            bool shouldUseLaser =
                !AutoSwitchLaser || hasTarget;

            if (shouldUseLaser)
            {
                ApplyLaserMode(true);
            }
            else if (!trigger && !grip)
            {
                ApplyLaserMode(false);
            }
        }
        else if (IsLaserMode && !trigger && !grip)
        {
            ApplyLaserMode(false);
        }

        // Laser click handling
        if (pointer != null && IsLaserMode)
        {
            if (TriggerDown)
                pointer.Press();

            if (TriggerUp)
                pointer.Release();
        }
    }

    // -------------------------
    // INPUT HELPERS
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

    public void ClearLaserClickCallbacks()
    {
        OnLaserClick = null;
    }

    public void Dispose()
    {
        if (pointer != null)
        {
            pointer.StartSelect.RemoveListener(HandlePointerClick);
            pointer.StartPoint.RemoveListener(HandlePointerHover);
            pointer.enabled = originalPointerEnabled;
        }

        if (grabInput != null)
            grabInput.enabled = originalGrabInputEnabled;

        OnLaserClick = null;
        OnLaserHover = null;
        OnLaserModeChanged = null;
    }
}
