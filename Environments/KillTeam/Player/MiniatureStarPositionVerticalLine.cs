
using UnityEngine;
using Autohand;


public class MiniatureStarPositionVerticalLine : MiniatureVerticalLine
{
    protected override LayerMask placementLayers => (int)KillTeamGameManager.LayerType.PlayerSetupStrip;

}
