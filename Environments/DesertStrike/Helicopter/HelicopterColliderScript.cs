using UnityEngine;
using System.Collections;

public class HelicopterColliderScript : ColliderScriptBase
{

    //------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <param name="lCollision"></param>
    public override void _OnCollisionWithOtherPlayer(Collision lCollision)
    {
        if(lCollision.gameObject.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Player))
        {
            GameObject lMe    = MyGlobals.GetBaseParentObject(this.gameObject);
            GameObject lOther = MyGlobals.GetBaseParentObject(lCollision.gameObject);

            lMe.SendMessage(MyGlobals.Messages.StartRandomDeathScript, SendMessageOptions.DontRequireReceiver);
            lOther.SendMessage(MyGlobals.Messages.StartRandomDeathScript, SendMessageOptions.DontRequireReceiver);
        }

    }

    //------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <param name="lCollider"></param>
    public override void _OnTriggerEnterOtherPlayer(Collider lCollider)
    {
        if (lCollider.gameObject.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Player))
        {
            GameObject lMe    = MyGlobals.GetBaseParentObject(this.gameObject);
            GameObject lOther = MyGlobals.GetBaseParentObject(lCollider.gameObject);

            lMe.SendMessage(MyGlobals.Messages.StartRandomDeathScript, SendMessageOptions.DontRequireReceiver);
            lOther.SendMessage(MyGlobals.Messages.StartRandomDeathScript, SendMessageOptions.DontRequireReceiver);
        }

    }

    //------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <param name="lCollider"></param>
    public override void MESSAGE_DAMAGE(int lAmount)
    {

    }



}
