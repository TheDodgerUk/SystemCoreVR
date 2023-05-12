using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Garage : MonoBehaviour {

	// Use this for initialization
	void Awake ()
    {
        this.enabled = false;
	}



    //--------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Disabale every thing 
    /// </summary>
    void OnEnable()
    {
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);

        List<System.Type> lList = new List<System.Type>();
        lList.Add(typeof(Garage));        
        MyGlobals.CheckAllComponentsInGameObject(lParent, MyGlobals.ECheckAllComponentsInGameObject.EnableAllInList_AllElseDisable, lList);


        List<System.Type> lListHealth = new List<System.Type>();
        lListHealth.Add(typeof(Health));
        MyGlobals.CheckAllGameObjectsWithComponents(lParent, MyGlobals.ECheckAllGameObjectsWithComponents.DisableAllInList, lListHealth);

        MyGlobals.ClearWaypointsFromParent(lParent);
    }

    //----------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Scale down till dispear then remove
    /// </summary>
    void Update()
    {
        GameObject lParent = MyGlobals.GetBaseParentObject(this.gameObject);
        lParent.transform.localScale = Vector3.Slerp(lParent.transform.localScale, Vector3.zero, Time.deltaTime);   //-------------------------------------
        if (MyGlobals.IsVector3Same(lParent.transform.localScale, Vector3.zero, 1) == true)
        {
            Destroy(MyGlobals.GetBaseParentObject(lParent));
        }
    }
}
