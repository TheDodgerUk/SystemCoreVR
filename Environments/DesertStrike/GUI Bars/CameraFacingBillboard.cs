using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour 
{
    
    Camera m_Camera;
    void Awake()
    {
        m_Camera = Camera.main;
        MyGlobals.ClearLocalTransforms(this.gameObject, MyGlobals.ELocalTransform.All);
    }


    //-------------------------------------------------------------------------------------
    void Update()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);
    }
}

