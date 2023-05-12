using UnityEngine;
using System.Collections;

public class AddSmokeTrail : MonoBehaviour {

	public GameObject m_smokeTrail;
    public GameObject m_smokeTrailEmitPoint;
    public float m_timeAfterMainDestroy = 2;
	private GameObject m_smokeTrailClone = null;
	// Use this for initialization
	void OnDisable()
	{
		///m_smokeTrailClone.GetComponent<ParticleEmitter>().emit = false;
	}

	void OnEnable()
	{
		////m_smokeTrailClone.GetComponent<ParticleEmitter>().emit = true;
	}


	void Awake()
	{
		if (m_smokeTrail == null)
		{
			Debug.LogError("need a smoke trail on object: " + gameObject);
			return;
		}
		
		if (m_smokeTrailEmitPoint)
		{
			m_smokeTrailClone = Instantiate(m_smokeTrail, m_smokeTrailEmitPoint.transform.position, m_smokeTrailEmitPoint.transform.rotation) as GameObject;
			m_smokeTrailClone.transform.parent = m_smokeTrailEmitPoint.transform;
		}
		else
		{
			m_smokeTrailClone = Instantiate(m_smokeTrail, this.transform.position, this.transform.rotation) as GameObject;
			m_smokeTrailClone.transform.parent = this.transform;
		}

	}
	void Start () 
    {


	}
	


    void OnDestroy()
    {
		if (m_smokeTrailClone)
		{
			m_smokeTrailClone.transform.parent = null;
        	Destroy(m_smokeTrailClone, m_timeAfterMainDestroy);
		}
    }
}
