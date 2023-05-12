using UnityEngine;
using System.Collections;

public class HomingMissile : MonoBehaviour {

    enum ETargetType
    {
        GAME_OBJECT,
        POSITION,
        NO_TARGET,
    };
    private ETargetType m_targetType = ETargetType.NO_TARGET;
	private GameObject m_targetObject;
	private Vector3    m_targetPosition;

	public float  m_turnSpeed       = 1;
	public float  m_delayBeforeTurn = 1;
	private float m_timer           = 0;

    public float m_randomDistanceForNonTarget = 0;


    public void SetTargetGameObjectMessage(GameObject target) { m_targetObject = target;  m_targetType  = ETargetType.GAME_OBJECT; }
    public void SetTargetPositionMessage(Vector3 target)    { m_targetPosition = target; m_targetType = ETargetType.POSITION; }   
	// Use this for initialization

	// Update is called once per frame
	void FixedUpdate () 
	{
		m_timer += Time.deltaTime;
		if ( m_timer < m_delayBeforeTurn) 
			return;
		
		
		Vector3 targetPosition = Vector3.zero;


        switch (m_targetType)
        {
			case ETargetType.NO_TARGET: 
				return; 
            case ETargetType.GAME_OBJECT: 
				if(m_targetObject)
				{
					targetPosition = m_targetObject.transform.position; 
				}
				else
				{
					return;
				}
				break;
            case ETargetType.POSITION: 
				targetPosition = m_targetPosition; 
				break;
        };


		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - this.transform.position ), m_turnSpeed * Time.deltaTime);

	}
}
