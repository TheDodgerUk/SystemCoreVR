
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

// ============================================
// VR CONTROLLER SETUP
// ============================================

public class VRControllerSetup : MonoBehaviour
{
    [Serializable]
    public class HandSettings
    {
        public Autohand.Hand hand;
        public HandGameObjectPointer_OnVrHands laser;
        public Behaviour grabInput;
    }

    // ============================================
    // INSPECTOR
    // ============================================

    [Header("Controllers")]
    public HandSettings left = new HandSettings();
    public HandSettings right = new HandSettings();

    [Header("Laser Settings")]
    public float laserDistance = 5f;
    public LayerMask selectableLayers = ~0;
    public bool autoSwitchLaser = true;

    // ============================================
    // PUBLIC CONTROLLERS
    // ============================================

    public ControllerVr LeftController { get; private set; }
    public ControllerVr RightController { get; private set; }
    public ControllerVrGroup BothControllers { get; private set; }

    // ============================================
    // CALLBACKS
    // ============================================

    private Action<GameObject> leftCallback;
    private Action<GameObject> rightCallback;

    // ============================================
    // INITIALISATION
    // ============================================

    private void Awake()
    {
        LeftController = CreateController(
            XRNode.LeftHand,
            left);

        RightController = CreateController(
            XRNode.RightHand,
            right);

        BothControllers = new ControllerVrGroup(
            LeftController,
            RightController);
    }

    private ControllerVr CreateController(
        XRNode node,
        HandSettings settings)
    {
        ControllerVr controller = new ControllerVr(node);

        if (settings.laser != null)
        {
            settings.laser.raycastLength = laserDistance;
            settings.laser.selectableLayers = selectableLayers;
        }

        controller.SetAutoHand(
            settings.hand,
            settings.laser,
            settings.grabInput);

        controller.AutoSwitchLaser = autoSwitchLaser;
        controller.SetLaserMode(true);

        return controller;
    }

    // ============================================
    // UPDATE
    // ============================================

    private void Update()
    {
        LeftController?.Update();
        RightController?.Update();
    }

    // ============================================
    // LASER MODE
    // ============================================

    public void SetLeftLaserMode(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        SetLaserMode(
            LeftController,
            enabled,
            onClick,
            ref leftCallback);
    }

    public void SetRightLaserMode(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        SetLaserMode(
            RightController,
            enabled,
            onClick,
            ref rightCallback);
    }

    public void SetBothLaserModes(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        SetLeftLaserMode(enabled, onClick);
        SetRightLaserMode(enabled, onClick);
    }

    private void SetLaserMode(
        ControllerVr controller,
        bool enabled,
        Action<GameObject> onClick,
        ref Action<GameObject> callback)
    {
        if (controller == null)
            return;

        if (callback != null)
            controller.OnLaserClick -= callback;

        callback = enabled ? onClick : null;

        if (callback != null)
            controller.OnLaserClick += callback;

        controller.SetLaserMode(enabled);
    }

    // ============================================
    // CLEANUP
    // ============================================

    private void OnDestroy()
    {
        Cleanup(LeftController, leftCallback);
        Cleanup(RightController, rightCallback);

        BothControllers = null;
        LeftController = null;
        RightController = null;
    }

    private void Cleanup(
        ControllerVr controller,
        Action<GameObject> callback)
    {
        if (controller == null)
            return;

        if (callback != null)
            controller.OnLaserClick -= callback;

        controller.Dispose();
    }
}

// ============================================
// CONTROLLER GROUP
// ============================================

public class ControllerVrGroup
{
    private readonly ControllerVr left;
    private readonly ControllerVr right;

    public ControllerVr Left => left;
    public ControllerVr Right => right;

    public ControllerVrGroup(
        ControllerVr left,
        ControllerVr right)
    {
        this.left = left;
        this.right = right;
    }

    // ============================================
    // LASER CONTROL
    // ============================================

    public void SetLaserMode(bool enabled)
    {
        left?.SetLaserMode(enabled);
        right?.SetLaserMode(enabled);
    }

    public void ToggleLaserMode()
    {
        left?.ToggleLaserMode();
        right?.ToggleLaserMode();
    }

    public bool IsLaserMode =>
        (left?.IsLaserMode ?? false) ||
        (right?.IsLaserMode ?? false);

    public bool BothLasersActive =>
        (left?.IsLaserMode ?? false) &&
        (right?.IsLaserMode ?? false);

    // ============================================
    // BUTTONS - EITHER CONTROLLER
    // ============================================

    public bool PrimaryButton =>
        (left?.PrimaryButton ?? false) ||
        (right?.PrimaryButton ?? false);

    public bool SecondaryButton =>
        (left?.SecondaryButton ?? false) ||
        (right?.SecondaryButton ?? false);

    public bool TriggerPressed =>
        (left?.TriggerPressed ?? false) ||
        (right?.TriggerPressed ?? false);

    public bool GripPressed =>
        (left?.GripPressed ?? false) ||
        (right?.GripPressed ?? false);

    public bool MenuButton =>
        (left?.MenuButton ?? false) ||
        (right?.MenuButton ?? false);

    // ============================================
    // BUTTON DOWN
    // ============================================

    public bool PrimaryButtonDown =>
        (left?.PrimaryButtonDown ?? false) ||
        (right?.PrimaryButtonDown ?? false);

    public bool SecondaryButtonDown =>
        (left?.SecondaryButtonDown ?? false) ||
        (right?.SecondaryButtonDown ?? false);

    public bool TriggerDown =>
        (left?.TriggerDown ?? false) ||
        (right?.TriggerDown ?? false);

    public bool GripDown =>
        (left?.GripDown ?? false) ||
        (right?.GripDown ?? false);

    public bool JoystickButtonDown =>
        (left?.JoystickButtonDown ?? false) ||
        (right?.JoystickButtonDown ?? false);

    // ============================================
    // BUTTON UP
    // ============================================

    public bool PrimaryButtonUp =>
        (left?.PrimaryButtonUp ?? false) ||
        (right?.PrimaryButtonUp ?? false);

    public bool SecondaryButtonUp =>
        (left?.SecondaryButtonUp ?? false) ||
        (right?.SecondaryButtonUp ?? false);

    public bool TriggerUp =>
        (left?.TriggerUp ?? false) ||
        (right?.TriggerUp ?? false);

    public bool GripUp =>
        (left?.GripUp ?? false) ||
        (right?.GripUp ?? false);

    public bool JoystickButtonUp =>
        (left?.JoystickButtonUp ?? false) ||
        (right?.JoystickButtonUp ?? false);

    // ============================================
    // BOTH CONTROLLERS SIMULTANEOUSLY
    // ============================================

    public bool BothGripsPressed =>
        (left?.GripPressed ?? false) &&
        (right?.GripPressed ?? false);

    public bool BothTriggersPressed =>
        (left?.TriggerPressed ?? false) &&
        (right?.TriggerPressed ?? false);

    public bool BothPrimaryButtonsPressed =>
        (left?.PrimaryButton ?? false) &&
        (right?.PrimaryButton ?? false);

    // ============================================
    // CONNECTION
    // ============================================

    public bool IsConnected =>
        (left?.IsConnected ?? false) ||
        (right?.IsConnected ?? false);

    public bool BothConnected =>
        (left?.IsConnected ?? false) &&
        (right?.IsConnected ?? false);
}
