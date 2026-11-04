
using UnityEngine;
using Autohand;

[RequireComponent(typeof(Grabbable))]
[DisallowMultipleComponent]
public class BarracadePlacementValidator : TerrainPlacementValidator
{

    protected override KillTeamGameManager.LayerType mLayer => KillTeamGameManager.LayerType.Barracade;

    protected override void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        grab.enabled = false;
        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainPlacement:
                break;
            case KillTeamGameManager.GameState.BarracadePlacement:
                    grab.enabled = true;
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                break;
            case KillTeamGameManager.GameState.ReadyToStart:
                break;
            case KillTeamGameManager.GameState.Playing:
                break;
            case KillTeamGameManager.GameState.Finished:
                break;
            default:
                break;
        }
    }

}
