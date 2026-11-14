
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

public class VRControllerSetup : MonoBehaviour
{
    [Serializable]
    public class HandSettings
    {
        public Autohand.Hand hand;
        public HandGameObjectPointer_OnVrHands laser;
        public Behaviour grabInput;
    }

    [Header("Controllers")]
    public HandSettings left = new HandSettings();
    public HandSettings right = new HandSettings();

    [Header("Laser Settings")]
    public float laserDistance = 5f;
    public LayerMask selectableLayers = ~0;
    public bool autoSwitchLaser = true;

    public ControllerVr LeftController { get; private set; }
    public ControllerVr RightController { get; private set; }

    private Action<GameObject> leftCallback;
    private Action<GameObject> rightCallback;

    private void Awake()
    {
        LeftController = CreateController(
            XRNode.LeftHand, left);

        RightController = CreateController(
            XRNode.RightHand, right);
    }

    private ControllerVr CreateController(
        XRNode node,
        HandSettings settings)
    {
        var controller = new ControllerVr(node);

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

    private void Update()
    {
        LeftController?.Update();
        RightController?.Update();
    }

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

    private void OnDestroy()
    {
        Cleanup(LeftController, leftCallback);
        Cleanup(RightController, rightCallback);
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
