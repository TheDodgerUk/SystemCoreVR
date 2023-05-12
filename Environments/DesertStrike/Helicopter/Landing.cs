using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Landing : MonoBehaviour
{

    enum State
    {
        Landing,
        Landed,
        LandedAndFullyStoped,
        FullyShrunk,
    }

    private Rotors m_Rotors;
    float          m_LandingSpeed = 1.0f/ 5.0f;
    State          m_State        = State.Landing;
    Vector3 m_StartingPoint       = Vector3.zero;
    Vector3 m_EndPoint            = Vector3.zero;
    float m_TimeStep              = 0;

    //--------------------------------------------------------------------------------------------------------------------------------------------
    void Awake()
    {
        this.enabled       = false;
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        m_Rotors           = lParent.GetComponent<Rotors>();
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------
    void OnEnable()
    {

        List<System.Type> lList = new List<System.Type>();
        lList.Add(typeof(HelicopterControls));
        lList.Add(typeof(HelicopterControlsAI));
        lList.Add(typeof(Hover));
        lList.Add(typeof(TerrainFollowing));

        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        MyGlobals.CheckAllComponentsInGameObject(lParent, MyGlobals.ECheckAllComponentsInGameObject.DisableAllInList, lList);

        List<System.Type> lListHealth = new List<System.Type>();
        lListHealth.Add(typeof(Health));
        MyGlobals.CheckAllGameObjectsWithComponents(lParent, MyGlobals.ECheckAllGameObjectsWithComponents.DisableAllInList, lListHealth);


        MyGlobals.ClearWaypointsFromParent(lParent);

        m_StartingPoint = lParent.transform.position;
        m_EndPoint      = m_StartingPoint;
        m_EndPoint.y    = 0;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        RigidBodyController lRigidBodyController = lParent.GetComponent<RigidBodyController>();
        GameObject lGameObject = lRigidBodyController.GetGameObject(0);

        switch (m_State)
        {
            case State.Landing:
                this.transform.position = Vector3.Slerp(m_StartingPoint, m_EndPoint, m_TimeStep);
                m_TimeStep += Time.deltaTime * m_LandingSpeed;
                break;

            case State.Landed:
                float lSpeed = lGameObject.GetComponent<Rigidbody>().velocity.magnitude;
                if (lSpeed < 0.5)
                {
                    MyGlobals.ChangeAllLayersTo(this.gameObject, InfoManager.LayerInfo.Safe);
                    lGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    m_State = State.LandedAndFullyStoped;
                    MyGlobals.BroadcastAll(MyGlobals.Messages.MESSAGE_CANVAS_ADD_SCORE, 10);                                                //-------------------------------------
                }
                break;

            case State.LandedAndFullyStoped:
                lGameObject.transform.localScale = Vector3.Slerp(lGameObject.transform.localScale, Vector3.zero, Time.deltaTime);   //-------------------------------------
                if (MyGlobals.IsVector3Same(lGameObject.transform.localScale, Vector3.zero, 1) == true)
                {
                    m_State = State.FullyShrunk;
                }
                break;

            case State.FullyShrunk:
                Destroy(MyGlobals.GetBaseParentObject(lGameObject));                                                                //-------------------------------------
                break;

        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        if (contact.thisCollider.gameObject.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Landing) == true)
        {
            ContactWithLandingPad();
        }
    }


    //--------------------------------------------------------------------------------------------------------------------------------------------
    void OnTriggerEnter(Collider lCollider)
    {
        if (lCollider.gameObject.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Landing) == true)
        {
            ContactWithLandingPad();
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------
    void ContactWithLandingPad()
    {
        m_Rotors.SetRotorState(Rotors.RotorState.PowerDown);
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);


        RigidBodyController lRigidBodyController = lParent.GetComponent<RigidBodyController>();

        // 0 body
        // 1 rotor main 
        // 2 rotor tail
        // 3 overall
        lRigidBodyController.SetIndex(0, RigidBodyController.RigedBodyType.Gravity_NoKinematic, RigidBodyController.ColliderType.MeshCollider, RigidBodyController.ColliderTrigger.NonTrigger);
        lRigidBodyController.SetIndex(3, RigidBodyController.RigedBodyType.None, RigidBodyController.ColliderType.None, RigidBodyController.ColliderTrigger.NonTrigger);
        // lRigidBodyController.SetIndex(0, RigidBodyController.RigedBodyType.None, RigidBodyController.ColliderType.None, RigidBodyController.ColliderTrigger.NonTrigger);
        m_State = State.Landed;
    }

}
