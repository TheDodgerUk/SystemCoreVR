
using System;
using UnityEngine;
using UnityEngine.XR;
using Autohand;

public class VRControllerSetup : MonoBehaviour
{
    [Header("Left AutoHand")]
    public Autohand.Hand leftHand;
    public HandGameObjectPointer leftLaser;
    public Behaviour leftGrabInput;

    [Header("Right AutoHand")]
    public Autohand.Hand rightHand;
    public HandGameObjectPointer rightLaser;
    public Behaviour rightGrabInput;

    [Header("Laser Settings")]
    public float laserDistance = 5f;
    public LayerMask selectableLayers = ~0;

    [Header("Mode Settings")]
    public bool autoSwitchLaser = true;

    public ControllerVr Left { get; private set; }
    public ControllerVr Right { get; private set; }

    private Action<GameObject> leftClickCallback;
    private Action<GameObject> rightClickCallback;

    private void Awake()
    {
        Left = new ControllerVr(XRNode.LeftHand);
        Right = new ControllerVr(XRNode.RightHand);

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

        Left.SetAutoHand(leftHand, leftLaser, leftGrabInput);
        Right.SetAutoHand(rightHand, rightLaser, rightGrabInput);

        Left.AutoSwitchLaser = autoSwitchLaser;
        Right.AutoSwitchLaser = autoSwitchLaser;
        Left.SetLaserMode(true);
        Right.SetLaserMode(true);
    }

    private void Update()
    {
        Left?.Update();
        Right?.Update();
    }

    // -------------------------
    // LEFT LASER
    // -------------------------

    public void SetLeftLaserMode(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        if (Left == null)
            return;

        if (leftClickCallback != null)
            Left.OnLaserClick -= leftClickCallback;

        leftClickCallback = enabled ? onClick : null;

        if (leftClickCallback != null)
            Left.OnLaserClick += leftClickCallback;

        Left.SetLaserMode(enabled);
    }

    // -------------------------
    // RIGHT LASER
    // -------------------------

    public void SetRightLaserMode(
        bool enabled,
        Action<GameObject> onClick = null)
    {
        if (Right == null)
            return;

        if (rightClickCallback != null)
            Right.OnLaserClick -= rightClickCallback;

        rightClickCallback = enabled ? onClick : null;

        if (rightClickCallback != null)
            Right.OnLaserClick += rightClickCallback;

        Right.SetLaserMode(enabled);
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
        if (Left != null)
        {
            if (leftClickCallback != null)
                Left.OnLaserClick -= leftClickCallback;

            Left.Dispose();
        }

        if (Right != null)
        {
            if (rightClickCallback != null)
                Right.OnLaserClick -= rightClickCallback;

            Right.Dispose();
        }
    }
}
