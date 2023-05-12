using UnityEngine;
using System.Collections;

public class CarControlsAI : ControlsAI
{
    public float      m_waypointDistance = 5;
    private WayPoints m_WayPoints;
    private float     m_inputSteer       = 0;
    private float     m_inputTorque      = 0;
    CarControls       m_CarControls;
    SafePlace         m_SafePlace;
    Garage            m_Garage;
    void Awake()
    {
        m_WayPoints   = GetComponentInChildren<WayPoints>();
        m_CarControls = GetComponent<CarControls>();
        m_SafePlace   = GetComponent<SafePlace>();
        m_Garage      = GetComponent<Garage>();
    }

    //------------------------------------------------------------------------------------------
    void TowardsWayPoint()
    {
        GameObject lWaypoint = null;
        if (m_WayPoints.GetCurrentWayPoint(this.gameObject.transform.position, out lWaypoint) == true)
        {

            Vector3 lRelativeWaypointPosition = transform.InverseTransformPoint(lWaypoint.transform.position.x, transform.position.y, lWaypoint.transform.position.z);

            m_inputSteer = lRelativeWaypointPosition.x / lRelativeWaypointPosition.magnitude;
            m_inputSteer = lRelativeWaypointPosition.x / lRelativeWaypointPosition.magnitude;
            if (Mathf.Abs(m_inputSteer) < 0.5)
            {
                float temp = lRelativeWaypointPosition.magnitude - Mathf.Abs(m_inputSteer);
                m_inputTorque = lRelativeWaypointPosition.z / temp;

            }
            else
            {
                m_inputTorque = 0.0f;
            }
        }
        else
        {
            StopCar();
        }  
    }

    //------------------------------------------------------------------------------------------
    void StopCar()
    {
        if (m_CarControls.GetTorque() > 0)
        {
            m_inputTorque = -1.0f;
        }
        else
        {
            m_inputTorque = 1.0f;
        }
    }

    //------------------------------------------------------------------------------------------
    void SafeCar()
    {
        float lCurrentSpeed = m_CarControls.GetTorque();
        if (Mathf.Abs(lCurrentSpeed) < 0.001f)
        {
            Garage();
        }
    }

    //------------------------------------------------------------------------------------------
    void Garage()
    {
        m_Garage.enabled = true;
    }

    //------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        if (m_WayPoints != null)
        {
            TowardsWayPoint();
        }
        else
        {
            m_WayPoints = GetComponentInChildren<WayPoints>();
            m_inputTorque = 1.0f;
        }

        bool lOverSafePlace = m_SafePlace.IsSafePlace();
        if (lOverSafePlace == false)
        {
            m_CarControls.InputSteer(m_inputSteer);
            m_CarControls.InputTorque(m_inputTorque);
        }
        else
        {
            StopCar();
            SafeCar();

        }
    }

}
