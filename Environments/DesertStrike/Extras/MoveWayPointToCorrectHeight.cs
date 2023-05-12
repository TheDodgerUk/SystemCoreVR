using UnityEngine;
using System.Collections;

public class MoveWayPointToCorrectHeight : MonoBehaviour {

    float m_TimeAlter     = 2.0f/1.0f;
    float m_Timer         = 0;
    float m_Height        = 100;
    Vector3 m_OldPosition = Vector3.one;
    Vector3 m_NewPosition = Vector3.one;

    public void SetHeight(float lAmount)
    {
        m_OldPosition = this.transform.position;
        m_NewPosition = new Vector3(this.transform.position.x, lAmount, this.transform.position.z);
        m_Height      = lAmount;
    }

    // Update is called once per frame
    void Update ()
    {
	
        if(m_NewPosition != Vector3.one)
        {
            this.transform.position =  Vector3.Slerp(m_OldPosition, m_NewPosition, m_Timer);
            m_Timer += Time.deltaTime * m_TimeAlter;
        }

        if(m_Timer >= 1)
        {
            this.enabled = false;
        }
	}
}
