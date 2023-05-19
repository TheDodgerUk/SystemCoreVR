using UnityEngine;
using System.Collections;

public class WeaponsSystemCreateAndFire : WeaponSystemBase {

	

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
		switch(m_fireType)
		{
		case FireType.OneAfterAnother:
			Instantiate(m_ammo.m_projectile, m_weaponPoints[m_weaponPointCurrent].m_weaponPoint.transform.position, m_weaponPoints[m_weaponPointCurrent].m_weaponPoint.transform.rotation);
			break;
			
		case FireType.InPairs:
			for (int i = 0; i < m_weaponPoints.Length; i++)
				Instantiate(m_ammo.m_projectile, m_weaponPoints[i].m_weaponPoint.transform.position, m_weaponPoints[i].m_weaponPoint.transform.rotation);
			break;
			
		case FireType.AllAtOnce:
			for (int i = 0; i < m_weaponPoints.Length; i++)
				Instantiate(m_ammo.m_projectile, m_weaponPoints[i].m_weaponPoint.transform.position, m_weaponPoints[i].m_weaponPoint.transform.rotation);
			break;
			
		default: 
			Debug.LogError("not here"); 
			break;
		}
		return true;
	}
}
