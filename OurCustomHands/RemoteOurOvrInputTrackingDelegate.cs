using Oculus.Avatar2;
using Oculus.Interaction.Input;
using UnityEngine;
using static Oculus.Avatar2.CAPI;
using Node = UnityEngine.XR.XRNode;




/// this is  stright copy of SampleInputTrackingDelegate aprt from parts below which i will mark
/*
 *
 */
public class RemoteOurOvrInputTrackingDelegate : OvrAvatarInputTrackingDelegate
{
    private const bool PHYSICS_HAND_ROTATION = false;

    private OVRCameraRig _ovrCameraRig = null;

    private Rigidbody m_LeftCustomSkeletonHandRB;
    private Rigidbody m_RightCustomSkeletonHandRB;

    private Transform m_LeftWrist;
    private Transform m_RightWrist;

    public float positionThreshold = 0.005f;
    public float maxDistance = 1f;

    private OvrAvatarInputTrackingState m_ControllerTargetData = default;



    private NetworkPlayerAvatar m_NetworkPlayerAvatar;
    private AutoHandAvatarInputManager m_AutoHandAvatarInputManager;

    public RemoteOurOvrInputTrackingDelegate(OVRCameraRig ovrCameraRig, NetworkPlayerAvatar remoteOffsetInputManager)
    {
        _ovrCameraRig = ovrCameraRig;
        m_NetworkPlayerAvatar = remoteOffsetInputManager;
        m_AutoHandAvatarInputManager = CameraControllerVR.Instance.transform.GetComponentInChildren<AutoHandAvatarInputManager>(true);
    }

    public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
    {
        inputTrackingState = default;
        inputTrackingState.headsetActive = true;
        inputTrackingState.leftControllerActive = true;
        inputTrackingState.rightControllerActive = true;
        inputTrackingState.leftControllerVisible = true;
        inputTrackingState.rightControllerVisible = true;

        inputTrackingState.headset = (CAPI.ovrAvatar2Transform)m_NetworkPlayerAvatar.m_AvatarHead.transform;
        inputTrackingState.headset.position = m_NetworkPlayerAvatar.m_AvatarHead.transform.localPosition;
        inputTrackingState.headset.orientation = m_NetworkPlayerAvatar.m_AvatarHead.transform.localRotation;
        inputTrackingState.headset.scale = m_NetworkPlayerAvatar.m_AvatarHead.transform.localScale;

        inputTrackingState.leftController = (CAPI.ovrAvatar2Transform)m_NetworkPlayerAvatar.m_AvatarLeftHand.transform;
        inputTrackingState.leftController.position = m_NetworkPlayerAvatar.m_AvatarLeftHand.transform.localPosition;////// nope + OurOvrInputTrackingDelegate.LEFT_OFFSET_POSITION;
        inputTrackingState.leftController.orientation = m_NetworkPlayerAvatar.m_AvatarLeftHand.transform.localRotation;
        inputTrackingState.leftController.scale = m_NetworkPlayerAvatar.m_AvatarLeftHand.transform.localScale;

        inputTrackingState.rightController = (CAPI.ovrAvatar2Transform)m_NetworkPlayerAvatar.m_AvatarRightHand.transform;
        inputTrackingState.rightController.position = m_NetworkPlayerAvatar.m_AvatarRightHand.transform.localPosition;////// nope + OurOvrInputTrackingDelegate.RIGHT_OFFSET_POSITION;
        inputTrackingState.rightController.orientation = m_NetworkPlayerAvatar.m_AvatarRightHand.transform.localRotation;
        inputTrackingState.rightController.scale = m_NetworkPlayerAvatar.m_AvatarRightHand.transform.localScale;


        ////Debug.LogError($"m_RemoteOffsetInputManager.m_GlobalRightHand.transform.position {m_RemoteOffsetInputManager.m_AvatarRightHand.transform.position}");
        return true;
    }


}
