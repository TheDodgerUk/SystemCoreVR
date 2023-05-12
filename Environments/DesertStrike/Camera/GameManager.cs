#define Instantiate3
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

public class GameManager : MonoBehaviour
{


    public ArrowToPlayer m_ArrowToPlayer;

    private float m_CountDownTimer = 0;
    public ListRandom<PlayerItems> m_RandomItems = new ListRandom<PlayerItems>();

    [NotNull]
    [SerializeField]
    private GameObject m_Helicopter;

    [NotNull]
    [SerializeField]
    private GameObject m_Humvee;

    public static class HeigtsOfItems
    {
        public const float Helicopter = 21;
        public const float Humvee     = 3.0f;
        public const float SemiRare   = 6.75f;
        public const float Rare       = 3.33f;
        public const float VeryRare   = 1.25f;
    }



    public enum PlayerItems
    {
        HeliCopter,
        AttackHelicopter,
        Humvee,

    }

    [SerializeField]
    GameObject m_BasePovObjects;
    
    List<GameObject> m_PovObjects  = new List<GameObject>();
    public List<GameObject> m_PlayerItems = new List<GameObject>();

    GameObject m_SpawnObjectBase;
    GameObject m_SpawnObjectChild;
    //-------------------------------------------------------------------------
    void _IgnoreLayer(int lLayer)
    {
        const int TotalLayers = 14;
        for (int i = 0; i < TotalLayers; i++)
        {
            if (i != lLayer)
            {
                Physics.IgnoreLayerCollision(i, lLayer, true);
            }
        }
    }

    //-----------------------------------------------------------------------------------------------
    void Awake()
    {
        //----------------------------------------------
        //  make placement objects
        //----------------------------------------------
        m_SpawnObjectBase                   = new GameObject();        
        m_SpawnObjectBase.name              = "SpawnPointBase";
        m_SpawnObjectChild                  = new GameObject();
        m_SpawnObjectChild.name             = "SpawnObjectChild";
        m_SpawnObjectChild.transform.parent = m_SpawnObjectBase.transform;

        if (MyGlobals.IsValidObject("BaseBorder", m_BasePovObjects) == true)
        {
            Transform[] lTransforms = m_BasePovObjects.GetComponentsInChildren<Transform>();
            for(int i = 0; i < lTransforms.Length; i++)
            {
                if (lTransforms[i].gameObject != m_BasePovObjects)
                {
                    m_PovObjects.Add(lTransforms[i].gameObject);
                }
            }
        }

        _IgnoreLayer(InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.FakeFloor));


        SetSpawnRatio();

