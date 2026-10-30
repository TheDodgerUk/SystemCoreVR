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
    void Awake()
    {

        foreach (CatalogueEntry catItem in Core.Catalogue.GetCatalogue)
        {
            var data = catItem.GetMetaData<ContentKillTeamMetaData>();

            int count = data != null
                ? Mathf.Max(0, data.m_OperativeStats.numberToGenerate)
                : 1;

            for (int i = 0; i < count; i++)
            {
                Core.Scene.SpawnObject(catItem, (item) =>
                {
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
                    }
                    else
                    {
                        Transform assetsFolder = transform.Find("Assets");

                        if (assetsFolder == null)
                        {
                            GameObject go = new GameObject("Assets");
                            assetsFolder = go.transform;
                            assetsFolder.SetParent(transform, false);
                        }

                        item.gameObject.transform.SetParent(assetsFolder, false);
                        item.gameObject.transform.localPosition = Vector3.zero;
                        item.gameObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);

                        _assets.Add(item.gameObject);
                    }
                });
            }
        }
    }
}

