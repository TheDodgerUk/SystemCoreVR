using UnityEngine;
using System.Collections;

public class Radar_Turret : MonoBehaviour
{

    public enum RadarTurn
    {
        Normal,
        Stop,
        Reverse,
    }

    Health m_Health;
    GameObject m_Turret;
    Quaternion m_From = Quaternion.Euler(0, 45, 0);
    Quaternion m_To = Quaternion.Euler(0, -45, 0);
    float m_Timer = 0;
    float m_TimerIncrement = 0.5f;
    float m_CurrentHealth = 0;

    ListRandom<RadarTurn> m_RadarTurnListRandom = new ListRandom<RadarTurn>();


    #region Setup
    //-----------------------------------------------------------------------------------------------------
    void SetRandomStart()
    {
        if (Random.Range(0.0f, 1.0f) > 0.5f)
        {
            MyGlobals.Swap(ref m_From, ref m_To);
        }
        m_Timer = Random.Range(0.0f, 1.0f);
    }

    //-----------------------------------------------------------------------------------------------------
    void SetTurret()
    {
        GameObject lParent = this.gameObject;
        Transform[] lTransform = GetComponentsInChildren<Transform>();
        for (int i = 0; i < lTransform.Length; i++)
        {
            if (lParent != lTransform[i].gameObject)
            {
                m_Turret = lTransform[i].gameObject;
                break;
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------
    void SetHealth()
    {
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        m_Health = lParent.GetComponentInChildren<Health>();
    }
    #endregion


    //-----------------------------------------------------------------------------------------------------
    void Awake()
    {
        SetRandomStart();
        SetTurret();
        SetHealth();
        m_RadarTurnListRandom.Clear();
        m_RadarTurnListRandom.Add(RadarTurn.Normal, 50);
    }

    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Message
    /// </summary>
    void MESSAGE_DAMAGE()
    {
        float lCurrentHealth = m_Health.GetPercentage0to1();
        int lPercentage0to100 = (int)(m_CurrentHealth * 100);


        m_RadarTurnListRandom.Clear();
        m_RadarTurnListRandom.Add(RadarTurn.Normal, 80);

        //------------------------------------------------
        //      Stop Amount
        //------------------------------------------------
        if (lPercentage0to100 < 80)
        {
            int lAmount = (80-lPercentage0to100);
            m_RadarTurnListRandom.Add(RadarTurn.Stop, lAmount);
        }

        //------------------------------------------------
        //      reverse Amount
        //------------------------------------------------
        if (lPercentage0to100 < 50)
        {
            int lAmount = (50 -lPercentage0to100);
            m_RadarTurnListRandom.Add(RadarTurn.Reverse, lAmount);
        }

    }


    //-----------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        switch (m_RadarTurnListRandom.GetRandom())
        {
            case RadarTurn.Normal:
                {
                    m_Timer += Time.deltaTime * m_TimerIncrement;
                    if (m_Timer < 0 || m_Timer > 1)
                    {
                        m_Timer = 0;
                        MyGlobals.Swap(ref m_From, ref m_To);
                    }
                    if (m_Turret != null)
                    {
                        m_Turret.transform.localRotation = Quaternion.Slerp(m_From, m_To, m_Timer);
                    }
                }
                break;

            case RadarTurn.Stop:
                break;

            case RadarTurn.Reverse:
                {
                    m_Timer -= Time.deltaTime * m_TimerIncrement;

                    if (m_Timer < 0)
                    {
                        m_Timer = 1;
                        MyGlobals.Swap(ref m_From, ref m_To);
                    }
                    if (m_Turret != null)
                    {
                        m_Turret.transform.localRotation = Quaternion.Slerp(m_From, m_To, m_Timer);
                    }

                }
                break;
        }

    }
}
