using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand.Demo{
    public class XRTeleporterLink : MonoBehaviour{
        public Teleporter hand;
        public XRNode role;
        public CommonButton button;
        
        bool teleporting = false;
        InputDevice device;
        List<InputDevice> devices;

        void Start(){
            devices = new List<InputDevice>();
        }

        InputButtonStateHandler m_Controller;
        public void SetController(InputButtonStateHandler controller)
        {
            m_Controller.Begin += Begin;
            m_Controller.Begin += End;
        }

        private void Begin(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
            if (teleporting)
            {
                hand.Teleport();
                teleporting = false;
            }
        }

        private void End(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
            if (!teleporting)
            {
                hand.StartTeleport();
                teleporting = true;
            }
        }

        void FixedUpdate(){
            InputDevices.GetDevicesAtXRNode(role, devices);
            if(devices.Count > 0)
                device = devices[0];

            if(device != null && device.isValid){
                //Sets hand fingers wrap
                if(device.TryGetFeatureValue(XRHandControllerLink.GetCommonButton(button), out bool teleportButton)) {
                    if(teleporting && !teleportButton){
                        hand.Teleport();
                        teleporting = false;
                    }
                    else if(!teleporting && teleportButton){
                        hand.StartTeleport();
                        teleporting = true;
                    }
                }
            }
        }
    }
}
