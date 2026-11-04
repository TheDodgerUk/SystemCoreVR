
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static KillTeamGameManager;
using static ObjectInteraction;
using static UnityEngine.UI.ContentSizeFitter;


public class KillTeamGameManager : MonoBehaviour
{
    public static KillTeamGameManager Instance { get; private set; }


    public enum Player
    {
        Player1 = 0,
        Player2
    }
    public enum LayerType
    {
        Player = 16,
        Terrain,
        Barracade,
        PlayerSetupStrip,
        TableBoard
    }

    public enum LayerInteractionType
    {
        Player1Hands = 22,
        Player2Hands,
        Player1Interaction,
        Player2Interaction
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
        TerrainAssetCreation,
        TerrainPlacement,
        BarracadeAssetCreation,
        BarracadePlacement,
        MissionSetup,
        TeamSelection,
        TeamsAssetCreation,
        SetCharacterToStartPlacement,
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

    [Header("Players")]
    [SerializeField]
    private List<PlayerData> players = new List<PlayerData>();

    public AssetHolder AssetHolder { get; private set; }

    public GenericStateMachine GenericStateMachine { get; private set; }





    public event Action<GameState> OnStateChanged;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action<PlayerData> OnTeamSelected;
    public event Action<PlayerData> OnDeploymentComplete;
    public event Action OnGameStarted;

    // Tracks the state last processed by SetState.
    // Inspector edits are detected during Play Mode in Update.
    private GameState lastAppliedState;

    public GameMode CurrentGameMode => gameMode;
    public GameState CurrentState => currentState;

    public IReadOnlyList<PlayerData> Players => players;



    public bool IsSinglePlayer => gameMode == GameMode.SinglePlayer;
    public bool IsMultiplayer => gameMode == GameMode.Multiplayer;

    private Scene m_currentScene;
    public Scene currentScene => m_currentScene;

        
    [SerializeField]
    private Dictionary<Player, GameObject> playerSetupStrip = new Dictionary<Player, GameObject> ();

    public class Helpers
    {
        public string Barracade => "KillTeam/Barracade";
        public string Ignore => "KillTeam/Ignore";

        public Dictionary<Player, Vector3> playerStartTable = new Dictionary<Player, Vector3>()
        {
            { Player.Player1, new Vector3(0.3479143f, 0.4992428f, 0.4959428f) },
            { Player.Player2, new Vector3(-0.3479143f, 0.4992428f, -0.4959428f) }
        };



        [Header("Board")]
        [SerializeField]
        public GameObject boardRoot;

        [SerializeField]
        public Transform terrainRoot;

        [SerializeField]
        public Transform operativeRoot;

        [SerializeField]
        public GameObject menuPoint;
        public Dictionary<Player, Vector3> DynamicImagePlanePoint = new Dictionary<Player, Vector3>()        {
            { Player.Player1, new Vector3(0.4243714f, 0.4678285f + 0.1f, 0f) },
            { Player.Player2, new Vector3(-0.4243714f, 0.4678285f + 0.1f, 0f) }
        };

        public GameObject BoardRoot => boardRoot;

        public VrInteraction Button;
    }

    public Helpers helpers;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        helpers = new Helpers();

        lastAppliedState = currentState;


        playerSetupStrip[Player.Player1] = this.transform.FindChildRecursive($"{LayerType.PlayerSetupStrip}1").gameObject;
        playerSetupStrip[Player.Player1].layer = (int)LayerType.PlayerSetupStrip;
        playerSetupStrip[Player.Player1].SetActive(false);

        playerSetupStrip[Player.Player2] = this.transform.FindChildRecursive($"{LayerType.PlayerSetupStrip}2").gameObject;
        playerSetupStrip[Player.Player2].layer = (int)LayerType.PlayerSetupStrip;
        playerSetupStrip[Player.Player2].SetActive(false);


        var assetHolder = new GameObject("assetHolder");
        assetHolder.transform.SetParent(transform, false);
        AssetHolder = assetHolder.transform.AddComponent<AssetHolder>();



        var vRPrefabMenu = new GameObject("GenericStateMachine");
        vRPrefabMenu.transform.SetParent(transform, false);
        GenericStateMachine = vRPrefabMenu.AddComponent<GenericStateMachine>();

        helpers.boardRoot = this.transform.FindChildRecursive(LayerType.TableBoard.ToString()).gameObject;
        helpers.boardRoot.layer = (int)LayerType.TableBoard;

