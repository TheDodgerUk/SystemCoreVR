using UnityEngine;
using System.Collections;

public class turret : MonoBehaviour {


    [System.Serializable]
    public class Turret
    {
		public GameObject m_gameObject;
        public float m_turnSpeed = 50;

        [HideInInspector]
        public float m_currentRotationAmount = 0;
    }

	//--------------------------------------------------
	[System.Serializable]
	public class TurretX : Turret
	{
		public float m_rotationMaxUp = 45;
		public float m_rotationMaxDown = -5;
	}

	//--------------------------------------------------
    [System.Serializable]
	public class TurretY : Turret
    {
        public float m_turnLimitedAmount = 45;
        public bool m_isTurnLimited      = false;
    }

	//--------------------------------------------------
	public TurretX m_axisX;
    public TurretY m_axisY;
	public WeaponSystemBase m_weaponSystem;






	// Use this for initialization
	void Start () {
        if (!m_axisX.m_gameObject && !m_axisY.m_gameObject)
            Debug.LogError("the turret: " + this.gameObject + " has no axis assigned"); 

		if (m_weaponSystem == null)
			Debug.LogError("the turret has no weapon system: " + this.gameObject ); 
	
	}

    public void RotateX(float amount)
    {
        if (!m_axisX.m_gameObject) 
            return;

		m_axisX.m_gameObject.transform.Rotate(-m_axisX.m_currentRotationAmount, 0, 0);
		m_axisX.m_currentRotationAmount += Mathf.Clamp(amount, -1, 1) * Time.deltaTime * m_axisX.m_turnSpeed;

		m_axisX.m_currentRotationAmount = Mathf.Clamp(m_axisX.m_currentRotationAmount, -m_axisX.m_rotationMaxUp, -m_axisX.m_rotationMaxDown); // inverted and reverss cause - if upwards
		m_axisX.m_gameObject.transform.Rotate(m_axisX.m_currentRotationAmount, 0, 0);

    }

    public void RotateY(float amount)
    {
		if (!m_axisY.m_gameObject) 
			return;

		m_axisY.m_gameObject.transform.Rotate(0, -m_axisY.m_currentRotationAmount, 0);                         // reset
		m_axisY.m_currentRotationAmount += Mathf.Clamp(amount, -1, 1) * Time.deltaTime * m_axisY.m_turnSpeed;  // make justment

		if (m_axisY.m_isTurnLimited)
			m_axisY.m_currentRotationAmount = Mathf.Clamp(m_axisY.m_currentRotationAmount, -m_axisY.m_turnLimitedAmount, m_axisY.m_turnLimitedAmount); // limit if BOOL

		m_axisY.m_gameObject.transform.Rotate(0, m_axisY.m_currentRotationAmount, 0); // impliment  ajustment

    }


	public void FireGunAtTarget(GameObject passedGameObject)
	{
		m_weaponSystem.FireAtGameObject(passedGameObject);
	}

    public void FireGun()
    {
        m_weaponSystem.Fire();
    }






}
