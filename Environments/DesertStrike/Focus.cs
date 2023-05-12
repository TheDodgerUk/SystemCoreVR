using UnityEngine;
using System.Collections;

public class Focus : MonoBehaviour {

    public enum EFocus
    {
        Show,
        Hide,
    }
    float   m_ShowHeight = 0;
    float   m_HideHeight = -50;
    EFocus  m_Focus      = EFocus.Hide;
    Vector3 m_Position   = Vector3.zero;
    float   m_Height     = 0;

    [SerializeField]
    float m_ShowTimeSeconds = 3;
    [SerializeField]
    float m_RotateSpeed = 200;
    float m_ShowTimeSecondsConverted = 0;
    //-----------------------------------------------------------------------------------------------------------------
    void Start()
    {
        MyGlobals.ClearLocalTransforms(this.gameObject, MyGlobals.ELocalTransform.PositionRotation);
        m_Height = m_HideHeight;
        UpdateY();
        m_ShowTimeSecondsConverted = 1.0f / m_ShowTimeSeconds;
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void SetFocus(EFocus lFocus)
    {
        m_Focus = lFocus;
    }

    //-----------------------------------------------------------------------------------------------------------------
    void UpdateY()
    {        
        m_Position.y = m_Height;
        this.gameObject.transform.localPosition = m_Position;
    }

    //-----------------------------------------------------------------------------------------------------------------
    void Update ()
    {
        float lTargetHeight = 0;

        switch(m_Focus)
        {
            case EFocus.Hide:
                lTargetHeight = m_HideHeight;
                break;
            case EFocus.Show:
                lTargetHeight = m_ShowHeight;
                break;
        }

        m_Height = Mathf.Lerp(lTargetHeight, m_HideHeight, Time.deltaTime * m_ShowTimeSecondsConverted);
        this.transform.Rotate(new Vector3(0, m_RotateSpeed * Time.deltaTime, 0));
        UpdateY();      
    }


}
