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
        switch (KillTeamGameManager.Instance.CurrentState)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainSetup:
                KillTeamGameManager.Instance.SetUpItem(null, entry, obj, KillTeamGameManager.LayerType.Terrain);
                break;
            case KillTeamGameManager.GameState.BarracadeSetup:
                KillTeamGameManager.Instance.SetUpItem(null, entry, obj, KillTeamGameManager.LayerType.Barracade);
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                break;
            case KillTeamGameManager.GameState.Deployment:
                KillTeamGameManager.Instance.SetUpItem(null, entry, obj, KillTeamGameManager.LayerType.Player);
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

    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {

        m_menu.SetBackgroundColor(Color.grey);
        m_menu.transform.position = KillTeamGameManager.Instance.menuPoint.transform.position;
        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainSetup:
                m_menu.SetEntries(KillTeamGameManager.Instance.AssetHolder.catalogueEntryTerrain);
                m_menu.AddButton(KillTeamGameManager.Instance.AssetHolder.nextIcon, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadeSetup);
                });
                break;
            case KillTeamGameManager.GameState.BarracadeSetup:
                m_menu.SetEntries(KillTeamGameManager.Instance.AssetHolder.catalogueEntryBaracade);
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
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.Deployment);
                });
                break;
            case KillTeamGameManager.GameState.Deployment:
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



}
