using UnityEngine;
using System.Collections;

public abstract class ColliderScriptBase : MonoBehaviour
{
     public abstract void _OnCollisionWithOtherPlayer(Collision lCollision);


    //------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <param name="lCollider"></param>
    public abstract void _OnTriggerEnterOtherPlayer(Collider lCollider);


    //------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// </summary>
    /// <param name="lCollider"></param>
    public abstract void MESSAGE_DAMAGE(int lAmount);
 
}