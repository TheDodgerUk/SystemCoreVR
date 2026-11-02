using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class VRPrefabMenuController : MonoBehaviour
{
    private VRPrefabMenu m_menu;
    // Start is called before the first frame update
    void Start()
    {
        m_menu = this.AddComponent<VRPrefabMenu>();
        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
        m_menu.SpawnedCallback += (entry, item) =>
        {
            Setup(entry, item);
        };
    }

    private void Setup(CatalogueEntry entry, ModuleObject obj)
    {
    //    this.gameObject.SetActive(false);
    //    switch (KillTeamGameManager.Instance.CurrentState)
    //    {
    //        case KillTeamGameManager.GameState.WaitingForPlayers:
    //            break;
    //        case KillTeamGameManager.GameState.BoardSetup:
    //            break;
    //        case KillTeamGameManager.GameState.TerrainSetup:
    //            this.gameObject.SetActive(false);
    //        case KillTeamGameManager.GameState.BarracadeSetup:
    //            this.gameObject.SetActive(false);
    //            KillTeamGameManager.Instance.SetUpItem(null, entry, obj, KillTeamGameManager.LayerType.Barracade);
    //            break;
    //        case KillTeamGameManager.GameState.MissionSetup:
    //            break;
    //        case KillTeamGameManager.GameState.TeamSelection:
    //            break;
    //        case KillTeamGameManager.GameState.Deployment:
    //            this.gameObject.SetActive(true);
    //            KillTeamGameManager.Instance.SetUpItem(null, entry, obj, KillTeamGameManager.LayerType.Player);
    //            break;
    //        case KillTeamGameManager.GameState.ReadyToStart:
    //            break;
    //        case KillTeamGameManager.GameState.Playing:
    //            break;
    //        case KillTeamGameManager.GameState.Finished:
    //            break;
    //        default:
    //            break;
    //    }
    }
    int item;
    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
       
        m_menu.SetBackgroundColor(Color.grey);
        m_menu.transform.position = KillTeamGameManager.Instance.helpers.menuPoint.transform.position;
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
                m_menu.SetEntries(new List<CatalogueEntry>());
                m_menu.AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
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
                m_menu.SetEntries(new List<CatalogueEntry>());
                m_menu.AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamSelection);
                });
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                m_menu.SetEntries(KillTeamGameManager.Instance.AssetHolder.teamIcons, (item, other) =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamsAssetCreation);
                });
                break;
            case KillTeamGameManager.GameState.TeamsAssetCreation:
                KillTeamGameManager.Instance.AssetHolder.SpawnPlayers((ContentKillTeamMetaData.TeamType)(item), ()=>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });
                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                m_menu.SetEntries(new List<CatalogueEntry>());
                m_menu.AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
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
