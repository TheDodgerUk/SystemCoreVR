using UnityEngine;
using System.Collections;

public class WeaponSystemCreateAndHideInPlace : WeaponSystemBase 
{
	private GameObject[]  m_weaponCreateAndHold;

	void Start () 
	{
		m_weaponCreateAndHold = new GameObject[m_ammo.m_ammoMax];
		// check if any has been added
		if (m_weaponPoints.Length == 0)
			Debug.LogError("No weapon points made");
		ReloadAmmo();
	}
	
	
	
	override public  void ReloadAmmo()
	{
		m_weaponPointCurrent = 0;
		for (int ammoIndex = 0; ammoIndex < m_ammo.m_ammoMax; ammoIndex++)
		{
			if (m_weaponCreateAndHold[ammoIndex] == null || m_weaponCreateAndHold[ammoIndex].transform.parent == null)
			{			
				int index = WeaponPointIncremetBy(ammoIndex);
				GameObject creationPoint = m_weaponPoints[index].m_weaponPoint;

				m_weaponCreateAndHold[ammoIndex] = Instantiate(m_ammo.m_projectile, creationPoint.transform.position, creationPoint.transform.rotation) as GameObject;
				m_weaponCreateAndHold[ammoIndex].transform.parent = creationPoint.transform;
				m_weaponCreateAndHold[ammoIndex].GetComponent<Collider>().enabled = false;
				m_weaponCreateAndHold[ammoIndex].SetActive(false);
			}

		}
		m_ammo.m_ammoCurrent = m_ammo.m_ammoMax;
	}
	
	
	override protected  bool FireWeapon()
	{
		switch(m_fireType)
		{
		case FireType.OneAfterAnother:
			EnableWeaponIndex(m_ammo.m_ammoCurrent-1);
			break;

		case FireType.InPairs:
			EnableWeaponIndex(m_ammo.m_ammoCurrent-1);
			EnableWeaponIndex(m_ammo.m_ammoCurrent-2);
			break;
		case FireType.AllAtOnce:
			for (int ammoIndex = 0; ammoIndex < m_ammo.m_ammoMax; ammoIndex++)
			{
				EnableWeaponIndex(ammoIndex);
			}
			break;

		default: 
			Debug.LogError("not here"); 
			Debug.Break(); 
			break;
		}

		return true;
	}

	void EnableWeaponIndex(int index)
	{
		m_weaponCreateAndHold[index].SetActive(true);
		m_weaponCreateAndHold[index].transform.parent = null;
	}
}
