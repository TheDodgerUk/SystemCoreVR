using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WayPoints : MonoBehaviour {


    List<GameObject> m_Points = new List<GameObject>();
    [SerializeField]
    float m_MaxDistanceToCreate = 0.2f;

    GameObject m_RootObject;

    [SerializeField]
    float m_RemoveAtDistance = 10;

    Color m_Colour;

    public void SetColour(Color lColour)
    {
        m_Colour = lColour;
    }


    //------------------------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void Awake()
    {
        m_RootObject                  = new GameObject();
        m_RootObject.name             = this.gameObject.name + "waypoints";
    }

    public void OnDrawGizmos()
    {
        if(m_Points.Count != 0)
        {

            Gizmos.DrawWireSphere(m_Points[0].transform.position, m_RemoveAtDistance);
        }
    }

    public void SetRemoveDistance(float lRemoveAtDistance)
    {
        m_RemoveAtDistance = lRemoveAtDistance;
    }
    //------------------------------------------------------------------------------------------------------------------
    public void AddPoint(Vector3 lPoint, float lAboveAmount)
    {
        
        if (m_Points.Count == 0)
        {
            _Add(lPoint, lAboveAmount);
        }
        else
        {
            float dist = Vector3.Distance(m_Points[m_Points.Count - 1].transform.position, lPoint);
            if(dist > m_MaxDistanceToCreate)
            {
                _Add(lPoint, lAboveAmount);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------
    private void _Add(Vector3 lPoint, float lAboveAmount)
    {        
        GameObject lSphere                              = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lSphere.transform.position                      = lPoint;
        lSphere.layer                                   = InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Waypoint);
        lSphere.transform.parent                        = m_RootObject.transform.transform;
        lSphere.GetComponent<Renderer>().material.color = m_Colour;
        m_Points.Add(lSphere);

        MoveWayPointToCorrectHeight lMoveWayPointToCorrectHeight = lSphere.AddComponent<MoveWayPointToCorrectHeight>();
        lMoveWayPointToCorrectHeight.SetHeight(lAboveAmount);

        lSphere.name = m_Points.Count.ToString();
    }

    //------------------------------------------------------------------------------------------------------------------
    public void ClearAll()
    {
        for (int i = 0; i < m_Points.Count; i++)
        {
            Destroy(m_Points[i].gameObject);
        }

        m_Points.Clear();
    }


    //------------------------------------------------------------------------------------------------------------------
    private void _Remove(Vector3 lPosition)
    {
        if (m_Points.Count != 0)
        {
            GameObject lFirst = m_Points[0];
            float dist = MyGlobals.GetFlatDistance(lFirst, lPosition);
            if (dist < m_RemoveAtDistance)
            {
                Destroy(lFirst.gameObject);
                m_Points.RemoveAt(0);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------
	public bool GetCurrentWayPoint(Vector3 lPosition, out Vector3 lWayPoint)
	{
		lWayPoint = Vector3.zero;
		
		_Remove(lPosition);
		if(m_Points.Count != 0)
		{
			lWayPoint = m_Points[0].transform.position;
			return true;
		}
		
		return false;
	}

    //------------------------------------------------------------------------------------------------------------------
    public bool GetCurrentWayPoint(Vector3 lPosition, out GameObject lWayPointObject)
    {
        lWayPointObject = null;
        _Remove(lPosition);
        if (m_Points.Count != 0)
        {
            lWayPointObject = m_Points[0];
            return true;
        }

        return false;
    }


}
