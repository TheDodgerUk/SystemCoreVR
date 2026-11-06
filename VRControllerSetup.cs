
using Autohand;
using System;
using UnityEngine;
using UnityEngine.XR;


public class VRControllerSetup : MonoBehaviour
{
    [Header("AutoHand - Left")]
    public Autohand.Hand leftHand;
    public HandDistanceGrabber leftLaser;
    public Behaviour leftGrabInput;
    public Transform leftLaserOrigin;

    [Header("AutoHand - Right")]
    public Autohand.Hand rightHand;
    public HandDistanceGrabber rightLaser;
    public Behaviour rightGrabInput;
    public Transform rightLaserOrigin;

    [Header("Laser Settings")]
    public float laserDistance = 5f;
    public LayerMask selectableLayers = ~0;

    [Header("Controls")]
    public bool toggleWithPrimaryButton = true;

    private ControllerVr left;
    private ControllerVr right;

    public ControllerVr LeftController => left;
    public ControllerVr RightController => right;
    private bool previousX;
    private bool previousA;

    // -------------------------
    // START
    // -------------------------

    private void Start()
    {
        // LEFT CONTROLLER
        left = new ControllerVr(XRNode.LeftHand);

        left.SetAutoHand(
            leftHand,
            leftLaser,
            leftGrabInput);

        left.LaserOrigin = leftLaserOrigin;
        left.LaserDistance = laserDistance;
        left.LaserLayers = selectableLayers;

        left.OnLaserClick += OnLeftLaserClick;
        left.OnLaserHover += OnLeftLaserHover;

        // RIGHT CONTROLLER
        right = new ControllerVr(XRNode.RightHand);

        right.SetAutoHand(
            rightHand,
            rightLaser,
            rightGrabInput);

        right.LaserOrigin = rightLaserOrigin;
        right.LaserDistance = laserDistance;
        right.LaserLayers = selectableLayers;

        right.OnLaserClick += OnRightLaserClick;
        right.OnLaserHover += OnRightLaserHover;
    }

    // -------------------------
    // UPDATE
    // -------------------------

    private void Update()
    {
        if (toggleWithPrimaryButton)
        {
            // X button - left hand
            bool x = left.PrimaryButton;

            if (x && !previousX)
                left.ToggleLaserMode();

            previousX = x;

            // A button - right hand
            bool a = right.PrimaryButton;

            if (a && !previousA)
                right.ToggleLaserMode();

            previousA = a;
        }

        // Process laser selection
        left.UpdateLaser();
        right.UpdateLaser();
    }

    // -------------------------
    // LEFT CALLBACKS
    // -------------------------

    private void OnLeftLaserClick(GameObject obj)
    {
        Debug.Log(
            "[LEFT LASER] Clicked: " + obj.name);

        HandleSelection(obj, XRNode.LeftHand);
    }

    private void OnLeftLaserHover(GameObject obj)
    {
        if (obj != null)
        {
            Debug.Log(
                "[LEFT LASER] Hover: " + obj.name);
        }
    }

    // -------------------------
    // RIGHT CALLBACKS
    // -------------------------

    private void OnRightLaserClick(GameObject obj)
    {
        Debug.Log(
            "[RIGHT LASER] Clicked: " + obj.name);

        HandleSelection(obj, XRNode.RightHand);
    }

    private void OnRightLaserHover(GameObject obj)
    {
        if (obj != null)
        {
            Debug.Log(
                "[RIGHT LASER] Hover: " + obj.name);
        }
    }

    // -------------------------
    // SHARED SELECTION
    // -------------------------

    private void HandleSelection(
        GameObject obj,
        XRNode controller)
    {
        if (obj == null)
            return;

        Debug.Log(
            $"Selected {obj.name} using {controller}");

        // Add your Kill Team selection logic here.
        //
        // Example:
        // var miniature =
        //     obj.GetComponentInParent<Miniature>();
        //
        // if (miniature != null)
        //     SelectMiniature(miniature);
    }

    // -------------------------
    // PUBLIC MODE CONTROLS
    // -------------------------

    private System.Action<GameObject> leftClickCallback;
    public void SetLeftLaserMode(
        bool enabled,
        System.Action<GameObject> onClick = null)
    {
        if (left == null)
            return;

        // Remove previous callback to avoid duplicates
        if (leftClickCallback != null)
            left.OnLaserClick -= leftClickCallback;

        leftClickCallback = enabled ? onClick : null;

        if (leftClickCallback != null)
            left.OnLaserClick += leftClickCallback;

        left.SetLaserMode(enabled);
    }


    private System.Action<GameObject> rightClickCallback;
    public void SetRightLaserMode(
        bool enabled,
        System.Action<GameObject> onClick = null)
    {
        if (right == null)
            return;

        // Remove previous callback to avoid duplicates
        if (rightClickCallback != null)
            left.OnLaserClick -= rightClickCallback;

        rightClickCallback = enabled? onClick : null;

        if (rightClickCallback != null)
            right.OnLaserClick += rightClickCallback;

        right.SetLaserMode(enabled);
    }



    // -------------------------
    // CLEANUP
    // -------------------------

    private void OnDestroy()
    {
        if (left != null)
        {
            left.OnLaserClick -= OnLeftLaserClick;
            left.OnLaserHover -= OnLeftLaserHover;
            left.Dispose();
        }

        if (right != null)
        {
            right.OnLaserClick -= OnRightLaserClick;
            right.OnLaserHover -= OnRightLaserHover;
            right.Dispose();
        }
    }
}
