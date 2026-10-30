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

                    var data = item.Entry.GetMetaData<ContentKillTeamMetaData>();
                    if (data != null)
                    {
                        if (data.m_OperativeStats.rotateX == true)
                        {
                            item.gameObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        }
                        else
                        {
                            item.gameObject.transform.localRotation = Quaternion.identity;
                        }
                    }
                    else
                    {
                        Debug.Log("No Kill Team Meta Data");
                    }
                    var display = item.AddComponent<KillTeamMetaDataDisplay>();
                    display.Initialise(data);






                    item.gameObject.AddComponent<InitiliseMiniture>();
                    _chars.Add(item.gameObject);


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
