using UnityEngine;
using System.Collections;

public class CarControlsPlayer : ControlsPlayer
{

    CarControls m_controls;
	// Use this for initialization
	void Start () {
        m_controls = GetComponent<CarControls>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        if ( Input.GetKey(KeyCode.I))
            m_controls.InputTorque(1);

        if (Input.GetKey(KeyCode.K))
            m_controls.InputTorque(-1);

        if (Input.GetKey(KeyCode.J))
            m_controls.InputSteer(-1);

        if (Input.GetKey(KeyCode.L))
            m_controls.InputSteer(1);

        if (Input.GetKey(KeyCode.M))
            m_controls.HandBrake();

	}
}
