using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericStateMachine : MonoBehaviour
{

    private Dictionary<KillTeamGameManager.Player, DynamicImagePlane> m_menus = new Dictionary<KillTeamGameManager.Player, DynamicImagePlane>();

    private Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType> TeamSelection = new Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType>();

    private List<KillTeamGameManager.Player> taskActionList = new List<KillTeamGameManager.Player>();


    private Dictionary<KillTeamGameManager.Player, int> playerTeamIndex = new Dictionary<KillTeamGameManager.Player, int>();


    void Start()
    {
        var men1 = new GameObject("DynamicImagePlane1");
        var men2 = new GameObject("DynamicImagePlane2");
        m_menus[KillTeamGameManager.Player.Player1] = men1.AddComponent<DynamicImagePlane>();
        m_menus[KillTeamGameManager.Player.Player2] = men2.AddComponent<DynamicImagePlane>();

        m_menus[KillTeamGameManager.Player.Player1].transform.position = KillTeamGameManager.Instance.helpers.DynamicImagePlanePoint[KillTeamGameManager.Player.Player1];
        m_menus[KillTeamGameManager.Player.Player2].transform.position = KillTeamGameManager.Instance.helpers.DynamicImagePlanePoint[KillTeamGameManager.Player.Player2];


        KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
    }


    private void TaskActionTest(TaskAction task, KillTeamGameManager.Player player)
    {
        if (taskActionList.Contains(player) == false)
        {
            taskActionList.Add(player);
            task.Increment();
        }
    }

    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        PhysicalButtons.Instance.ClearAllButtonCallbacks();
        taskActionList.Clear();
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

                // buttons for player 1 and 2 to move to next state
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Next, () =>
                {
                    RemoveNonPlacedTerrain();
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.BarracadeAssetCreation);
                });
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Next, () =>
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

                TaskAction BothPlayersPlacedBarricades = new TaskAction(NumberOfPlayers, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamSelection);
                });
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Next, () =>
                {
                    RemoveNonPlacedTerrain();
                    TaskActionTest(BothPlayersPlacedBarricades, KillTeamGameManager.Player.Player1);
                });
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Next, () =>
                {
                    RemoveNonPlacedTerrain();
                    TaskActionTest(BothPlayersPlacedBarricades, KillTeamGameManager.Player.Player2);
                });


                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                TeamSelectionMethod(NumberOfPlayers);
                break;
            case KillTeamGameManager.GameState.TeamsAssetCreation:
                KillTeamGameManager.Instance.AssetHolder.SpawnPlayers(TeamSelection, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Next, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Next, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.SetCharacterToStartPlacement);
                });

                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                TaskAction BothTeamsChoisen1 = new TaskAction(NumberOfPlayers, () =>
                {
                    KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.ReadyToStart);
                });

                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Next, () =>
                {
                    TaskActionTest(BothTeamsChoisen1, KillTeamGameManager.Player.Player1);
                });
                PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Next, () =>
                {
                    TaskActionTest(BothTeamsChoisen1, KillTeamGameManager.Player.Player2);
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


    private void TeamSelectionMethod(int numPlayers)
    {
        TaskAction BothTeamsChoisen = new TaskAction(numPlayers, () =>
        {
            KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamsAssetCreation);
        });



        var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player1]];
        m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);

        var image2 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player2]];
        m_menus[KillTeamGameManager.Player.Player1].SetImage(image2);


        // right 
        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Right, () =>
        {
            playerTeamIndex[KillTeamGameManager.Player.Player1]++;
            if (playerTeamIndex[KillTeamGameManager.Player.Player1] > KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1)
            {
                playerTeamIndex[KillTeamGameManager.Player.Player1] = 0;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player1]];
            m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);
        });

        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Right, () =>
        {
            playerTeamIndex[KillTeamGameManager.Player.Player2]++;
            if (playerTeamIndex[KillTeamGameManager.Player.Player2] > KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1)
            {
                playerTeamIndex[KillTeamGameManager.Player.Player2] = 0;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player2]];
            m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);
        });

        // left 
        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Left, () =>
        {
            playerTeamIndex[KillTeamGameManager.Player.Player1]--;
            if (playerTeamIndex[KillTeamGameManager.Player.Player1] < 0)
            {
                playerTeamIndex[KillTeamGameManager.Player.Player1] = KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player1]];
            m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);
        });

        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Left, () =>
        {
            playerTeamIndex[KillTeamGameManager.Player.Player2]--;
            if(playerTeamIndex[KillTeamGameManager.Player.Player2] < 0)
            {
                playerTeamIndex[KillTeamGameManager.Player.Player2] = KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player2]];
            m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);
        });




        // next buttons
        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Next, () =>
        {
            TaskActionTest(BothTeamsChoisen, KillTeamGameManager.Player.Player1);
        });
        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Next, () =>
        {
            TaskActionTest(BothTeamsChoisen, KillTeamGameManager.Player.Player2);
        });



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
            if (val != null && val.canPlace == false)
            {
                nonValid.Add(item);
            }

            var val1 = item.gameObject.GetComponent<BarracadePlacementValidator>();
            if (val1 != null && val.canPlace == false)
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