using UnityEngine;
using System.Collections;

abstract public class WeaponSystemBase : MonoBehaviour
{
	public string       m_name;
	
	[System.Serializable]
	public class WeaponPoints
	{
		public GameObject m_weaponPoint;
		public string m_animationName;
	}
	
	
	public WeaponPoints[] m_weaponPoints = new WeaponPoints[1];
	protected int         m_weaponPointCurrent = 0;
	
	
	
	public enum FireAtType
	{
		Forward,
		Position,
		Direction,
		DirectionAndPosition,
		Object,
		Collider,
		Overide,
	};
	
	public enum FireType
	{
		OneAfterAnother,
		AllAtOnce,
		InPairs,
	};
	
	[System.Serializable]
	public class Delay
	{
		public float m_delayTimeBetweenFiring = 1;
		[HideInInspector]
		public float m_delayTime = 0;
	}
	
	
	
	[System.Serializable]
	public class Ammo
	{
		public GameObject   	m_projectile;
		public int  m_ammoMax = 100;
		public bool m_ammoInfinite = false;
		[HideInInspector]
		public int  m_ammoCurrent = 0;
		
	};
	
	
	
	
	public Ammo 			 m_ammo;
	public FireType          m_fireType;
	public Delay             m_delay;
	
	
	protected FireAtType       m_fireAtType = FireAtType.Forward;
	
	[System.Serializable]
	public class Sounds
	{
		public AudioClip m_weaponFireSound;
		public AudioClip m_weaponEmptySound;
		
	};
	
	public Sounds m_sounds;

	protected Vector3   	m_fireTypeDirection;
	protected Vector3   	m_fireTypePosition;
	protected Quaternion    m_fireTypeQuaternion;
	protected GameObject   	m_fireTypeObject;
	protected Collider   	m_fireTypeCollider;
	
	
	
	abstract public      void ReloadAmmo();
	abstract protected   bool FireWeapon();
	
	
	void Awake()
	{
		if (m_ammo.m_projectile == null)
		{
			Debug.LogError(" m_projectile has not been set on:" + gameObject);
		}
	}
	
	
	//---------------------------------------------------------------------------------------------------------
	private void m_playAnimations()
	{
		switch(m_fireType)
		{
		case FireType.OneAfterAnother:
		{
			if ( m_weaponPoints[m_weaponPointCurrent].m_animationName != string.Empty)
			{
				Debug.LogError("animation: " + m_weaponPoints[m_weaponPointCurrent].m_animationName);
				GetComponent<Animation>().Play(m_weaponPoints[m_weaponPointCurrent].m_animationName);
			}
		}
			break;
		default:
		{
			for (int i = 0; i < m_weaponPoints.Length; i++)
			{
				if ( m_weaponPoints[i].m_animationName != string.Empty)
				{
					Debug.LogError("animation: " + m_weaponPoints[i].m_animationName);
					GetComponent<Animation>().Play(m_weaponPoints[i].m_animationName);
				}
			}
		}
			break;
		}
	}
	
	
	
	private void m_resetDelayTimer()
	{
		m_delay.m_delayTime = m_delay.m_delayTimeBetweenFiring;
	}
	
	//----------------------------------------------------
	public bool GetHasEnoughTimePassed()
	{
		return (m_delay.m_delayTime <= 0);
	}
	
	//----------------------------------------------------
	public virtual bool GetHasEnoughAmmo()
	{
		bool returnValue = false;
		// return false if no ammo
		if (!m_ammo.m_ammoInfinite)
		{
			switch (m_fireType)
			{
			case FireType.OneAfterAnother:
				if (m_ammo.m_ammoCurrent >= 1)
					returnValue = true;
				break;
			case FireType.InPairs:
				if (m_ammo.m_ammoCurrent >= 2)
					returnValue = true;
				break;
			case FireType.AllAtOnce:
				if (m_ammo.m_ammoCurrent >= m_weaponPoints.Length)
					returnValue = true;
				break;
			}
		}
		else
		{
			return true;
		}
		
		

		return returnValue;
	}
	
	
	//------------------------------------------------------
	public int WeaponPointIncremetBy(int amount)
	{
		int temp = m_weaponPointCurrent + amount;
		while (temp >= m_weaponPoints.Length)
		{
			temp -= m_weaponPoints.Length;
		}
		return temp;
	}
	
