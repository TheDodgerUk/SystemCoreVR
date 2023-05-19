using UnityEngine;
using System.Collections;

public class WeaponSystemCreateFireFlack : WeaponSystemBase 
{

	public float m_degreesOffTarget           = 1.80f;
	public float m_percentageDistanceOffTaget = 10.80f;
	void Start () 
	{
		// check if any has been added
		if (m_weaponPoints.Length == 0)
			Debug.LogError("No weapon points made");
		ReloadAmmo();
	}
	
	
	override public  void ReloadAmmo()
	{
		m_weaponPointCurrent = 0;
		m_ammo.m_ammoCurrent = m_ammo.m_ammoMax;
	}
	
	//----------------------------------------------------------------------------------------
	override protected  bool FireWeapon()
	{
		if (m_fireAtType != FireAtType.Object)
		{
			Debug.LogError("this can only work with a gameobject: " + gameObject);
			return false;
		}
		GameObject clone = null;
		switch(m_fireType)
		{
		case FireType.OneAfterAnother:
			clone = Instantiate(m_ammo.m_projectile, m_weaponPoints[m_weaponPointCurrent].m_weaponPoint.transform.position, m_weaponPoints[m_weaponPointCurrent].m_weaponPoint.transform.rotation) as GameObject;
			m_randomHitPoint(clone, m_weaponPoints[m_weaponPointCurrent].m_weaponPoint.transform.position);
			break;
			
		case FireType.InPairs:
			for (int i = 0; i < m_weaponPoints.Length; i++)
			{
				clone = Instantiate(m_ammo.m_projectile, m_weaponPoints[i].m_weaponPoint.transform.position, m_weaponPoints[i].m_weaponPoint.transform.rotation) as GameObject;
				m_randomHitPoint(clone, m_weaponPoints[i].m_weaponPoint.transform.position);
			}
			break;
			
		case FireType.AllAtOnce:
			for (int i = 0; i < m_weaponPoints.Length; i++)
			{
				clone = Instantiate(m_ammo.m_projectile, m_weaponPoints[i].m_weaponPoint.transform.position, m_weaponPoints[i].m_weaponPoint.transform.rotation) as GameObject;
				//m_randomHitPoint(clone, m_weaponPoints[i].m_weaponPoint.transform.position);
			}
			break;
			
		default: 
			Debug.LogError("not here"); 
			break;
		}
		return true;
	}



	private void m_randomHitPoint(GameObject clone, Vector3 creationPoint)
	{
		Projectile projectile = clone.GetComponent<Projectile>();
		projectile.m_timeDistance.m_timeDistance = Projectile.ETimeDistance.Distance;


		// change the distance amount
		float distance = Vector3.Distance( creationPoint, this.m_fireTypeObject.transform.position);
		float percentageDistance = 100 - Random.Range(-m_percentageDistanceOffTaget, m_percentageDistanceOffTaget);
		projectile.m_timeDistance.m_amount = (distance/ 100 ) * percentageDistance;


		float offtarget = Random.Range(-m_degreesOffTarget, m_degreesOffTarget);

		// rotate it
		int vector = Random.Range(0, 3);
		switch (vector)
		{
			case 0: clone.transform.Rotate(offtarget, 0, 0); break;
			case 1: clone.transform.Rotate(0, offtarget, 0); break;
			case 2: clone.transform.Rotate(0, 0, offtarget); break;
		}


	}
}