        m_currentScene = this.gameObject.scene;

        Core.Environment.OnEnvironmentLoadingComplete += () =>
        {
            InitiliseAll();
        };

        this.OnStateChanged  += KillTeamGameManager_OnStateChanged; ;
    }

    private void InitiliseAll()
    {
        GenericStateMachine.transform.SetParent(null);
        AssetHolder.transform.SetParent(null);
    }

    private void KillTeamGameManager_OnStateChanged(KillTeamGameManager.GameState state)
    {
        playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(false));

        switch (state)
        {
            case KillTeamGameManager.GameState.WaitingForPlayers:
                break;
            case KillTeamGameManager.GameState.BoardSetup:
                break;
            case KillTeamGameManager.GameState.TerrainPlacement:
                break;
            case KillTeamGameManager.GameState.BarracadePlacement:
                playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(true));
                break;
            case KillTeamGameManager.GameState.MissionSetup:
                break;
            case KillTeamGameManager.GameState.TeamSelection:
                break;
            case KillTeamGameManager.GameState.SetCharacterToStartPlacement:
                playerSetupStrip.Values.ToList().ForEach(obj => obj.SetActive(true));
                break;
            case KillTeamGameManager.GameState.ReadyToStart:
                break;
            case KillTeamGameManager.GameState.Playing:
                break;
            case KillTeamGameManager.GameState.Finished:
                break;
            default:
                break;
        }
    }


    public void SetUpItem(Transform trans, CatalogueEntry entry, ModuleObject item, LayerType itemType, KillTeamGameManager.Player playerNumber)
    {
        if (item == null)
            return;

        item.gameObject.transform.SetParent(null, false);
        // can only be done if no parent, otherwise it will be moved to the scene of the parent
        SceneManager.MoveGameObjectToScene(
            item.gameObject,
            m_currentScene
        );

        ObjectInteraction interaction = item.gameObject.ForceComponent<ObjectInteraction>();
        if (playerNumber == Player.Player1)
        {
            interaction.InitiliseState(InteractionState.Player1Interaction);
        }
        else
        {
            interaction.InitiliseState(InteractionState.Player2Interaction);
        }
        switch (itemType)
        {
            case LayerType.Player:
                // need to assign  what player index it is in here somewere

                
                item.gameObject.AddComponent<InitiliseMiniture>();
                var display = item.AddComponent<KillTeamMetaDataDisplay>();
                var data = entry.GetMetaData<ContentKillTeamMetaData>();
                display.Initialise(data, playerNumber);
                break;
            case LayerType.Terrain:
                item.gameObject.ForceComponent<ObjectInteraction>();
                var t = item.gameObject.AddComponent<InitiliseTerrain>();
                break;
            case LayerType.Barracade:
                item.gameObject.ForceComponent<ObjectInteraction>();
                var i = item.gameObject.AddComponent<InitiliseBarracade>();
                break;
            default:
                break;
        }

        item.gameObject.transform.SetParent(trans, false);
        item.gameObject.transform.localPosition = Vector3.zero;

    }
    private void Update()
    {
#if UNITY_EDITOR
        // Changing the serialized enum in the Inspector bypasses SetState.
        // Reapply it through SetState so listeners receive the event.
        if (currentState != lastAppliedState)
        {
            GameState inspectorState = currentState;
            currentState = lastAppliedState;
            SetState(inspectorState);
        }
#endif
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

    public void SetState(GameState state)
    {
        if (currentState == state)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Beep();
#endif
            Debug.LogError($"Kill Team Game Manager: Attempted to set state to {state}, but it is already the current state.");
            return;
        }
        currentState = state;
        lastAppliedState = state;

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

        SetState(GameState.TerrainPlacement);

        return true;
    }

    public bool CompleteTerrainSetup(int playerId)
    {
        if (!IsHost(playerId))
            return false;

        if (currentState != GameState.TerrainPlacement)
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
            SetState(GameState.SetCharacterToStartPlacement);

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
        if (currentState != GameState.SetCharacterToStartPlacement)
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

        if (currentState != GameState.SetCharacterToStartPlacement ||
            player.deploymentComplete)
            return false;

        return player.operatives.Remove(operative);
    }

    // --------------------------------------------------
    // DEPLOYMENT
    // --------------------------------------------------

    public bool CompleteDeployment(int playerId)
    {
        if (currentState != GameState.SetCharacterToStartPlacement)
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
            BattleStateMachine.Instance.StartBattle();


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
