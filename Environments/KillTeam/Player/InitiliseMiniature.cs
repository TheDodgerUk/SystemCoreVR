using Autohand;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(MiniaturePlacementValidator))]
public class InitiliseMiniture : MonoBehaviour
{


    // Start is called before the first frame update
    void Awake()
    {
        CameraControllerVR.Instance.AutoHandPlayerRef.useMovement = false;
        CameraControllerVR.Instance.AutoHandPlayerRef.snapTurning = false;
        CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer.position = Vector3.zero;
        CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer.rotation = Quaternion.identity;


        gameObject.AddComponent<TestAutoHand>();
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void OnDestroy()
    {
    }
}
