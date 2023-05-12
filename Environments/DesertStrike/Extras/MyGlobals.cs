using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MyGlobals : MonoBehaviour {

	void Awake()
	{
		DontDestroyOnLoad(this);
	}

	public enum Direction
	{
		upDown, 
		leftRight,
	};

    #region ConstStrings
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public struct GameObjectNames
    {
        public static string HealthFuelBar = "HealthFuelBar";
    }

    public struct Messages
    {
        public static string MESSAGE_CANVAS_ADD_SCORE    = "MESSAGE_CANVAS_ADD_SCORE";
        public static string StartRandomDeathScript      = "StartRandomDeathScript";
        public static string MESSAGE_DAMAGE              = "MESSAGE_DAMAGE";
        public static string _OnTriggerEnterOtherPlayer  = "_OnTriggerEnterOtherPlayer";
        public static string _OnCollisionWithOtherPlayer = "_OnCollisionWithOtherPlayer";
        public static string SetTargetGameObjectMessage  = "SetTargetGameObjectMessage";
    }
    #endregion

    #region Angles
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    private static float LeftRight(GameObject player, Vector3 targetPosition)
	{
		//extreemly expensive Waypoint Off doing this 
		Vector3 thisVectorRight       = player.transform.right;
		thisVectorRight.y = 0;
				
		Vector3 thisVectorForward     = player.transform.forward;
		thisVectorForward.y = 0;

		Vector3 targetDir      = targetPosition - player.transform.position;
		targetDir.y = 0;
				
		float degrees = Vector3.Angle(targetDir, thisVectorForward);
		if ( Vector3.Dot(targetDir, thisVectorRight) <0)
			degrees = -degrees;
	
		return 	degrees;
	}

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    private static float UpDown(GameObject player, Vector3 targetPosition)
	{
		//extreemly expensive Waypoint Off doing this 
		Vector3 thisVectorRight       = player.transform.up;
		thisVectorRight.x = 0;
				
		Vector3 thisVectorForward     = player.transform.forward;
		thisVectorForward.x = 0;

		Vector3 targetDir      = targetPosition - player.transform.position;
		targetDir.x = 0;

		
		float degrees = Vector3.Angle(targetDir, thisVectorForward);
		if ( Vector3.Dot(targetDir, thisVectorRight) <0)
			degrees = -degrees;
		
		return 	degrees;
	}

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static float GetAngle(GameObject player, Vector3 targetPosition, Direction direction)
	{
		float  returnValue = 0;
		switch(direction)
		{
		case Direction.leftRight:
			returnValue = LeftRight(player, targetPosition);
			break;
		case Direction.upDown:
			returnValue = UpDown(player, targetPosition);
			break;

		}
		return returnValue;
	}

    #endregion


    #region FlatDistance
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static float GetFlatDistance(Vector3 player, Vector3 target)
    {
        Vector3 playerFlat = player;
        playerFlat.y = 0;

        Vector3 targetFlat = target;
        targetFlat.y = 0;

        return Vector3.Distance(playerFlat, targetFlat);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static float GetFlatDistance(GameObject player, GameObject target)
    {
        return GetFlatDistance(player.transform.position, target.transform.position);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static float GetFlatDistance(Vector3 player, GameObject target)
    {
        return GetFlatDistance(player, target.transform.position);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static float GetFlatDistance(GameObject player, Vector3 target)
    {
        return GetFlatDistance(player.transform.position, target);
    }

    #endregion
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static GameObject GetBaseParentObject(GameObject lGameObject)
    {
        GameObject lTemp = lGameObject;
        for(;;)     
        {
            if (lTemp.transform.parent == null)
            {
                break;
            }
            if (lTemp.GetComponent<BaseParent>() != null)
            {
                break;
            }

            lTemp = lTemp.transform.parent.gameObject;
            
        }
        return lTemp;
    }

    #region ScreenToreal

    //------------------------------------------------------------------------------------------------
    /// <summary>GetRayFromMouseTouchPositionToScreen </summary>
    public static Ray GetRayFromTouchMousePositionToScreen(int lIndex)
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_STANDALONE
        return Camera.main.ScreenPointToRay(Input.mousePosition);
#else
		return Camera.main.ScreenPointToRay (Input.GetTouch(lIndex).position);
#endif
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>GetRayFromMouseTouchPositionToScreen </summary>
    public static Vector3 GetPositionOfTouchMouse(int lIndex)
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_STANDALONE
        return Input.mousePosition;
#else
		return Input.GetTouch(lIndex).position;
#endif
    }

    #endregion
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public enum ECheckAllComponentsInGameObject
    {
        EnableAllInList_AllElseDisable,
        DisableAllInList_AllElseEnable,
        EnableAllInList,  //just turn these on 
        DisableAllInList, //just turn these off 
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public enum ECheckAllGameObjectsWithComponents
    {
        EnableAllInList,  //just turn these on 
        DisableAllInList, //just turn these off 
    }


    // List<System.Type> lList = new List<System.Type>();
    // lList.Add(typeof(DeathScript_Rotors));
    // lList.Add(typeof(DeathScript));
    public static void CheckAllComponentsInGameObject(GameObject lGameObject, ECheckAllComponentsInGameObject lTest, List<System.Type> lList)
    {
        bool lEnable = true;
        if(lTest == ECheckAllComponentsInGameObject.DisableAllInList_AllElseEnable || lTest == ECheckAllComponentsInGameObject.DisableAllInList)
        {
            lEnable = false;
        }

        MonoBehaviour[] lAllComponents = lGameObject.GetComponents<MonoBehaviour>();

        if (lTest == ECheckAllComponentsInGameObject.DisableAllInList_AllElseEnable || lTest == ECheckAllComponentsInGameObject.EnableAllInList_AllElseDisable)
        {
            foreach (MonoBehaviour lMono in lAllComponents)
            {
                bool lFound = false;
                for (int i = 0; i < lList.Count; i++)
                {
                    if (lMono.GetType() == lList[i])
                    {
                        lFound = true;
                        break;
                    }
                }

                if (lFound == true)
                {
                    lMono.enabled = lEnable;
                }
                else
                {
                    lMono.enabled = !lEnable;
                }
            }
        }

        //----------------------------------------------------------------------
        //          EnableAllInList   DisableAllInList
        //----------------------------------------------------------------------
        else
        {
            foreach (MonoBehaviour lMono in lAllComponents)
            {
                bool lFound = false;
                for (int i = 0; i < lList.Count; i++)
                {
                    if (lMono.GetType() == lList[i])
                    {
                        lFound = true;
                        break;
                    }
                }

                if (lFound == true)
                {
                    lMono.enabled = lEnable;
                }

            }

         }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lGameObject"></param>
    /// <param name="lTest"></param>
    /// <param name="lList">List of all the items</param>
    // if a gameobject contains this script then it disables it 
    // List<System.Type> lList = new List<System.Type>();
    // lList.Add(typeof(DeathScript_Rotors));
    // lList.Add(typeof(DeathScript));
    public static void CheckAllGameObjectsWithComponents(GameObject lGameObject, ECheckAllGameObjectsWithComponents lTest, List<System.Type> lList)
    {
        bool lEnable = true;
        if (lTest == ECheckAllGameObjectsWithComponents.DisableAllInList)
        {
            lEnable = false;
        }

        MonoBehaviour[] lAllComponents = lGameObject.GetComponents<MonoBehaviour>();


        foreach (MonoBehaviour lMono in lAllComponents)
        {
            bool lFound = false;
            for (int i = 0; i < lList.Count; i++)
            {
                if (lMono.GetType() == lList[i])
                {
                    lFound = true;
                    break;
                }
            }

            if (lFound == true)
            {
                lMono.gameObject.SetActive(lEnable);
            }
        }
    }




    //-----------------------------------------------------------------------------------------------------------------------------------------------
    // List<System.Type> lList = new List<System.Type>();
    // lList.Add(typeof(DeathScript_Rotors));
    // lList.Add(typeof(DeathScript));
    public static void CheckAllComponentsInGameObject(GameObject lGameObject, ECheckAllComponentsInGameObject lTest, List<string> lList)
    {
        bool lEnable = true;
        if (lTest == ECheckAllComponentsInGameObject.DisableAllInList_AllElseEnable || lTest == ECheckAllComponentsInGameObject.DisableAllInList)
        {
            lEnable = false;
        }

        MonoBehaviour[] lAllComponents = lGameObject.GetComponents<MonoBehaviour>();

        if (lTest == ECheckAllComponentsInGameObject.DisableAllInList_AllElseEnable || lTest == ECheckAllComponentsInGameObject.EnableAllInList_AllElseDisable)
        {
            foreach (MonoBehaviour lMono in lAllComponents)
            {
                bool lFound = false;
                for (int i = 0; i < lList.Count; i++)
                {
                    if (lMono.name == lList[i])
                    {
                        lFound = true;
                        break;
                    }
                }

                if (lFound == true)
                {
                    lMono.enabled = lEnable;
                }
                else
                {
                    lMono.enabled = !lEnable;
                }
            }
        }

        //----------------------------------------------------------------------
        //          EnableAllInList   DisableAllInList
        //----------------------------------------------------------------------
        else
        {
            foreach (MonoBehaviour lMono in lAllComponents)
            {
                bool lFound = false;
                for (int i = 0; i < lList.Count; i++)
                {
                    if (lMono.name == lList[i])
                    {
                        lFound = true;
                        break;
                    }
                }

                if (lFound == true)
                {
                    lMono.enabled = lEnable;
                }

            }

        }
    }


    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Return Gameobject with lName
    /// </summary>
    /// <returns></returns>
    public static GameObject GetGameObjectByName(GameObject lGameObject,string lName)
    {
        Transform[] lAllTransforms = lGameObject.GetComponents<Transform>();
        foreach (Transform lTransform in lAllTransforms)
        {
            if(lTransform.gameObject.name == lName)
            {
                return lTransform.gameObject;
            }
        }
        return null;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary> Checks if it in the correct place in space</summary>
    /// <summary> Coded long way to be able to chek it all </summary>
    /// <summary> return  (m_LetterObject.transform.position == m_PlaceInScenePosition)  was checking it to be TO accurate </summary>
    public static bool IsVector3Same(Vector3 lVector1, Vector3 lVector2, int lToDecimalPlaces = 2)
    {
        int lMuliplyer = 10 * lToDecimalPlaces;
        if (lToDecimalPlaces == 0)
        {
            lMuliplyer = 1;
        }

        int x1 = Mathf.RoundToInt(lVector1.x * lMuliplyer);
        int x2 = Mathf.RoundToInt(lVector2.x * lMuliplyer);
        if (x1 != x2)
        {
            return false;
        }

        int y1 = Mathf.RoundToInt(lVector1.y * lMuliplyer);
        int y2 = Mathf.RoundToInt(lVector2.y * lMuliplyer);
        if (y1 != y2)
        {
            return false;
        }

        int z1 = Mathf.RoundToInt(lVector1.z * lMuliplyer);
        int z2 = Mathf.RoundToInt(lVector2.z * lMuliplyer);
        if (z1 != z2)
        {
            return false;
        }

        return true;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary></summary>
    /// <param name="lGameObject"></param>
    public static void ClearWaypointsFromParent(GameObject lGameObject)
    {
        GameObject lParent   = MyGlobals.GetBaseParentObject(lGameObject);
        WayPoints lWaypoints = lParent.GetComponent<WayPoints>();
        if (lWaypoints != null)
        {
            lWaypoints.ClearAll();
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary></summary>
    /// <param name="lMessageString"></param>
    /// <param name="lMessage"></param>
    public static void BroadcastAll(string lMessageString, System.Object lMessage)
    {
        GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject go in gos)
        {
            if (go && go.transform.parent == null)
            {
                go.gameObject.BroadcastMessage(lMessageString, lMessage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///  prints message if GameObject is NULL
    /// </summary>
    /// <param name="lName"></param>
    /// <param name="lObject"></param>
    /// <returns></returns>
    public static bool IsValidObject(string lName, GameObject lObject)
    {
#if UNITY_EDITOR
        if (lObject == null)
        {
            EditorUtility.DisplayDialog("Invalid:" + lName, "Invalid:" + lName, "OK");
            Debug.LogError("Invalid:" + lName);
            Debug.Break();
        }
#endif

        return (lObject != null);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    public static void CheckObjectHave(GameObject lGameObject, System.Type lType)
    {
#if UNITY_EDITOR

        bool lFound = false;
        MonoBehaviour[] lAllComponents = lGameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour lMono in lAllComponents)
        {
            if(lMono.GetType() == lType)
            {
                lFound = true;
                break;
            }
        }

        if (lFound == false)
        {
            EditorUtility.DisplayDialog("Invalid:" + lGameObject.name, "Does Not Have:" + lType.ToString(), "OK");
            Debug.LogError("Invalid:" + lGameObject.name);
        }       
#endif

    }


    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Swap B and B
    /// </summary>
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// change all layers in this and children to this layer
    /// </summary>
    public static void ChangeAllLayersTo(GameObject lGameObject, InfoManager.LayerInfo lNewLayer)
    {
        foreach (Transform lTransform in lGameObject.GetComponentsInChildren<Transform>(true))
        {
            lTransform.gameObject.layer = InfoManager.Instance.GetOnlyThisLayer(lNewLayer);
        }
    }



    public enum ELocalTransform
    {
        All, 
        Position,
        Scale,
        Rotation,
        PositionScale,
        PositionRotation,
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// ClearLocalTransforms
    /// </summary>
    /// <param name="lGameObject"></param>
    public static void ClearLocalTransforms(GameObject lGameObject, ELocalTransform ELocalTransform)
    {
        switch(ELocalTransform)
        {
            case ELocalTransform.All:
                lGameObject.transform.localPosition = Vector3.zero;
                lGameObject.transform.localScale    = Vector3.one;
                lGameObject.transform.localRotation = Quaternion.identity;
                break;

            case ELocalTransform.Position:
                lGameObject.transform.localPosition = Vector3.zero;
                break;

            case ELocalTransform.Scale:
                lGameObject.transform.localScale    = Vector3.one;
                break;

            case ELocalTransform.Rotation:
                lGameObject.transform.localRotation = Quaternion.identity;
                break;

            case ELocalTransform.PositionScale:
                lGameObject.transform.localPosition = Vector3.zero;
                lGameObject.transform.localScale    = Vector3.one;
                break;

            case ELocalTransform.PositionRotation:
                lGameObject.transform.localPosition = Vector3.zero;
                lGameObject.transform.localRotation = Quaternion.identity;
                break;
                


        }
    }

    public enum EGetGameObject
    {
        ExcludeBase,
        IncludeBase,
        OnlyMainChildren,

    }
    //------------------------------------------------------------------------------------------------------
    public static GameObject[] GetGameObjectsInOnlyChildren(GameObject lGameobject, EGetGameObject lEGetGameObject)
    {
        List<GameObject> lList  = new List<GameObject>();
        Transform[] lTransforms = lGameobject.GetComponentsInChildren<Transform>();
        for(int i = 0; i < lTransforms.Length; i++)
        {
            Transform lTransform = lTransforms[i];
            switch(lEGetGameObject)
            {
                case EGetGameObject.ExcludeBase:
                    {
                        if (lTransform.gameObject != lGameobject)
                        {
                            lList.Add(lTransform.gameObject);
                        }
                    }
                    break;

                case EGetGameObject.IncludeBase:
                    {
                        lList.Add(lTransform.gameObject);
                    }
                    break;

                case EGetGameObject.OnlyMainChildren:
                    {
                        if (lTransform.gameObject != lGameobject && lTransform.parent.gameObject == lGameobject)
                        {
                            lList.Add(lTransform.gameObject);
                        }
                    }
                    break;
            }

        }

        return lList.ToArray();
    }

    //------------------------------------------------------------------------------------------------------
    public static GameManager GetGameManager()
    {
        return MyGlobals.GetBaseParentObject(Camera.main.gameObject).GetComponent<GameManager>();
    }
}
