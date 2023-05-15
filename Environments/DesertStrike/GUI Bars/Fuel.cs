using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;


public class Fuel : MonoBehaviour
{
    [SerializeField]
    float m_StandardFuelUsedAmount = 1.0f/5.0f;

    float m_SpeedPercentage1 = 0;

    [SerializeField]
    private GameObject m_FloatingBarObject;
    private FloatingBar m_FloatingBarScipt;



    [SerializeField]
    private float m_MaxAmount;
    private float m_CurrentAmount;
    //---------------------------------------------------------------------------------------------
    // Use this for initialization
    void Awake()
    {
        m_CurrentAmount     = m_MaxAmount;
        
       // m_FloatingBarScipt  = m_FloatingBarObject.GetComponent<FloatingBar>();
    }

    //------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <param name="lAmount"></param>
    public void AddAmount(float lAmount)
    {
        SendAmount(lAmount);
    }

    //------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <param name="lAmount"></param>
    public void RemoveAmount(float lAmount)
    {
        SendAmount(-lAmount);
    }

    //------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// <param name="lAmount"></param>
    void SendAmount(float lAmount)
    {
        m_CurrentAmount += lAmount;
        m_CurrentAmount = Mathf.Clamp(m_CurrentAmount, 0, m_MaxAmount);
        if (m_FloatingBarScipt != null)
        {
            m_FloatingBarScipt.SetFloatingToPercentage(m_CurrentAmount / m_MaxAmount);
        }
    }

    public void SetSpeedPercentage1(float lAmount)
    {
        m_SpeedPercentage1 = lAmount;
    }
    //------------------------------------------------------------------------------------------------------------------------------------
    void Update()
    {
        //RemoveAmount(Time.deltaTime * m_StandardFuelUsedAmount);
        //RemoveAmount(Time.deltaTime * Mathf.Abs(m_SpeedPercentage1));
       // if (m_CurrentAmount <= 0)
        {
       //     this.GetComponent<DeathScriptRandomiser>().StartRandomDeathScript();
        }
    }
}
