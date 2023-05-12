using UnityEngine;
using System.Collections;

public class WeaponSystemInstantGun : WeaponSystemBase 
{
	[System.Serializable]
	public class RestrictAngle
	{
		public bool  m_useRestriction = false;
		public float m_angle          = 110.0f;
	}
	public RestrictAngle m_restrictAngle;



	protected Projectile m_projectileInstant;

	void Start () 
	{
		// check if any has been added
		if (m_weaponPoints.Length == 0)
			Debug.LogError("No weapon points made");

		if( m_ammo.m_projectile == null)
		{
			Debug.LogError("need projectile " + gameObject);
			Debug.Break();
		}
		m_projectileInstant = m_ammo.m_projectile.GetComponent<Projectile>();
	}


	override public  void ReloadAmmo()
	{
		m_ammo.m_ammoCurrent = m_ammo.m_ammoMax;
	}

	override protected  bool FireWeapon()
	{

			
		Vector3 firingDirection = new Vector3(m_fireTypeDirection.x, 0, m_fireTypeDirection.y);

		if (m_restrictAngle.m_useRestriction)
		{
			// return early if angle of gun fre is behind it 
				
			Vector3 helicopterDirection = this.transform.forward;
			helicopterDirection.y = 0;
			if( Mathf.Abs(Vector3.Angle(helicopterDirection, firingDirection)) > 110)
				return false;
		}
				

		
		
		Vector3 hitPoint        = Vector3.zero;
		Vector3 hitNormal       = Vector3.zero;
		GameObject impactObject = null;
		float gunRange 			= m_fireTypeDirection.z;


		// places the position of the raycast to  Y = -100 for fake raycast check
		Vector3 position = m_fireTypePosition;
		position.y       = -100;

		// get fake impact , inlcude position and object and distance
		float distanceToFakeCollider      = float.PositiveInfinity;
		RaycastHit hit;
		if (Physics.Raycast(position, firingDirection, out hit, gunRange))
		{
			// gets the correct position of the REAL collider and aims it at that  
			// X and Z stay the same , the Y is gotten from the real COLLIDER
			hitPoint   = hit.point;
			hitNormal  = hit.normal;
			hitPoint.y = hit.collider.gameObject.GetComponent<FakeCollider>().m_objectWithCollider.transform.position.y; // real Y position

			impactObject = hit.collider.gameObject.GetComponent<FakeCollider>().m_objectWithCollider;
			distanceToFakeCollider = Vector3.SqrMagnitude( position - hitPoint);  				 
		}

		// get terrain impact point 
		Vector3 terrainImpactPoint = m_fireTypePosition + ( firingDirection * gunRange);
		terrainImpactPoint.y       = Terrain.activeTerrain.SampleHeight(terrainImpactPoint);


		// ray cast to terrain 
		RaycastHit hitDirect;
		// for testing Debug.DrawRay(m_fireTypePosition,  (terrainImpactPoint - m_fireTypePosition) * 100);
		Vector3 rayDirection = terrainImpactPoint - m_fireTypePosition;
		rayDirection.y = Mathf.Clamp(rayDirection.y, float.NegativeInfinity, 0); // no up direction allowed


		Physics.Raycast(m_fireTypePosition, rayDirection, out hitDirect, gunRange * 2.0f); // double the gun range to compisate for the angle 
		float interceptDistance = Vector3.SqrMagnitude( position - hitDirect.point);

		// if it hits something else on way to terrain
		// if it did not hit fake then distanceFake is MAX so interceptDistance is smaller
		if( interceptDistance < distanceToFakeCollider)
		{
			hitPoint  = hitDirect.point;
			hitNormal = hitDirect.normal;
			impactObject = hitDirect.collider.gameObject;
		}

		


		


		m_projectileInstant.ImpactBehaviour(impactObject, hitPoint, hitNormal, Quaternion.Euler(m_fireTypePosition - hitPoint));
		return true;
	}
}
