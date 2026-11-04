using Fixtures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using UnityEngine;
using static GameManagerDesertStrike;

public class AssetHolder : MonoBehaviour
{
    [SerializeField]
    public List<CatalogueEntry> catalogueEntryTerrain = new List<CatalogueEntry>();

    [SerializeField]
    public List<CatalogueEntry> catalogueEntryBaracade = new List<CatalogueEntry>();
    public SimpleDictionaryList<ContentKillTeamMetaData.TeamType, CatalogueEntry> catalogueEntryPlayer = new SimpleDictionaryList<ContentKillTeamMetaData.TeamType, CatalogueEntry>();

    [SerializeField]
    public List<Texture2D> teamIcons = new List<Texture2D>();

    [SerializeField]
    public Texture2D nextIcon;

    [SerializeField]
    public Material wallMaterail;
    const string ASSETS = "Assets";

    [SerializeField]
    public SimpleDictionaryList<KillTeamGameManager.Player, CatalogueEntry> PlayersTeamCatalogueEntry = new SimpleDictionaryList<KillTeamGameManager.Player, CatalogueEntry>();

    [SerializeField]
    SimpleDictionaryList<KillTeamGameManager.Player, GameObject> playerGameObjects = new SimpleDictionaryList<KillTeamGameManager.Player, GameObject>();

    public void Initilise()
    {
        int totalCount = 0;
        foreach (CatalogueEntry catItem in Core.Catalogue.GetCatalogue)
        {
            var data = catItem.GetMetaData<ContentKillTeamMetaData>();

            if (data != null)
            {
                totalCount += Mathf.Max(0, data.m_OperativeStats.numberToGenerate);

                for (int i = 0; i < Mathf.Max(0, data.m_OperativeStats.numberToGenerate); i++)
                {
                    catalogueEntryPlayer.AddToList(data.m_OperativeStats.teamType, catItem);
                }
                ;
            }
            else
            {
                if (catItem.CatalogueFilters.Contains(KillTeamGameManager.Instance.helpers.Ignore) == true || catItem.CatalogueFilters.Count == 0)
                {
                    // reversed logic as easy to read
                }
                else
                {
                    if (catItem.CatalogueFilters.Contains(KillTeamGameManager.Instance.helpers.Barracade))
                    {
                        // we need 3 of them
                        catalogueEntryBaracade.Add(catItem);
                        catalogueEntryBaracade.Add(catItem);
                        catalogueEntryBaracade.Add(catItem);
                    }
                    else
                    {
                        catalogueEntryTerrain.Add(catItem);
                    }
                }
            }
                totalCount++;          
        }

        Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItem(Core.Mono, "Next", (texture) =>
        {
            nextIcon = texture;
            if (texture == null)
            {
                Debug.LogError("Next icon not found");
            }
        });

        Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(Core.Mono, "Wall_Brick_Generic_Mat_Scale4", (textmature) =>
        {
            wallMaterail = textmature;
            if (wallMaterail == null)
            {
                Debug.LogError("Wall material not found");
            }
        });

        foreach (var team in ContentKillTeamMetaData.TeamType.GetValues(typeof(ContentKillTeamMetaData.TeamType)))
        {
            Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItem(Core.Mono, team.ToString(), (texture) =>
            {
                
                if (texture == null)
                {
                    Debug.Log($"Team icon not found for {team}");
                }
                else
                {
                    teamIcons.Add(texture);
                }
            });
        }

    }

    public void SpawnTerrain(Action callback)
    {
        int parts2 = Enum.GetValues(typeof(KillTeamGameManager.Player)).Length;
        SimpleDictionaryList<KillTeamGameManager.Player, GameObject> terrainGameObjects = new SimpleDictionaryList<KillTeamGameManager.Player, GameObject>();
        
        TaskAction task = new TaskAction(catalogueEntryTerrain.Count * parts2, () =>
        {
            PrefabRowSpawner spawnerAssets = this.AddComponent<PrefabRowSpawner>();
            spawnerAssets.Spawn(KillTeamGameManager.Instance.helpers.playerStartTable[KillTeamGameManager.Player.Player1], terrainGameObjects[KillTeamGameManager.Player.Player1]);
            spawnerAssets.Spawn(KillTeamGameManager.Instance.helpers.playerStartTable[KillTeamGameManager.Player.Player2], terrainGameObjects[KillTeamGameManager.Player.Player2]);
            DestroyImmediate(spawnerAssets);
            callback();
        });

        foreach (KillTeamGameManager.Player player in Enum.GetValues(typeof(KillTeamGameManager.Player)))
        {
            foreach (CatalogueEntry catItem in catalogueEntryTerrain)
            {
                Core.Scene.SpawnObject(catItem, (item) =>
                {
                    Transform assetsFolder = transform.Find(ASSETS);
                    if (assetsFolder == null)
                    {
                        GameObject go = new GameObject(ASSETS);
                        assetsFolder = go.transform;
                        assetsFolder.SetParent(transform, false);
                    }
                    KillTeamGameManager.Instance.SetUpItem(assetsFolder, catItem, item, KillTeamGameManager.LayerType.Terrain, KillTeamGameManager.Player.Player1 /*notused*/);
                    terrainGameObjects.AddToList(player, item.gameObject);
                    task.Increment();
                });
            }
        }
    }

