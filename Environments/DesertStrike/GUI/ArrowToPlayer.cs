using UnityEngine;
using System.Collections.Generic;

public class ArrowToPlayer : MonoBehaviour
{
    List<GameObject> m_ListPlayers = new List<GameObject>();

    [SerializeField]
    ObjectPoolLoopScript m_ArrowPool;

    [SerializeField]
    ObjectPoolLoopScript m_BoxPool;

    Vector3 m_ScreenCenter = Vector3.zero;
    Vector3 m_ScreenBounds = Vector3.zero;

    void Awake()
    {
        m_ScreenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        m_ScreenBounds = m_ScreenCenter * 0.9f;
    }


    public void SetObjects(List<GameObject> lList)
    {
        m_ListPlayers = lList;
    }


    //----------------------------------------------------------------------------------------------------
    void Update()
    {
        m_ArrowPool.ResetPool();
        m_BoxPool.ResetPool();
        for (int i = 0; i < m_ListPlayers.Count; i++)
        {           
            Vector3 lScreenPosition = Camera.main.WorldToScreenPoint(m_ListPlayers[i].transform.position);

            if (lScreenPosition.z < 0)
            {
                //screenpos *= -1;
                lScreenPosition.x = Screen.width  - lScreenPosition.x;
                lScreenPosition.y = Screen.height - lScreenPosition.y;
            }

            if (lScreenPosition.z > 0 &&
                lScreenPosition.x > 0 && lScreenPosition.x < Screen.width &&
                lScreenPosition.y > 0 && lScreenPosition.y < Screen.height)
            {
                GameObject lBox = m_BoxPool.GetPooledObject();
                lBox.SetActive(true);
                lScreenPosition -= m_ScreenCenter;
                lScreenPosition.z = 0;
                lBox.transform.localPosition = lScreenPosition;
            }
            else
            {
                GameObject lArrow = m_ArrowPool.GetPooledObject();
                lArrow.SetActive(true);
                lScreenPosition -= m_ScreenCenter;
                float lAngle = Mathf.Atan2(lScreenPosition.y, lScreenPosition.x);
                lAngle -= 90 * Mathf.Deg2Rad;
                float lCos = Mathf.Cos(lAngle);
                float lSIn = -Mathf.Sin(lAngle);

                lScreenPosition = m_ScreenCenter + new Vector3(lSIn * 250, lCos * 250, 0);
                float m = lCos / lSIn;

                if (lCos > 0)
                {
                    lScreenPosition = new Vector3(m_ScreenBounds.y / m, m_ScreenBounds.y, 0); // up
                }
                else
                {
                    lScreenPosition = new Vector3(-m_ScreenBounds.y /m, -m_ScreenBounds.y, 0); //down
                }

                if (lScreenPosition.x > m_ScreenBounds.x)
                {
                    lScreenPosition = new Vector3(m_ScreenBounds.x, m_ScreenBounds.x * m, 0);
                }
                else if (lScreenPosition.x < -m_ScreenBounds.x)
                {
                    lScreenPosition = new Vector3(-m_ScreenBounds.x, -m_ScreenBounds.x * m, 0);
                }

                //lScreenPosition += m_ScreenCenter; // not needed with new style 
                lArrow.transform.localPosition = lScreenPosition;
                lArrow.transform.localRotation = Quaternion.Euler(0, 0, lAngle * Mathf.Rad2Deg);
            }
        }
    }



}﻿

