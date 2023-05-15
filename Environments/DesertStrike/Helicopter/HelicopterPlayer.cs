using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class HelicopterPlayer : HelicopterBase
{

	void Update () 
    {

		var current = Gamepad.current;
		if(current == null)
        {
			return;
        }

		Pitch(current.leftStick.ReadValue().y );
		Tilt(-current.leftStick.ReadValue().x);



		FireGun ();



	
		if (current.bButton.isPressed)
			m_TerrainFollowing.HeightChange(1);

        if (current.aButton.isPressed)
            m_TerrainFollowing.HeightChange(1);

        ////if (current.bButton.isPressed)
        ////	ReloadAll();


        ////      // make triggers into buttons
        ////if (InputManager.ActiveDevice.LeftTrigger.Value >= 0.9f && InputManager.ActiveDevice.LeftTrigger.LastValue < 0.9f)
        ////      {
        ////          m_helicopter.SwitchWeaponSystem();
        ////      }



        ////      // make triggers into buttons
        ////if (InputManager.ActiveDevice.RightTrigger.Value >= 0.9f && InputManager.ActiveDevice.RightTrigger.LastValue < 0.9f)
        ////{
        ////      	m_helicopter.FireWeaponSystem();
        ////}


    }


    void FireGun()
	{
		//if (Mathf.Abs(InputManager.ActiveDevice.RightStickY) < 0.2 && Mathf.Abs(InputManager.ActiveDevice.RightStickX) < 0.2)
		//	return;

		// fire gun relative to the camera
		Vector3 cameraDirectionForward = Camera.main.transform.forward;
		cameraDirectionForward.y = 0;

		
		Vector3 cameraDirectionRight = Camera.main.transform.right;
		cameraDirectionRight.y = 0;

		
		//Vector3 firingDirection = (cameraDirectionRight * InputManager.ActiveDevice.RightStickX) + (cameraDirectionForward * InputManager.ActiveDevice.RightStickY);


		//m_helicopter.FireGun(firingDirection.x , firingDirection.z );
	}
}
