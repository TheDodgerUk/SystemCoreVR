using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeathScript_Rotors : DeathScriptBase {

    float COMPINSATE_FOR_VELOSITY = 1.3f;
    float COMPINSATE_FOR_ANGLE_VELOSITY = 1.3f;


    RigidBodyController m_RigidBodyController;
    int m_fixedUpdateCount = 0;
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
        List<System.Type> lList = new List<System.Type>();
        lList.Add(typeof(DeathScript_Rotors));
        MyGlobals.CheckAllComponentsInGameObject(m_RigidBodyController.GetGameObject(3), MyGlobals.ECheckAllComponentsInGameObject.EnableAllInList_AllElseDisable, lList);

        //-----------------------------------------------------------------------------------
        //          Change the RigidBody
        //-----------------------------------------------------------------------------------
        m_RigidBodyController.SetIndex(0, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        m_RigidBodyController.SetIndex(1, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        m_RigidBodyController.SetIndex(2, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        m_RigidBodyController.SetIndex(3, RigidBodyController.RigedBodyType.None, RigidBodyController.ColliderType.None, RigidBodyController.ColliderTrigger.NonTrigger);


        //-----------------------------------------------------------------------------------
        //          add velosity to the base
        //-----------------------------------------------------------------------------------
        Vector3 lCurrentTranslate = this.gameObject.GetComponent<HelicopterControlsAI>().GetCurrentTranslate(false);
        m_RigidBodyController.GetGameObject(0).GetComponent<Rigidbody>().velocity = lCurrentTranslate * COMPINSATE_FOR_VELOSITY;


        //-----------------------------------------------------------------------------------
        //          Rotate the rotors 
        //-----------------------------------------------------------------------------------
        Rotors lRotors = this.gameObject.GetComponent<Rotors>();
        for (int i = 0; i < lRotors.m_rotors.Length; i++)
        {
            Vector3 lDirection = lRotors.GetRotationDirection(i, false);
            m_RigidBodyController.GetGameObject(1 + i).GetComponent<Rigidbody>().angularVelocity = lDirection * COMPINSATE_FOR_ANGLE_VELOSITY;
        }

    }



    //--------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        m_fixedUpdateCount++;


        if (m_fixedUpdateCount == 4)
        {
            //-----------------------------------------------------------------------------------
            //      Disconnect from all afer velosity has been added
            //-----------------------------------------------------------------------------------
            m_RigidBodyController.GetGameObject(1).transform.parent = null;
            m_RigidBodyController.GetGameObject(2).transform.parent = null;
            m_RigidBodyController.GetGameObject(3).transform.parent = null;
        }
    }
}
