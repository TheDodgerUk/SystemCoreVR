using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class WeaponSystemCreateAndShowInPlace : WeaponSystemBase 
{


	private GameObject[]  m_weaponCreateAndHold;


    [ExecuteInEditMode]
    void OnValidate()
    {
        //m_weaponCreateAndHold = new GameObject[m_weaponPoints.Length];
        //ReloadAmmo();
    }

    //-----------------------------------------------------------------------------------------------------------------------------
    void Start () 
	{
		m_weaponCreateAndHold = new GameObject[m_weaponPoints.Length];
		// check if any has been added
		if (m_weaponPoints.Length == 0)
			Debug.LogError("No weapon points made");
		ReloadAmmo();
	}

    //-----------------------------------------------------------------------------------------------------------------------------
    bool IsInstantiateObjectThere(GameObject lGameObject)
    {
        Transform[] lTransform = lGameObject.GetComponentsInChildren<Transform>();
        for (int j = 0; j < lTransform.Length; j++)
        {
            if (lGameObject != lTransform[j].gameObject)
            {
                return true;
            }
        }
        return false;
    }

    //-----------------------------------------------------------------------------------------------------------------------------
	override public  void ReloadAmmo()
	{
        // remove all 
        for (int i = 0; i < m_weaponPoints.Length; i++)
        {
            GameObject[] lGameObjects = MyGlobals.GetGameObjectsInOnlyChildren(m_weaponPoints[i].m_weaponPoint, MyGlobals.EGetGameObject.OnlyMainChildren);

            for (int j = 0; j < lGameObjects.Length; j++)
            {
                Destroy(lGameObjects[j]);
            }
        }


        m_weaponPointCurrent = 0;
        for (int i = 0; i < m_weaponPoints.Length; i++)
        {
            m_weaponCreateAndHold[i] = Instantiate(m_ammo.m_projectile, m_weaponPoints[i].m_weaponPoint.transform.position, m_weaponPoints[i].m_weaponPoint.transform.rotation) as GameObject;
            m_weaponCreateAndHold[i].transform.parent = m_weaponPoints[i].m_weaponPoint.transform;
            m_weaponCreateAndHold[i].GetComponentInChildren<Collider>().enabled = false;
            MonoBehaviour[] scripts = m_weaponCreateAndHold[i].GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }
            
        }

        m_ammo.m_ammoCurrent = m_ammo.m_ammoMax ; // this sets it for the correct index
	}

    //---------------------------------------------------------------------------------------------------------------
    public void ReloadAmmoPointsForInspector()
    {


        for (int i = 0; i < m_weaponPoints.Length; i++)
        {
            GameObject[] lGameObjects = MyGlobals.GetGameObjectsInOnlyChildren(m_weaponPoints[i].m_weaponPoint, MyGlobals.EGetGameObject.OnlyMainChildren);
            if (lGameObjects.Length > 1)
            {
                for (int j = 0; j < lGameObjects.Length-1; j++)
                {
                    DestroyImmediate(lGameObjects[j]);
                }
            }

            if (lGameObjects.Length == 0)
            {
                GameObject lGameObject                                 = Instantiate(m_ammo.m_projectile, m_weaponPoints[i].m_weaponPoint.transform.position, m_weaponPoints[i].m_weaponPoint.transform.rotation) as GameObject;
                lGameObject.transform.parent                           = m_weaponPoints[i].m_weaponPoint.transform;
                lGameObject.GetComponentInChildren<Collider>().enabled = false;
                MonoBehaviour[] lScripts                               = lGameObject.GetComponentsInChildren<MonoBehaviour>();
                foreach (MonoBehaviour lScript in lScripts)
                {
                    lScript.enabled = false;
                }                    
            } 
                     
        }
    }

	//----------------------------------------------------------------------------------------
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
			break;
		}

		return true;
	}

	//--------------------------------------------------------------
	void EnableWeaponIndex(int index)
	{
        //--------------------------------------
        // Turn Onscripts
        //--------------------------------------
        MonoBehaviour[] scripts = m_weaponCreateAndHold[index].GetComponentsInChildren<MonoBehaviour>();
        for (int i = 0; i < scripts.Length; i++)
        {
            scripts[i].enabled = true;
        }
        GameObject[] lGameObjects = MyGlobals.GetGameObjectsInOnlyChildren(m_weaponCreateAndHold[index].gameObject, MyGlobals.EGetGameObject.IncludeBase);
        for(int i = 0; i < lGameObjects.Length; i++)
        {
            lGameObjects[i].SendMessage(MyGlobals.Messages.SetTargetGameObjectMessage, m_fireTypeObject, SendMessageOptions.DontRequireReceiver);
        }

        m_weaponCreateAndHold[index].transform.parent = null;

        //m_weaponCreateAndHold[index].transform.position = m_weaponPoints[index].m_weaponPoint.transform.position;
    }


}
