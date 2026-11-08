using Autohand;
using ExitGames.Client.Photon.StructWrapping;
using KillTeam;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using static KillTeamGameManager;
using System.Runtime.CompilerServices;

public class GenericStateMachine : MonoBehaviour
{



    private List<KillTeamGameManager.Player> taskActionList = new List<KillTeamGameManager.Player>();




    private UnitTestingLayout unitTestingLayout;

    void Start()
    {

        Core.Environment.OnEnvironmentLoadingComplete += () =>
        {
            unitTestingLayout = this.AddComponent<UnitTestingLayout>();



            KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1] = 0;
            KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2] = 0;

            KillTeamGameManager.Instance.AssetHolder.TeamSelection.Clear();
            KillTeamGameManager.Instance.AssetHolder.TeamSelection.Add(KillTeamGameManager.Player.Player1, ContentKillTeamMetaData.TeamType.Kommandos);
            KillTeamGameManager.Instance.AssetHolder.TeamSelection.Add(KillTeamGameManager.Player.Player2, ContentKillTeamMetaData.TeamType.AngelsOfDeath);


            KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
            KillTeamGameManager_OnStateChanged(KillTeamGameManager.Instance.CurrentState);

            this.AddComponent<SkyboxController>();
            this.AddComponent<VRInputDebug>();

            this.AddComponent<XRTrackingDebug>();


