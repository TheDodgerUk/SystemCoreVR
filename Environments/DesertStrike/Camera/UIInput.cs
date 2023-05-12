using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

public class UIInput : MonoBehaviour
{

    public enum Pov
    {
        Top,
        Bottom,
        Left,
        Right,
    }

    List<GameObject> m_PovObjects = new List<GameObject>();


    public enum MouseState
    {
        FirstPress,
        Held,
        Released,
        Nothing

    }

    public enum MouseType
    {
        PlayerMove,
        ScrollScreen,
    }

    bool m_SetOnce = false;
    private WayPoints m_WayPoints;


    private MouseState m_MouseState = MouseState.Nothing;
    private MouseType m_MouseType = MouseType.PlayerMove;


    public MouseState GetMouseState() { return m_MouseState; }
    public MouseType GetMouseType() { return m_MouseType; }

    private GameObject m_CurrentSelectedObject = null;

    //[NotNull]
    [SerializeField]
    GameObject m_FakeFloor;

    float m_AboveAmount = 5;


    [SerializeField]
    float m_CameraMoveSpeed = 10;
    private Vector3 m_NewMousePosition;
    private Vector3 m_LastMousePosition;


    //-------------------------------------------------------------------------
    void _SetRemoveDistance(GameObject lGameObject, float lRemoveDistance)
    {
        m_WayPoints.SetRemoveDistance(lRemoveDistance);
    }



    
    //-------------------------------------------------------------------------
    // Use this for initialization
    void Awake()
    {
        float lAngle     = Camera.main.fieldOfView /2;
        float lAngleSide = Camera.main.fieldOfView  * (16f / 9f) / ((float)Camera.main.pixelWidth / Camera.main.pixelHeight);
        int lLength      = System.Enum.GetValues(typeof(Pov)).Length;
        for (int i = 0; i < lLength; i++)
        {
            GameObject lGameObject         = new GameObject();
            lGameObject.transform.parent = this.gameObject.transform;
            lGameObject.transform.localPosition = Vector3.zero;
            lGameObject.transform.localRotation = Quaternion.identity;
            Pov lDirection = (Pov)i;
            if (Camera.main.orthographic == false)
            {
                switch (lDirection)
                {
                    case Pov.Top:
                        lGameObject.transform.localRotation = Quaternion.Euler(-lAngle, 0, 0);
                        break;

                    case Pov.Bottom:
                        lGameObject.transform.localRotation = Quaternion.Euler(lAngle, 0, 0);
                        break;

                    case Pov.Left:
                        lGameObject.transform.localRotation = Quaternion.Euler(0, -lAngleSide, 0);
                        break;

                    case Pov.Right:
                        lGameObject.transform.localRotation = Quaternion.Euler(0, lAngleSide, 0);
                        break;

                }
            }
            else
            {
                float lSize = Camera.main.orthographicSize;
                switch (lDirection)
                {
                    case Pov.Top:
                        lGameObject.transform.localPosition += new Vector3(0, lSize,0);
                        break;

                    case Pov.Bottom:
                        lGameObject.transform.localPosition += new Vector3(0, -lSize,0);
                        break;

                    case Pov.Left:
                        lGameObject.transform.localPosition += new Vector3(-lSize*2, 0, 0);
                        break;

                    case Pov.Right:
                        lGameObject.transform.localPosition += new Vector3(lSize*2, 0, 0);
                        break;

                }
                lGameObject.transform.localRotation = Quaternion.Euler(Vector3.down);

            }
            
            lGameObject.name = lDirection.ToString();
            m_PovObjects.Add(lGameObject);
        }

    }

    //--------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnGUI()
    {
        string lString = "#if UNITY_EDITOR   \nm_MouseState:" + m_MouseState.ToString() + "\nm_MouseType" + m_MouseType.ToString();
        GUI.Label(new Rect(100, 100, Screen.width, Screen.height), lString);
    }
#endif
    //--------------------------------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void Update()
    {
        m_LastMousePosition = m_NewMousePosition;
        m_NewMousePosition = MyGlobals.GetPositionOfTouchMouse(0);
        _UpdateMouseState();
        _UpdateMouseMovement();
    }

