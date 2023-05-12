using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
public class InfoManager{
    //---------------------------------------------------------------------------------------
    public static InfoManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new InfoManager();


            }
            return m_Instance;
        }
    }
    //---------------------------------------------------------------------------------------
    public enum TagInfo
    {
        Player,
        Waypoint,
        Terrain,
        PlayerAndWayPoint,
        Landing,
        Boundings,
        Ammo,
        FakeFloor,
    }


    public enum LayerInfo
    {
        Player,
        Waypoint,
        Terrain,
        PlayerAndWayPoint,
        Landing,
        Boundings,
        Ammo,
        FakeFloor,
        Buildings,
        Crashed,
        Safe,
        Parking,
        Stationary,
    }

    int m_Player            = 0;
    int m_WayPoint          = 0;
    int m_PlayerAndWayPoint = 0;
    int m_Terrain           = 0;
    int m_Landing           = 0;
    int m_Boundings         = 0;
    int m_Ammo              = 0;
    int m_FakeFloor         = 0;
    int m_Buildings         = 0;
    int m_Crashed           = 0;
    int m_Safe              = 0;
    int m_Parking           = 0;
    int m_Stationary        = 0;
    public static InfoManager m_Instance;
    // Use this for initialization
    List<Color> m_Colour = new List<Color>();
    int m_currentColourIndex = 0;
    private InfoManager()
    {
        m_Player     = LayerMask.NameToLayer("Player");
        m_WayPoint   = LayerMask.NameToLayer("WayPoint");
        m_Terrain    = LayerMask.NameToLayer("Terrain");
        m_Landing    = LayerMask.NameToLayer("Landing");
        m_Boundings  = LayerMask.NameToLayer("Boundings");
        m_Ammo       = LayerMask.NameToLayer("Ammo");
        m_FakeFloor  = LayerMask.NameToLayer("FakeFloor");
        m_Buildings  = LayerMask.NameToLayer("Buildings");
        m_Crashed    = LayerMask.NameToLayer("Crashed");
        m_Safe       = LayerMask.NameToLayer("Safe");
        m_Parking    = LayerMask.NameToLayer("Parking");
        m_Stationary = LayerMask.NameToLayer("Stationary");
 
         m_PlayerAndWayPoint  = 1 <<  LayerMask.NameToLayer("Player");
        m_PlayerAndWayPoint |= (1 << LayerMask.NameToLayer("WayPoint"));


        m_Colour.Add(Color.red);
        m_Colour.Add(Color.blue);
        m_Colour.Add(Color.white);
        m_Colour.Add(Color.black);
    }


    //---------------------------------------------------------------------------------------
    public float GetRemoveDistance(TagInfo lLayerInfo)
    {
        return 10;
    }

    //---------------------------------------------------------------------------------------
    public Color GetNextWayPointColour()
    {
        Color lTemp = m_Colour[m_currentColourIndex];

        m_currentColourIndex++;
        if(m_currentColourIndex >= m_Colour.Count)
        {
            m_currentColourIndex = 0;
        }
        return lTemp;
    }


    public int GetOnlyThisLayer(LayerInfo lLayerInfo)
    {
        int lLayer = 0;
        switch (lLayerInfo)
        {
            case LayerInfo.Player:
                return m_Player;

            case LayerInfo.Waypoint:
                return m_WayPoint;

            case LayerInfo.Terrain:
                return m_Terrain;

            case LayerInfo.Landing:
                return  m_Landing;

            case LayerInfo.Boundings:
                return m_Boundings;

            case LayerInfo.Ammo:
                return m_Ammo;

            case LayerInfo.FakeFloor:
                return m_FakeFloor;

            case LayerInfo.Buildings:
                return m_Buildings;

            case LayerInfo.Crashed:
                return m_Crashed;

            case LayerInfo.Safe:
                return m_Safe;

            case LayerInfo.Parking:
                return m_Parking;

            case LayerInfo.Stationary:
                return m_Stationary;

            case LayerInfo.PlayerAndWayPoint:
                UnityEngine.Assertions.Assert.IsTrue(true, "Invalid");
                break;

        }

        return lLayer;


    }

    //---------------------------------------------------------------------------------------
    public int GetOnlyThisLayerMask(LayerInfo lLayerInfo)
    {
        int lLayer = 0;
        switch(lLayerInfo)
        {
            case LayerInfo.Player:
                return (1 << m_Player);

            case LayerInfo.Waypoint:
                return (1 << m_WayPoint);

            case LayerInfo.Terrain:
                return (1 << m_Terrain);

            case LayerInfo.Landing:
                return (1 << m_Landing);

            case LayerInfo.Boundings:
                return (1 << m_Boundings);

            case LayerInfo.Ammo:
                return (1 << m_Ammo);

            case LayerInfo.FakeFloor:
                return (1 << m_FakeFloor);

            case LayerInfo.PlayerAndWayPoint:
                return ((1 << m_Player) | (1 << m_WayPoint));

        }
        return lLayer;
    }


    //---------------------------------------------------------------------------------------
    public int GetEveryThingButThisLayerMask(LayerInfo lLayerInfo)
    {
        int lLayer = 0;
        switch (lLayerInfo)
        {
            case LayerInfo.Player:
                return ~(1 << m_Player);                

            case LayerInfo.Waypoint:
                return ~(1 << m_WayPoint);

            case LayerInfo.Terrain:
                return ~(1 << m_Terrain);


            case LayerInfo.Landing:
                return ~(1 << m_Landing);

            case LayerInfo.Boundings:
                return ~(1 << m_Boundings);

            case LayerInfo.Ammo:
                return ~(1 << m_Ammo);

            case LayerInfo.FakeFloor:
                return ~(1 << m_FakeFloor);

            case LayerInfo.PlayerAndWayPoint:
                return ~((1 << m_Player) | (1 << m_WayPoint));

        }
        return lLayer;
    }
}
