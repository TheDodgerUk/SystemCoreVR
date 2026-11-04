
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(CapsuleCollider))]
public class MiniatureStarPositionVerticalLine : MiniatureVerticalLine
{
    protected override LayerMask placementLayers => (int)KillTeamGameManager.LayerType.PlayerSetupStrip;

}
