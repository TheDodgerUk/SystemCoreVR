using UnityEngine;
using System.Collections;

public class HelicopterBase : MonoBehaviour
{
    Fuel m_Fuel;

    public Transform m_pivotPoint;
    public WeaponSystemInstantGun m_gun;
    public float m_gunRange = 100;
	public WeaponSystemBase m_rocket;
	public WeaponSystemBase m_hellfire;

    [System.Serializable]
    public class CPitch
    {
        public float m_pitchForwardMax = 25;
        public float m_pitchBackwardMax = -10;
        public float m_pitchIncrement = 10;
        public float m_drag = 0.99f;
        [HideInInspector]
        public float m_pitchCurrent = 0;
    }
    public CPitch m_pitch = new CPitch();

    [System.Serializable]
    public class CTilt
    {
        public float m_tiltMax = 25;
        public float m_tiltIncrement = 15;
        public float m_drag = 0.99f;
        public float m_rotationSpeedMax = 30.0f;
        [HideInInspector]
        public float m_tiltCurrent = 0;
        [HideInInspector]
        public float m_rotationAmount = 0;

    }
    public CTilt m_tilt = new CTilt();



    [System.Serializable]
    public class CSpeed
    {
        
        public float m_speedForwardMax = 20;
        public float m_speedReverseMax = -10;
        public float m_drag = 0.99f;

        //[HideInInspector]
        public float m_speedForwardCurrent = 0;
    }
    public CSpeed m_speed = new CSpeed();
    private float m_hoverOffSet;


    public enum WeaponSystemChoice
    {
        Current = -1,
        Start,
        Rockets = Start,
        Hellfire,  
        count,
    };

    public WeaponSystemChoice m_weaponSystemChoice = WeaponSystemChoice.Hellfire;

    private Rotors m_Rotors;
    public TerrainFollowing m_TerrainFollowing;
    public Hover m_Hover;


    void Awake () 
    {
        m_pivotPoint = this.gameObject.SearchComponent<Transform>("RootModel");

        m_pitch.m_pitchCurrent = 0;

        m_Fuel = this.gameObject.ForceComponent<Fuel>();
        m_TerrainFollowing = this.AddComponent<TerrainFollowing>();

        m_Rotors = this.AddComponent<Rotors>();
        m_Rotors.Initilise("SM_Veh_Helicopter_Attack_01_Blades_Main", "SM_Veh_Helicopter_Attack_01_Blades_Back");

        m_gun = this.AddComponent<WeaponSystemInstantGun>();
        m_gun.Initilise("SM_Veh_Helicopter_Attack_01_Gun_Horizontal");


        m_Hover = this.AddComponent<Hover>();
    }

    //
    //
    public float GetSpeedPercentage() 
    { 
        return m_speed.m_speedForwardCurrent / m_speed.m_speedForwardMax; 
    }

    //
    //
    public float GetHoverOffset() 
    { 
        return m_hoverOffSet; 
    }

    //
    //
    public void SwitchWeaponSystem()
    {
        // rotate around system
        m_weaponSystemChoice++;
        if (m_weaponSystemChoice >= WeaponSystemChoice.count)
        {
            m_weaponSystemChoice = WeaponSystemChoice.Start;
        }
        Debug.LogError("weaponSystem Choice " + m_weaponSystemChoice);
    }

    //
    //
    public void FireWeaponSystem(WeaponSystemChoice choice)
    {
        switch (choice)
        {
            case WeaponSystemChoice.Hellfire: m_hellfire.Fire();  break;
            case WeaponSystemChoice.Rockets: m_rocket.Fire();     break;
            default: /* error message */break;
        }
    }

