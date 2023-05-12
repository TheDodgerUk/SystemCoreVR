using UnityEngine;
using System.Collections;

public class RigidBodyController : MonoBehaviour {


    public enum RigedBodyType
    {
        NoGravity_Kinematic,
        NoGravity_NoKinematic,
        Gravity_Kinematic,
        Gravity_NoKinematic,
        None,
        Manual,
    }

    public enum ColliderTrigger
    {
        Trigger,
        NonTrigger,
    }

    public enum ColliderType
    {
        BoxCollider,
        MeshCollider,
        None,
        Manual,
    }

    [System.Serializable]
    public class RigidBodyControllerItem
    {
        public float m_Mass                      = 1000;
        public GameObject      m_GameObject;
        public ColliderType    m_ColliderType    = ColliderType.BoxCollider;
        public ColliderTrigger m_ColliderTrigger = ColliderTrigger.Trigger;
        public RigedBodyType   m_RigedBodyType   = RigedBodyType.NoGravity_Kinematic;

        public bool            m_ColliderAllChildren = false;
        [HideInInspector]
        public Collider m_Collider;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
    };

    [SerializeField]
    RigidBodyControllerItem[] m_RigidBodyControllerList;
    void Awake()
    {
        for(int i = 0; i < m_RigidBodyControllerList.Length; i++)
        {
            RigidBodyControllerItem lItem = m_RigidBodyControllerList[i];

            SetIndex(i, lItem.m_RigedBodyType, lItem.m_ColliderType, lItem.m_ColliderTrigger);
        }
    }

    //-------------------------------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    public int GetCount()
    {
        return  m_RigidBodyControllerList.Length;
    }

    //-------------------------------------------------------------------------------------------------
    /// <summary></summary>
    /// <param name="lIndex"></param>
    /// <returns></returns>
    public GameObject GetGameObject(int lIndex)
    {
        return  m_RigidBodyControllerList[lIndex].m_GameObject;
    }

    //-------------------------------------------------------------------------------------------------
    /// <summary></summary>
    /// <param name="lIndex"></param>
    /// <param name="lSetup"></param>
    /// <param name="lColliderType"></param>
    /// <param name="lColliderTrigger"></param>
    public void SetIndex(int lIndex, RigedBodyType lSetup, ColliderType lColliderType, ColliderTrigger lColliderTrigger)
    {
        SetCollider(lIndex, lColliderType);
        SetColliderTrigger(lIndex, lColliderTrigger, lColliderType);
        SetSetup(lIndex, lSetup);
    }

    //-------------------------------------------------------------------------------------------------
    void SetColliderTrigger(int lIndex, ColliderTrigger lColliderTrigger, ColliderType lColliderType)
    {
        if (lColliderType == ColliderType.None)
            return;


        switch(lColliderTrigger)
        {

            case ColliderTrigger.Trigger:
                m_RigidBodyControllerList[lIndex].m_Collider.isTrigger = true;
                break;

            case ColliderTrigger.NonTrigger:
                m_RigidBodyControllerList[lIndex].m_Collider.isTrigger = false;
                break;
        }
    }

