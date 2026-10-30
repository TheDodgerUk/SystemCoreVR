using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetHolder : MonoBehaviour
{
    // Start is called before the first frame update
    private List<GameObject> _assets = new List<GameObject>();
    private List<GameObject> _chars = new List<GameObject>();
    void Awake()
    {

        foreach (CatalogueEntry catItem in Core.Catalogue.GetCatalogue)
        {
            Core.Scene.SpawnObject(catItem, (item) =>
            {
                var all = item.Entry.GetAllMetaData();
                if (item.Entry.GetMetaData<ContentKillTeamMetaData>() != null)
                {
                    item.gameObject.transform.SetParent(transform);
                    item.gameObject.transform.localPosition = Vector3.zero;
                    item.gameObject.transform.localRotation = Quaternion.identity;

                    item.gameObject.AddComponent<InitiliseMiniture>();
                    _chars.Add(item.gameObject);

                    var data = item.Entry.GetMetaData<ContentKillTeamMetaData>();
                    var display = item.AddComponent<KillTeamMetaDataDisplay>();
                    display.Initialise(data);
                }
                else
                {
                    item.gameObject.transform.SetParent(transform);
                    item.gameObject.transform.localPosition = Vector3.zero;
                    item.gameObject.transform.localRotation = Quaternion.identity;
                    _assets.Add(item.gameObject);
                }
            });
        }
    }


}
