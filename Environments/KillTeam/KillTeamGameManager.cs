
using System;
using System.Collections.Generic;
using UnityEngine;

public class KillTeamGameManager : MonoBehaviour
{
    public static KillTeamGameManager Instance { get; private set; }



    public enum LayerType
    {
        Player =16,
        AI,
        Terrain,
        Start,
        Board
    }
    public enum GameMode
    {
        SinglePlayer,
        Multiplayer
    }

    public enum GameState
    {
        WaitingForPlayers,
        BoardSetup,
        TerrainSetup,
        MissionSetup,
        TeamSelection,
        Deployment,
        ReadyToStart,
        Playing,
        Finished
    }

    [Serializable]
    public class PlayerData
    {
        public int playerId;
        public string playerName;
        public string teamId;

        public bool isHost;
        public bool isAI;

        public int deploymentZone;

        public bool teamSelected;
        public bool deploymentComplete;
        public bool ready;

        public List<GameObject> operatives = new List<GameObject>();
    }

    [Header("Game Settings")]
    [SerializeField]
    private GameMode gameMode = GameMode.SinglePlayer;

    [SerializeField]
    private GameState currentState = GameState.WaitingForPlayers;

    [SerializeField]
    private bool autoStartSinglePlayer = true;

    [Header("Board")]
    [SerializeField]
    private GameObject boardRoot;

    [SerializeField]
    private Transform terrainRoot;

    [SerializeField]
    private Transform operativeRoot;

    [SerializeField]
    private GameObject menuPoint;

    [SerializeField]
    private GameObject start;

    [Header("Players")]
    [SerializeField]
    private List<PlayerData> players = new List<PlayerData>();

    public AssetHolder AssetHolder { get; private set; }
    public VRPrefabMenu PrefabMenu { get; private set; }

    public event Action<GameState> OnStateChanged;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action<PlayerData> OnTeamSelected;
    public event Action<PlayerData> OnDeploymentComplete;
    public event Action OnGameStarted;

    public GameMode CurrentGameMode => gameMode;
    public GameState CurrentState => currentState;

    public IReadOnlyList<PlayerData> Players => players;

    public GameObject BoardRoot => boardRoot;
    public Transform TerrainRoot => terrainRoot;
    public Transform OperativeRoot => operativeRoot;

    public bool IsSinglePlayer => gameMode == GameMode.SinglePlayer;
    public bool IsMultiplayer => gameMode == GameMode.Multiplayer;

    public string Barracade => "KillTeam/Barracade";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        menuPoint = this.transform.FindChildRecursive("MenuPoint").gameObject;
        menuPoint.GetComponent<MeshRenderer>().enabled = false;


        start = this.transform.FindChildRecursive("Start").gameObject;
        start.layer = (int)LayerType.Start;
        start.GetComponent<MeshRenderer>().enabled = false;

        var assetHolder = new GameObject("assetHolder");
        assetHolder.transform.SetParent(transform, false);
        AssetHolder = assetHolder.transform.AddComponent<AssetHolder>();


        var vRPrefabMenu = new GameObject("vRPrefabMenu");
        vRPrefabMenu.transform.SetParent(transform, false);
        PrefabMenu = vRPrefabMenu.AddComponent<VRPrefabMenu>();



        boardRoot = this.transform.FindChildRecursive("TableBoard").gameObject;
        boardRoot.layer = (int)LayerType.Board;

