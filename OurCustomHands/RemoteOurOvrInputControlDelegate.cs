using Oculus.Avatar2;

using Button = OVRInput.Button;
using Touch = OVRInput.Touch;

public class RemoteOurOvrInputControlDelegate : OvrAvatarInputControlDelegate
{

    public CAPI.ovrAvatar2ControllerType controllerType = CAPI.ovrAvatar2ControllerType.Invalid;
    public override bool GetInputControlState(out OvrAvatarInputControlState inputControlState)
    {
        inputControlState = default;
        inputControlState.type = controllerType;


        return true;
    }
}
