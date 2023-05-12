using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeathScript_RotorsWobble : DeathScriptBase {

    RigidBodyController m_RigidBodyController;

    void Awake()
    {
        m_RigidBodyController = this.gameObject.GetComponent<RigidBodyController>();
        this.enabled          = false;
    }

    void OnEnable()
    {
        // 0 body
        // 1 rotor main 
        // 2 rotor tail
        // 3 overall

        //-----------------------------------------------------------------------------------
        //  disable all but this 
        //-----------------------------------------------------------------------------------
        /*
        List<System.Type> lList = new List<System.Type>();
        lList.Add(typeof(DeathScript_Rotors));
        MyGlobals.CheckAllComponentsInGameObject(m_RigidBodyController.GetGameObject(3), MyGlobals.ECheckAllComponentsInGameObject.EnableAllInList_AllElseDisable, lList);


        m_RigidBodyController.SetIndex(0, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        m_RigidBodyController.SetIndex(1, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        m_RigidBodyController.SetIndex(2, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        m_RigidBodyController.SetIndex(3, RigidBodyController.RigedBodyType.None, RigidBodyController.ColliderType.None, RigidBodyController.ColliderTrigger.NonTrigger);

    */
        Vector3 lAngle = m_RigidBodyController.GetGameObject(1).transform.rotation.eulerAngles;
        lAngle.y += 5;
        m_RigidBodyController.GetGameObject(1).transform.rotation = Quaternion.Euler(lAngle);
    }
}
