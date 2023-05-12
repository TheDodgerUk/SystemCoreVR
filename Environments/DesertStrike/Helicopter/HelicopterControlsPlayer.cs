using UnityEngine;
using System.Collections;

public class HelicopterControlsPlayer : ControlsPlayer {

    private HelicopterControls m_helicopter;
    private TerrainFollowing   m_TerrainFollowing;
	// Use this for initialization
	void Start () 
	{
        

	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        /*

        m_helicopter.Pitch(InputManager.ActiveDevice.LeftStickY );
		m_helicopter.Tilt(-InputManager.ActiveDevice.LeftStickX );



		FireGun ();



	
		if (InputManager.ActiveDevice.Action1.IsPressed)
			m_TerrainFollowing.HeightChange(-1);

		if (InputManager.ActiveDevice.Action2.IsPressed)
			m_TerrainFollowing.HeightChange(1);
		
		if (InputManager.ActiveDevice.Action3.IsPressed)
			m_helicopter.ReloadAll();


        // make triggers into buttons
		if (InputManager.ActiveDevice.LeftTrigger.Value >= 0.9f && InputManager.ActiveDevice.LeftTrigger.LastValue < 0.9f)
        {
            m_helicopter.SwitchWeaponSystem();
        }



        // make triggers into buttons
		if (InputManager.ActiveDevice.RightTrigger.Value >= 0.9f && InputManager.ActiveDevice.RightTrigger.LastValue < 0.9f)
		{
        	m_helicopter.FireWeaponSystem();
		}

    */
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
