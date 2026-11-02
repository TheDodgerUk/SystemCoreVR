using Fixtures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using UnityEngine;

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


    void Awake()
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
                if (catItem.CatalogueFilters.Contains(KillTeamGameManager.Instance.Barracade))
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
                totalCount++;
            }

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
                teamIcons.Add(texture);
                if (texture == null)
                {
                    Debug.LogError($"Team icon not found for {team}");
                }
            });
        }

    }

    public void SpawnTerrain()
    {

        int muliplier = 2;
        List<GameObject> terrainGameObjects = new List<GameObject>();
        TaskAction task = new TaskAction(catalogueEntryTerrain.Count * muliplier, () =>
        {
            PrefabRowSpawner spawnerAssets = this.AddComponent<PrefabRowSpawner>();
            spawnerAssets.Spawn(KillTeamGameManager.Instance.StartTable, terrainGameObjects);
            DestroyImmediate(spawnerAssets);
        });

        for (int i = 0; i < muliplier; i++)
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
                KillTeamGameManager.Instance.SetUpItem(assetsFolder, catItem, item, KillTeamGameManager.LayerType.Terrain);
                terrainGameObjects.Add(item.gameObject);
                task.Increment();
            });
        }
    }
    }

    public void SpawnBarracades()
    {
        List<GameObject> barracadeGameObjects = new List<GameObject>();
        TaskAction task = new TaskAction(catalogueEntryBaracade.Count, () =>
        {
            PrefabRowSpawner spawnerAssets = this.AddComponent < PrefabRowSpawner>();
            spawnerAssets.Spawn(KillTeamGameManager.Instance.StartTable, barracadeGameObjects);
            DestroyImmediate(spawnerAssets);
        });

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
                KillTeamGameManager.Instance.SetUpItem(assetsFolder, catItem, item, KillTeamGameManager.LayerType.Barracade);
                barracadeGameObjects.Add(item.gameObject);
                task.Increment();
            });
        }
    }

    public void SpawnPlayers(ContentKillTeamMetaData.TeamType teamType)
    {
        List<GameObject> playerGameObjects = new List<GameObject>();

        List<CatalogueEntry> all = catalogueEntryPlayer[teamType];

        TaskAction task = new TaskAction(all.Count, () =>
        {
            PrefabRowSpawner spawnerAssets = this.AddComponent<PrefabRowSpawner>();
            spawnerAssets.Spawn(
                KillTeamGameManager.Instance.StartTable,
                playerGameObjects
            );
            DestroyImmediate(spawnerAssets);
        });

        foreach (CatalogueEntry catItem in all)
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

                KillTeamGameManager.Instance.SetUpItem(
                    assetsFolder,
                    catItem,
                    item,
                    KillTeamGameManager.LayerType.Player
                );

                playerGameObjects.Add(item.gameObject);

                task.Increment();
            });
        }
    }
}