    #region MOUSE
    //--------------------------------------------------------------------------------------------------------------------------
    /// <summary>_UpdateMouseState</summary>
    private void _UpdateMouseState()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (m_MouseState == MouseState.Nothing)
            {
                m_MouseState = MouseState.FirstPress;
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_MouseState == MouseState.Held || m_MouseState == MouseState.FirstPress)
            {
                m_MouseState = MouseState.Released;
            }
        }
        else
        {
            if (m_MouseState == MouseState.FirstPress)
            {
                m_MouseState = MouseState.Held;
            }
            else if (m_MouseState == MouseState.Released)
            {
                m_MouseState = MouseState.Nothing;
            }

        }

    }


    //-------------------------------------------------------------------------
    /// <summary>_UpdateMouseMovement</summary>
    private void _UpdateMouseMovement()
    {

        RaycastHit lHit;
        Ray lRay;
        switch (m_MouseState)
        {
            case MouseState.FirstPress:

                lRay = MyGlobals.GetRayFromTouchMousePositionToScreen(0);
                if (Physics.Raycast(lRay, out lHit, Mathf.Infinity))
                {
                    m_MouseType = MouseType.PlayerMove;
                    GameObject lObjectHit = MyGlobals.GetBaseParentObject(lHit.transform.gameObject);

                    if (lObjectHit.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Player))
                    {
                        //---------------------------------------------
                        // move fake floor to height 
                        //----------------------------------------------
                        m_FakeFloor.transform.position = new Vector3(m_FakeFloor.transform.position.x, lObjectHit.gameObject.transform.position.y, m_FakeFloor.transform.position.z);
                        m_WayPoints = lObjectHit.GetComponent<WayPoints>();
                        if (m_WayPoints == null)
                        {
                            m_WayPoints = lObjectHit.AddComponent<WayPoints>();
                            m_WayPoints.SetColour(InfoManager.Instance.GetNextWayPointColour());
                            _SetRemoveDistance(lObjectHit, InfoManager.Instance.GetRemoveDistance(InfoManager.TagInfo.Player));

                        }
                        m_WayPoints.ClearAll();

                        //---------------------------------------------------------------------------
                        //  make ray from the chosen obect to the terrain, to get the m_AboveAmount amount;
                        //---------------------------------------------------------------------------
                        lRay = new Ray(m_WayPoints.gameObject.transform.position, Vector3.down);
                        if (Physics.Raycast(lRay, out lHit, Mathf.Infinity, InfoManager.Instance.GetOnlyThisLayerMask(InfoManager.LayerInfo.Terrain)))
                        {
                            m_AboveAmount = Vector3.Distance(lHit.point, m_WayPoints.gameObject.transform.position);                           
                        }

  
                    }
                    //------------------------------------------------------
                    // kept finding layer = 0  dont know why  
                    //------------------------------------------------------
                    else //if (lObjectHit.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Terrain) || lObjectHit.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.FakeFloor))
                    {
                        m_MouseType = MouseType.ScrollScreen;
                    }
                    /*
                    else
                    {
                        GameObject lNew = new GameObject();
                        lNew.transform.position = lHit.transform.position;
                        Debug.LogError(lObjectHit.layer);
                        m_MouseState = MouseState.Nothing;
                    }
                    */

                }
                break;

            case MouseState.Held:
                if (m_MouseType == MouseType.PlayerMove)
                {
                    AddWayPoint();
                }
                else if (m_MouseType == MouseType.ScrollScreen)
                {
                    MoveScreen();
                }

                break;
            case MouseState.Released:
                if (m_MouseType == MouseType.PlayerMove)
                {
                    m_FakeFloor.transform.position = new Vector3(m_FakeFloor.transform.position.x, 0, m_FakeFloor.transform.position.z);
                }
                m_MouseState = MouseState.Nothing;
                break;
            case MouseState.Nothing:
                break;

        }

    }

    #endregion




    void AddWayPoint()
    {
        RaycastHit lHit;
        Ray lRay;
        if (m_WayPoints != null)
        {
            lRay = MyGlobals.GetRayFromTouchMousePositionToScreen(0);
            if (Physics.Raycast(lRay, out lHit, Mathf.Infinity, InfoManager.Instance.GetOnlyThisLayerMask(InfoManager.LayerInfo.FakeFloor)))
            {
                Vector3 lStartWaypointPosition = lHit.point; 
                Vector3 lPoint       = new Vector3(lHit.point.x, 100, lHit.point.z);
                float lAboveAmount   = m_AboveAmount;
                if (Physics.Raycast(lPoint,Vector3.down, out lHit, Mathf.Infinity, InfoManager.Instance.GetOnlyThisLayerMask(InfoManager.LayerInfo.Terrain)))
                {
                    lAboveAmount = lHit.point.y + m_AboveAmount;
                }
                
                m_WayPoints.AddPoint(lStartWaypointPosition, lAboveAmount);
            }
        }
    }



    void MoveScreen()
    {
        Vector3 lMouse = m_LastMousePosition - m_NewMousePosition;
        var lMainCamerOverHead = MyGlobals.GetBaseParentObject(Camera.main.gameObject);
        lMainCamerOverHead.transform.Translate(Vector3.right   * lMouse.x * Time.deltaTime * m_CameraMoveSpeed);
        lMainCamerOverHead.transform.Translate(Vector3.forward * lMouse.y * Time.deltaTime * m_CameraMoveSpeed);

        bool lLeftRight;
        bool lUpDown;
        if (IsOutOfBounds(out lLeftRight, out lUpDown) == true)
        {
            if (lUpDown == true)
            {
                lMainCamerOverHead.transform.Translate(Vector3.forward * -lMouse.y * Time.deltaTime * m_CameraMoveSpeed);
            }
            if(lLeftRight == true)
            {
                lMainCamerOverHead.transform.Translate(Vector3.right * -lMouse.x * Time.deltaTime * m_CameraMoveSpeed);
            }
        }
    }


 

    bool IsOutOfBounds(out bool lLeftRight,out bool lUpDown)
    {
        Pov lPov = Pov.Top;
        lLeftRight  = false;
        lUpDown     = false;
        RaycastHit lHit;
        for (int i = 0; i <m_PovObjects.Count; i++)
        {

            Ray lRay = new Ray(m_PovObjects[i].transform.position, m_PovObjects[i].transform.forward);
            Debug.DrawRay(m_PovObjects[i].transform.position, m_PovObjects[i].transform.forward * 1000, Color.red);
            if (Physics.Raycast(lRay, out lHit, 1000 )== true)
            {
                if (lHit.collider.gameObject.layer == InfoManager.Instance.GetOnlyThisLayer(InfoManager.LayerInfo.Boundings))
                {
                    lPov = (Pov)i;
                    if (lPov == Pov.Top || lPov == Pov.Bottom)
                    {
                        lUpDown = true;
                    }
                    else
                    {
                        lLeftRight = true;
                    }

                }
            }
        }
        return (lLeftRight == true || lUpDown == true);

  }



}
