using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolLoopScript : MonoBehaviour
{
    int m_Index = 0;


    public GameObject m_Parent;
    public GameObject m_PooledObject;
    public int pooledAmount = 20;
    bool willGrow = true;

    List<GameObject> pooledObjects;

    void Start()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject lGameObject = (GameObject)Instantiate(m_PooledObject);
            lGameObject.SetActive(false);
            pooledObjects.Add(lGameObject);
            if(m_Parent != null)
            {
                lGameObject.transform.SetParent(m_Parent.transform);
            }
        }

        m_Index = 0;
    }



    //-------------------------------------------------------------------------------------------
    public void ResetPool()
    {
        m_Index = 0;
        for(int i = 0; i < pooledObjects.Count; i++)
        {
            pooledObjects[i].SetActive(false);
        }
    }


    //-------------------------------------------------------------------------------------------
    public GameObject GetPooledObject()
    {
        GameObject lObject = null;
        if(m_Index <= pooledObjects.Count)
        {
            lObject = pooledObjects[m_Index];
        }
        else
        {
            GameObject lNewGameObject = (GameObject)Instantiate(m_PooledObject);
            pooledObjects.Add(lNewGameObject);
            lObject = lNewGameObject;
            if (m_Parent != null)
            {
                lNewGameObject.transform.parent = m_Parent.transform;
            }
        }

        m_Index++;
        lObject.SetActive(true);
        return lObject;
    }

}