        m_CountDownTimer = Random.Range(3, 5);
    }

    //-----------------------------------------------------------------------------------------------
    void SetSpawnRatio()
    {
        m_RandomItems.Clear();
        m_RandomItems.Add(PlayerItems.AttackHelicopter, 50);
        m_RandomItems.Add(PlayerItems.Humvee, 45);

    }


    //-----------------------------------------------------------------------------------------------
    Vector3 GetRandomPointFromGameObject(GameObject lRandomObject)
    {
        Vector3 lScale = lRandomObject.transform.localScale;
      
        Vector3 lLerpPosition = Vector3.zero;
        lLerpPosition.x       = Mathf.Lerp((-lScale.x / 2), (lScale.x / 2), Random.Range(0.2f, 0.8f));
        lLerpPosition.y       = Mathf.Lerp((-lScale.y / 2), (lScale.y / 2), Random.Range(0.2f, 0.8f));
        lLerpPosition.z       = Mathf.Lerp((-lScale.z / 2), (lScale.z / 2), Random.Range(0.2f, 0.8f));

        m_SpawnObjectBase.transform.position        = lRandomObject.transform.position;
        m_SpawnObjectBase.transform.rotation        = Quaternion.identity;
        m_SpawnObjectBase.transform.rotation        = lRandomObject.transform.rotation;
        m_SpawnObjectChild.transform.localPosition  = lLerpPosition;

        return m_SpawnObjectChild.transform.position;
    }

    //-----------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the correct prefab
    /// </summary>
    /// <param name="lPlayerItems"></param>
    Object GetPrefabPlayerItem(PlayerItems lPlayerItems)
    {
        Object lObject           = null;
        switch(lPlayerItems)
        {
            case PlayerItems.AttackHelicopter:
                lObject = m_Helicopter;
                break;

            case PlayerItems.Humvee:
                lObject = m_Humvee;
                break;
        }
        return lObject;
    }

    //-----------------------------------------------------------------------------------------------
    /// <summary>
    /// lGameObject might be info in here
    /// </summary>
    /// <param name="lPlayerItems"></param>
    /// <param name="lGameObject"></param>
    /// <returns></returns>
    float GetStartingOffsetHeight(PlayerItems lPlayerItems, GameObject lGameObject)
    {
        float  lHeight = 2;
        switch (lPlayerItems)
        {
            case PlayerItems.AttackHelicopter:
                lHeight               = HeigtsOfItems.Helicopter;
                var lTerrainFollowing = lGameObject.GetComponent<TerrainFollowing>();
                if(lTerrainFollowing != null)
                {
                    lHeight = lTerrainFollowing.GetTerrainFollowingHeight();
                }
                break;

            case PlayerItems.Humvee:
                lHeight = HeigtsOfItems.Humvee;
                break;
        }
        return lHeight;
    }

    //--------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Find height of terrian were player will be insanited 
    /// </summary>
    /// <param name="lPosition"></param>
    /// <returns></returns>
    float GetTerrainHeight(Vector3 lPosition)
    {
        float lheightOfRay = 100;
        float lHeight = 0;

        RaycastHit lHitInfo;
        if (Physics.Raycast(new Vector3(lPosition.x, lheightOfRay, lPosition.z), Vector3.down, out lHitInfo, lheightOfRay * 2, InfoManager.Instance.GetOnlyThisLayerMask(InfoManager.LayerInfo.Terrain)) == true)
        {
            return lHitInfo.point.y;
        }
        return lHeight;
    }

    //--------------------------------------------------------------------------------------------------------------
    void InstantiateRandomItem()
    {
        GameObject lRandomObjectGenerationPointModel       = m_PovObjects[Random.Range(0, m_PovObjects.Count)];
        Vector3 lItemPoint                                 = GetRandomPointFromGameObject(lRandomObjectGenerationPointModel);
        PlayerItems lPlayerItems                           = m_RandomItems.GetRandom();
        Object lObject                                     = GetPrefabPlayerItem(lPlayerItems);
        GameObject lGameObject                             = Instantiate(lObject) as GameObject;
        lGameObject.transform.forward                      = lRandomObjectGenerationPointModel.transform.forward;
        lGameObject.transform.position                     = lItemPoint;

       // lGameObject.transform.position -= lRandomObjectGenerationPointModel.transform.forward * 10; // move it back a bit 

        float lStartingOffsetHeight    = GetStartingOffsetHeight(lPlayerItems, lGameObject);
        float lTerrainHeight           = GetTerrainHeight(lItemPoint);
        float lTotalHeight             = lStartingOffsetHeight + lTerrainHeight;
        lGameObject.transform.position = new Vector3(lGameObject.transform.position.x, lTotalHeight, lGameObject.transform.position.z);
        m_PlayerItems.Add(lGameObject);
    }

    //--------------------------------------------------------------------------------------------------------------
    /// <summary> 
    /// Remove player if null or no longer Layer PLAYER
    /// </summary>
    /// 
    void RemoveInvalidPlayerItem()
    {
        for(int i = 0; i < m_PlayerItems.Count; i++)
        {
            if (m_PlayerItems[i].gameObject == null  || m_PlayerItems[i].gameObject.layer != InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Player))
            {
                m_PlayerItems.RemoveAt(i);
            }
        }       
    }


    //-----------------------------------------------------------------------------------------------
    void Update()
    {
        //-------------------------------------------
        //      Update Players canvas stuff
        //-------------------------------------------
        RemoveInvalidPlayerItem();

        m_ArrowToPlayer.SetObjects(m_PlayerItems);

        m_CountDownTimer -= Time.deltaTime;
        if (m_CountDownTimer < 0)
        {
            m_CountDownTimer = Random.Range(10, 15);
#if Instantiate3
            m_CountDownTimer = 3;
#endif
            InstantiateRandomItem();
        }

    }
}
