using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    InfoManager.LayerInfo[] m_IgnoreLayer;



    [System.Serializable]
    public class ImpactEffect
    {
        public string     m_MaterialName;
        public GameObject m_ImpactEffect;
        public bool       m_ReflectImpactVector      = false;
        public bool       m_OverrideVectorToVirtical = false;
        public AudioClip  m_ImpactAudio;
    }


    public ImpactEffect[] m_ImpactEffects = new ImpactEffect[4];


    [System.Serializable]
    public class Creation
    {
        public GameObject[] m_TurnOnCreation;
        public GameObject   m_CreationEffect;
        public AudioClip    m_CreationAudio;
    }
    [SerializeField]
    Creation m_Creation;
    [SerializeField]
    float    m_Damage        = 10;
    [SerializeField]
    float    m_Velocity      = 10;
    [SerializeField]
    bool     m_EnableGravity = false;

    //----------------------------------------------------------
    // time and distance
    [System.Serializable]
    public enum ETimeDistance
    {
        Time,
        Distance,
        AllwaysOn,
    }

    [System.Serializable]
    public class TimeDistance
    {
        public ETimeDistance m_timeDistance;
        public float m_amountBeforeDestroyMin = 10;
        public float m_amountBeforeDestroyMax = 10;
        [HideInInspector]
        public float m_amountBeforeDestroyResult = 0;
        public float m_amountBeforeActive = 0.1f;
        [HideInInspector]
        public float m_amount = 0;
        [HideInInspector]
        public Vector3 m_startPosition;
    }

    public TimeDistance m_timeDistance;
    //----------------------------------------------------------
    void Awake()
    {
        if(m_timeDistance.m_timeDistance != ETimeDistance.AllwaysOn)
        {
            this.gameObject.GetComponent<Collider>().enabled = false;
        }

        this.GetComponent<Rigidbody>().useGravity = m_EnableGravity;
        m_timeDistance.m_amountBeforeDestroyResult = Random.Range(m_timeDistance.m_amountBeforeDestroyMin, m_timeDistance.m_amountBeforeDestroyMax);       
    }

    //--------------------------------------------------------------------------------------------------------------
    void Start()
    {
        m_timeDistance.m_startPosition = this.transform.position;

        if (m_Creation.m_CreationAudio)
        {
            AudioSource.PlayClipAtPoint(m_Creation.m_CreationAudio, this.transform.position);
        }



        if (m_Creation.m_CreationEffect)
            Instantiate(m_Creation.m_CreationEffect, transform.position, transform.rotation);

        for(int i = 0; i < m_Creation.m_TurnOnCreation.Length; i++)
        {
            m_Creation.m_TurnOnCreation[i].SetActive(true);
        }
    }

    //---------------------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_Velocity != 0)
        {
            this.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * m_Velocity; // move with constant amount
        }


        // test if  activate the collider or not 
        float amount = 0.0f;
        switch (m_timeDistance.m_timeDistance)
        {
            case ETimeDistance.Time:
                {
                    m_timeDistance.m_amount += Time.deltaTime;
                    amount = m_timeDistance.m_amount * m_timeDistance.m_amount; //this needs  To be squared for speed			
                }
                break;

            case ETimeDistance.Distance:
                {
                    Vector3 distance = (this.transform.position - m_timeDistance.m_startPosition);
                    amount = distance.sqrMagnitude; // this is not rooted for speed
                }
                break;

        }

        if (m_timeDistance.m_timeDistance != ETimeDistance.AllwaysOn)
        {
            // enable collider if need to 
            if (this.GetComponent<Collider>().enabled == false && amount > (m_timeDistance.m_amountBeforeActive * m_timeDistance.m_amountBeforeActive))
            {
                //this needs  To be squared for speed, cause amount has been squared
                this.GetComponent<Collider>().enabled = true;
            }

            // destroy object when need to 
            if (amount > (m_timeDistance.m_amountBeforeDestroyResult * m_timeDistance.m_amountBeforeDestroyResult))
            {
                //this needs  To be squared for speed, cause amount has been squared
                Destroy(this.gameObject);
            }
        }

    }

    //--------------------------------------------------------------------------------------------------------
    void OnCollisionEnter(Collision col)
    {
        bool lSendMessage = false;
        if(m_IgnoreLayer.Length == 0)
        {
            lSendMessage = true;
        }
        else
        {
            lSendMessage = true;
            int lLayer = col.collider.gameObject.layer;

            for (int i = 0; i < m_IgnoreLayer.Length; i++)
            {
                int lCheckLayer = InfoManager.Instance.GetOnlyThisLayer(m_IgnoreLayer[i]);
                if (lLayer == lCheckLayer)
                {
                    lSendMessage = false;
                    break;
                }
            }
        }
        
        if(lSendMessage == true)
        {
            ImpactBehaviour(col.collider.gameObject, this.transform.position, col.contacts[0].normal, this.transform.rotation);
            Destroy(this.gameObject);
        }
    }

    //----------------------------------------------------------
    public void ImpactBehaviour(GameObject impactGameObject, Vector3 position, Vector3 impactsNormal, Quaternion rotation)
    {
        impactGameObject.BroadcastMessage(MyGlobals.Messages.MESSAGE_DAMAGE, m_Damage, SendMessageOptions.DontRequireReceiver);

        if (m_ImpactEffects.Length != 0)
        {
            string materialName = impactGameObject.GetComponent<Collider>().material.name;

            GameObject lImpactEffect = null;
            AudioClip lImpactSound   = null;
            bool lReflect            = false;
            bool lVertical           = false;
            bool lFound              = false;
            for (int i = 0; i < m_ImpactEffects.Length; i++)
            {
                if (m_ImpactEffects[i].m_MaterialName == materialName)
                {
                    lImpactEffect = m_ImpactEffects[i].m_ImpactEffect;
                    lImpactSound  = m_ImpactEffects[i].m_ImpactAudio;
                    lReflect      = m_ImpactEffects[i].m_ReflectImpactVector;
                    lVertical     = m_ImpactEffects[i].m_OverrideVectorToVirtical;
                    lFound        = true;
                    break;
                }
            }

            // cant find a match then uses the first one
            if (lFound == false)
            {
                lImpactEffect = m_ImpactEffects[0].m_ImpactEffect;
                lImpactSound  = m_ImpactEffects[0].m_ImpactAudio;
                lReflect      = m_ImpactEffects[0].m_ReflectImpactVector;
                lVertical     = m_ImpactEffects[0].m_OverrideVectorToVirtical;
            }

            if (lImpactEffect != null)
            {
                // assign to vector so can refelct
                // could be changed to optimize , but less chance of going wrong 
                // and easyer to read
                Vector3 direction = rotation.eulerAngles;
                if (lReflect)
                    direction = Vector3.Reflect(direction, impactsNormal);

                if (lVertical)
                    direction = Vector3.up;

                // effects and sound
                Instantiate(lImpactEffect, position, Quaternion.Euler(direction));
            }

            if (lImpactSound != null)
            {
                AudioSource.PlayClipAtPoint(lImpactSound, position);
            }
        }



    }









}
