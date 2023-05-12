using UnityEngine;
using System.Collections;

public class ColliderCheat : MonoBehaviour {

    [SerializeField]
    bool m_IgnoreTerrain = true;

    //------------------------------------------------------------------------------------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        GameObject lOther  = MyGlobals.GetBaseParentObject(collision.gameObject);

        if(lParent != lOther && Ignore(lOther) == false)
        {
            lParent.SendMessage(MyGlobals.Messages._OnCollisionWithOtherPlayer, collision, SendMessageOptions.DontRequireReceiver);
        }
    }


    //------------------------------------------------------------------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        GameObject lOther = MyGlobals.GetBaseParentObject(other.gameObject);
        if (lParent != lOther && Ignore(lOther) == false)
        {
            lParent.SendMessage(MyGlobals.Messages._OnTriggerEnterOtherPlayer, other, SendMessageOptions.DontRequireReceiver);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------
    bool Ignore(GameObject lOther)
    {
        if (m_IgnoreTerrain == true && lOther.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Terrain))
        {
            return true;
        }
        return false;
    }

}
