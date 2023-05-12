using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TurretControlsAI : MonoBehaviour
{

    public float  m_TrackingRange       = 100;
    public float  m_FiringRange         = 80;
    public float  m_WithinArcToFire     = 15;
    public float  m_RotateDeadAngle     = 5;
    private float m_TrackingFiringTimer = 0;
    const float   TRACKING_FIRE_TIME    = 4;




    GameObject m_Target = null;

    [HideInInspector]
    public turret m_Turret;

    private float m_DegreeInX = 0;
    private float m_DegreeInY = 0;
    float m_RangeFloat = float.MaxValue;
    FocusRange m_FocusRange;
    GameManagerDesertStrike m_GameManager;
    //----------------------------------------------------------------------------------------------------------	
    void Start()
    {
        m_Turret = GetComponent<turret>();
        m_FocusRange = GetComponent<FocusRange>();
        m_GameManager = MyGlobals.GetGameManager();
    }

    //----------------------------------------------------------------------------------------------------------
    private void RotateX() // up and down
    {
        if (!m_Turret.m_axisX.m_gameObject)
            return;

        m_DegreeInX = MyGlobals.GetAngle(m_Turret.m_axisX.m_gameObject, m_Target.transform.position, MyGlobals.Direction.upDown);//   Vector3.Dot(targetDir, forward)* Mathf.Rad2Deg;

        if (Mathf.Abs(m_DegreeInX) < m_RotateDeadAngle)
            return;


        if (m_DegreeInX < 0)
            m_Turret.RotateX(1);
        else
            m_Turret.RotateX(-1);
    }

    //----------------------------------------------------------------------------------------------------------
    private void RotateY() // twist
    {
        if (!m_Turret.m_axisY.m_gameObject)
            return;

        m_DegreeInY = MyGlobals.GetAngle(m_Turret.m_axisY.m_gameObject, m_Target.transform.position, MyGlobals.Direction.leftRight);

        if (Mathf.Abs(m_DegreeInY) < m_RotateDeadAngle)
            return;

        if (m_DegreeInY < 0)
            m_Turret.RotateY(-1);
        else
            m_Turret.RotateY(1);
    }

    //----------------------------------------------------------------------------------------------------------
    GameObject GetValidTarget()
    {
        m_Target = null;
        float lDistance = float.PositiveInfinity;
        for (int i = 0; i < m_GameManager.m_PlayerItems.Count; i++)
        {
            Vector3 lPosition = this.transform.position;
            float lThisDistance = MyGlobals.GetFlatDistance(this.gameObject, m_GameManager.m_PlayerItems[i].gameObject);
            if (lThisDistance < lDistance)
            {
                lDistance = lThisDistance;
                m_Target = m_GameManager.m_PlayerItems[i].gameObject;

            }
        }
        return m_Target;
    }

    //----------------------------------------------------------------------------------------------------------
    private bool IsTargetInRange(float lAmount)
    {
        if (m_Target != null)
        {
            m_RangeFloat = Vector3.Distance(m_Target.transform.position, this.transform.position);
            if (m_RangeFloat < lAmount)
                return true;

        }
        return false;
    }


    //----------------------------------------------------------------------------------------------------------
    bool IsWithinAngle()
    {
        return ((Mathf.Abs(m_DegreeInY) + Mathf.Abs(m_DegreeInX)) < m_WithinArcToFire);
    }

    //----------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        m_Target = GetValidTarget();

        if (m_Target == null)
        {
            m_TrackingFiringTimer = 0;
            return;
        }

        m_FocusRange.SetTarget(m_Target);

        if (IsTargetInRange(m_TrackingRange))
        {
            m_TrackingFiringTimer += Time.deltaTime;
            RotateX();
            RotateY();
        }
        else
        {
            m_TrackingFiringTimer = 0;
        }


        if(m_TrackingFiringTimer > TRACKING_FIRE_TIME)
        {
            m_Turret.FireGunAtTarget(m_Target);
            m_TrackingFiringTimer = 0;
        }        
    }

}
