using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugColliderDetector : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogError($"OnCollisionEnter", collision.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.LogError($"OnCollisionStay", collision.gameObject);
    }
}