    //-------------------------------------------------------------------------------------------------
    /// <param name="lColliderType"></param>
    void SetCollider(int lIndex, ColliderType lColliderType)
    {

        //---------------------------------------------
        //      lRemovedCollider needed other wise it not work
        //---------------------------------------------
        bool lRemovedCollider = false;
        switch (lColliderType)
        {
            case ColliderType.Manual:
                m_RigidBodyControllerList[lIndex].m_Collider = m_RigidBodyControllerList[lIndex].m_GameObject.GetComponent<BoxCollider>();
                break;

            case ColliderType.BoxCollider:
                if (m_RigidBodyControllerList[lIndex].m_Collider != null && m_RigidBodyControllerList[lIndex].m_Collider.GetType() != typeof(BoxCollider))
                {
                    lRemovedCollider = true;
                    MeshCollider lBox = (MeshCollider)m_RigidBodyControllerList[lIndex].m_Collider;
                    ColliderCheat lCheat = m_RigidBodyControllerList[lIndex].m_GameObject.GetComponent<ColliderCheat>();
                    if (lCheat != null)
                    {
                        Destroy(lCheat);
                    }
                    Destroy(lBox);
                }

                if (m_RigidBodyControllerList[lIndex].m_Collider == null  || lRemovedCollider == true)
                {
                    m_RigidBodyControllerList[lIndex].m_Collider = m_RigidBodyControllerList[lIndex].m_GameObject.AddComponent<BoxCollider>();
                    m_RigidBodyControllerList[lIndex].m_GameObject.AddComponent<ColliderCheat>();
                }
                
                break;

            case ColliderType.MeshCollider:
                if (m_RigidBodyControllerList[lIndex].m_Collider != null && m_RigidBodyControllerList[lIndex].m_Collider.GetType() != typeof(MeshCollider))
                {
                    lRemovedCollider = true;
                    BoxCollider lBox = (BoxCollider)m_RigidBodyControllerList[lIndex].m_Collider;
                    ColliderCheat  lCheat1 = m_RigidBodyControllerList[lIndex].m_GameObject.GetComponent<ColliderCheat>();
                    if (lCheat1 != null)
                    {
                        Destroy(lCheat1);
                    }
                    Destroy(lBox);
                }

                if (m_RigidBodyControllerList[lIndex].m_Collider == null || lRemovedCollider == true)
                {
                    m_RigidBodyControllerList[lIndex].m_Collider = m_RigidBodyControllerList[lIndex].m_GameObject.AddComponent<MeshCollider>();
                    MeshCollider lTemp = (MeshCollider)m_RigidBodyControllerList[lIndex].m_Collider;
                    m_RigidBodyControllerList[lIndex].m_GameObject.AddComponent<ColliderCheat>();
                    lTemp.convex = true;
                }
                break;

            case ColliderType.None:
                if (m_RigidBodyControllerList[lIndex].m_Collider != null)
                {
                    Destroy(m_RigidBodyControllerList[lIndex].m_Collider);
                }
                ColliderCheat lCheat2 = m_RigidBodyControllerList[lIndex].m_GameObject.GetComponent<ColliderCheat>();
                if (lCheat2 != null)
                {
                    Destroy(lCheat2);
                }

                m_RigidBodyControllerList[lIndex].m_Collider = null;
                break;
        }
        m_RigidBodyControllerList[lIndex].m_ColliderType = lColliderType;
    }


    //-------------------------------------------------------------------------------------------------
    void SetSetup(int lIndex, RigedBodyType lSetup)
    {
        m_RigidBodyControllerList[lIndex].m_Rigidbody = m_RigidBodyControllerList[lIndex].m_GameObject.GetComponent<Rigidbody>();
        switch (lSetup)
        {
            case RigedBodyType.None:
                if (m_RigidBodyControllerList[lIndex].m_Rigidbody != null)
                {
                    Destroy(m_RigidBodyControllerList[lIndex].m_Rigidbody);
                }
                //------------------------------------------------------------------------
                //          EARLY RETURN 
                //------------------------------------------------------------------------
                return;

            case RigedBodyType.Manual:
                break;


            default:
                if (m_RigidBodyControllerList[lIndex].m_Rigidbody == null)
                {
                    m_RigidBodyControllerList[lIndex].m_GameObject.AddComponent<Rigidbody>();
                    m_RigidBodyControllerList[lIndex].m_Rigidbody      = m_RigidBodyControllerList[lIndex].m_GameObject.GetComponent<Rigidbody>();
                    m_RigidBodyControllerList[lIndex].m_Rigidbody.mass = m_RigidBodyControllerList[lIndex].m_Mass;
                }
                break;
        }


        switch (lSetup)
        {
            case RigedBodyType.NoGravity_Kinematic:
                m_RigidBodyControllerList[lIndex].m_Rigidbody.useGravity  = false;
                m_RigidBodyControllerList[lIndex].m_Rigidbody.isKinematic = true;
                break;

            case RigedBodyType.NoGravity_NoKinematic:
                m_RigidBodyControllerList[lIndex].m_Rigidbody.useGravity  = false;
                m_RigidBodyControllerList[lIndex].m_Rigidbody.isKinematic = false;
                break;

            case RigedBodyType.Gravity_Kinematic:
                m_RigidBodyControllerList[lIndex].m_Rigidbody.useGravity  = true;
                m_RigidBodyControllerList[lIndex].m_Rigidbody.isKinematic = true;
                break;

            case RigedBodyType.Gravity_NoKinematic:
                m_RigidBodyControllerList[lIndex].m_Rigidbody.useGravity  = true;
                m_RigidBodyControllerList[lIndex].m_Rigidbody.isKinematic = false;
                break;

        }

    }

    void _OnCollisionEnter(Collision collision)
    {

    }
    void OnCollisionEnter(Collision collision)
    {
        _OnCollisionEnter(collision);
    }


    void _OnTriggerEnter(Collider other)
    {

    }

    void OnTriggerEnter(Collider other)
    {
        _OnTriggerEnter(other);
    }

}
