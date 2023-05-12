using UnityEngine;
using System.Collections;

public class SceneCheck : MonoBehaviour
{

    public InfoManager.LayerInfo m_Layer = InfoManager.LayerInfo.Boundings;
    public bool m_AddMeshCollider        = false;
    // Use this for initialization
    void Awake()
    {
        GameObject lThis       = this.gameObject;
        Transform[] lTransform = GetComponentsInChildren<Transform>();

        for (int i = 0; i < lTransform.Length; i++)
        {
            if (lThis != lTransform[i].gameObject)
            {
                lTransform[i].gameObject.layer = InfoManager.Instance.GetOnlyThisLayer(m_Layer);
                if (m_AddMeshCollider == true)
                {
                    Collider lCollider = lTransform[i].gameObject.GetComponent<Collider>();
                    if (lCollider == null)
                    {
                        MeshCollider lMeshCollider = lTransform[i].gameObject.AddComponent<MeshCollider>();
                        lMeshCollider.isTrigger = false;
                        lMeshCollider.convex = true;
                    }
                }
            }
        }
    }


}

