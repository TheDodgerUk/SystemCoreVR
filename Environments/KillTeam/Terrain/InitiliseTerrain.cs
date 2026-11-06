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
    private TerrainPlacementValidator terrainPlacementValidator;

    void Awake()
    {
        terrainPlacementValidator = gameObject.AddComponent<TerrainPlacementValidator>();

        gameObject.AddComponent<TestAutoHand>();
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        this.gameObject.layer = (int)KillTeamGameManager.LayerType.Terrain;
    }


    public void OnDestroy()
    {
    }
}
