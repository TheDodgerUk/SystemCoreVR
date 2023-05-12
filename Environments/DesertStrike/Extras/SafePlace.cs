using UnityEngine;
using System.Collections;

public class SafePlace : MonoBehaviour {

    GameObject m_Parent;
    public InfoManager.LayerInfo m_SafePlace = InfoManager.LayerInfo.Landing;


    void Awake()
    {
        m_Parent = MyGlobals.GetBaseParentObject(this.gameObject);
    }


    //----------------------------------------------------------------------------------------------------------------
    public bool  IsSafePlace()
    {
        if (Physics.Raycast(m_Parent.transform.position, Vector3.down, Mathf.Infinity, InfoManager.Instance.GetOnlyThisLayerMask(m_SafePlace)) == true)
        {
            return true;
        }
        return false;
    }
    
}
