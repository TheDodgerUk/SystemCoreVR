using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTeamMetaDataDisplay : MonoBehaviour
{
    [SerializeField]
    public ContentKillTeamMetaData contentKillTeamMetaData;

    [SerializeField]
    public KillTeamGameManager.Player player;
    // Start is called before the first frame update
    public void Initialise(ContentKillTeamMetaData data, KillTeamGameManager.Player playerNumber)
    {
        contentKillTeamMetaData = data;
        player = playerNumber;
    }
}
