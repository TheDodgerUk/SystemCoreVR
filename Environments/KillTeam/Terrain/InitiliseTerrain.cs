using Autohand;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))]
public class InitiliseTerrain : MonoBehaviour
{
    private VRMiniatureGrab vRMiniatureGrab;
    private TerrainPlacementValidator terrainPlacementValidator;
    // Start is called before the first frame update
    void Awake()
    {
        CameraControllerVR.Instance.AutoHandPlayerRef.useMovement = false;
        CameraControllerVR.Instance.AutoHandPlayerRef.snapTurning = false;
        CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer.position = Vector3.zero;
        CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer.rotation = Quaternion.identity;


        terrainPlacementValidator = gameObject.AddComponent<TerrainPlacementValidator>();
        vRMiniatureGrab = gameObject.AddComponent<VRMiniatureGrab>();

        gameObject.AddComponent<TestAutoHand>();
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        this.gameObject.layer = (int)KillTeamGameManager.LayerType.Terrain;
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
