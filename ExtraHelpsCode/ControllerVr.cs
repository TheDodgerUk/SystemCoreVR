
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

public class ControllerVr : IDisposable
{
    // ============================================
    // CONTROLLER
    // ============================================

    public XRNode Node { get; private set; }

    private InputDevice device;

    private HandGameObjectPointer_OnVrHands pointer;
    private Behaviour grabInput;

    private bool originalGrabInputEnabled;
    private bool originalPointerEnabled;
    private bool laserSelectionEnabled;
    private bool disposed;

    public bool IsLaserMode { get; private set; }

    public bool AutoSwitchLaser { get; set; } = true;
    public float ButtonThreshold { get; set; } = 0.5f;

    // ============================================
    // EVENTS
    // ============================================

    public event Action<GameObject> OnLaserClick;
    public event Action<GameObject> OnLaserHover;
    public event Action<bool, GameObject> OnLaserModeChanged;

    // ============================================
    // BUTTON STATES
    // ============================================

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

    // ============================================
    // CONSTRUCTOR
    // ============================================

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

    // ============================================
    // AUTOHAND SETUP
    // ============================================

    public void SetAutoHand(
        Autohand.Hand autoHand,
        HandGameObjectPointer_OnVrHands laserPointer,
        Behaviour grabInputComponent)
    {
        if (disposed)
            return;

        // Remove old listeners and restore old components.
        RestoreAutoHand();

        pointer = laserPointer;
        grabInput = grabInputComponent;

        originalGrabInputEnabled =
            grabInput != null && grabInput.enabled;

        originalPointerEnabled =
            pointer != null && pointer.enabled;

        if (pointer != null)
        {
            pointer.OnLaserClick.AddListener(
                HandlePointerClick);

            pointer.OnLaserHoverEnter.AddListener(
                HandlePointerHover);

            pointer.enabled = false;
        }

        laserSelectionEnabled = false;
        IsLaserMode = false;
    }

    // ============================================
    // LASER MODE
    // ============================================

    public void SetLaserMode(bool enabled)
    {
        if (disposed)
            return;

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

        // When auto-switching is disabled,
        // immediately activate laser mode.
        if (!AutoSwitchLaser)
            ApplyLaserMode(true);
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

        // Disable grabbing while using the laser.
        if (grabInput != null)
        {
            grabInput.enabled =
                enabled ? false : originalGrabInputEnabled;
        }

        if (!enabled &&
            pointer != null &&
            pointer.IsSelecting)
        {
            pointer.Release();
        }

        GameObject target =
            enabled && pointer != null
                ? pointer.currTarget
                : null;

        OnLaserModeChanged?.Invoke(enabled, target);
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

    public bool LaserEnabled => laserSelectionEnabled;

    // ============================================
    // LASER CALLBACKS
    // ============================================

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
        if (IsLaserMode && target != null)
            OnLaserHover?.Invoke(target);
    }

    public void ClearLaserClickCallbacks()
    {
        OnLaserClick = null;
    }

    // ============================================
    // XR DEVICE
    // ============================================

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

    // ============================================
    // BUTTON INPUT
    // ============================================

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

    // ============================================
    // ANALOGUE INPUT
    // ============================================

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

    // ============================================
    // TRACKING
    // ============================================

    public Vector3 Position =>
        GetVector3(CommonUsages.devicePosition);

    public Quaternion Rotation =>
        GetQuaternion(CommonUsages.deviceRotation);

    public bool IsTracked =>
        GetButton(CommonUsages.isTracked);

    // ============================================
    // UPDATE
    // ============================================

    public void Update()
    {
        if (disposed)
            return;

        UpdateButtons();
        UpdateLaser();
    }

    // ============================================
    // BUTTON STATE UPDATE
    // ============================================

    private void UpdateButtons()
    {
        bool primary = PrimaryButton;
        bool secondary = SecondaryButton;
        bool trigger = TriggerPressed;
        bool grip = GripPressed;
        bool joystick = JoystickButton;

        PrimaryButtonDown =
            primary && !previousPrimary;

        SecondaryButtonDown =
            secondary && !previousSecondary;

        TriggerDown =
            trigger && !previousTrigger;

        GripDown =
            grip && !previousGrip;

        JoystickButtonDown =
            joystick && !previousJoystick;

        PrimaryButtonUp =
            !primary && previousPrimary;

        SecondaryButtonUp =
            !secondary && previousSecondary;

        TriggerUp =
            !trigger && previousTrigger;

        GripUp =
            !grip && previousGrip;

        JoystickButtonUp =
            !joystick && previousJoystick;

        previousPrimary = primary;
        previousSecondary = secondary;
        previousTrigger = trigger;
        previousGrip = grip;
        previousJoystick = joystick;
    }

    // ============================================
    // LASER UPDATE
    // ============================================

    private void UpdateLaser()
    {
        if (pointer == null)
            return;

        if (!laserSelectionEnabled)
            return;

        pointer.UpdatePointer();

        bool shouldUseLaser =
            !AutoSwitchLaser ||
            pointer.HasValidTarget();

        // Do not switch away while interacting.
        if (!shouldUseLaser &&
            (TriggerPressed ||
             GripPressed ||
             pointer.IsSelecting))
        {
            shouldUseLaser = IsLaserMode;
        }

        ApplyLaserMode(shouldUseLaser);

        if (!IsLaserMode)
            return;

        // ControllerVr owns trigger-based laser input.
        if (TriggerDown)
            pointer.Press();

        if (TriggerUp && pointer.IsSelecting)
            pointer.Release();
    }

    // ============================================
    // INPUT HELPERS
    // ============================================

    private bool GetButton(
        InputFeatureUsage<bool> usage)
    {
        RefreshDevice();

        return device.isValid &&
               device.TryGetFeatureValue(
                   usage, out bool value) &&
               value;
    }

    private float GetFloat(
        InputFeatureUsage<float> usage)
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

    // ============================================
    // CLEANUP
    // ============================================

    private void RestoreAutoHand()
    {
        if (pointer != null)
        {
            if (pointer.IsSelecting)
                pointer.Release();

            pointer.OnLaserClick.RemoveListener(
                HandlePointerClick);

            pointer.OnLaserHoverEnter.RemoveListener(
                HandlePointerHover);

            pointer.enabled = originalPointerEnabled;
        }

        if (grabInput != null)
            grabInput.enabled = originalGrabInputEnabled;

        pointer = null;
        grabInput = null;
        laserSelectionEnabled = false;
        IsLaserMode = false;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        RestoreAutoHand();

        OnLaserClick = null;
        OnLaserHover = null;
        OnLaserModeChanged = null;

        disposed = true;
    }
}
