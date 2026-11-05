using Crosstales.FB.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KillTeamGameManager;

public class Table : MonoBehaviour
{

    public static Table Instance { get; private set; }



    public string Barracade => "KillTeam/Barracade";
    public string Ignore => "KillTeam/Ignore";

    public Dictionary<KillTeamGameManager.Player, GameObject> playerStartTable = new Dictionary<KillTeamGameManager.Player, GameObject>();



    [Header("Board")]
    [SerializeField]
    private GameObject boardRoot;

    [SerializeField]
    public GameObject BoardRoot => boardRoot;

    [SerializeField]
    public Dictionary<KillTeamGameManager.Player, GameObject> playerSetupStrip = new Dictionary<KillTeamGameManager.Player, GameObject>();











    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        AssignData();
    }


    private void AssignData()
    {
        try
        {
            playerSetupStrip[Player.Player1] = this.transform.FindChildRecursive($"{LayerType.PlayerSetupStrip}1").gameObject;
            playerSetupStrip[Player.Player1].layer = (int)LayerType.PlayerSetupStrip;
            playerSetupStrip[Player.Player1].SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to setup Player1 strip: {ex.Message}\n{ex.StackTrace}");
        }

        try
        {
            playerSetupStrip[Player.Player2] = this.transform.FindChildRecursive($"{LayerType.PlayerSetupStrip}2").gameObject;
            playerSetupStrip[Player.Player2].layer = (int)LayerType.PlayerSetupStrip;
            playerSetupStrip[Player.Player2].SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to setup Player2 strip: {ex.Message}\n{ex.StackTrace}");
        }

        try
        {
            boardRoot = this.transform.FindChildRecursive(LayerType.TableBoard.ToString()).gameObject;
            boardRoot.layer = (int)LayerType.TableBoard;
        }
        catch (Exception ex)
        {
            Debug.LogError($"helpers.boardRoot : {ex.Message}\n{ex.StackTrace}");
        }

        try
        {
            playerStartTable[Player.Player1] = this.transform.FindChildRecursive("StartTableHolder1").gameObject.transform.FindChildRecursive("Image").gameObject;
            playerStartTable[Player.Player2] = this.transform.FindChildRecursive("StartTableHolder2").gameObject.transform.FindChildRecursive("Image").gameObject;
        }
        catch (Exception ex)
        {
            Debug.LogError($"helpers.playerStartTable: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