            foreach (var pointer in CameraControllerVR.Instance.HandGameObjectPointerRef)
            {
                if (pointer == null)
                    continue;
                pointer.OnLaserClick.AddListener(OnClick);
                pointer.OnLaserRelease.AddListener(OnRelease);
                pointer.OnLaserHoverEnter.AddListener(OnHoverEnter);
                pointer.OnLaserHoverExit.AddListener(OnHoverExit);
            }
        };
    }

    private void OnClick(Vector3 point, GameObject target)
    {
        Debug.Log("Clicked: " + target.name);
    }

    private void OnRelease(Vector3 point, GameObject target)
    {
        Debug.Log("Released: " + target.name);
    }

    private void OnHoverEnter(Vector3 point, GameObject target)
    {
        Debug.Log("Hover Enter: " + target.name);
    }

    private void OnHoverExit(Vector3 point, GameObject target)
    {
        Debug.Log("Hover Exit: " + target.name);
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
        // delete all tests
        unitTestingLayout.ClearAll();
        Table.Instance.playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(false));

        if (PhysicalButtons.Instance != null && PhysicalButtons.Instance.m_menus != null && PhysicalButtons.Instance.m_menus.Count != 0)
        {
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player1].SetActive(false);
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player2].SetActive(false);
        }
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
                Table.Instance.playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(true));
                TaskAction BothPlayersPlacedBarricades = new TaskAction(NumberOfPlayers, "BarracadePlacement", () =>
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
                TeamSelectionMethod(NumberOfPlayers, KillTeamGameManager.GameState.TeamsAssetCreation);
                break;
            case KillTeamGameManager.GameState.TeamsAssetCreation:
                KillTeamGameManager.Instance.AssetHolder.SpawnPlayers(KillTeamGameManager.Instance.AssetHolder.TeamSelection, () =>
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
                Table.Instance.playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(true));
                TaskAction BothTeamsChoisen1 = new TaskAction(NumberOfPlayers, "SetCharacterToStartPlacement", () =>
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


                // quick testing 
            case KillTeamGameManager.GameState.DEBUG_SlowSceneCreation:
                QuickSceneCreation(5f);
                break;


            case KillTeamGameManager.GameState.DEBUG_SlowPlayerCreation:
                QuickPlayerCreation(5f);
                break;


                // unit testing 
            case KillTeamGameManager.GameState.UnitTest_StartStripTesting:
                unitTestingLayout.StartStripTesting();
                break;
            case KillTeamGameManager.GameState.UnitTest_StartMainTesting:
                unitTestingLayout.StartMainTesting();
                break;
            default:
                break;
        }

    }


    private void TeamSelectionMethod(int numPlayers, KillTeamGameManager.GameState nextstate)
    {
        if (numPlayers == 1)
        {
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player1].SetActive(true);
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player2].SetActive(false);
        }
        else
        {
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player1].SetActive(true);
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player2].SetActive(true);
        }



        TaskAction BothTeamsChoisen = new TaskAction(numPlayers, "TeamSelectionMethod", () =>
        {
            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1]];
            var image2 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2]];
            KillTeamGameManager.Instance.AssetHolder.TeamSelection.Clear();
            KillTeamGameManager.Instance.AssetHolder.TeamSelection.Add(KillTeamGameManager.Player.Player1, ContentKillTeamMetaData.TeamType.Kommandos);
            KillTeamGameManager.Instance.AssetHolder.TeamSelection.Add(KillTeamGameManager.Player.Player2, ContentKillTeamMetaData.TeamType.Kommandos);

            if (Enum.TryParse(image1.name, out ContentKillTeamMetaData.TeamType player1))
            {
                KillTeamGameManager.Instance.AssetHolder.TeamSelection[KillTeamGameManager.Player.Player1] = player1;
            }
            if (Enum.TryParse(image2.name, out ContentKillTeamMetaData.TeamType player2))
            {
                KillTeamGameManager.Instance.AssetHolder.TeamSelection[KillTeamGameManager.Player.Player2] = player2;
            }
            KillTeamGameManager.Instance.SetState(nextstate);
        });



        var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1]];
        PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);

        var image2 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2]];
        PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player2].SetImage(image2);


        // right 
        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Right, () =>
        {
            KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1]++;
            if (KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1] > KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1)
            {
                KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1] = 0;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1]];
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);
        });

        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Right, () =>
        {
            KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2]++;
            if (KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2] > KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1)
            {
                KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2] = 0;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2]];
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player2].SetImage(image1);
        });

        // left 
        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player1, PhysicalButtons.ButtonType.Left, () =>
        {
            KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1]--;
            if (KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1] < 0)
            {
                KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1] = KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player1]];
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);
        });

        PhysicalButtons.Instance.OnButtonPressed(KillTeamGameManager.Player.Player2, PhysicalButtons.ButtonType.Left, () =>
        {
            KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2]--;
            if (KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2] < 0)
            {
                KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2] = KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[KillTeamGameManager.Instance.AssetHolder.playerTeamIndex[KillTeamGameManager.Player.Player2]];
            PhysicalButtons.Instance.m_menus[KillTeamGameManager.Player.Player2].SetImage(image1);
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





    private void QuickSceneCreation(float waitTime)
    {
        int itemsToMake = 3;
        TaskAction taskTerrain = new TaskAction(itemsToMake, "QuickSceneCreation",() =>
        {
            RemoveNonPlacedTerrain();
        });

        KillTeamGameManager.Instance.AssetHolder.SpawnTerrain(() =>
        {
            var mesh = Table.Instance.BoardRoot.GetComponent<BoxCollider>();
            var all = Core.Scene.GetAllSpawnedVrInteraction();
            for (int i = 0; i < itemsToMake; i++)
            {
                GameObject obj = all[i].gameObject;
                obj.gameObject.transform.position = mesh.GetRandomPointOnBox(0.01f);
                obj.gameObject.transform.position = new Vector3(obj.gameObject.transform.position.x, obj.gameObject.transform.position.y + 0.2f, obj.gameObject.transform.position.z);


                Grabbable grabbable = obj.GetComponent<Grabbable>();

                if (grabbable != null)
                {
                    StartCoroutine(FakeGrabAndRelease(waitTime, grabbable, taskTerrain));
                }
            }
        });
    }


    private void QuickPlayerCreation(float waitTime)
    {
        int itemsToMake = KillTeamGameManager.Instance.AssetHolder.TotalPlayerCount();


        TaskAction taskTerrain = new TaskAction(itemsToMake, "QuickPlayerCreation", () =>
        {
            Debug.Log($"QuickPlayerCreation taskTerrain");


             RemoveNonPlacedTerrain();

            foreach (KillTeamGameManager.Player p in (KillTeamGameManager.Player[])Enum.GetValues(typeof(KillTeamGameManager.Player)))
            {
                Table.Instance.playerSetupStrip[p].SetActive(false);
            }
        });



        foreach (KillTeamGameManager.Player p in (KillTeamGameManager.Player[])Enum.GetValues(typeof(KillTeamGameManager.Player)))
        {
            Table.Instance.playerSetupStrip[p].SetActive(true);
        }

        KillTeamGameManager.Instance.AssetHolder.SpawnPlayers(KillTeamGameManager.Instance.AssetHolder.TeamSelection, () =>
            {
                foreach (KillTeamGameManager.Player p in (KillTeamGameManager.Player[])Enum.GetValues(typeof(KillTeamGameManager.Player)))
                {

                    var player1Items = KillTeamGameManager.Instance.AssetHolder.playerGameObjects[p];
                    var mesh = Table.Instance.playerSetupStrip[p].GetComponent<BoxCollider>();
                    unitTestingLayout.FitBoxColliderToMesh(mesh);
                    foreach (var obj in player1Items)
                    {
                        obj.gameObject.SetActive(true);
                        obj.gameObject.transform.position = mesh.GetRandomPointOnBox();
                        obj.gameObject.transform.position = new Vector3(obj.gameObject.transform.position.x, obj.gameObject.transform.position.y +0.2f, obj.gameObject.transform.position.z);
                        Grabbable grabbable = obj.GetComponent<Grabbable>();

                        if (grabbable != null)
                        {
                            StartCoroutine(FakeGrabAndRelease(waitTime, grabbable, taskTerrain));
                        }
                        else
                        {
                            Debug.LogError($"{obj.gameObject.name} , does not have Grabbable ");
                        }
                    }
                }
            });
    }





    private IEnumerator FakeGrabAndRelease(float waitTime, Grabbable grabbable, TaskAction task)
    {
        // Allow physics to update after moving the object
        yield return new WaitForFixedUpdate();

        yield return new WaitForSeconds(waitTime / 2);

        grabbable.OnGrabEvent?.Invoke(null, grabbable);

        yield return new WaitForSeconds(waitTime / 2);

        grabbable.OnReleaseEvent?.Invoke(null, grabbable);
        yield return new WaitForSeconds(1f);
        task.Increment();
        Debug.Log($" taskAction TargetCount : {task.TargetCount}  currentCount {task.GetCount}  name: {task.Name}");
    }

    private void RemoveNonPlacedTeam()
    {

    }

    private void RemoveNonPlacedTerrain()
    {
        List<VrInteraction> nonValid = new List<VrInteraction>();
        var all = Core.Scene.GetAllSpawnedVrInteraction();
        foreach (var item in all)
        {
            var val = item.gameObject.GetComponent<TerrainPlacementValidator>();
            if (val != null && val.IsValidPlacement() == false)
            {
                nonValid.Add(item);
            }

            var val1 = item.gameObject.GetComponent<BarracadePlacementValidator>();
            if (val1 != null && val1.IsValidPlacement() == false)
            {
                nonValid.Add(item);
            }

            var val2 = item.gameObject.GetComponent<MiniaturePlacementValidator>();
            if (val2 != null && val2.IsValidPlacement() == false)
            {
                nonValid.Add(item);
            }

        }

        Debug.Log($"RemoveNonPlacedTerrain count {nonValid.Count}");

        foreach (var item in nonValid)
        {
            Core.Scene.RemoveSpawnedVrInteractionAndGameObject(item);
        }
    }


}