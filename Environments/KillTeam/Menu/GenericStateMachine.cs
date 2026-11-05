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

public class GenericStateMachine : MonoBehaviour
{

    private Dictionary<KillTeamGameManager.Player, DynamicImagePlane> m_menus = new Dictionary<KillTeamGameManager.Player, DynamicImagePlane>();

    private Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType> TeamSelection = new Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType>();

    private List<KillTeamGameManager.Player> taskActionList = new List<KillTeamGameManager.Player>();

    private Dictionary<KillTeamGameManager.Player, int> playerTeamIndex = new()
    {
        { KillTeamGameManager.Player.Player1, 0 },
        { KillTeamGameManager.Player.Player2, 0 }
    };


    void Start()
    {

        Core.Environment.OnEnvironmentLoadingComplete += () =>
        {
            var men1 = new GameObject("DynamicImagePlane1");
            var men2 = new GameObject("DynamicImagePlane2");
            m_menus[KillTeamGameManager.Player.Player1] = men1.AddComponent<DynamicImagePlane>();
            m_menus[KillTeamGameManager.Player.Player2] = men2.AddComponent<DynamicImagePlane>();

            m_menus[KillTeamGameManager.Player.Player1].transform.position = KillTeamGameManager.Instance.helpers.DynamicImagePlanePoint[KillTeamGameManager.Player.Player1];
            m_menus[KillTeamGameManager.Player.Player2].transform.position = KillTeamGameManager.Instance.helpers.DynamicImagePlanePoint[KillTeamGameManager.Player.Player2];

            m_menus[KillTeamGameManager.Player.Player1].CreatePlane(m_menus[KillTeamGameManager.Player.Player1].transform.position, new Vector3(0.1f, 0.1f, 0.1f), null);
            m_menus[KillTeamGameManager.Player.Player2].CreatePlane(m_menus[KillTeamGameManager.Player.Player2].transform.position, new Vector3(0.1f, 0.1f, 0.1f), null);
            m_menus[KillTeamGameManager.Player.Player1].SetActive(false);
            m_menus[KillTeamGameManager.Player.Player2].SetActive(false);

            m_menus[KillTeamGameManager.Player.Player1].SetRotation(new Vector3(0, -90f, 0f));
            m_menus[KillTeamGameManager.Player.Player2].SetRotation(new Vector3(0, +90f, 0f));


            playerTeamIndex[KillTeamGameManager.Player.Player1] = 0;
            playerTeamIndex[KillTeamGameManager.Player.Player2] = 0;

            TeamSelection.Clear();
            TeamSelection.Add(KillTeamGameManager.Player.Player1, ContentKillTeamMetaData.TeamType.Kommandos);
            TeamSelection.Add(KillTeamGameManager.Player.Player2, ContentKillTeamMetaData.TeamType.AngelsOfDeath);


            KillTeamGameManager.Instance.OnStateChanged += KillTeamGameManager_OnStateChanged;
            KillTeamGameManager_OnStateChanged(KillTeamGameManager.Instance.CurrentState);
        };
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

        KillTeamGameManager.Instance.helpers.playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(false));
        m_menus[KillTeamGameManager.Player.Player1].SetActive(false);
        m_menus[KillTeamGameManager.Player.Player2].SetActive(false);
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
                KillTeamGameManager.Instance.helpers.playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(true));
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
                TeamSelectionMethod(NumberOfPlayers, KillTeamGameManager.GameState.TeamsAssetCreation);
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
                KillTeamGameManager.Instance.helpers.playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(true));
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
            case KillTeamGameManager.GameState.DEBUG_QuickTeamSelect:
                KillTeamGameManager.Instance.SetState(KillTeamGameManager.GameState.TeamsAssetCreation);
                break;

            case KillTeamGameManager.GameState.DEBUG_QuickSceneCreation:
                QuickSceneCreation(1f);
                break;

            case KillTeamGameManager.GameState.DEBUG_SlowSceneCreation:
                QuickSceneCreation(5f);
                break;
            case KillTeamGameManager.GameState.DEBUG_VerySlowSceneCreation:
                QuickSceneCreation(15f);
                break;

            case KillTeamGameManager.GameState.DEBUG_QuickPlayerCreation:
                QuickPlayerCreation(1f);
                break;

            case KillTeamGameManager.GameState.DEBUG_SlowPlayerCreation:
                QuickPlayerCreation(5f);
                break;

            case KillTeamGameManager.GameState.DEBUG_VerySloPlayerCreation:
                QuickPlayerCreation(15f);
                break;
            case KillTeamGameManager.GameState.UnitTest_StartStripTesting:
                StartStripTesting();
                break;



            default:
                break;
        }

    }


    private void TeamSelectionMethod(int numPlayers, KillTeamGameManager.GameState nextstate)
    {
        if (numPlayers == 1)
        {
            m_menus[KillTeamGameManager.Player.Player1].SetActive(true);
            m_menus[KillTeamGameManager.Player.Player2].SetActive(false);
        }
        else
        {
            m_menus[KillTeamGameManager.Player.Player1].SetActive(true);
            m_menus[KillTeamGameManager.Player.Player2].SetActive(true);
        }



        TaskAction BothTeamsChoisen = new TaskAction(numPlayers, () =>
        {
            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player1]];
            var image2 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player2]];
            TeamSelection.Clear();
            TeamSelection.Add(KillTeamGameManager.Player.Player1, ContentKillTeamMetaData.TeamType.Kommandos);
            TeamSelection.Add(KillTeamGameManager.Player.Player2, ContentKillTeamMetaData.TeamType.Kommandos);

            if (Enum.TryParse(image1.name, out ContentKillTeamMetaData.TeamType player1))
            {
                TeamSelection[KillTeamGameManager.Player.Player1] = player1;
            }
            if (Enum.TryParse(image2.name, out ContentKillTeamMetaData.TeamType player2))
            {
                TeamSelection[KillTeamGameManager.Player.Player2] = player2;
            }
            KillTeamGameManager.Instance.SetState(nextstate);
        });



        var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player1]];
        m_menus[KillTeamGameManager.Player.Player1].SetImage(image1);

        var image2 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player2]];
        m_menus[KillTeamGameManager.Player.Player2].SetImage(image2);


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
            m_menus[KillTeamGameManager.Player.Player2].SetImage(image1);
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
            if (playerTeamIndex[KillTeamGameManager.Player.Player2] < 0)
            {
                playerTeamIndex[KillTeamGameManager.Player.Player2] = KillTeamGameManager.Instance.AssetHolder.teamIcons.Count - 1;
            }

            var image1 = KillTeamGameManager.Instance.AssetHolder.teamIcons[playerTeamIndex[KillTeamGameManager.Player.Player2]];
            m_menus[KillTeamGameManager.Player.Player2].SetImage(image1);
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
        TaskAction taskTerrain = new TaskAction(itemsToMake, () =>
        {
            RemoveNonPlacedTerrain();
        });



        KillTeamGameManager.Instance.AssetHolder.SpawnTerrain(() =>
        {
            var mesh = KillTeamGameManager.Instance.helpers.BoardRoot.GetComponent<BoxCollider>();
            var all = Core.Scene.GetAllSpawnedVrInteraction();
            for (int i = 0; i < itemsToMake; i++)
            {
                GameObject obj = all[i].gameObject;
                obj.gameObject.transform.position = mesh.GetRandomPointOnBox(0.01f);
                obj.gameObject.transform.position = new Vector3(all[i].gameObject.transform.position.x, 0.5f, all[i].gameObject.transform.position.z);

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
        int itemsToMake = 0;

        itemsToMake += KillTeamGameManager.Instance.AssetHolder.playerGameObjects[KillTeamGameManager.Player.Player1].Count;
        itemsToMake += KillTeamGameManager.Instance.AssetHolder.playerGameObjects[KillTeamGameManager.Player.Player2].Count;

#if UNITY_EDITOR
        Vector3 player1Min;
        Vector3 player1Max;
        Vector3 player2Min;
        Vector3 player2Max;


        var box1 = KillTeamGameManager.Instance.helpers.playerSetupStrip[KillTeamGameManager.Player.Player1].GetComponent<BoxCollider>();
        var box2 = KillTeamGameManager.Instance.helpers.playerSetupStrip[KillTeamGameManager.Player.Player2].GetComponent<BoxCollider>();
        box1.GetWorldBoxMinMax(out player1Min, out player1Max);
        box2.GetWorldBoxMinMax(out player2Min, out player2Max);
#endif

        TaskAction taskTerrain = new TaskAction(itemsToMake, () =>
        {
            RemoveNonPlacedTerrain();
        });

        KillTeamGameManager.Instance.AssetHolder.SpawnPlayers(TeamSelection, () =>
        {
            foreach (KillTeamGameManager.Player p in (KillTeamGameManager.Player[])Enum.GetValues(typeof(KillTeamGameManager.Player)))
            {
                KillTeamGameManager.Instance.helpers.playerSetupStrip[p].SetActive(true);
                var player1Items = KillTeamGameManager.Instance.AssetHolder.playerGameObjects[p];
                var mesh = KillTeamGameManager.Instance.helpers.playerSetupStrip[p].GetComponent<BoxCollider>();
                FitBoxColliderToMesh(mesh);
                foreach (var obj in player1Items)
                {
                    obj.gameObject.transform.position = mesh.GetRandomPointOnBox();
                    obj.gameObject.transform.position = new Vector3(obj.gameObject.transform.position.x, 0.5f, obj.gameObject.transform.position.z);

                    Grabbable grabbable = obj.GetComponent<Grabbable>();

                    if (grabbable != null)
                    {
                        StartCoroutine(FakeGrabAndRelease(waitTime, grabbable, taskTerrain));
                    }
                }
            }
        });
    }


    private void StartStripTesting()
    {
#if UNITY_EDITOR
        ClearConsole();
#endif
        const int LOOP_NUMBER = 20;
        const float CUBE_SIZE = 0.005f;
        const float EDGE_PADDING = 0f;

        System.Text.StringBuilder log = new System.Text.StringBuilder();

        foreach (KillTeamGameManager.Player p in
            (KillTeamGameManager.Player[])Enum.GetValues(typeof(KillTeamGameManager.Player)))
        {
            if (!KillTeamGameManager.Instance.helpers.playerSetupStrip.TryGetValue(p, out GameObject strip) || strip == null)
            {
                Debug.LogWarning($"Missing player setup strip for {p}");
                continue;
            }

            BoxCollider box = strip.GetComponent<BoxCollider>();
            if (box == null)
            {
                Debug.LogWarning($"No BoxCollider on {strip.name}");
                continue;
            }

            // The old collider centre/size did not line up with the visible plane.
            // Fit it to the actual MeshFilter geometry before calculating points.
            FitBoxColliderToMesh(box);

            Vector3[] corners = box.GetWorldBoxCorners();
            box.GetWorldBoxMinMax(out Vector3 min, out Vector3 max);

            log.AppendLine($"========== {p} ==========");
            log.AppendLine($"Transform: {strip.transform.position.ToString("F6")}");
            log.AppendLine($"Collider world centre: {strip.transform.TransformPoint(box.center).ToString("F6")}");
            log.AppendLine($"MIN: {min.ToString("F6")}");
            log.AppendLine($"MAX: {max.ToString("F6")}");
            log.AppendLine("CORNERS:");

            for (int i = 0; i < corners.Length; i++)
            {
                CreateDebugCube($"Corner_{p}_{i}", corners[i], CUBE_SIZE, Color.red);
                log.AppendLine($"{i}: {corners[i].ToString("F6")}");
            }

            log.AppendLine("RANDOM POSITIONS:");
            for (int i = 0; i < LOOP_NUMBER; i++)
            {
                Vector3 pos = box.GetRandomPointOnBox(EDGE_PADDING);
                CreateDebugCube($"Random_{p}_{i + 1}", pos, CUBE_SIZE, Color.green);
                log.AppendLine($"{i}: {pos.ToString("F6")}");
            }
            log.AppendLine();
        }

        Debug.Log(log.ToString());
    }

    private static void CreateDebugCube(string name, Vector3 position, float size, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * size;
        cube.GetComponent<Renderer>().material.color = color;
    }

    // Fit the collider to the mesh IN LOCAL SPACE. Works even when the strip is inactive.
    // This intentionally updates the BoxCollider to match the visible mesh.
    public static void FitBoxColliderToMesh(BoxCollider box)
    {
        if (box == null) return;
        MeshFilter filter = box.GetComponent<MeshFilter>();
        if (filter == null || filter.sharedMesh == null)
        {
            Debug.LogWarning($"Cannot fit collider on {box.name}: missing MeshFilter or mesh.");
            return;
        }

        Bounds meshBounds = filter.sharedMesh.bounds;
        Vector3 size = meshBounds.size;
        // Give planar meshes a little thickness for reliable physics queries.
        if (size.y < 0.001f) size.y = 0.001f;
        box.center = meshBounds.center;
        box.size = size;
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
        }
        foreach (var item in nonValid)
        {
            Core.Scene.RemoveSpawnedVrInteractionAndGameObject(item);
        }



    }


#if UNITY_EDITOR
    public static void ClearConsole()
    {
        var assembly = typeof(UnityEditor.Editor).Assembly;

        var type = assembly.GetType("UnityEditor.LogEntries");

        var method = type?.GetMethod(
            "Clear",
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic
        );

        method?.Invoke(null, null);
    }
#endif
}