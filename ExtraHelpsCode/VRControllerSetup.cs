
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

public class VRControllerSetup : MonoBehaviour
{
    [Header("Left AutoHand")]
    public Autohand.Hand leftHand;
    public HandGameObjectPointer_OnVrHands leftLaser;
    public Behaviour leftGrabInput;

    [Header("Right AutoHand")]
    public Autohand.Hand rightHand;
    public HandGameObjectPointer_OnVrHands rightLaser;
    public Behaviour rightGrabInput;

    [Header("Laser Settings")]
    public float laserDistance = 5f;
    public LayerMask selectableLayers = ~0;

    [Header("Mode Settings")]
    public bool autoSwitchLaser = true;

    public ControllerVr LeftController { get; private set; }
    public ControllerVr RightController { get; private set; }

    private Action<GameObject> leftClickCallback;
    private Action<GameObject> rightClickCallback;

    private void Awake()
    {
        LeftController = new ControllerVr(XRNode.LeftHand);
        RightController = new ControllerVr(XRNode.RightHand);

        if (leftLaser != null)
        {
            leftLaser.raycastLength = laserDistance;
            leftLaser.selectableLayers = selectableLayers;
        }

        if (rightLaser != null)
        {
            rightLaser.raycastLength = laserDistance;
            rightLaser.selectableLayers = selectableLayers;
        }

        LeftController.SetAutoHand(leftHand, leftLaser, leftGrabInput);
        RightController.SetAutoHand(rightHand, rightLaser, rightGrabInput);

        LeftController.AutoSwitchLaser = autoSwitchLaser;
        RightController.AutoSwitchLaser = autoSwitchLaser;

        Debug.LogError("NNNNEEEED CHEACKING ");
        LeftController.SetLaserMode(true);
        RightController.SetLaserMode(true);
    }

    private void Update()
    {
        LeftController?.Update();
        RightController?.Update();
    }

    // -------------------------
    // LEFT LASER
    // -------------------------

    public void SetLeftLaserMode(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        if (LeftController == null)
            return;

        if (leftClickCallback != null)
            LeftController.OnLaserClick -= leftClickCallback;

        leftClickCallback = enabled ? onClick : null;

        if (leftClickCallback != null)
            LeftController.OnLaserClick += leftClickCallback;

        LeftController.SetLaserMode(enabled);
    }

    // -------------------------
    // RIGHT LASER
    // -------------------------

    public void SetRightLaserMode(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        if (RightController == null)
            return;

        if (rightClickCallback != null)
            RightController.OnLaserClick -= rightClickCallback;

        rightClickCallback = enabled ? onClick : null;

        if (rightClickCallback != null)
            RightController.OnLaserClick += rightClickCallback;

        RightController.SetLaserMode(enabled);
    }

    public void SetBothLaserModes(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        SetLeftLaserMode(enabled, onClick);
        SetRightLaserMode(enabled, onClick);
    }

    private void OnDestroy()
    {
        if (LeftController != null)
        {
            if (leftClickCallback != null)
                LeftController.OnLaserClick -= leftClickCallback;

            LeftController.Dispose();
        }

        if (RightController != null)
        {
            if (rightClickCallback != null)
                RightController.OnLaserClick -= rightClickCallback;

            RightController.Dispose();
        }
    }
}
