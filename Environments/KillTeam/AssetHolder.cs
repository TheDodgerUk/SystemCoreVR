using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class AssetHolder : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private List<GameObject> _assets = new List<GameObject>();
    [SerializeField]
    private SimpleDictionaryList<ContentKillTeamMetaData.TeamType, GameObject> _chars = new SimpleDictionaryList<ContentKillTeamMetaData.TeamType, GameObject>();
    const string ASSETS= "Assets";
    void Awake()
    {

        Dictionary<ContentKillTeamMetaData.TeamType, Vector3> teamDict = new Dictionary<ContentKillTeamMetaData.TeamType, Vector3>();

        teamDict.Add(ContentKillTeamMetaData.TeamType.AngelsOfDeath, new Vector3(0.5f, 0.32f,0));
        teamDict.Add(ContentKillTeamMetaData.TeamType.Deathwatch, new Vector3(0.5f, 0.32f, 0.3f));
        teamDict.Add(ContentKillTeamMetaData.TeamType.Kommandos, new Vector3(0.5f, 0.32f, -0.3f));
        teamDict.Add(ContentKillTeamMetaData.TeamType.PlagueMarines, new Vector3(0.5f, 0.32f, -0.5f));
        teamDict.Add(ContentKillTeamMetaData.TeamType.WolfScouts, new Vector3(0.5f, 0.32f, 0));
        Vector3 assetsPos = new Vector3(-0.5f, 0.32f, 0.5f);


        int totalCount = 0;
        foreach (CatalogueEntry catItem in Core.Catalogue.GetCatalogue)
        {
            var data = catItem.GetMetaData<ContentKillTeamMetaData>();

            if(data != null)
            {
                totalCount += Mathf.Max(0, data.m_OperativeStats.numberToGenerate);
                Debug.Log($"task {data.m_OperativeStats.operativeName} {data.m_OperativeStats.numberToGenerate}");
            }

        }


        List<string> names = new List<string>();
        Debug.Log($"task FULL  {totalCount}");
        TaskAction task = new TaskAction(totalCount, () =>
        {
            Debug.Log($"TaskAction done  {totalCount}");
            var keys = _chars.GetKeys();
            foreach (var key in keys)
            {
                var list = _chars.GetList(key);
                PrefabRowSpawner spawner = gameObject.transform.Find(key.ToString()).AddComponent<PrefabRowSpawner>();
                spawner.Spawn(teamDict[key], _chars[key]);
            }

            if (gameObject.transform.Find(ASSETS) == true)
            {
                PrefabRowSpawner spawnerAssets = gameObject.transform.Find(ASSETS).AddComponent<PrefabRowSpawner>();
                spawnerAssets.Spawn(assetsPos, _assets);
            }
        });


        int localCount = 0;
        foreach (CatalogueEntry catItem in Core.Catalogue.GetCatalogue)
        {
            names.Add(catItem.ShortName);
            var data = catItem.GetMetaData<ContentKillTeamMetaData>();

            int count = 1;
            if(data != null)
            {
                count = Mathf.Max(0, data.m_OperativeStats.numberToGenerate);

            }
            localCount += count;
            Debug.Log($"task localCount {localCount} of {totalCount}");
            for (int i = 0; i < count; i++)
            {
                Core.Scene.SpawnObject(catItem, (item) =>
                {
                    data = catItem.GetMetaData<ContentKillTeamMetaData>();
                    if (data != null)
                    {
                        // Find or create team parent
                        string teamName = data.m_OperativeStats.teamType.ToString();

                        Transform child = transform.Find(teamName);

                        if (child == null)
                        {
                            GameObject go = new GameObject(teamName);
                            child = go.transform;
                            child.SetParent(transform, false);
                        }

                        // Parent miniature
                        item.gameObject.transform.SetParent(child, false);
                        item.gameObject.transform.localPosition = Vector3.zero;

                        // Rotation
                        item.gameObject.transform.localRotation =
                            data.m_OperativeStats.rotateX
                                ? Quaternion.Euler(-90, 0, 0)
                                : Quaternion.identity;

                        // Metadata
                        var display = item.AddComponent<KillTeamMetaDataDisplay>();
                        display.Initialise(data);

                        // Initialise miniature
                        item.gameObject.AddComponent<InitiliseMiniture>();

                        _chars.AddToList(data.m_OperativeStats.teamType, item.gameObject);
                        task.Increment();
                        Debug.Log($"task {task.GetCount}");
                    }
                    else
                    {
                        Transform assetsFolder = transform.Find(ASSETS);

                        if (assetsFolder == null)
                        {
                            GameObject go = new GameObject(ASSETS);
                            assetsFolder = go.transform;
                            assetsFolder.SetParent(transform, false);
                        }

                        item.gameObject.transform.SetParent(assetsFolder, false);
                        item.gameObject.transform.localPosition = Vector3.zero;
                        item.gameObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);

                        _assets.Add(item.gameObject);
                        task.Increment();
                        Debug.Log($"task {task.GetCount}");
                    }

                });
            }
        }

    }
}

