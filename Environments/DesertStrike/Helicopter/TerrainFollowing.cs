using UnityEngine;
using System.Collections;

public class TerrainFollowing : MonoBehaviour
{

    float m_TerrainFollowingHeight  = 20;
    float m_AngleOfMaxAngleForCheck = 45;
    float m_HeightChangeSpeedMax    = 2;
    GameObject m_tempGameObject;
    HelicopterBase m_HelicopterBase;

    Hover m_Hover;
    //---------------------------------------------------------------------------------------------------------
    void Start()
    {
        m_tempGameObject                         = new GameObject("HelicopterTerrainFollowing");
        m_tempGameObject.transform.parent        = this.gameObject.transform;
        m_tempGameObject.transform.localPosition = Vector3.zero;
        m_tempGameObject.transform.localRotation = Quaternion.identity;
        m_tempGameObject.transform.localPosition += (this.transform.forward * 10); // this  just makes it more promentnet

        m_Hover = GetComponent<Hover>();
        m_HelicopterBase = GetComponent<HelicopterBase>(); 
    }

    //---------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        m_tempGameObject.transform.LookAt(this.transform.forward * 10);

        float rotateAmount = m_HelicopterBase.GetSpeedPercentage() * m_AngleOfMaxAngleForCheck; // figures aout the correct angle
        m_tempGameObject.transform.Rotate(90 - rotateAmount, 0, 0);


        //  finds the correct position  with the hover NOT included
        Vector3 raycastPosition = this.transform.position;
        raycastPosition.y -= m_Hover.GetOffset();

        // raycast it 
        RaycastHit hit;
        Physics.Raycast(raycastPosition, m_tempGameObject.transform.forward, out hit);

        // cast position.y - hitposition.y , gives relitve grounddistanceHeight
        float groundDistanceRelativeHeight = raycastPosition.y - hit.point.y;


        if (Mathf.Abs(groundDistanceRelativeHeight - m_TerrainFollowingHeight) < (m_HeightChangeSpeedMax * Time.deltaTime))
        {
            return;
        }

        var translateAmount = m_HeightChangeSpeedMax * Time.deltaTime;
        if (groundDistanceRelativeHeight > m_TerrainFollowingHeight)
        {
            this.transform.Translate(0, -translateAmount, 0, Space.World);
        }
        else
        {
            this.transform.Translate(0, translateAmount, 0, Space.World);
        }
    }

    //---------------------------------------------------------------------------------------------------------
    public void HeightChange(float amount)
    {
        m_TerrainFollowingHeight += amount * Time.deltaTime * m_HeightChangeSpeedMax;
    }

    //---------------------------------------------------------------------------------------------------------
    public float GetTerrainFollowingHeight()
    {
        return m_TerrainFollowingHeight;
    }

}