    public void SpawnBarracades(Action callback)
    {
        int parts2 = Enum.GetValues(typeof(KillTeamGameManager.Player)).Length;
        SimpleDictionaryList<KillTeamGameManager.Player, GameObject> barracadeGameObjects = new SimpleDictionaryList<KillTeamGameManager.Player, GameObject>();
        
        TaskAction task = new TaskAction(catalogueEntryBaracade.Count * parts2, () =>
        {
            PrefabRowSpawner spawnerAssets = this.AddComponent < PrefabRowSpawner>();
            spawnerAssets.Spawn(KillTeamGameManager.Instance.helpers.playerStartTable[KillTeamGameManager.Player.Player1], barracadeGameObjects[KillTeamGameManager.Player.Player1]);
            spawnerAssets.Spawn(KillTeamGameManager.Instance.helpers.playerStartTable[KillTeamGameManager.Player.Player2], barracadeGameObjects[KillTeamGameManager.Player.Player2]);
            DestroyImmediate(spawnerAssets);
            callback();
        });

        foreach (KillTeamGameManager.Player player in Enum.GetValues(typeof(KillTeamGameManager.Player)))
        {
            foreach (CatalogueEntry catItem in catalogueEntryBaracade)
            {
                Core.Scene.SpawnObject(catItem, (item) =>
                {
                    Transform assetsFolder = transform.Find(ASSETS);
                    if (assetsFolder == null)
                    {
                        GameObject go = new GameObject(ASSETS);
                        assetsFolder = go.transform;
                        assetsFolder.SetParent(transform, false);
                    }
                    KillTeamGameManager.Instance.SetUpItem(assetsFolder, catItem, item, KillTeamGameManager.LayerType.Barracade, player /*notused*/);
                    barracadeGameObjects.AddToList(player, item.gameObject);
                    task.Increment();
                });
            }
        }
    }

    public void SpawnPlayers(Dictionary<KillTeamGameManager.Player, ContentKillTeamMetaData.TeamType> teamTypes, Action callback)
    {

        // collects it all 
        foreach (KillTeamGameManager.Player player in Enum.GetValues(typeof(KillTeamGameManager.Player)))
        {
            PlayersTeamCatalogueEntry.AddToList(player, catalogueEntryPlayer[teamTypes[player]]);
        }

        
        TaskAction task = new TaskAction(PlayersTeamCatalogueEntry.GetValuesAll().Count, () =>
        {
            PrefabRowSpawner spawnerAssets = this.AddComponent<PrefabRowSpawner>();
            spawnerAssets.Spawn(KillTeamGameManager.Instance.helpers.playerStartTable[KillTeamGameManager.Player.Player1], playerGameObjects[KillTeamGameManager.Player.Player1]);
            spawnerAssets.Spawn(KillTeamGameManager.Instance.helpers.playerStartTable[KillTeamGameManager.Player.Player2], playerGameObjects[KillTeamGameManager.Player.Player2]);
            DestroyImmediate(spawnerAssets);
            callback();
        });


        foreach (KillTeamGameManager.Player player in Enum.GetValues(typeof(KillTeamGameManager.Player)))
        {
            foreach (CatalogueEntry catItem in PlayersTeamCatalogueEntry[player])
            {
                Core.Scene.SpawnObject(catItem, (item) =>
                {
                    Transform assetsFolder = transform.Find(ASSETS);

                    if (assetsFolder == null)
                    {
                        GameObject go = new GameObject(ASSETS);
                        assetsFolder = go.transform;
                        assetsFolder.SetParent(transform, false);
                    }

                    KillTeamGameManager.Instance.SetUpItem(assetsFolder, catItem, item, KillTeamGameManager.LayerType.Player, player);

                    playerGameObjects.AddToList(player, item.gameObject);

                    task.Increment();
                });
            }
        }
    }
}

