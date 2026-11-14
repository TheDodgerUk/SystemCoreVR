using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;
using UnityEngine.Events;

// ============================================
// VR CONTROLLER SETUP
// ============================================

public class ExtraControllerInteractions : MonoBehaviour
{
    [Serializable]
    public class HandSettings
    {
        public HandGameObjectPointer_OnVrHands laserGameObject;
    }

    // ============================================
    // INSPECTOR
    // ============================================

    private HandSettings leftHandSettings = new HandSettings();
    private HandSettings rightHandSettings = new HandSettings();

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

    private UnityAction<Vector3, GameObject> leftCallback;
    private UnityAction<Vector3, GameObject> rightCallback;

    // ============================================
    // INITIALISATION
    // ============================================

    private void Awake()
    {
        LeftController = CreateController(
            XRNode.LeftHand,
            leftHandSettings);

        RightController = CreateController(
            XRNode.RightHand,
            rightHandSettings);

        BothControllers = new ControllerVrGroup(
            LeftController,
            RightController);
    }

    private ControllerVr CreateController(
        XRNode node,
        HandSettings settings)
    {
        ControllerVr controller = new ControllerVr(node);

        var cameraController = CameraControllerVR.Instance;
        if (cameraController == null || cameraController.AutoHandPlayerRef == null)
        {
            Debug.LogError("ExtraControllerInteractions: CameraControllerVR or AutoHandPlayerRef is missing.");
            return controller;
        }

        var hand = node == XRNode.LeftHand
            ? cameraController.AutoHandPlayerRef.handLeft
            : cameraController.AutoHandPlayerRef.handRight;

        if (hand == null)
        {
            Debug.LogError($"ExtraControllerInteractions: {node} AutoHand hand is missing.");
            return controller;
        }

        var canvasPointer = hand.GetComponentInChildren<HandCanvasPointer>(true);
        if (canvasPointer == null)
        {
            Debug.LogError($"ExtraControllerInteractions: No HandCanvasPointer found on {node}.");
            return controller;
        }

        // Reuse a pointer if one already exists on this object.
        settings.laserGameObject = canvasPointer.GetComponent<HandGameObjectPointer_OnVrHands>();

        if (settings.laserGameObject == null)
            settings.laserGameObject = canvasPointer.gameObject.AddComponent<HandGameObjectPointer_OnVrHands>();

        if (settings.laserGameObject != null)
        {
            settings.laserGameObject.raycastLength = laserDistance;
            settings.laserGameObject.selectableLayers = selectableLayers;
        }

        // Register AutoHand pointer with ControllerVr.
        controller.SetAutoHand(
            null,
            settings.laserGameObject,
            null);

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
        ref UnityAction<Vector3, GameObject> callback)
    {
        if (controller == null)
            return;

        // Remove the exact delegate previously registered.
        if (callback != null)
            controller.OnLaserClick.RemoveListener(callback);

        callback = null;

        // Adapt the existing one-argument API to the two-argument event.
        if (enabled && onClick != null)
        {
            callback = (point, target) => onClick(target);
            controller.OnLaserClick.AddListener(callback);
        }

        controller.SetLaserMode(enabled);
    }

    // ============================================
    // CLEANUP
    // ============================================

    private void OnDestroy()
    {
        Cleanup(LeftController, leftCallback);
        Cleanup(RightController, rightCallback);

        leftCallback = null;
        rightCallback = null;

        BothControllers = null;
        LeftController = null;
        RightController = null;
    }

    private void Cleanup(
        ControllerVr controller,
        UnityAction<Vector3, GameObject> callback)
    {
        if (controller == null)
            return;

        if (callback != null)
        {
            controller.OnLaserClick.RemoveListener(callback);
        }

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
