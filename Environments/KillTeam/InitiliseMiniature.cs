using Autohand;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Grabbable))]
public class InitiliseMiniture : MonoBehaviour
{

    private MiniatureMovement miniatureMovement;
    private VRMiniatureGrab vRMiniatureGrab;
    private MiniatureVerticalLine miniatureVerticalLine;
    private MiniatureMovementLine miniatureMovementLine;
    private MiniaturePlacementValidator miniaturePlacementValidator;
    // Start is called before the first frame update
    void Start()
    {
        miniatureMovement = gameObject.AddComponent<MiniatureMovement>();
        miniaturePlacementValidator = gameObject.AddComponent<MiniaturePlacementValidator>();
        vRMiniatureGrab = gameObject.AddComponent<VRMiniatureGrab>();
        miniatureVerticalLine = gameObject.AddComponent<MiniatureVerticalLine>();
        miniatureMovementLine = gameObject.AddComponent<MiniatureMovementLine>();

        gameObject.AddComponent<TestAutoHand>();
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }


    public void SetGrabEnabled(bool can)
    {
        vRMiniatureGrab.SetGrabEnabled(can);
    }
    public void OnDestroy()
    {
        Destroy(GetComponent<MiniatureMovement>());
        Destroy(GetComponent<VRMiniatureGrab>());
    }
}
