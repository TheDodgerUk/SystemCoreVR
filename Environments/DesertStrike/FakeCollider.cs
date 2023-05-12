using UnityEngine;
using System.Collections;

public class FakeCollider : MonoBehaviour 

{
    
    public GameObject m_objectWithCollider;
    private Collider m_realCollider ;
    Vector3 m_position = new Vector3(0, -100, 0);
    private Collider m_fakeCollider = null;


    void Start()
    {

        

        m_realCollider = m_objectWithCollider.GetComponent<Collider>();
            

        if ( !m_realCollider)
        {
            Debug.LogError("please  add the real collider");
        }



        if (m_realCollider.GetType() == typeof(BoxCollider))
        {

            m_fakeCollider = gameObject.AddComponent<BoxCollider>();
            BoxCollider temp = (BoxCollider)m_fakeCollider;

            temp.size   = m_realCollider.GetComponent<BoxCollider>().size;
            temp.center = m_realCollider.GetComponent<BoxCollider>().center;
        }

        else if (m_realCollider.GetType() == typeof(SphereCollider))
        {
            m_fakeCollider = gameObject.AddComponent<SphereCollider>();
            SphereCollider temp = (SphereCollider)m_fakeCollider;
            temp.radius = m_realCollider.GetComponent<SphereCollider>().radius;
            temp.center = m_realCollider.GetComponent<SphereCollider>().center;
        }

        else if (m_realCollider.GetType() == typeof(CapsuleCollider))
        {
            m_fakeCollider = gameObject.AddComponent<CapsuleCollider>();
            CapsuleCollider temp = (CapsuleCollider)m_fakeCollider;
            temp.radius = m_realCollider.GetComponent<CapsuleCollider>().radius;
            temp.center = m_realCollider.GetComponent<CapsuleCollider>().center;
            temp.direction = m_realCollider.GetComponent<CapsuleCollider>().direction;
            temp.height = m_realCollider.GetComponent<CapsuleCollider>().height;
        }

        else if (m_realCollider.GetType() == typeof(MeshCollider))
        {
            m_fakeCollider = gameObject.AddComponent<MeshCollider>();
            //MeshCollider temp = (MeshCollider)m_fakeCollider;
        }

        else
        {
            Debug.LogError("collider type not found");
        }
		m_fakeCollider.transform.rotation = m_objectWithCollider.transform.rotation;
        m_fakeCollider.transform.position = m_objectWithCollider.transform.position;
        m_fakeCollider.isTrigger = true;
    }

    //
    //
    void FixedUpdate()
    {
		if (!m_realCollider)
		{
			Destroy(this.gameObject);
		}

		// place collider below ground
        m_position = m_objectWithCollider.transform.position;
        m_position.y = -100;
        m_fakeCollider.transform.position = m_position;

		// update
		//m_fakeCollider.transform.rotation = m_objectWithCollider.transform.rotation;
    }
}
