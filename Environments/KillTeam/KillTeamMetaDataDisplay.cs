using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTeamMetaDataDisplay : MonoBehaviour
{
    [SerializeField]
    private ContentKillTeamMetaData contentKillTeamMetaData;
    // Start is called before the first frame update
    public void Initialise(ContentKillTeamMetaData data)
    {
        contentKillTeamMetaData = data;
    }
}
