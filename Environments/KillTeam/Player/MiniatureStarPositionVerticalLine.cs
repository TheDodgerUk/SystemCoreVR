
using UnityEngine;
using Autohand;


public class MiniatureStarPositionVerticalLine : MiniatureVerticalLine
{
    private void Reset()
    {
        placementLayers =
            1 << (int)KillTeamGameManager.LayerType.PlayerSetupStrip;
    }

}
