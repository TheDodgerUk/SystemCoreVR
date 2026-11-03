using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class VRPrefabMenuController : MonoBehaviour
{
    private Dictionary<KillTeamGameManager.Player, VRPrefabMenu> m_menus = new Dictionary<KillTeamGameManager.Player, VRPrefabMenu>();

    private Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType> TeamSelection = new Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType>();
    // Start is called before the first frame update
    void Start()
    {

        var men1 = new GameObject("VRPrefabMenu1");
        var men2 = new GameObject("VRPrefabMenu2");
        m_menus[KillTeamGameManager.Player.Player1] = men1.AddComponent<VRPrefabMenu>();
        m_menus[KillTeamGameManager.Player.Player2] = men2.AddComponent<VRPrefabMenu>();


        m_menus[KillTeamGameManager.Player.Player1].transform.position = KillTeamGameManager.Instance.helpers.menuPointTable[KillTeamGameManager.Player.Player1];
        m_menus[KillTeamGameManager.Player.Player2].transform.position = KillTeamGameManager.Instance.helpers.menuPointTable[KillTeamGameManager.Player.Player2];

        m_menus[KillTeamGameManager.Player.Player1].SetBackgroundColor(Color.grey);
        m_menus[KillTeamGameManager.Player.Player2].SetBackgroundColor(Color.grey);
        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
    }

    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {

        int NumberOfPlayers = KillTeamGameManager.Instance.IsSinglePlayer ? 1 : 2;

        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainAssetCreation:
                KillTeamGameManager.Instance.AssetHolder.SpawnTerrain(() =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TerrainPlacement);
                });
                break;
            case KillTeamGameManager.GameState.TerrainPlacement:
                m_menus[KillTeamGameManager.Player.Player1].SetEntries(new List<CatalogueEntry>());
                m_menus[KillTeamGameManager.Player.Player1].AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadeAssetCreation);                   
                });


                // buttons for player 1 and 2 to move to next state
                KillTeamGameManager.Instance.OnButttonPressed(KillTeamGameManager.Player.Player1, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadeAssetCreation);
                });
                KillTeamGameManager.Instance.OnButttonPressed(KillTeamGameManager.Player.Player2, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadeAssetCreation);
                });


                break;
            case KillTeamGameManager.GameState.BarracadeAssetCreation:
                KillTeamGameManager.Instance.AssetHolder.SpawnBarracades(() =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadePlacement);
                });
                break;
            case KillTeamGameManager.GameState.BarracadePlacement:
                m_menus[KillTeamGameManager.Player.Player1].SetEntries(new List<CatalogueEntry>());
                m_menus[KillTeamGameManager.Player.Player1].AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamSelection);
                });
                KillTeamGameManager.Instance.OnButttonPressed(KillTeamGameManager.Player.Player1, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamSelection);
                });
                KillTeamGameManager.Instance.OnButttonPressed(KillTeamGameManager.Player.Player2, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamSelection);
                });


                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:

                TaskAction BothTeamsChoisen = new TaskAction(2, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamsAssetCreation);
                });

                m_menus[KillTeamGameManager.Player.Player1].SetEntries(KillTeamGameManager.Instance.AssetHolder.teamIcons, (itemIndex, other) =>
                {
                    TeamSelection[KillTeamGameManager.Player.Player1] = (ContentKillTeamMetaData.TeamType)itemIndex;
                    BothTeamsChoisen.Increment();
                });
                m_menus[KillTeamGameManager.Player.Player2].SetEntries(KillTeamGameManager.Instance.AssetHolder.teamIcons, (itemIndex, other) =>
                {
                    TeamSelection[KillTeamGameManager.Player.Player2] = (ContentKillTeamMetaData.TeamType)itemIndex;
                    BothTeamsChoisen.Increment();
                });

                break;
            case KillTeamGameManager.GameState.TeamsAssetCreation:
                KillTeamGameManager.Instance.AssetHolder.SpawnPlayers(TeamSelection, ()=>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });
                KillTeamGameManager.Instance.OnButttonPressed(KillTeamGameManager.Player.Player1, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });
                KillTeamGameManager.Instance.OnButttonPressed(KillTeamGameManager.Player.Player2, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });

                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                m_menus[KillTeamGameManager.Player.Player1].SetEntries(new List<CatalogueEntry>());
                m_menus[KillTeamGameManager.Player.Player1].AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.ReadyToStart);
                });
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

    private void Instance_OnButttonPressed()
    {
        throw new NotImplementedException();
    }

    private void RemoveNonPlacedTerrain()
    {
        List<VrInteraction> nonValid = new List<VrInteraction>();
        var all = Core.Scene.GetAllSpawnedVrInteraction();
        foreach (var item in all)
        {
            var val = item.gameObject.GetComponent<TerrainPlacementValidator>();
            if(val != null && val.canPlace == false)
            {
                nonValid.Add(item);
            }
        }
        foreach (var item in nonValid)
        {
            Core.Scene.RemoveSpawnedVrInteractionAndGameObject(item);
        }



    }



}
