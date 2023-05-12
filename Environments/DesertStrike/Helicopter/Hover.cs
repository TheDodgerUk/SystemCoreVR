using UnityEngine;
using System.Collections;

public class Hover : MonoBehaviour {

    public float m_sinSpeed = 1f;
    public float m_maxSinWaveAmount = 1;
    private float m_timer = 0;
    private float m_maxAmount;
    
    private float m_tiltMulitplyer;
    private float m_offSet;

    private HelicopterControls m_helicopterControls;
	// Use this for initialization
	void Start () 
    {
        m_helicopterControls = this.GetComponent<HelicopterControls>();
        m_maxAmount          = m_helicopterControls.m_speed.m_speedForwardMax + m_helicopterControls.m_tilt.m_tiltMax;
        m_tiltMulitplyer     = m_helicopterControls.m_speed.m_speedForwardMax / m_helicopterControls.m_tilt.m_tiltMax;
        
	}

    public float GetOffset() { return m_offSet; }
	// Update is called once per frame
	void FixedUpdate ()    
    {

        float current   = m_helicopterControls.m_speed.m_speedForwardCurrent + ( m_helicopterControls.m_tilt.m_tiltCurrent *m_tiltMulitplyer);

        float multiplyer = 1 - Mathf.Abs((current / m_maxAmount)); // will give 1 if no movement  and zero if all movement
	    m_timer += Time.deltaTime;
        m_offSet = Mathf.Sin(m_timer * m_sinSpeed) * (m_maxSinWaveAmount * multiplyer);
	}
}
