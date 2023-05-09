using Oculus.Avatar2;

using Button = OVRInput.Button;
using Touch = OVRInput.Touch;

public class OurOvrInputControlDelegate : OvrAvatarInputControlDelegate
{

    public CAPI.ovrAvatar2ControllerType controllerType = CAPI.ovrAvatar2ControllerType.Invalid;
    public override bool GetInputControlState(out OvrAvatarInputControlState inputControlState)
    {
        inputControlState = default;
        inputControlState.leftControllerState.isActive = true;
        inputControlState.rightControllerState.isActive = true;
        inputControlState.type = controllerType;


        return true;
    }
}
