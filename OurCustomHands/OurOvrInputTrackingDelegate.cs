#if VR_INTERACTION
using Oculus.Avatar2;
using Oculus.Interaction.Input;
using System;
using UnityEngine;
using static Oculus.Avatar2.CAPI;
using Node = UnityEngine.XR.XRNode;


public class OurOvrInputTrackingDelegate : OvrAvatarInputTrackingDelegate
{

    public OurOvrInputTrackingDelegate(OVRCameraRig ovrCameraRig, AutoHandAvatarInputManager autoHandAvatarInputManager)
    {

    }


#if UNITY_ANDROID
    public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState avatarRendererTrackingState)
    {
        avatarRendererTrackingState = default;
        avatarRendererTrackingState.headsetActive = true;
        avatarRendererTrackingState.leftControllerActive = true;
        avatarRendererTrackingState.rightControllerActive = true;
        avatarRendererTrackingState.leftControllerVisible = true;
        avatarRendererTrackingState.rightControllerVisible = true;

        if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position,
            OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headPos))
        {
            avatarRendererTrackingState.headset.position = headPos;
        }

        if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation,
            OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headRot))
        {
            avatarRendererTrackingState.headset.orientation = headRot;
        }

        avatarRendererTrackingState.headset.scale = Vector3.one;
        avatarRendererTrackingState.leftController.scale = Vector3.one;
        avatarRendererTrackingState.rightController.scale = Vector3.one;

        if (CameraControllerVR.Instance != null && CameraControllerVR.Instance.HandLeftRef != null)
        {
            avatarRendererTrackingState.leftController.position = CameraControllerVR.Instance.HandLeftRef.transform.localPosition;
            avatarRendererTrackingState.leftController.orientation = CameraControllerVR.Instance.HandLeftRef.transform.localRotation;

            avatarRendererTrackingState.rightController.position = CameraControllerVR.Instance.HandRightRef.transform.localPosition;
            avatarRendererTrackingState.rightController.orientation = CameraControllerVR.Instance.HandRightRef.transform.localRotation;
        }

        return true;
    }

#else
    public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState avatarRendererTrackingState)
    {
        avatarRendererTrackingState = default;
        return false;
    }
#endif


}
#endif