	//------------------------------------------------------
	private void m_checkFireAndFire()
	{
		if (GetHasEnoughTimePassed())
		{
			if (GetHasEnoughAmmo())
			{
				// fire script
				if(!FireWeapon())
					return;
				
				// play any animations associated
				m_playAnimations();
				
				
				
				// fire sounds
				if ( m_sounds.m_weaponFireSound)
				{
					/*switch (m_fireType)
					{
					case FireType.OneAfterAnother:
						AudioSource.PlayClipAtPoint(m_sounds.m_weaponFireSound, m_weaponPoints[WeaponPointIncremetBy(0)].m_weaponPoint.transform.position);
						break;
					case FireType.InPairs:
						AudioSource.PlayClipAtPoint(m_sounds.m_weaponFireSound, m_weaponPoints[WeaponPointIncremetBy(0)].m_weaponPoint.transform.position);
						AudioSource.PlayClipAtPoint(m_sounds.m_weaponFireSound, m_weaponPoints[WeaponPointIncremetBy(1)].m_weaponPoint.transform.position);
						break;
					case FireType.AllAtOnce:
						for(int i = 0; i < m_weaponPoints.Length; i++)
							AudioSource.PlayClipAtPoint(m_sounds.m_weaponFireSound, m_weaponPoints[i].m_weaponPoint.transform.position);
						break;
					}*/
				}
				
				// decrement ammo
				switch (m_fireType)
				{
				case FireType.OneAfterAnother:
					m_ammo.m_ammoCurrent -= 1;
					m_weaponPointCurrent = WeaponPointIncremetBy(1);
					break;
				case FireType.InPairs:
					m_ammo.m_ammoCurrent -= 2;
					m_weaponPointCurrent = WeaponPointIncremetBy(2);
					break;
				case FireType.AllAtOnce:
					m_ammo.m_ammoCurrent -= m_weaponPoints.Length;
					break;
				}
				
				
				// reset timer
				m_resetDelayTimer();
			}
			else // has no ammo
			{
				//assign audioClip
				if(m_sounds.m_weaponEmptySound)
					AudioSource.PlayClipAtPoint(m_sounds.m_weaponEmptySound, m_weaponPoints[m_weaponPointCurrent].m_weaponPoint.transform.position);
			}
			
			
		}
		
		
		
	}
	
	
	public virtual void Fire()
	{
		m_fireAtType = FireAtType.Forward;
		m_checkFireAndFire();		
	}
	
	public virtual void FireAtPosition(Vector3 position)
	{
		m_fireTypePosition = position;
		m_fireAtType      = FireAtType.Position;
		m_checkFireAndFire();
	}



	public virtual void FireAtDirection(Vector3 direction)
	{
		m_fireTypeDirection = direction;
		m_fireAtType      = FireAtType.Direction;
		m_checkFireAndFire();
	}

	public virtual void FireAtDirectionAndPosition(Vector3 direction, Vector3 position)
	{
		m_fireTypeDirection = direction;
		m_fireTypePosition  = position;
		m_fireAtType        = FireAtType.DirectionAndPosition;
		m_checkFireAndFire();
	}

	public virtual void FireAtGameObject(GameObject passedGameObject)
	{
		m_fireTypeObject = passedGameObject;
		m_fireAtType     = FireAtType.Object;
		m_checkFireAndFire();
	}
	
	public virtual void FireAtGameObject(GameObject passedGameObject,Vector3 position, Quaternion quaternion)
	{
		m_fireTypeObject     = passedGameObject;
		m_fireTypePosition   = position;
		m_fireTypeQuaternion = quaternion;
		m_fireAtType         = FireAtType.Object;
		m_checkFireAndFire();
	}
	
	public virtual void FireAtCollider(Collider passedCollider, Vector3 position, Quaternion quaternion)
	{
		m_fireTypeCollider   = passedCollider;
		m_fireTypePosition   = position;
		m_fireTypeQuaternion = quaternion;
		m_fireAtType         = FireAtType.Collider;
		m_checkFireAndFire();
	}
	
	
	// Update is called once per frame
	void Update() 
	{
		m_delay.m_delayTime -= Time.deltaTime;
	}
}
