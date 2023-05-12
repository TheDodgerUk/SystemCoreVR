#define KEEP_MOVING_FORWARD
using UnityEngine;
using System.Collections;


public class HelicopterControlsAI : ControlsAI {

    const float   m_LANDING_SPEED = 0.5f;

    GameObject    m_WaypointObject;
	public  float m_slowDownAtRange = 100;   

	private HelicopterControls m_helicopter;
	private TerrainFollowing   m_TerrainFollowing;
	private float              m_currentAngleToTarget;
    private WayPoints          m_WayPoints;
    private SafePlace          m_SafePlace;
    private Hover              m_Hover;
    private Landing            m_Landing;
	// Use this for initialization


	void Awake () 
	{
		m_helicopter       = GetComponent<HelicopterControls>();
		m_TerrainFollowing = GetComponent<TerrainFollowing>();
        m_WayPoints        = GetComponentInChildren<WayPoints>();
        m_SafePlace        = GetComponent<SafePlace>();
        m_Hover            = GetComponent<Hover>();
        m_Landing          = GetComponentInChildren<Landing>();

    }

    // Update is called once per frame
    void FixedUpdate () 
	{
        if(m_WayPoints != null)
        {
            _UpdateMovement();
        }
        else
        {
            m_WayPoints = GetComponentInChildren<WayPoints>();           
        }

        Movement();
        
        //FireGun();
    }

    //--------------------------------------------------------------------------------------------------------------
    /// <summary> disable hover</summary>
    void Landing()
    {
        m_Landing.enabled = true;
    }

    //--------------------------------------------------------------------------------------------------------------
    private void _UpdateMovement()
    {
        m_WaypointObject = null;
        if (m_WayPoints.GetCurrentWayPoint(this.transform.position, out m_WaypointObject) == true)
        {
            m_currentAngleToTarget = MyGlobals.GetAngle(this.gameObject, m_WaypointObject.transform.position, MyGlobals.Direction.leftRight);
        }
    }

    //--------------------------------------------------------------------------------------------------------------
    void CheckHeight()
	{
		//m_TerrainFollowing.HeightChange(-1);		
		//m_TerrainFollowing.HeightChange(1);
	}

    //----------------------------------------------------------------------------------------------------------------
    void TurnTowardsTarget()
	{
        if (m_WaypointObject != null)
        {
            m_helicopter.SetTiltToAmountIfCan(-m_currentAngleToTarget);
        }
	}

	//--------------------------------------------------------------------------------------------------------------
	void AccelerationTowardsTarget()
	{
		float currentSpeed = m_helicopter.m_speed.m_speedForwardCurrent;
		float flatDistance = MyGlobals.GetFlatDistance(this.gameObject, m_WaypointObject);

        float lPercentage     = (100.0f / 90.0f) /100.0f;
        float pitchPercentage = 1.0f - m_currentAngleToTarget* lPercentage;
       // float pitchPercentage = flatDistance/ m_slowDownAtRange;
		if (Mathf.Abs(m_currentAngleToTarget) > 90)
		{
			pitchPercentage *= -1;
		}

		m_helicopter.Pitch(pitchPercentage);
	}

	//-----------------------------------------------------------
	void Movement()
	{
        bool lOverSafePlace = m_SafePlace.IsSafePlace();
        if (m_WaypointObject != null)
        {
            TurnTowardsTarget();
            if (lOverSafePlace == false)
            {
                AccelerationTowardsTarget();
            }
        }
        else
        {

#if KEEP_MOVING_FORWARD
            //-------------------------------------------------
            // keep moving forward  when no waypoints
            //-------------------------------------------------
            if (lOverSafePlace == false)
            {
                m_helicopter.Pitch(100);
            }
#endif

#if !KEEP_MOVING_FORWARD
            //-------------------------------------------------
            // keep moving forward  until waypoints added , this only happens when you have added some
            // this means that until you have set anyway points then it will keep moving forward
            //-------------------------------------------------
            if(m_WayPoints == null)
            {
                m_helicopter.Pitch(100);
            }
#endif
        }


        if (lOverSafePlace == true)
        {
            StopHelicopter();
            LandHelicopter();
        }

        
    }

    //---------------------------------------------------------------------------------------------------------
    void StopHelicopter()
    {
        float lCurrentSpeed = m_helicopter.GetCurrentSpeed();
        if(lCurrentSpeed > 5f)
        {
            m_helicopter.Pitch(-0.8f);
        }

        float lCurrentTilt =  m_helicopter.GetCurrentTilt();
        m_helicopter.SetTiltToAmountIfCan(-lCurrentTilt);

    }

    public Vector3 GetCurrentTranslate(bool lWithDeltaTime)
    {
        return m_helicopter.GetCurrentTranslate(lWithDeltaTime);
    }
    //---------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    void LandHelicopter()
    {
        float lCurrentSpeed = m_helicopter.GetCurrentSpeed();
        if(Mathf.Abs(lCurrentSpeed) < m_LANDING_SPEED)
        {
            Landing();
        }
    }
    //---------------------------------------------------------------------------------------------------------
    void Weapons()
	{
		m_helicopter.SwitchWeaponSystem();
		m_helicopter.FireWeaponSystem();
	}

    //---------------------------------------------------------------------------------------------------------
    void FireGun()
	{
		float gunRange         = m_helicopter.m_gunRange;
		float distanceToTarget = Vector3.Distance(m_WaypointObject.transform.position, m_helicopter.transform.position);

		if (distanceToTarget < gunRange  )
		{
			Vector3 direction = m_WaypointObject.transform.position - this.transform.position;
			direction.y = 0;
			direction.Normalize();
			m_helicopter.FireGun(direction.x, direction.z);
		}
	}
}
