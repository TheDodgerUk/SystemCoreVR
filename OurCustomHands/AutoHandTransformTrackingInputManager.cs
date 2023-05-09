using Oculus.Avatar2;
using UnityEngine;
using Node = UnityEngine.XR.XRNode;

public class AutoHandTrackingTransformsInputControlDelegate : OvrAvatarInputControlDelegate
{
    public CAPI.ovrAvatar2ControllerType controllerType = CAPI.ovrAvatar2ControllerType.Invalid;

    public override bool GetInputControlState(out OvrAvatarInputControlState inputControlState)
    {
        inputControlState = default;
        inputControlState.type = controllerType;

        return true;
    }
}

public class AutoHandTrackingTransformsInputTrackingDelegate : OvrAvatarInputTrackingDelegate
{
    private AutoHandTransformTrackingInputManager _transforms;

    public AutoHandTrackingTransformsInputTrackingDelegate(AutoHandTransformTrackingInputManager transforms)
    {
        _transforms = transforms;
    }

    public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
    {
        inputTrackingState = default;

        if (_transforms.hmd)
        {
            inputTrackingState.headset = (CAPI.ovrAvatar2Transform)_transforms.hmd;
            inputTrackingState.headsetActive = true;
        }


        if (_transforms.leftController)
        {
            inputTrackingState.leftController = (CAPI.ovrAvatar2Transform)_transforms.leftController;
            inputTrackingState.leftControllerActive = true;
            inputTrackingState.leftControllerVisible = _transforms.controllersVisible;
        }
        else
        {
            inputTrackingState.leftControllerActive = false;
        }

        if (_transforms.rightController)
        {
            inputTrackingState.rightController = (CAPI.ovrAvatar2Transform)_transforms.rightController;
            inputTrackingState.rightControllerActive = true;
            inputTrackingState.rightControllerVisible = _transforms.controllersVisible;
        }
        else
        {
            inputTrackingState.rightControllerActive = false;
        }

        if (CameraControllerVR.Instance != null)
        {
            if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position,
                OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headPos))
            {
                inputTrackingState.headset.position = headPos;
            }

            if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation,
                OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headRot))
            {
                inputTrackingState.headset.orientation = headRot;
            }


            if (CameraControllerVR.Instance.HandLeftRef != null)
            {
                inputTrackingState.leftController.position = CameraControllerVR.Instance.HandLeftRef.transform.localPosition;
                inputTrackingState.leftController.orientation = CameraControllerVR.Instance.HandLeftRef.transform.localRotation;

                inputTrackingState.rightController.position = CameraControllerVR.Instance.HandRightRef.transform.localPosition;
                inputTrackingState.rightController.orientation = CameraControllerVR.Instance.HandRightRef.transform.localRotation;
            }
        }

        return true;
    }




}

// This class assigns Transform data to body tracking system
// so that avatar can be controlled without a headset
public class AutoHandTransformTrackingInputManager : OvrAvatarInputManager
{
    public Transform hmd;

    public Transform leftController;

    public Transform rightController;

    public bool controllersVisible = false;

    private void Start()
    {
        if (BodyTracking != null)
        {
            BodyTracking.InputControlDelegate = new AutoHandTrackingTransformsInputControlDelegate();
            BodyTracking.InputTrackingDelegate = new AutoHandTrackingTransformsInputTrackingDelegate(this);
        }
    }
}
