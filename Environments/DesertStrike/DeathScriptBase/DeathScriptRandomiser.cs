using UnityEngine;
using System.Collections;


public interface IDeathScriptRandomiser
{
    void StartRandomDeathScript();
}

public class DeathScriptRandomiser : MonoBehaviour , IDeathScriptRandomiser
{
    ListRandom<int> m_List = new ListRandom<int>();
    [System.Serializable]
    public class DeathScriptRandomiserList
    {
        public DeathScriptBase m_DeathScript;
        public int m_Number = 1;

    };

    public DeathScriptRandomiserList[] m_DeathScripts;
    
    
    //----------------------------------------------------------------------------------------------------------------------
    void Awake()
    {
        for (int i = 0; i < m_DeathScripts.Length; i++)
        {
            m_List.Add(i, m_DeathScripts[i].m_Number);            
        }
    }

    //----------------------------------------------------------------------------------------------------------------------
    public void StartRandomDeathScript()
    {
        GameObject lParent   = MyGlobals.GetBaseParentObject(this.gameObject);


        //--------------------------------------------------------
        // clear all waypoints
        //--------------------------------------------------------
        WayPoints lWayPoints = lParent.GetComponent<WayPoints>();
        if(lWayPoints !=null)
        {
            lWayPoints.ClearAll();
        }

        //--------------------------------------------------------
        // Remove all the  floating bars 
        //--------------------------------------------------------
        FloatingBar[] lList = GetComponentsInChildren<FloatingBar>();
        {
            for(int i = 0; i < lList.Length; i++)
            {
                Destroy(lList[i].gameObject);
            }
        }

        //--------------------------------------------------------
        // change all sub layers to crashed
        //--------------------------------------------------------
        MyGlobals.ChangeAllLayersTo(this.gameObject, InfoManager.LayerInfo.Crashed);

        int lIndex = m_List.GetRandom();
        m_DeathScripts[lIndex].m_DeathScript.enabled = true;
    }

    // Use this for initialization


}
