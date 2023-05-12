using UnityEngine;
using System.Collections;

public class TurretControlsPlayer : ControlsPlayer {

    private turret m_turret;

    public float m_xAxisDeadZone       = 0.2f;
    public float m_yAxisDeadZone       = 0.2f;
    public float m_triggerAxisDeadZone = 0.2f;
	// Use this for initialization
	void Start () {
        m_turret = GetComponent<turret>();
	}
    void XboxControls()
    {
        float amount;
        amount = Input.GetAxis("L_XAxis_1");
        if (Mathf.Abs(amount) > m_xAxisDeadZone)
            m_turret.RotateX(amount);

        amount = Input.GetAxis("L_YAxis_1");
        if (Mathf.Abs(amount) > m_yAxisDeadZone)
            m_turret.RotateY(amount);

        amount = Input.GetAxis("TriggersL_1");
        if (Mathf.Abs(amount) > m_triggerAxisDeadZone)
            m_turret.FireGun();
    }
    void KeyboardControls()
    {
        if (Input.GetKey(KeyCode.S))
            m_turret.RotateX(1);

        if (Input.GetKey(KeyCode.W))
            m_turret.RotateX(-1);

        if (Input.GetKey(KeyCode.A))
            m_turret.RotateY(1);

        if (Input.GetKey(KeyCode.D))
            m_turret.RotateY(-1);

        if (Input.GetKey(KeyCode.S))
            m_turret.RotateY(1);

        if (Input.GetKey(KeyCode.Space))
            m_turret.FireGun();

    }

	// Update is called once per frame
	void FixedUpdate () 
    {
        XboxControls();
        KeyboardControls();



	}
}
