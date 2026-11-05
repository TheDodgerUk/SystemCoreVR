
using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleStateMachine : MonoBehaviour
{
    public static BattleStateMachine Instance { get; private set; }

    public enum BattleState
    {
        Inactive,
        StartTurningPoint,
        DetermineInitiative,
        SelectOperative,
        OperativeActivation,
        EndActivation,
        EndTurningPoint,
        Finished
    }

    public enum OperativeState
    {
        Idle,
        Selected,
        Moving,
        Aiming,
        Shooting,
        Fighting,
        ResolvingAction,
        Incapacitated
    }

    public enum BattleAction
    {
        Move,
        Dash,
        Shoot,
        Fight,
        Charge,
        FallBack,
        Guard,
        PickUp,
        MissionAction,
        Pass
    }

    [Serializable]
    public class RunTimePlayerDetails
    {
        public GameObject gameObject;
        public KillTeamGameManager.Player playerIndex;

        public ContentKillTeamMetaData.OperativeStats stats;

        public List<ContentKillTeamMetaData.WeaponStats> weapons =
            new List<ContentKillTeamMetaData.WeaponStats>();

        public OperativeState state = OperativeState.Idle;
        public bool incapacitated;

        [NonSerialized] public bool activated;
        [NonSerialized] public int remainingAP;

        public int ActionPointLimit =>
            stats != null ? stats.actionPointLimit : 0;

        public float MoveInches =>
            stats != null ? stats.moveInches : 0f;

        public int CurrentWounds =>
            stats != null ? stats.currentWounds : 0;

        public int MaxWounds =>
            stats != null ? stats.maxWounds : 0;

        public string Name =>
            stats != null ? stats.operativeName : "";

        public ContentKillTeamMetaData.TeamType TeamType =>
            stats != null ? stats.teamType : default(ContentKillTeamMetaData.TeamType);
    }

    [Header("Battle")]
    [SerializeField]
    private BattleState currentState = BattleState.Inactive;

    [SerializeField, Min(1)]
    private int maxTurningPoints = 4;

    [SerializeField]
    private int currentTurningPoint;

    [SerializeField]
    private KillTeamGameManager.Player currentPlayer =
        KillTeamGameManager.Player.Player1;

    [SerializeField]
    private KillTeamGameManager.Player initiativePlayer =
        KillTeamGameManager.Player.Player1;

    [SerializeField]
    private bool autoDetermineInitiative = true;

    [SerializeField]
    private bool autoAdvanceTurningPoints = true;

    [Header("Players")]
    [SerializeField] private bool playerOneIsAI;
    [SerializeField] private bool playerTwoIsAI = true;
    [SerializeField] private int playerOneScore;
    [SerializeField] private int playerTwoScore;

    [Header("Operatives")]
    [SerializeField]
    private List<RunTimePlayerDetails> operatives = new List<RunTimePlayerDetails>();

    [SerializeField]
    private GameObject activeOperativeObject;

    [Header("Inspector Testing")]
    [SerializeField]
    private int testOperativeIndex;

    [SerializeField]
    private BattleAction testAction = BattleAction.Move;

    [SerializeField, Min(0)]
    private int testActionCost = 1;

    private RunTimePlayerDetails activeOperative;
    private bool initialized;

    public BattleState CurrentState => currentState;
    public int CurrentTurningPoint => currentTurningPoint;
    public KillTeamGameManager.Player CurrentPlayer => currentPlayer;
    public KillTeamGameManager.Player InitiativePlayer => initiativePlayer;
    public GameObject ActiveOperative => activeOperativeObject;
    public RunTimePlayerDetails ActiveOperativeData => activeOperative;
    public int RemainingAP => activeOperative != null ? activeOperative.remainingAP : 0;
    public IReadOnlyList<RunTimePlayerDetails> Operatives => operatives;
    public bool IsCurrentPlayerAI => IsAI(currentPlayer);

    public bool IsBattleRunning =>
        currentState != BattleState.Inactive &&
        currentState != BattleState.Finished;

    public event Action<BattleState> OnBattleStateChanged;
    public event Action<int> OnTurningPointStarted;
    public event Action<int> OnTurningPointEnded;
    public event Action<KillTeamGameManager.Player> OnInitiativeDetermined;
    public event Action<KillTeamGameManager.Player> OnPlayerChanged;
    public event Action<RunTimePlayerDetails> OnOperativeActivated;
    public event Action<RunTimePlayerDetails> OnOperativeFinished;
    public event Action<RunTimePlayerDetails, OperativeState> OnOperativeStateChanged;
    public event Action<RunTimePlayerDetails, BattleAction, int> OnActionCompleted;
    public event Action<RunTimePlayerDetails, int> OnWoundsChanged;
    public event Action<int, int> OnScoreChanged;
    public event Action<KillTeamGameManager.Player?> OnBattleFinished;
    public event Action<KillTeamGameManager.Player> OnAITurnRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private bool IsValidPlayer(KillTeamGameManager.Player player)
    {
        return player == KillTeamGameManager.Player.Player1 ||
               player == KillTeamGameManager.Player.Player2;
    }

    private KillTeamGameManager.Player GetOtherPlayer(
        KillTeamGameManager.Player player)
    {
        return player == KillTeamGameManager.Player.Player1
            ? KillTeamGameManager.Player.Player2
            : KillTeamGameManager.Player.Player1;
    }

    public bool IsAI(KillTeamGameManager.Player player)
    {
        if (player == KillTeamGameManager.Player.Player1)
            return playerOneIsAI;

        if (player == KillTeamGameManager.Player.Player2)
            return playerTwoIsAI;

        return false;
    }

    public void SetAI(KillTeamGameManager.Player player, bool isAI)
    {
        if (player == KillTeamGameManager.Player.Player1)
            playerOneIsAI = isAI;
        else if (player == KillTeamGameManager.Player.Player2)
            playerTwoIsAI = isAI;
    }

    // --------------------------------------------------
    // COPY METADATA
    // --------------------------------------------------

    private ContentKillTeamMetaData.OperativeStats CopyStats(
        ContentKillTeamMetaData.OperativeStats source)
    {
        if (source == null)
            return null;

        return new ContentKillTeamMetaData.OperativeStats
        {
            teamType = source.teamType,
            operativeName = source.operativeName,
            numberToGenerate = source.numberToGenerate,
            actionPointLimit = source.actionPointLimit,
            moveInches = source.moveInches,
            save = source.save,
            maxWounds = source.maxWounds,
            currentWounds = source.currentWounds,
            rotateX = source.rotateX,
            abilities = source.abilities
        };
    }

    private List<ContentKillTeamMetaData.WeaponStats> CopyWeapons(
        List<ContentKillTeamMetaData.WeaponStats> source)
    {
        var result = new List<ContentKillTeamMetaData.WeaponStats>();

        if (source == null)
            return result;

        foreach (var weapon in source)
        {
            if (weapon == null)
                continue;

            result.Add(new ContentKillTeamMetaData.WeaponStats
            {
                weaponType = weapon.weaponType,
                weaponName = weapon.weaponName,
                attacks = weapon.attacks,
                hit = weapon.hit,
                normalDamage = weapon.normalDamage,
                criticalDamage = weapon.criticalDamage,
                weaponRules = weapon.weaponRules != null
                    ? (string[])weapon.weaponRules.Clone()
                    : null
            });
        }

        return result;
    }

    // --------------------------------------------------
    // REGISTER OPERATIVES
    // --------------------------------------------------

    public bool RegisterOperative(
        GameObject obj,
        KillTeamGameManager.Player player,
        ContentKillTeamMetaData metadata)
    {
        if (obj == null ||
            !IsValidPlayer(player) ||
            metadata == null ||
            metadata.m_OperativeStats == null ||
            FindOperative(obj) != null)
            return false;

        var stats = CopyStats(metadata.m_OperativeStats);

        stats.actionPointLimit = Mathf.Max(1, stats.actionPointLimit);
        stats.maxWounds = Mathf.Max(1, stats.maxWounds);
        stats.currentWounds = Mathf.Clamp(
            stats.currentWounds, 0, stats.maxWounds);

        RunTimePlayerDetails op = new RunTimePlayerDetails
        {
            gameObject = obj,
            playerIndex = player,
            stats = stats,
            weapons = CopyWeapons(metadata.weapons),
            incapacitated = stats.currentWounds <= 0
        };

        op.state = op.incapacitated
            ? OperativeState.Incapacitated
            : OperativeState.Idle;

        operatives.Add(op);

        Debug.Log(
            $"Registered {op.Name} | {player} | " +
            $"APL {op.ActionPointLimit} | " +
            $"Move {op.MoveInches} | " +
            $"Wounds {op.CurrentWounds}/{op.MaxWounds}",
            obj);

        return true;
    }

    // Each GameObject has its own metadata.
    public int RegisterTeam(
        IEnumerable<GameObject> team,
        KillTeamGameManager.Player player,
        Func<GameObject, ContentKillTeamMetaData> getMetadata)
    {
        if (team == null || getMetadata == null ||
            !IsValidPlayer(player))
            return 0;

        int added = 0;

        foreach (GameObject obj in team)
        {
            if (obj == null)
                continue;

            ContentKillTeamMetaData metadata = getMetadata(obj);

            if (metadata == null)
            {
                Debug.LogWarning(
                    $"No Kill Team metadata found for {obj.name}", obj);
                continue;
            }

            if (RegisterOperative(obj, player, metadata))
                added++;
        }

        return added;
    }

    public List<GameObject> GetTeam(KillTeamGameManager.Player player)
    {
        var result = new List<GameObject>();

        foreach (RunTimePlayerDetails op in operatives)
        {
            if (op != null &&
                op.playerIndex == player &&
                op.gameObject != null)
                result.Add(op.gameObject);
        }

        return result;
    }

    public List<GameObject> GetEligibleTeam(
        KillTeamGameManager.Player player)
    {
        var result = new List<GameObject>();

        foreach (RunTimePlayerDetails op in operatives)
        {
            if (IsEligible(op, player))
                result.Add(op.gameObject);
        }

        return result;
    }

    private bool IsEligible(
        RunTimePlayerDetails op,
        KillTeamGameManager.Player player)
    {
        return op != null &&
               op.playerIndex == player &&
               !op.activated &&
               !op.incapacitated &&
               op.gameObject != null &&
               op.gameObject.activeInHierarchy;
    }

    public bool HasEligibleOperatives(
        KillTeamGameManager.Player player)
    {
        foreach (RunTimePlayerDetails op in operatives)
        {
            if (IsEligible(op, player))
                return true;
        }

        return false;
    }

    public RunTimePlayerDetails FindOperative(GameObject obj)
    {
        return operatives.Find(
            op => op != null && op.gameObject == obj);
    }

    public bool UnregisterOperative(GameObject obj)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        if (op == null || op == activeOperative)
            return false;

        bool removed = operatives.Remove(op);

        if (removed && currentState == BattleState.SelectOperative)
            AdvanceToEligiblePlayer();

        return removed;
    }

    // --------------------------------------------------
    // BATTLE START
    // --------------------------------------------------

    public void StartBattle()
    {
        if (IsBattleRunning)
            return;

        if (!HasEligibleOperatives(KillTeamGameManager.Player.Player1) ||
            !HasEligibleOperatives(KillTeamGameManager.Player.Player2))
        {
            Debug.LogWarning(
                "Both players must have at least one available miniature.",
                this);
            return;
        }

        initialized = true;
        currentTurningPoint = 0;

        currentPlayer = KillTeamGameManager.Player.Player1;
        initiativePlayer = KillTeamGameManager.Player.Player1;

        playerOneScore = 0;
        playerTwoScore = 0;

        activeOperative = null;
        activeOperativeObject = null;

        foreach (RunTimePlayerDetails op in operatives)
        {
            if (op == null)
                continue;

            op.activated = false;
            op.remainingAP = 0;

            op.state = op.incapacitated
                ? OperativeState.Incapacitated
                : OperativeState.Idle;
        }

        StartNextTurningPoint();
    }

    private void SetState(BattleState next)
    {
        currentState = next;

        Debug.Log(
            $"[Battle] {next} | TP {currentTurningPoint} | {currentPlayer}",
            this);

        OnBattleStateChanged?.Invoke(next);
    }

    // --------------------------------------------------
    // TURNING POINTS
    // --------------------------------------------------

    public void StartNextTurningPoint()
    {
        if (!initialized || currentState == BattleState.Finished)
            return;

        if (currentTurningPoint >= maxTurningPoints)
        {
            FinishBattle();
            return;
        }

        currentTurningPoint++;
        SetState(BattleState.StartTurningPoint);

        foreach (RunTimePlayerDetails op in operatives)
        {
            if (op == null)
                continue;

            op.activated = false;
            op.remainingAP = 0;

            SetOperativeStateInternal(
                op,
                op.incapacitated
                    ? OperativeState.Incapacitated
                    : OperativeState.Idle);
        }

        OnTurningPointStarted?.Invoke(currentTurningPoint);

        if (currentState != BattleState.StartTurningPoint)
            return;

        SetState(BattleState.DetermineInitiative);

        if (autoDetermineInitiative &&
            currentState == BattleState.DetermineInitiative)
        {
            // Placeholder initiative selection.
            var player = UnityEngine.Random.Range(0, 2) == 0
                ? KillTeamGameManager.Player.Player1
                : KillTeamGameManager.Player.Player2;

            SetInitiative(player);
        }
    }

    public bool SetInitiative(KillTeamGameManager.Player player)
    {
        if (currentState != BattleState.DetermineInitiative ||
            !IsValidPlayer(player))
            return false;

        initiativePlayer = player;
        currentPlayer = player;

        OnInitiativeDetermined?.Invoke(player);
        OnPlayerChanged?.Invoke(player);

        SetState(BattleState.SelectOperative);
        AdvanceToEligiblePlayer();

        return true;
    }

    // --------------------------------------------------
    // ACTIVATION
    // --------------------------------------------------

    public bool CanActivate(GameObject obj)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        return currentState == BattleState.SelectOperative &&
               IsEligible(op, currentPlayer);
    }

    public bool ActivateOperative(GameObject obj)
    {
        if (!CanActivate(obj))
            return false;

        activeOperative = FindOperative(obj);
        activeOperativeObject = obj;

        // Use this miniature's own APL.
        activeOperative.remainingAP =
            activeOperative.ActionPointLimit;

        SetOperativeStateInternal(
            activeOperative, OperativeState.Selected);

        SetState(BattleState.OperativeActivation);

        OnOperativeActivated?.Invoke(activeOperative);

        return true;
    }

    public bool SetActiveOperativeState(OperativeState next)
    {
        if (currentState != BattleState.OperativeActivation ||
            activeOperative == null ||
            activeOperative.incapacitated)
            return false;

        SetOperativeStateInternal(activeOperative, next);
        return true;
    }

    private void SetOperativeStateInternal(
        RunTimePlayerDetails op, OperativeState next)
    {
        if (op.state == next)
            return;

        op.state = next;
        OnOperativeStateChanged?.Invoke(op, next);
    }

    public bool CompleteAction(
        BattleAction action, int actionPointCost = 1)
    {
        if (currentState != BattleState.OperativeActivation ||
            activeOperative == null ||
            activeOperative.incapacitated ||
            actionPointCost < 0 ||
            actionPointCost > activeOperative.remainingAP)
            return false;

        RunTimePlayerDetails op = activeOperative;

        op.remainingAP -= actionPointCost;

        OnActionCompleted?.Invoke(op, action, actionPointCost);

        if (activeOperative != op ||
            currentState != BattleState.OperativeActivation)
            return true;

        if (op.remainingAP == 0 || action == BattleAction.Pass)
            EndActivation();
        else
            SetOperativeStateInternal(op, OperativeState.Selected);

        return true;
    }

    public bool EndActivation()
    {
        if (currentState != BattleState.OperativeActivation ||
            activeOperative == null)
            return false;

        SetState(BattleState.EndActivation);

        RunTimePlayerDetails finished = activeOperative;

        finished.activated = true;
        finished.remainingAP = 0;

        SetOperativeStateInternal(
            finished,
            finished.incapacitated
                ? OperativeState.Incapacitated
                : OperativeState.Idle);

        activeOperative = null;
        activeOperativeObject = null;

        OnOperativeFinished?.Invoke(finished);

        if (currentState != BattleState.EndActivation)
            return true;

        currentPlayer = GetOtherPlayer(finished.playerIndex);
        OnPlayerChanged?.Invoke(currentPlayer);

        SetState(BattleState.SelectOperative);
        AdvanceToEligiblePlayer();

        return true;
    }

    // --------------------------------------------------
    // WOUNDS AND DAMAGE
    // --------------------------------------------------

    public bool ApplyDamage(GameObject obj, int damage)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        if (op == null || op.stats == null ||
            op.incapacitated || damage <= 0)
            return false;

        op.stats.currentWounds = Mathf.Max(
            0, op.stats.currentWounds - damage);

        OnWoundsChanged?.Invoke(op, op.stats.currentWounds);

        if (op.stats.currentWounds == 0)
            MarkIncapacitated(obj);

        return true;
    }

    public bool HealOperative(GameObject obj, int amount)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        if (op == null || op.stats == null ||
            op.incapacitated || amount <= 0)
            return false;

        op.stats.currentWounds = Mathf.Min(
            op.stats.maxWounds,
            op.stats.currentWounds + amount);

        OnWoundsChanged?.Invoke(op, op.stats.currentWounds);
        return true;
    }

    public void MarkIncapacitated(GameObject obj)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        if (op == null || op.incapacitated)
            return;

        op.incapacitated = true;

        if (op.stats != null)
            op.stats.currentWounds = 0;

        SetOperativeStateInternal(
            op, OperativeState.Incapacitated);

        if (op == activeOperative)
            EndActivation();
        else if (currentState == BattleState.SelectOperative)
            AdvanceToEligiblePlayer();
    }

    // --------------------------------------------------
    // WEAPONS
    // --------------------------------------------------

    public List<ContentKillTeamMetaData.WeaponStats> GetWeapons(
        GameObject obj)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        return op != null
            ? op.weapons
            : new List<ContentKillTeamMetaData.WeaponStats>();
    }

    public ContentKillTeamMetaData.WeaponStats GetWeapon(
        GameObject obj, string weaponName)
    {
        RunTimePlayerDetails op = FindOperative(obj);

        if (op == null)
            return null;

        return op.weapons.Find(
            w => w != null && w.weaponName == weaponName);
    }

    // --------------------------------------------------
    // PLAYER SWITCHING
    // --------------------------------------------------

    private void AdvanceToEligiblePlayer()
    {
        if (currentState != BattleState.SelectOperative)
            return;

        bool player1Available =
            HasEligibleOperatives(KillTeamGameManager.Player.Player1);

        bool player2Available =
            HasEligibleOperatives(KillTeamGameManager.Player.Player2);

        if (!player1Available && !player2Available)
        {
            if (autoAdvanceTurningPoints)
                CompleteTurningPoint();

            return;
        }

        if (!HasEligibleOperatives(currentPlayer))
        {
            currentPlayer = GetOtherPlayer(currentPlayer);
            OnPlayerChanged?.Invoke(currentPlayer);
        }

        if (IsAI(currentPlayer))
            OnAITurnRequested?.Invoke(currentPlayer);
    }

    public bool CompleteTurningPoint()
    {
        if (currentState != BattleState.SelectOperative ||
            HasEligibleOperatives(KillTeamGameManager.Player.Player1) ||
            HasEligibleOperatives(KillTeamGameManager.Player.Player2))
            return false;

        SetState(BattleState.EndTurningPoint);
        OnTurningPointEnded?.Invoke(currentTurningPoint);

        if (currentState == BattleState.EndTurningPoint &&
            autoAdvanceTurningPoints)
            StartNextTurningPoint();

        return true;
    }

    // --------------------------------------------------
    // SCORING
    // --------------------------------------------------

    public void AddScore(
        KillTeamGameManager.Player player, int points)
    {
        if (!IsValidPlayer(player) || points < 0)
            return;

        if (player == KillTeamGameManager.Player.Player1)
            playerOneScore += points;
        else
            playerTwoScore += points;

        OnScoreChanged?.Invoke(playerOneScore, playerTwoScore);
    }

    public int GetScore(KillTeamGameManager.Player player)
    {
        if (player == KillTeamGameManager.Player.Player1)
            return playerOneScore;

        if (player == KillTeamGameManager.Player.Player2)
            return playerTwoScore;

        return 0;
    }

    // --------------------------------------------------
    // FINISH
    // --------------------------------------------------

    public void FinishBattle()
    {
        if (currentState == BattleState.Finished)
            return;

        initialized = false;
        activeOperative = null;
        activeOperativeObject = null;

        SetState(BattleState.Finished);

        KillTeamGameManager.Player? winner = null;

        if (playerOneScore > playerTwoScore)
            winner = KillTeamGameManager.Player.Player1;
        else if (playerTwoScore > playerOneScore)
            winner = KillTeamGameManager.Player.Player2;

        OnBattleFinished?.Invoke(winner);
    }

    // --------------------------------------------------
    // INSPECTOR TESTING
    // --------------------------------------------------

    [ContextMenu("Test/Start Battle")]
    private void TestStart()
    {
        if (Application.isPlaying)
            StartBattle();
    }

    [ContextMenu("Test/Initiative Player 1")]
    private void TestInitiative1()
    {
        if (Application.isPlaying)
            SetInitiative(KillTeamGameManager.Player.Player1);
    }

    [ContextMenu("Test/Initiative Player 2")]
    private void TestInitiative2()
    {
        if (Application.isPlaying)
            SetInitiative(KillTeamGameManager.Player.Player2);
    }

    [ContextMenu("Test/Activate Operative")]
    private void TestActivate()
    {
        if (!Application.isPlaying ||
            testOperativeIndex < 0 ||
            testOperativeIndex >= operatives.Count)
            return;

        RunTimePlayerDetails op = operatives[testOperativeIndex];

        if (op != null)
            ActivateOperative(op.gameObject);
    }

    [ContextMenu("Test/Complete Action")]
    private void TestCompleteAction()
    {
        if (Application.isPlaying)
            CompleteAction(testAction, testActionCost);
    }

    [ContextMenu("Test/End Activation")]
    private void TestEndActivation()
    {
        if (Application.isPlaying)
            EndActivation();
    }

    [ContextMenu("Test/Finish Battle")]
    private void TestFinish()
    {
        if (Application.isPlaying)
            FinishBattle();
    }
}
