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

    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        RemoveNonPLacedTerrain();
        m_menu.SetBackgroundColor(Color.grey);
        m_menu.transform.position = KillTeamGameManager.Instance.helpers.menuPoint.transform.position;
        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainSetup:
                KillTeamGameManager.Instance.AssetHolder.SpawnTerrain();
                m_menu.SetEntries(new List<CatalogueEntry>());
                m_menu.AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadeSetup);                   
                });
                break;
            case KillTeamGameManager.GameState.BarracadeSetup:
                KillTeamGameManager.Instance.AssetHolder.SpawnBarracades();
                m_menu.SetEntries(new List<CatalogueEntry>());
                m_menu.AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamSelection);
                });
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                m_menu.SetEntries(KillTeamGameManager.Instance.AssetHolder.teamIcons, (item, other) =>
                {
                    m_menu.SetEntries(KillTeamGameManager.Instance.AssetHolder.catalogueEntryPlayer[(ContentKillTeamMetaData.TeamType)(item)]);
                    KillTeamGameManager.Instance.AssetHolder.SpawnPlayers((ContentKillTeamMetaData.TeamType)(item));
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.Deployment);
                });
                break;
            case KillTeamGameManager.GameState.Deployment:
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

    private void RemoveNonPLacedTerrain()
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
