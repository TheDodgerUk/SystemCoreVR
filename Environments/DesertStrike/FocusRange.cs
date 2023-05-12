using UnityEngine;
using System.Collections;

public class FocusRange : MonoBehaviour
{
    public enum ERange
    {
        RealDistance,
        FlatDistance,
    }

    [SerializeField]
    ERange m_RangeEnum = ERange.RealDistance;

    [System.Serializable]
    public class CRange
    {
        public GameObject   m_GamObject;
        public float        m_MaxRadius = 200;
    }
    public CRange[] m_RangeClass;

    GameObject   m_Target;
    GameObject[] m_InstansitedGameObject;
    int          m_CurrentIndex = -10;
    float        m_OldIndexY    = 0;
    float        m_NewIndexY    = 0;
    //--------------------------------------------------------------------------------------------------------------------------------
    [ExecuteInEditMode]
    void OnValidate()
    {
        System.Array.Sort(m_RangeClass, delegate (CRange x, CRange y) { return x.m_MaxRadius.CompareTo(y.m_MaxRadius); });
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    void Awake ()
    {
        m_InstansitedGameObject = new GameObject[m_RangeClass.Length];
        for(int i = 0; i < m_RangeClass.Length; i++)
        {
            if (m_RangeClass[i].m_GamObject != null)
            {
                GameObject lGameObject           = Instantiate(m_RangeClass[i].m_GamObject, Vector3.zero, Quaternion.identity) as GameObject;
                lGameObject.name                 = "Focus:" + m_RangeClass[i].m_MaxRadius;
                lGameObject.transform.parent     = MyGlobals.GetBaseParentObject(this.gameObject).transform;
                m_InstansitedGameObject[i]       = lGameObject;                
                lGameObject.transform.localScale = new Vector3(m_RangeClass[i].m_MaxRadius * 2, m_RangeClass[i].m_GamObject.transform.localScale.y, m_RangeClass[i].m_MaxRadius* 2);
                MyGlobals.ClearLocalTransforms(lGameObject, MyGlobals.ELocalTransform.PositionRotation);
            }
        }
    }


    //-------------------------------------------------------------------------------------------------------------------
    public void SetTarget(GameObject lTarget)
    {
        m_Target = lTarget;
    }
    

    //-------------------------------------------------------------------------------------------------------------------
    void Update ()
    {
        //--------------------------------------------------
        //          if no target , then no focus
        //--------------------------------------------------
        if (m_Target == null)
        {
            for (int i = 0; i < m_RangeClass.Length; i++)
            {
                Focus.EFocus lFocus = Focus.EFocus.Hide;
                m_InstansitedGameObject[i].GetComponent<Focus>().SetFocus(lFocus);
            }
            return;
        }


        //--------------------------------------------------
        //          find distance
        //--------------------------------------------------
        float lRange = 0;
        switch (m_RangeEnum)
        {
            case ERange.RealDistance:
                lRange = Vector3.Distance(m_Target.transform.position, MyGlobals.GetBaseParentObject(this.gameObject).transform.position);
                break;

            case ERange.FlatDistance:
                lRange = MyGlobals.GetFlatDistance(m_Target.transform.position, MyGlobals.GetBaseParentObject(this.gameObject).transform.position);
                break;
        }


        //--------------------------------------------------
        //          get the correct m_RangeClass
        //--------------------------------------------------
        int lNewIndex = -1;
        for (int i = m_RangeClass.Length -1; i >= 0; i--)
        {
            if(m_RangeClass[i].m_MaxRadius > lRange)
            {
                lNewIndex = i;
                break;
            }
        }

        


       //--------------------------------------------------
       //         if range changed then turn on correct m_RangeClass
       //--------------------------------------------------
       if (m_CurrentIndex != lNewIndex)
       {
           
           m_CurrentIndex = lNewIndex;

           for (int i = 0; i < m_RangeClass.Length; i++)
           {
                Focus.EFocus lFocus = (m_CurrentIndex == i) ? Focus.EFocus.Show : Focus.EFocus.Hide;
               m_InstansitedGameObject[i].GetComponent<Focus>().SetFocus(lFocus);
           }
       }       
    }
    //-------------------------------------------------------------------------------------------------------------------


}
