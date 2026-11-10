using Autohand;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))]
public class InitiliseBarracade : MonoBehaviour
{
    private BarracadePlacementValidator barracadePlacementValidator;

    void Awake()
    {
        barracadePlacementValidator = gameObject.AddComponent<BarracadePlacementValidator>();

        gameObject.AddComponent<TestAutoHand>();
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        this.gameObject.layer = (int)KillTeamGameManager.LayerType.Barracade;
    }



    public void OnDestroy()
    {
    }
}