    //
    //
    public void FireWeaponSystem()
    {
        switch (m_weaponSystemChoice)
        {
            case WeaponSystemChoice.Hellfire:  m_hellfire.Fire(); break;
            case WeaponSystemChoice.Rockets:   m_rocket.Fire(); break;
            default: /* error message */break;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public void SetTiltToAmountIfCan(float amount)
	{

		float maxTiltPerUpdate = m_tilt.m_tiltIncrement * Time.deltaTime;
		if ( amount > m_tilt.m_tiltCurrent)
		{
			if ( m_tilt.m_tiltCurrent + maxTiltPerUpdate > amount)
			{
				m_tilt.m_tiltCurrent = amount;
			}
			else
			{
				m_tilt.m_tiltCurrent = m_tilt.m_tiltCurrent + maxTiltPerUpdate;
			}
		}
		else if ( amount < m_tilt.m_tiltCurrent)
		{
			if ( m_tilt.m_tiltCurrent - maxTiltPerUpdate < amount)
			{
				m_tilt.m_tiltCurrent = amount;
			}
			else
			{
				m_tilt.m_tiltCurrent = m_tilt.m_tiltCurrent - maxTiltPerUpdate;
			}
		}

	}

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public void Tilt(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);

        if (amount > 0)
        {
            if (amount > m_tilt.m_tiltCurrent / m_tilt.m_tiltMax)
                m_tilt.m_tiltCurrent += m_tilt.m_tiltIncrement * amount * Time.deltaTime;
        }
        else
        {
            if (amount < m_tilt.m_tiltCurrent / m_tilt.m_tiltMax)
                m_tilt.m_tiltCurrent += m_tilt.m_tiltIncrement * amount *Time.deltaTime;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public void ReloadAll()
    {
		WeaponSystemBase[] weapons = GetComponents<WeaponSystemBase>();
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].ReloadAmmo();
        }
    }
    //
    //
    public void FireGun(float amountX, float amountY)
    {
		m_gun.FireAtDirectionAndPosition(new Vector3(amountX, amountY, m_gunRange), m_gun.m_weaponPoints[0].m_weaponPoint.transform.position); // using spare z for gunrange
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public void Pitch(float amount)
    {

        amount = Mathf.Clamp(amount, -1, 1);
        if (amount > 0)
        {
			// this limits the pitch  so if only half way forward the helicopter will only go half pitch 
            if (amount > (m_pitch.m_pitchCurrent / m_pitch.m_pitchForwardMax))
                m_pitch.m_pitchCurrent += m_tilt.m_tiltIncrement * amount * Time.deltaTime;
        }
        else
        {

            if (amount < (m_tilt.m_tiltCurrent / m_pitch.m_pitchBackwardMax))
                m_pitch.m_pitchCurrent += m_tilt.m_tiltIncrement * amount * Time.deltaTime;

        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public float GetCurrentSpeed()
    {
        return m_speed.m_speedForwardCurrent;
    }
    public float GetCurrentTilt()
    {
        return m_tilt.m_tiltCurrent;
    }
    
    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    void ApplyDrag()
    {
        m_pitch.m_pitchCurrent = Mathf.Clamp(m_pitch.m_pitchCurrent, m_pitch.m_pitchBackwardMax, m_pitch.m_pitchForwardMax);
        m_pitch.m_pitchCurrent *= m_pitch.m_drag;



		// limit speed with pitch 
		bool applyPitch = true;
		if (m_speed.m_speedForwardCurrent > 0 && m_pitch.m_pitchCurrent > 0 )// forward
		{
			float forwardSpeedPercentage = m_speed.m_speedForwardCurrent / m_speed.m_speedForwardMax;
			float forwardPitchPercentage = m_pitch.m_pitchCurrent / m_pitch.m_pitchForwardMax;

			if ( forwardSpeedPercentage > forwardPitchPercentage)
			{
				applyPitch = false;
			}

		}
		else if( m_speed.m_speedForwardCurrent < 0 && m_pitch.m_pitchCurrent < 0)// backwards
		{
			float backwardsSpeedPercentage = Mathf.Abs(m_speed.m_speedForwardCurrent / m_speed.m_speedReverseMax);
			float backwardsPitchPercentage = Mathf.Abs(m_pitch.m_pitchCurrent / m_pitch.m_pitchBackwardMax);

			if ( backwardsSpeedPercentage < backwardsPitchPercentage)
			{
				applyPitch = false;
			}
		}

		//this looks like it was a safety feature will look again at it 
		//if(applyPitch)
		{
			m_speed.m_speedForwardCurrent += m_pitch.m_pitchCurrent * Time.deltaTime;
		}

       


		m_speed.m_speedForwardCurrent  = Mathf.Clamp(m_speed.m_speedForwardCurrent, m_speed.m_speedReverseMax, m_speed.m_speedForwardMax);
        m_speed.m_speedForwardCurrent *= m_speed.m_drag;

        m_tilt.m_tiltCurrent  = Mathf.Clamp(m_tilt.m_tiltCurrent, -m_tilt.m_tiltMax, m_tilt.m_tiltMax);
        m_tilt.m_tiltCurrent *= m_tilt.m_drag;

        float percentage = m_tilt.m_tiltCurrent / m_tilt.m_tiltMax;
        m_tilt.m_rotationAmount = -percentage * m_tilt.m_rotationSpeedMax * Time.deltaTime; // spin around

    }

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    void Move()
    {
        // rotate
        this.transform.Rotate(0, m_tilt.m_rotationAmount, 0); // spin around 
        // move
        this.transform.Translate(0, 0, m_speed.m_speedForwardCurrent * Time.deltaTime); // uses   the calculated forward to  do  world space


        m_pivotPoint.transform.rotation = this.transform.rotation; // this realigns the pivot point with the main body
        m_pivotPoint.transform.Rotate(m_pitch.m_pitchCurrent, 0, 0, Space.Self); // pitch it forward
        m_pivotPoint.transform.Rotate(0, 0, m_tilt.m_tiltCurrent, Space.Self); // pitch it sideways

    }

    //-------------------------------------------------------------------------------------------------------------------------------
    public Vector3 GetCurrentTranslate(bool lWithDeltaTime)
    {
        float lTime = 1;
        if(lWithDeltaTime == true)
        {
            lTime = Time.deltaTime;
        }
        return new Vector3(0, 0, m_speed.m_speedForwardCurrent * lTime);
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    void SendSpeedToFuel()
    {
        m_Fuel.SetSpeedPercentage1(GetSpeedPercentage());
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
	void FixedUpdate () 
    {
        ApplyDrag();
        Move();

        SendSpeedToFuel();
    }
}