        Core.Environment.OnEnvironmentLoadingComplete += () =>
        {
            PrefabMenu.SetEntries(AssetHolder.catalogueEntryTerrain);
            PrefabMenu.SetBackgroundColor(Color.grey);
            vRPrefabMenu.transform.position = menuPoint.transform.position;


            vRPrefabMenu.transform.SetParent(null);
            AssetHolder.transform.SetParent(null);
        };
    }

    private void Start()
    {
        if (autoStartSinglePlayer && IsSinglePlayer)
            SetupSinglePlayer();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --------------------------------------------------
    // GAME SETUP
    // --------------------------------------------------

    public void SetupSinglePlayer()
    {
        gameMode = GameMode.SinglePlayer;

        players.Clear();

        RegisterPlayer(1, "Player", true);
        RegisterPlayer(2, "AI Opponent", false);

        PlayerData human = GetPlayer(1);
        PlayerData ai = GetPlayer(2);

        human.isAI = false;
        human.deploymentZone = 0;

        ai.isAI = true;
        ai.deploymentZone = 1;

        SetState(GameState.BoardSetup);
    }

    public void SetupMultiplayer()
    {
        gameMode = GameMode.Multiplayer;

        players.Clear();

        SetState(GameState.WaitingForPlayers);
    }

    // --------------------------------------------------
    // PLAYERS
    // --------------------------------------------------

    public bool RegisterPlayer(
        int playerId,
        string playerName,
        bool isHost = false)
    {
        if (players.Count >= 2)
            return false;

        if (GetPlayer(playerId) != null)
            return false;

        PlayerData player = new PlayerData
        {
            playerId = playerId,
            playerName = playerName,
            isHost = isHost
        };

        players.Add(player);

        OnPlayerJoined?.Invoke(player);

        return true;
    }

    public void RemovePlayer(int playerId)
    {
        PlayerData player = GetPlayer(playerId);

        if (player == null)
            return;

        players.Remove(player);

        OnPlayerLeft?.Invoke(player);
    }

    public PlayerData GetPlayer(int playerId)
    {
        return players.Find(p => p.playerId == playerId);
    }

    public PlayerData GetHumanPlayer()
    {
        return players.Find(p => !p.isAI);
    }

    public PlayerData GetAIPlayer()
    {
        return players.Find(p => p.isAI);
    }

    public bool HasTwoPlayers()
    {
        return players.Count == 2;
    }

    public bool IsHost(int playerId)
    {
        PlayerData player = GetPlayer(playerId);

        return player != null && player.isHost;
    }

    // --------------------------------------------------
    // GAME STATE
    // --------------------------------------------------

    private void SetState(GameState state)
    {
        if (currentState == state)
            return;

        currentState = state;

        Debug.Log($"Kill Team State: {state}");

        OnStateChanged?.Invoke(state);
    }

    public bool BeginBoardSetup(int playerId)
    {
        if (!IsHost(playerId))
            return false;

        if (!HasTwoPlayers())
            return false;

        if (currentState != GameState.WaitingForPlayers)
            return false;

        SetState(GameState.BoardSetup);

        return true;
    }

    public bool CompleteBoardSetup(int playerId)
    {
        if (!IsHost(playerId))
            return false;

        if (currentState != GameState.BoardSetup)
            return false;

        SetState(GameState.TerrainSetup);

        return true;
    }

    public bool CompleteTerrainSetup(int playerId)
    {
        if (!IsHost(playerId))
            return false;

        if (currentState != GameState.TerrainSetup)
            return false;

        SetState(GameState.MissionSetup);

        return true;
    }

    public bool CompleteMissionSetup(int playerId)
    {
        if (!IsHost(playerId))
            return false;

        if (currentState != GameState.MissionSetup)
            return false;

        SetState(GameState.TeamSelection);

        return true;
    }

    // --------------------------------------------------
    // TEAM SELECTION
    // --------------------------------------------------

    public bool SelectTeam(int playerId, string teamId)
    {
        if (currentState != GameState.TeamSelection)
            return false;

        PlayerData player = GetPlayer(playerId);

        if (player == null || string.IsNullOrEmpty(teamId))
            return false;

        player.teamId = teamId;
        player.teamSelected = true;
        player.ready = false;

        OnTeamSelected?.Invoke(player);

        if (AllTeamsSelected())
            SetState(GameState.Deployment);

        return true;
    }

    public bool SelectSinglePlayerTeams(
        string humanTeamId,
        string aiTeamId)
    {
        if (!IsSinglePlayer)
            return false;

        if (currentState != GameState.TeamSelection)
            return false;

        if (string.IsNullOrEmpty(humanTeamId) ||
            string.IsNullOrEmpty(aiTeamId))
            return false;

        PlayerData human = GetHumanPlayer();
        PlayerData ai = GetAIPlayer();

        if (human == null || ai == null)
            return false;

        SelectTeam(human.playerId, humanTeamId);
        SelectTeam(ai.playerId, aiTeamId);

        return true;
    }

    public bool AllTeamsSelected()
    {
        return HasTwoPlayers() &&
               players.TrueForAll(p => p.teamSelected);
    }

    // --------------------------------------------------
    // OPERATIVES
    // --------------------------------------------------

    public bool RegisterOperative(
        int playerId,
        GameObject operative)
    {
        if (currentState != GameState.Deployment)
            return false;

        PlayerData player = GetPlayer(playerId);

        if (player == null || operative == null)
            return false;

        if (player.deploymentComplete)
            return false;

        if (player.operatives.Contains(operative))
            return false;

        player.operatives.Add(operative);

        return true;
    }

    public bool UnregisterOperative(
        int playerId,
        GameObject operative)
    {
        PlayerData player = GetPlayer(playerId);

        if (player == null || operative == null)
            return false;

        if (currentState != GameState.Deployment ||
            player.deploymentComplete)
            return false;

        return player.operatives.Remove(operative);
    }

    // --------------------------------------------------
    // DEPLOYMENT
    // --------------------------------------------------

    public bool CompleteDeployment(int playerId)
    {
        if (currentState != GameState.Deployment)
            return false;

        PlayerData player = GetPlayer(playerId);

        if (player == null ||
            player.operatives.Count == 0)
            return false;

        player.deploymentComplete = true;

        OnDeploymentComplete?.Invoke(player);

        if (players.TrueForAll(p => p.deploymentComplete))
            SetState(GameState.ReadyToStart);

        return true;
    }

    public bool SetPlayerReady(int playerId, bool ready)
    {
        if (currentState != GameState.ReadyToStart)
            return false;

        PlayerData player = GetPlayer(playerId);

        if (player == null)
            return false;

        player.ready = ready;

        if (players.TrueForAll(p => p.ready))
        {
            SetState(GameState.Playing);
            OnGameStarted?.Invoke();
        }

        return true;
    }

    // --------------------------------------------------
    // GAME FINISH / RESET
    // --------------------------------------------------

    public bool FinishGame(int playerId)
    {
        if (!IsHost(playerId))
            return false;

        if (currentState != GameState.Playing)
            return false;

        SetState(GameState.Finished);

        return true;
    }

    public void ResetGame()
    {
        foreach (PlayerData player in players)
        {
            player.teamId = null;
            player.teamSelected = false;
            player.deploymentComplete = false;
            player.ready = false;
            player.operatives.Clear();
        }

        if (IsSinglePlayer)
            SetupSinglePlayer();
        else
            SetupMultiplayer();
    }
}
