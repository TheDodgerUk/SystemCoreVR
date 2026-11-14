/*
 * ============================================================
 * ControllerVr.cs - HOW TO USE
 * ============================================================
 *
 * ControllerVr represents ONE XR controller (left or right).
 * Normally VRControllerSetup creates and updates both controllers.
 *
 * 1. ACCESS CONTROLLERS THROUGH VRControllerSetup
 *
 *    public VRControllerSetup setup;
 *
 *    ControllerVr left  = setup.LeftController;
 *    ControllerVr right = setup.RightController;
 *
 *    // VRControllerSetup also exposes a ControllerVrGroup:
 *    setup.BothControllers.SetLaserMode(true);
 *
 * 2. MANUAL SETUP (ONLY IF NOT USING VRControllerSetup)
 *
 *    ControllerVr controller = new ControllerVr(XRNode.LeftHand);
 *    controller.SetAutoHand(hand, laserPointer, grabInput);
 *    controller.AutoSwitchLaser = true;
 *    controller.SetLaserMode(true);
 *
 *    // Call once per frame from your MonoBehaviour.Update():
 *    controller.Update();
 *
 *    // Call during OnDestroy():
 *    controller.Dispose();
 *
 * 3. LASER MODE
 *
 *    setup.LeftController.SetLaserMode(true);   // Allow laser selection
 *    setup.RightController.SetLaserMode(false); // Disable laser selection
 *    setup.LeftController.ToggleLaserMode();
 *
 *    // With AutoSwitchLaser=true, laser selection activates
 *    // only when the pointer has a valid target.
 *    // When false, enabling laser mode activates it immediately.
 *
 *    bool active = setup.LeftController.IsLaserMode;
 *    GameObject target = setup.LeftController.LaserTarget;
 *
 * 4. BUTTON INPUT (CHECK IN Update() AFTER CONTROLLER UPDATE)
 *
 *    if (setup.LeftController.GripDown) Debug.Log("Grip pressed");
 *    if (setup.LeftController.GripPressed) Debug.Log("Grip held");
 *    if (setup.LeftController.GripUp) Debug.Log("Grip released");
 *    if (setup.RightController.TriggerDown) Debug.Log("Trigger pressed");
 *    if (setup.LeftController.PrimaryButtonDown) Debug.Log("X/A pressed");
 *    if (setup.LeftController.SecondaryButtonDown) Debug.Log("Y/B pressed");
 *
 *    // Down/Up are updated once per controller.Update() call.
 *    // Unity script execution order matters if another Update()
 *    // reads these values before VRControllerSetup.Update().
 *
 * 5. ANALOGUE INPUT AND TRACKING
 *
 *    float grip = setup.LeftController.Grip;       // 0..1
 *    float trigger = setup.LeftController.Trigger; // 0..1
 *    Vector2 stick = setup.LeftController.Joystick;
 *    bool connected = setup.LeftController.IsConnected;
 *    bool tracked = setup.LeftController.IsTracked;
 *    Vector3 position = setup.LeftController.Position;
 *    Quaternion rotation = setup.LeftController.Rotation;
 *
 *    // Position/Rotation are XR device tracking values,
 *    // not necessarily world-space hand Transform values.
 *
 * 6. LASER EVENTS
 *
 *    // Subscribe after VRControllerSetup has initialized:
 *    setup.LeftController.OnLaserClick.AddListener(OnSelected); // (Vector3, GameObject)
 *    setup.LeftController.OnLaserHover.AddListener(OnHovered);
 *    setup.LeftController.OnLaserModeChanged.AddListener(OnModeChanged);
 *
 *    // Matching method signatures:
 *    // void OnSelected(Vector3 point, GameObject target) { }
 *    // void OnHovered(GameObject target) { }
 *    // void OnModeChanged(bool enabled, GameObject target) { }
 *
 *    // Unsubscribe when your listener is destroyed:
 *    setup.LeftController.OnLaserClick.RemoveListener(OnSelected);
 *    setup.LeftController.OnLaserHover.RemoveListener(OnHovered);
 *    setup.LeftController.OnLaserModeChanged.RemoveListener(OnModeChanged);
 *
 *    // Or use VRControllerSetup's callback convenience method:
 *    setup.SetBothLaserModes(true, OnSelected);
 *
 * 7. IMPORTANT
 *
 *    - ControllerVr is a normal C# class, NOT a MonoBehaviour.
 *    - Do not call controller.Update() twice per frame.
 *    - Ensure no other component also calls pointer.Press()/Release()
 *      for the same trigger, or clicks may be duplicated.
 *    - VRControllerSetup owns controller disposal when used.
 *
 * ============================================================
 */

using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
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

    public readonly UnityEvent<Vector3, GameObject> OnLaserClick = new UnityEvent<Vector3, GameObject>();
    public readonly UnityEvent<Vector3, GameObject> OnLaserRelease = new UnityEvent<Vector3, GameObject>();
    public readonly UnityEvent<Vector3, GameObject> OnLaserHoverEnter = new UnityEvent<Vector3, GameObject>();
    public readonly UnityEvent<Vector3, GameObject> OnLaserHoverExit = new UnityEvent<Vector3, GameObject>();
    // Legacy convenience event, retained for existing listeners.
    public readonly UnityEvent<GameObject> OnLaserHover = new UnityEvent<GameObject>();
    public readonly UnityEvent<bool, GameObject> OnLaserModeChanged = new UnityEvent<bool, GameObject>();

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

            pointer.OnLaserRelease.AddListener(HandlePointerRelease);
            pointer.OnLaserHoverEnter.AddListener(HandlePointerHoverEnter);
            pointer.OnLaserHoverExit.AddListener(HandlePointerHoverExit);

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

        OnLaserModeChanged.Invoke(enabled, target);
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

    private void HandlePointerClick(Vector3 position, GameObject target)
    {
        if (IsLaserMode && target != null)
            OnLaserClick.Invoke(position, target);
    }

    private void HandlePointerRelease(Vector3 position, GameObject target)
    {
        // Releases may happen while laser mode is being disabled.
        OnLaserRelease.Invoke(position, target);
    }

    private void HandlePointerHoverEnter(Vector3 position, GameObject target)
    {
        if (!laserSelectionEnabled) return;
        OnLaserHoverEnter.Invoke(position, target);
        if (target != null) OnLaserHover.Invoke(target);
    }

    private void HandlePointerHoverExit(Vector3 position, GameObject target)
    {
        OnLaserHoverExit.Invoke(position, target);
    }

    public void ClearLaserClickCallbacks()
    {
        OnLaserClick.RemoveAllListeners();
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

            pointer.OnLaserRelease.RemoveListener(HandlePointerRelease);
            pointer.OnLaserHoverEnter.RemoveListener(HandlePointerHoverEnter);
            pointer.OnLaserHoverExit.RemoveListener(HandlePointerHoverExit);

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

        OnLaserClick.RemoveAllListeners();
        OnLaserRelease.RemoveAllListeners();
        OnLaserHoverEnter.RemoveAllListeners();
        OnLaserHoverExit.RemoveAllListeners();
        OnLaserHover.RemoveAllListeners();
        OnLaserModeChanged.RemoveAllListeners();

        disposed = true;
    }
}
