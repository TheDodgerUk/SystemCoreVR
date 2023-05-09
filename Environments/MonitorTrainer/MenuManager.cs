using Oculus.Platform.Models;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static MonitorTrainer.AchievementsManager;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class MenuManager : MonoBehaviour
    {
        public const string PRIVATE_ROOM = "Private";
        public static MenuManager Instance { get; private set; }

        private Vector3? m_LocalLapTopGUIPosition;
        private Quaternion? m_LocalLapTopGUIRotation;
        private Transform m_LaptopGUIHolder;

        private Vector3? m_LocalMainGUIPosition;
        private Quaternion? m_LocalMainGUIRotation;
        private Transform m_GuiHolder;
        private Transform m_GuiEndHolder;

        public enum ScreenMenu
        {
            MainRootScreen,
            TutorialPage,
            SoloPlayer,

            MultiplayerChoice,
            LobbyCode,
            MultiplayerLobbyHost,


            MultiplayerLobbyGuest,
            LeaveLobby,
            Customise,

            AddDecals,

            CustomisePod,
            AlterMaterials,
            TierProgression,
            Challenges,
            SettingsMainMenu,
            Credits,
            EndGameMenuMultiPlayer,
            EndGameMenuSinglePlayer,
            Loading,

            NetworkErrorScreen,

        }

        public class MenuAreaClass
        {
            public Transform m_OnArea;
            public Transform m_OffArea;
            public Transform m_Start;
            public Transform m_LapTop;
            public Transform m_Console;

            public void TurnOn(bool on)
            {
                m_OnArea?.SetActive(on);
                m_OffArea?.SetActive(!on);
                if (on == true)
                {
                    ApplyMenuGui();
                }
            }

            private void ApplyMenuGui()
            {
                MenuManager.Instance.m_LaptopGUIHolder.SetParent(m_LapTop.transform);
                MenuManager.Instance.m_LaptopGUIHolder.ClearPosRotLocals();
                MenuManager.Instance.m_LaptopGUIHolder.localPosition = (Vector3)MenuManager.Instance.m_LocalLapTopGUIPosition;
                MenuManager.Instance.m_LaptopGUIHolder.localRotation = (Quaternion)MenuManager.Instance.m_LocalLapTopGUIRotation;

                MenuManager.Instance.m_GuiHolder.SetParent(m_Console.transform);
                MenuManager.Instance.m_GuiHolder.ClearPosRotLocals();
                MenuManager.Instance.m_GuiHolder.localPosition = (Vector3)MenuManager.Instance.m_LocalMainGUIPosition;
                MenuManager.Instance.m_GuiHolder.localRotation = (Quaternion)MenuManager.Instance.m_LocalMainGUIRotation;
            }
        }

        public Dictionary<PlayersEnum, MenuAreaClass> m_MenuArea = new Dictionary<PlayersEnum, MenuAreaClass>();


        private ScreenMenu? m_Destination = null;

        public ScreenMenu m_CurrentScreenMenu = ScreenMenu.Loading;

        public class ToggleSwap
        {
            public Toggle m_Toggle;
            public Image m_On;
            public Image m_Off;
        }

        public class HorizontalData
        {
            public Transform m_Root;
            public HorizontalLayoutGroup m_HorizontalLayoutGroup;
            public GameObject m_Prefab;
            public PoolManagerLocalComponent<Toggle> m_TogglePool;
            public void Init()
            {
                m_TogglePool = new PoolManagerLocalComponent<Toggle>(m_Prefab, m_HorizontalLayoutGroup.transform);
            }
        }

        public class VerticalData
        {
            public Transform m_Root;
            public VerticalLayoutGroup m_VerticalLayoutGroup;
            public GameObject m_Prefab;
            public PoolManagerLocalComponent<Toggle> m_TogglePool;
            public void Init()
            {
                m_TogglePool = new PoolManagerLocalComponent<Toggle>(m_Prefab, m_VerticalLayoutGroup.transform);
            }
        }


        public class BaseNetwork
        {
            public string m_LobbyCode;
            public int m_OwnerActorNr;
        }

        public class KickPlayer : BaseNetwork { }
        public class PlayerReady : BaseNetwork
        {
            public bool m_IsReady;
            public bool m_FullyLoaded;
        }

        public class PlayerOrder : BaseNetwork
        {
            public PlayersEnum m_PlayersEnum;
            public int m_ActorNumber;
        }
        public class ButtonAmount
        {
            public Button m_Button;
            public TextMeshProUGUI m_ButtonAmountVariable;
            public TextMeshProUGUI m_ButtonAmountTotal;
        }

        public class PlayerData
        {
            public Image m_Background;
            public TextMeshProUGUI m_PlayerName;
            public Button m_KickPlayerButton;
            public Image m_PlayerReadyIcon;
            public int m_ActorNumber;
            public bool m_FullyLoaded;
            public Player m_PhotonPlayer;
        }
        public class BaseChoice
        {
            public Transform m_Root;
            public Button m_BaseBack;
            public Button m_BasePlayStyle;
            public Button m_BaseCustomise;
            public Button m_BaseStats;
            public Button m_BaseSettings;
        }

        public class MainRootScreenData : BaseChoice
        {
            public Button m_Tutorial;
            public Button m_Solo;
            public Button m_Multiplayer;
            public Transform m_ChallengeBoxTop;
            public Transform m_ChallengeBoxBottom;
        }

        public class TutorialPageData : BaseChoice
        {
            public Button m_LaunchTutorial;
        }

        public class SoloPlayerData : BaseChoice
        {
            public Button m_LaunchSolo;
            public Toggle m_Easy;
            public Toggle m_Medium;
            public Toggle m_Hard;
            public VerticalData m_VerticalData = new VerticalData();
        }

        public class MultiplayerChoiceData : BaseChoice
        {
            public Button m_JoinRandomLobby;
            public Button m_SetupLobby;
            public Button m_EnterLobbyCode;
            public ToggleSwap m_MakePrivate = new ToggleSwap();
        }

        public class LobbyCodeData : BaseChoice
        {
            public List<Button> m_NumberButtons = new List<Button>();
            public Button m_Delete;
            public Button m_CloseButton;
            public Button m_TickButton;
            public TextMeshProUGUI m_LobbyCodeText;
        }

        public class HostGuest : BaseChoice
        {
            public List<PlayerData> m_PlayerData = new List<PlayerData>();
        }
        public class MultiplayerLobbyHostData : HostGuest
        {
            public Button m_LaunchMultiplayer;
            public TextMeshProUGUI m_LobbyCodeString;
            public Button m_CloseButton;
            public Toggle m_Easy;
            public Toggle m_Medium;
            public Toggle m_Hard;
            public Button m_SwapTeamsButton;
            public VerticalData m_VerticalData = new VerticalData();
        }


        public class MultiplayerLobbyGuestData : HostGuest
        {
            public Toggle m_MarkReady;
            public TextMeshProUGUI m_LobbyCode;
            public Button m_CloseButton;
            public Button m_SwapTeamsButton;

            public TextMeshProUGUI m_SongTitle;
            public TextMeshProUGUI m_ArtistName;
            public TextMeshProUGUI m_SongLength;

            public Image m_LobbyDifficultyIcon_Easy;
            public Image m_LobbyDifficultyIcon_Medium;
            public Image m_LobbyDifficultyIcon_Hard;
        }

        public class LeaveLobbyData : BaseChoice
        {
            public Button m_Stay;
            public Button m_LeaveLobby;
        }

        public class CustomiseData : BaseChoice
        {
            public Button m_AddDecals;
            public Button m_CustomisePod;
            public Button m_AlterMaterials;
        }

        public class AddDecalsData : BaseChoice
        {
            public ButtonAmount m_MonitorButton = new ButtonAmount();
            public ButtonAmount m_FlightCasesButton = new ButtonAmount();
            public ButtonAmount m_LaptopButton = new ButtonAmount();
            public ButtonAmount m_LampButton = new ButtonAmount();

            public HorizontalData m_SelectableItems_Monitor = new HorizontalData();
            public HorizontalData m_SelectableItems_FlightCases = new HorizontalData();
            public HorizontalData m_SelectableItems_Laptop = new HorizontalData();
            public HorizontalData m_SelectableItems_Lamp = new HorizontalData();
        }

        public class CustomisePodData : BaseChoice
        {
            public ButtonAmount m_InstrumentsButton = new ButtonAmount();
            public ButtonAmount m_ConsumablesButton = new ButtonAmount();
            public ButtonAmount m_WeaponsButton = new ButtonAmount();
            public ButtonAmount m_MiscButton = new ButtonAmount();

            public HorizontalData m_SelectableItems_Instruments = new HorizontalData();
            public HorizontalData m_SelectableItems_Consumables = new HorizontalData();
            public HorizontalData m_SelectableItems_Weapons = new HorizontalData();
            public HorizontalData m_SelectableItems_Misc = new HorizontalData();
        }

        public class AlterMaterialsData : BaseChoice
        {
            public Button m_AllThemesButton;
            public Button m_UnlockedThemesButton;
            public HorizontalData m_SelectableItems_AllThemes = new HorizontalData();
        }

        public class TierProgressionData : BaseChoice
        {
            public Button m_TierProgressionButton;
            public Button m_ChallengesButton;
            public HorizontalData m_SelectableItems_Monitor = new HorizontalData();
        }

        public class ChallengesData : BaseChoice
        {
            public Button m_TierProgressionButton;
            public Button m_ChallengesButton;
            public HorizontalData m_SelectableItems_Challenges = new HorizontalData();
        }

        public class SettingsMainMenuData : BaseChoice
        {
            public Slider m_MasterVolumeSlider;
            public Slider m_MusicVolumeSlider;
            public Slider m_ChatVolumeSlider;

            public ToggleSwap m_VignetteToggle = new ToggleSwap();
            public ToggleSwap m_SnapsToggle = new ToggleSwap();
            public Button m_CreditsButton;
        }
        public class CreditsData : BaseChoice
        {
            public TextMeshProUGUI m_CreditsTitleText;
        }

        public class EndGameMenuMultiPlayerData : BaseChoice
        {
            public TextMeshProUGUI m_SongTitleString;
            public TextMeshProUGUI m_ArtistString;
            public TextMeshProUGUI m_SongLengthString;

            public TextMeshProUGUI m_TeamOnePoints;
            public TextMeshProUGUI m_TeamTwoPoints;

            public TextMeshProUGUI m_PlayerName1;
            public TextMeshProUGUI m_PlayerOneTasks;
            public TextMeshProUGUI m_PlayerOnePoints;

            public TextMeshProUGUI m_PlayerName2;
            public TextMeshProUGUI m_PlayerTwoTasks;
            public TextMeshProUGUI m_PlayerTwoPoints;

            public TextMeshProUGUI m_PlayerName3;
            public TextMeshProUGUI m_PlayerThreeTasks;
            public TextMeshProUGUI m_PlayerThreePoints;

            public TextMeshProUGUI m_PlayerName4;
            public TextMeshProUGUI m_PlayerFourTasks;
            public TextMeshProUGUI m_PlayerFourPoints;


            public Button m_LeaveLobby;
            public Button m_Rematch;
            public Button m_Continue;
        }

        public class EndGameMenuSinglePlayerData : BaseChoice
        {
            public TextMeshProUGUI m_SongTitleString;
            public TextMeshProUGUI m_ArtistString;
            public TextMeshProUGUI m_SongLengthString;

            public TextMeshProUGUI m_PlayerName1;
            public TextMeshProUGUI m_PlayerOneTasks;
            public TextMeshProUGUI m_PlayerOnePoints;


            public Button m_LeaveLobby;
            public Button m_Rematch;
            public Button m_Continue;
        }

        public class LoadingData : BaseChoice
        {
        }

        public class NetworkErrorData : BaseChoice
        {
            public Button m_Continue;
        }

        public Transform ConsoleCanvasRoot { get; private set; }
        public Transform ConsoleCanvasEndRoot { get; private set; }
        public MainRootScreenData m_MainRootScreenData = new MainRootScreenData();
        public TutorialPageData m_TutorialPageData = new TutorialPageData();
        public SoloPlayerData m_SoloPlayerData = new SoloPlayerData();
        public MultiplayerChoiceData m_MultiplayerChoiceData = new MultiplayerChoiceData();
        public LobbyCodeData m_LobbyCodeData = new LobbyCodeData();
        public MultiplayerLobbyHostData m_MultiplayerLobbyHostData = new MultiplayerLobbyHostData();
        public MultiplayerLobbyGuestData m_MultiplayerLobbyGuestData = new MultiplayerLobbyGuestData();
        public LeaveLobbyData m_LeaveLobbyData = new LeaveLobbyData();
        public CustomiseData m_CustomiseData = new CustomiseData();
        public AddDecalsData m_AddDecalsData = new AddDecalsData();
        public CustomisePodData m_CustomisePodData = new CustomisePodData();
        public AlterMaterialsData m_AlterMaterialsData = new AlterMaterialsData();
        public TierProgressionData m_TierProgressionData = new TierProgressionData();
        public ChallengesData m_ChallengesData = new ChallengesData();
        public SettingsMainMenuData m_SettingsMainMenuData = new SettingsMainMenuData();
        public CreditsData m_CreditsData = new CreditsData();
        public EndGameMenuMultiPlayerData m_EndGameMenuMultiPlayerData = new EndGameMenuMultiPlayerData();
        public EndGameMenuSinglePlayerData m_EndGameMenuSinglePlayerData = new EndGameMenuSinglePlayerData();
        public LoadingData m_LoadingDataData = new LoadingData();
        public NetworkErrorData m_NetworkErrorData = new NetworkErrorData();

        private bool m_IsPaused = false;
        public bool IsPaused
        {
            get { return m_IsPaused; }
            set
            {
                m_IsPaused = value;
                MusicSoundManager.Instance.PauseItem(m_IsPaused);
                CrowdAndGenericRockSoundManager.Instance.PauseItem(m_IsPaused);
            }
        }


        public List<SongData> AllSongData { get; private set; }

        public Dictionary<ScreenMenu, BaseChoice> m_AllScreens = new Dictionary<ScreenMenu, BaseChoice>();
        private Stack<ScreenMenu> m_StackScreenMenu = new Stack<ScreenMenu>();
        private ScreenMenu CurrentScreen => m_CurrentScreenMenu;


        private List<RoomInfo> m_CurrentLobbyRooms = new List<RoomInfo>();
        private List<RoomInfo> m_CurrentNotPrivateLobbyRooms = new List<RoomInfo>();
        private List<Player> m_CurrentRoomPlayers = new List<Player>();

        public PauseMenuManager PauseMenuManagerRef;
        public LaptopMenuManager LaptopMenuManagerRef;

        public List<UnityEngine.Rendering.Universal.DecalProjector> m_MenuDecals = new List<UnityEngine.Rendering.Universal.DecalProjector>();

        private bool m_IsInitialised = false;
        public void Initialise()
        {
            Instance = this;


            m_LaptopGUIHolder = transform.Search("LaptopGUIHolder");
            m_GuiHolder = transform.Search("GuiHolder");
            m_GuiEndHolder = transform.Search("GuiEndHolder");

            PauseMenuManagerRef = this.AddComponent<PauseMenuManager>();
            LaptopMenuManagerRef = this.AddComponent<LaptopMenuManager>();

            ConsoleCanvasRoot = m_GuiHolder.Search("ConsoleRoot");
            ConsoleCanvasRoot.gameObject.ForceComponent<VRUICanvas>();

            ConsoleCanvasEndRoot = m_GuiEndHolder.Search("ConsoleRoot");
            ConsoleCanvasEndRoot.gameObject.ForceComponent<VRUICanvas>();

            this.gameObject.ForceComponent<MenuManagerDebug>();

#if UNITY_EDITOR
            /////PRINT_SONG_DATA();
#endif

            var Decals_Player1 = this.transform.SearchComponent<Transform>("Decals_Player1");
            m_MenuDecals.AddRange(Decals_Player1.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true).ToList());

            InitiliseMainRootScreenData();
            InitiliseTutorialPageData();
            InitiliseSoloPlayerData();
            InitiliseMultiplayerChoiceData();
            InitiliseLobbyCodeData();
            InitiliseMultiplayerLobbyHostData();
            InitiliseMultiplayerLobbyGuestData();
            InitiliseLeaveLobbyData();
            InitiliseCustomiseData();
            InitiliseAddDecalsData();
            InitiliseCustomisePodData();
            InitiliseAlterMaterialsData();
            InitiliseTierProgressionData();
            InitiliseChallengesData();
            InitiliseSettingsMainMenuData();
            InitiliseCreditsData();
            InitiliseEndGameMulitplayerMenuData();
            InitiliseEndGameSinglePlayerMenuData();
            InitiliseLoadingData();
            InitiliseNetworkErrorData();

            BasicNetworkMessages();
            MenuManager.Instance.LaptopMenuManagerRef.UpdateMenuData();

            foreach (ScreenMenu screen in (ScreenMenu[])Enum.GetValues(typeof(ScreenMenu)))
            {
                ChangeToScreen(screen, true);
            }

            m_StackScreenMenu.Clear();
            ForwardToScreen(ScreenMenu.MainRootScreen);

            TurnOnMenuDecals();
            TurnOnMenuToggles();
            m_IsInitialised = true;

            NetworkMessagesManager.Instance.PhotonMultiplayerRef.OnOwnerChanged((player) =>
            {
                Debug.LogError($"Owner changed");
            });

            foreach (MonitorTrainerConsts.PlayersEnum playerEnum in Enum.GetValues(typeof(MonitorTrainerConsts.PlayersEnum)))
            {
                MenuAreaClass playerClass = new MenuAreaClass();
                int index = ((int)playerEnum + 1);
                playerClass.m_OnArea = this.gameObject.SearchComponent<Transform>($"Player{index}_Active");
                playerClass.m_OffArea = this.gameObject.SearchComponent<Transform>($"Player{index}_Off");
                playerClass.m_Start = playerClass.m_OnArea.gameObject.SearchComponent<Transform>("Start");

                playerClass.m_Console = playerClass.m_OnArea.gameObject.SearchComponent<Transform>("Quest_MonitorTrainer_Console_Prefab_Prefab_Menu");
                playerClass.m_LapTop = playerClass.m_OnArea.gameObject.SearchComponent<Transform>("Quest_Laptop_Prefab");
                m_MenuArea.Add(playerEnum, playerClass);
            }

            NetworkMessagesManager.Instance.ReceivePlayerOrder((data) =>
            {
                if (Core.PhotonMultiplayerRef.CurrentRoom != null && Core.PhotonMultiplayerRef.IsOwner == false)
                {
                    var foundSelf = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == Core.PhotonMultiplayerRef.MySelf.ActorNumber);
                    MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Clear();
                    MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected = data;

                    Debug.LogError($"Found self {foundSelf != null} ");
                    // not in original 
                    if (foundSelf == null)
                    {
                        foundSelf = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == Core.PhotonMultiplayerRef.MySelf.ActorNumber);
                        if (foundSelf != null)
                        {
                            // found in new data 
                            MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum = (PlayersEnum)foundSelf.CurrentPlayersEnum;
                            Teleport(MenuManager.Instance.m_MenuArea[(PlayersEnum)foundSelf.CurrentPlayersMenuPlaceEnum].m_Start);
                        }
                    }

                    TurnOnMenuBlocks();

                    if (m_CurrentScreenMenu == ScreenMenu.MultiplayerLobbyHost)
                    {
                        OrderTheDisplays(m_MultiplayerLobbyHostData);
                    }
                    else
                    {
                        OrderTheDisplays(m_MultiplayerLobbyGuestData);
                    }
                }
            });

            NetworkMessagesManager.Instance.PhotonMultiplayerRef.OnRoomPlayersChanged((player) =>
            {
                if (MonitorTrainerRoot.Instance.CurrentScenario != ScenarioEnum.Menu)
                {
                    return;
                }
                if (Core.PhotonMultiplayerRef.CurrentRoom == null || Core.PhotonMultiplayerRef.IsOwner == false)
                {
                    return;
                }
                Debug.LogError($"OnRoomPlayersChanged Old {MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Count}");

                #region Remove all not in list
                List<LocalPlayerData.SaveDataClass.SendData> foundAll = new List<LocalPlayerData.SaveDataClass.SendData>();
                for (int i = 0; i < player.Count; i++)
                {
                    var found = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == player[i].ActorNumber);
                    if (found != null)
                    {
                        foundAll.Add(found);
                    }

                }

                List<LocalPlayerData.SaveDataClass.SendData> removeAll = new List<LocalPlayerData.SaveDataClass.SendData>();

                foreach (var item in MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected)
                {
                    var found = foundAll.Find(e => e.ActorNumber == item.ActorNumber);
                    if (found == null)
                    {
                        removeAll.Add(item);
                    }
                }
                foreach (var item in removeAll)
                {
                    MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Remove(item);
                }
                #endregion Remove all not in list

                Debug.LogError($"OnRoomPlayersChanged removed {MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Count}");

                Debug.LogError($"Figure out play positions player count: {player.Count}");
                for (int i = 0; i < player.Count; i++)
                {
                    var found = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == player[i].ActorNumber);
                    if (found == null)
                    {
                        var newData = new LocalPlayerData.SaveDataClass.SendData();
                        newData.ActorNumber = player[i].ActorNumber;
                        newData.Nickname = player[i].NickName;

                        // find the first free enums
                        foreach (PlayersEnum playerEnum in Enum.GetValues(typeof(PlayersEnum)))
                        {
                            Debug.LogError("Player selection");
                            var team1Count = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team1);
                            var foundEnum1 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum == playerEnum);
                            if (foundEnum1 == null)
                            {
                                if (newData.CurrentPlayersMenuPlaceEnum == null)
                                {
                                    newData.CurrentPlayersEnum = playerEnum;
                                    newData.CurrentPlayersMenuPlaceEnum = playerEnum;

                                    if (team1Count.Count != 2)
                                    {
                                        newData.CurrentTeamEnum = TeamEnum.Team1;
                                    }
                                    else
                                    {
                                        newData.CurrentTeamEnum = TeamEnum.Team2;
                                    }
                                }
                            }
                        }
                        MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Add(newData);
                    }
                }

                foreach (var item in MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected)
                {
                    Debug.LogError($"item {item.Nickname} {item.CurrentPlayersMenuPlaceEnum}");
                }
                Debug.LogError($"OnRoomPlayersChanged new {MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Count}");

                TurnOnMenuBlocks();
                NetworkMessagesManager.Instance.SendPlayerOrder(MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected);

            });

            NetworkMessagesManager.Instance.ReceiveGuestSwap((guestSwap) =>
            {
                if (MonitorTrainerRoot.Instance.CurrentScenario != ScenarioEnum.Menu)
                {
                    return;
                }
                if (Core.PhotonMultiplayerRef.CurrentRoom == null || Core.PhotonMultiplayerRef.IsOwner == false)
                {
                    return;
                }
                // has to be the owner
                SwapTeams(guestSwap.ActorNumber);
                OrderTheDisplays(m_MultiplayerLobbyHostData);
                OrderTheDisplays(m_MultiplayerLobbyGuestData);
            });

            NetworkMessagesManager.Instance.ReceiveEndContinue(() =>
            {
                EndContinue();
            });
        }

        private void EndContinue()
        {
            m_LastTeleport = null;
            Teleport(MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start);
            m_StackScreenMenu.Clear();

            if (Core.PhotonMultiplayerRef.IsOwner == true)
            {
                ForwardToScreen(ScreenMenu.MultiplayerLobbyHost);
            }
            else
            {
                ForwardToScreen(ScreenMenu.MultiplayerLobbyGuest);
            }
        }

        private void OnEnable() => EnableAllObjects(true);
        private void OnDisable() => EnableAllObjects(false);

        private void EnableAllObjects(bool enable)
        {
            var all = Core.Scene.GetAllSpawnedVrInteractionScene(this.gameObject.scene);
            all.ForEach(e => e.gameObject.SetActive(enable));
        }


        private void TurnOnMenuBlocks()
        {
            MenuManager.Instance.m_MenuArea[PlayersEnum.Player1].TurnOn(true);
            MenuManager.Instance.m_MenuArea[PlayersEnum.Player2].TurnOn(false);
            MenuManager.Instance.m_MenuArea[PlayersEnum.Player3].TurnOn(false);
            MenuManager.Instance.m_MenuArea[PlayersEnum.Player4].TurnOn(false);

            foreach (var item in MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected)
            {
                Debug.LogError($"turn on {item.CurrentPlayersMenuPlaceEnum}");
                MenuManager.Instance.m_MenuArea[(PlayersEnum)item.CurrentPlayersMenuPlaceEnum].TurnOn(true);
            }

            // this turns it on for myself
            var foundSelf = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == Core.PhotonMultiplayerRef.MySelf.ActorNumber);
            MenuManager.Instance.m_MenuArea[(PlayersEnum)foundSelf.CurrentPlayersMenuPlaceEnum].TurnOn(true);
        }

        private void OrderTheDisplays(HostGuest hostGuest)
        {
            foreach (var item in hostGuest.m_PlayerData)
            {
                item.m_PlayerName.text = "";
            }

            var team1 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team1);
            var team2 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team2);
            for (int i = 0; i < team1.Count; i++)
            {
                hostGuest.m_PlayerData[i].m_PlayerName.text = team1[i].Nickname;
            }
            for (int i = 0; i < team2.Count; i++)
            {
                hostGuest.m_PlayerData[i + 2].m_PlayerName.text = team2[i].Nickname;
            }
        }

        public void Teleport(Transform teleportPosition)
        {
            if (m_LastTeleport != teleportPosition)
            {
                m_LastTeleport = teleportPosition;
                CameraControllerVR.Instance.TeleportAvatar(MenuManager.Instance.gameObject.scene, teleportPosition, null);
            }
        }

        private void SwapTeams(int actorNumber)
        {
            var foundSelf = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == actorNumber);
            if (foundSelf.CurrentTeamEnum == TeamEnum.Team1)
            {
                var team2 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team2);
                if (team2.Count != 2)
                {
                    foundSelf.CurrentTeamEnum = TeamEnum.Team2;
                    NetworkMessagesManager.Instance.SendPlayerOrder(MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected);
                }
            }
            else
            {
                var team1 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team1);
                if (team1.Count != 2)
                {
                    foundSelf.CurrentTeamEnum = TeamEnum.Team1;
                    NetworkMessagesManager.Instance.SendPlayerOrder(MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected);

                }

            }
        }

        private void PRINT_SONG_DATA()
        {
            // write the json file
            SongData song = new SongData();
            song.SongName = "fred";
            song.SongLength = 234.4f;
            CharacterDataClass b = new CharacterDataClass();
            b.Position = new Vector3(1.5f, 2.225f, -9.36f);
            b.Rotation = new Vector3(0f, 255f, 0f);
            song.BandMembers.Add(b);

            b = new CharacterDataClass();

            b.Position = new Vector3(-2.56f, 1.638f, -0.514f);
            b.Rotation = new Vector3(0f, 246.8854f, 0f);
            song.BandMembers.Add(b);

            b = new CharacterDataClass();

            b.Position = new Vector3(-4.625f, 1.637f, -4.969f);
            b.Rotation = new Vector3(0f, 270.0002f, 0f);
            song.BandMembers.Add(b);

            b = new CharacterDataClass();

            b.Position = new Vector3(2.013f, 2.598f, -0.576f);
            b.Rotation = new Vector3(0f, 255.6541f, 0f);
            song.BandMembers.Add(b);

            b = new CharacterDataClass();

            b.Position = new Vector3(2.013f, 2.598f, -0.576f);
            b.Rotation = new Vector3(0f, 255.6541f, 0f);
            song.BandMembers.Add(b);

            b = new CharacterDataClass();
            b.Position = new Vector3(-1.685f, 1.638f, -9.116f);
            b.Rotation = new Vector3(0f, 258.6313f, 0f);
            b.CharacterName = "MOVE";
            b.Abbreviation = "MOVE";
            b.Colour = new Color(45, 54, 33);
            b.ColourName = "MOVE";
            b.AuxEnum = AuxEnum.Bass;
            b.InputIndex = 1;
            song.BandMembers.Add(b);

            b = new CharacterDataClass();
            b.Position = new Vector3(2.123f, 2.483f, -4.998f);
            b.Rotation = new Vector3(0f, 324.1868f, 0f);
            song.BandMembers.Add(b);

            Json.JsonNet.WriteToFile(song, @"D:\SongData.json", true);
        }


        #region Helpers

        private void AddBasics(BaseChoice baseItem)
        {
            baseItem.m_BasePlayStyle = baseItem.m_Root.SearchComponent<Button>("PlayStyle", false);
            if (baseItem.m_BasePlayStyle != null)
            {
                baseItem.m_BasePlayStyle.onClick.AddListener(() => ForwardToScreen(ScreenMenu.MainRootScreen));
                baseItem.m_BasePlayStyle.GetComponent<Image>().raycastTarget = true;
            }

            baseItem.m_BaseCustomise = baseItem.m_Root.SearchComponent<Button>("Customise", false);
            if (baseItem.m_BaseCustomise != null)
            {
                baseItem.m_BaseCustomise.onClick.AddListener(() => ForwardToScreen(ScreenMenu.Customise));
                baseItem.m_BaseCustomise.GetComponent<Image>().raycastTarget = true;
            }

            baseItem.m_BaseStats = baseItem.m_Root.SearchComponent<Button>("Stats", false);
            if (baseItem.m_BaseStats != null)
            {
                baseItem.m_BaseStats.onClick.AddListener(() => ForwardToScreen(ScreenMenu.Challenges));
                baseItem.m_BaseStats.GetComponent<Image>().raycastTarget = true;
            }

            baseItem.m_BaseSettings = baseItem.m_Root.SearchComponent<Button>("Settings", false);
            if (baseItem.m_BaseSettings != null)
            {
                baseItem.m_BaseSettings.onClick.AddListener(() => ForwardToScreen(ScreenMenu.SettingsMainMenu));
                baseItem.m_BaseSettings.GetComponent<Image>().raycastTarget = true;
            }

            baseItem.m_BaseBack = baseItem.m_Root.SearchComponent<Button>("Back", false);
            if (baseItem.m_BaseBack != null)
            {
                baseItem.m_BaseBack.onClick.AddListener(() => BackPressed());
                baseItem.m_BaseBack.GetComponent<Image>().raycastTarget = true;
            }
        }
        private void InitiliseToggleSwap(ToggleSwap swap, Toggle toggle)
        {
            swap.m_Toggle = toggle;
            swap.m_Off = swap.m_Toggle.gameObject.GetComponentsInChildren<Image>()[0];
            swap.m_On = swap.m_Toggle.gameObject.GetComponentsInChildren<Image>()[1];
            swap.m_Toggle.onValueChanged.AddListener((state) =>
            {
                swap.m_Off.gameObject.SetActive(!state);
                swap.m_On.gameObject.SetActive(state);
            });
        }

        private void InitiliseButtonAmount(ButtonAmount swap, Button button)
        {
            swap.m_Button = button;
            swap.m_ButtonAmountVariable = swap.m_Button.gameObject.SearchComponent<TextMeshProUGUI>("ButtonAmountVariable");
            swap.m_ButtonAmountTotal = swap.m_Button.gameObject.SearchComponent<TextMeshProUGUI>("ButtonAmountTotal");
        }

        private void InitiliseHorizontalData(HorizontalData horizontalData, Transform scroll)
        {
            horizontalData.m_Root = scroll.transform;
            horizontalData.m_HorizontalLayoutGroup = scroll.GetComponentInChildren<HorizontalLayoutGroup>();
            int count = horizontalData.m_HorizontalLayoutGroup.transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    horizontalData.m_Prefab = horizontalData.m_HorizontalLayoutGroup.transform.GetChild(i).gameObject;
                    horizontalData.m_Prefab.SetActive(false);
                }
                else
                {
                    Destroy(horizontalData.m_HorizontalLayoutGroup.transform.GetChild(i).gameObject);
                }
            }
            horizontalData.Init();
        }




        private void InitiliseVerticalData(VerticalData verticalData, Transform scroll)
        {
            verticalData.m_Root = scroll.transform;
            verticalData.m_VerticalLayoutGroup = scroll.GetComponentInChildren<VerticalLayoutGroup>();
            int count = verticalData.m_VerticalLayoutGroup.transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    verticalData.m_Prefab = verticalData.m_VerticalLayoutGroup.transform.GetChild(i).gameObject;
                    verticalData.m_Prefab.SetActive(false);
                }
                else
                {
                    Destroy(verticalData.m_VerticalLayoutGroup.transform.GetChild(i).gameObject);
                }
            }
            verticalData.Init();
        }
        #endregion Helpers

        #region Initilise

        private void InitiliseMainRootScreenData()
        {
            m_AllScreens.Add(ScreenMenu.MainRootScreen, m_MainRootScreenData);
            m_MainRootScreenData.m_Root = ConsoleCanvasRoot.Search("MainRootScreen");
            AddBasics(m_MainRootScreenData);

            m_MainRootScreenData.m_Solo = m_MainRootScreenData.m_Root.transform.SearchComponent<Button>("Solo");
            m_MainRootScreenData.m_Solo.GetComponent<Image>().raycastTarget = true;
            m_MainRootScreenData.m_Solo.onClick.AddListener(() => ForwardToScreen(ScreenMenu.SoloPlayer));

            m_MainRootScreenData.m_Multiplayer = m_MainRootScreenData.m_Root.transform.SearchComponent<Button>("Multiplayer");
            m_MainRootScreenData.m_Multiplayer.GetComponent<Image>().raycastTarget = true;
            m_MainRootScreenData.m_Multiplayer.onClick.AddListener(() => ForwardToScreen(ScreenMenu.MultiplayerChoice));

            m_MainRootScreenData.m_Tutorial = m_MainRootScreenData.m_Root.transform.SearchComponent<Button>("Tutorial");
            m_MainRootScreenData.m_Tutorial.GetComponent<Image>().raycastTarget = true;
            m_MainRootScreenData.m_Tutorial.onClick.AddListener(() => ForwardToScreen(ScreenMenu.TutorialPage));

            m_MainRootScreenData.m_ChallengeBoxTop = m_MainRootScreenData.m_Root.transform.SearchComponent<Transform>("ChallengeBoxTop");
            m_MainRootScreenData.m_ChallengeBoxBottom = m_MainRootScreenData.m_Root.transform.SearchComponent<Transform>("ChallengeBoxBottom");
            UpdateMainRootScreenChallenges();
        }

        private void InitiliseTutorialPageData()
        {
            m_AllScreens.Add(ScreenMenu.TutorialPage, m_TutorialPageData);
            m_TutorialPageData.m_Root = ConsoleCanvasRoot.Search("TutorialPage");
            AddBasics(m_TutorialPageData);

            m_TutorialPageData.m_LaunchTutorial = m_TutorialPageData.m_Root.SearchComponent<Button>("LaunchTutorial");
            m_TutorialPageData.m_LaunchTutorial.GetComponent<Image>().raycastTarget = true;
            m_TutorialPageData.m_LaunchTutorial.onClick.AddListener(() => ForwardToScreen(ScreenMenu.TutorialPage));
        }

        private void InitiliseSoloPlayerData()
        {
            m_AllScreens.Add(ScreenMenu.SoloPlayer, m_SoloPlayerData);
            m_SoloPlayerData.m_Root = ConsoleCanvasRoot.Search("SoloPlayer");
            AddBasics(m_SoloPlayerData);

            m_SoloPlayerData.m_Easy = m_SoloPlayerData.m_Root.SearchComponent<Toggle>("Easy");
            m_SoloPlayerData.m_Easy.GetComponentInChildren<Image>(true).raycastTarget = true;
            m_SoloPlayerData.m_Easy.onValueChanged.AddListener((state) =>
            {
                if (state == true)
                {
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = DifficultyModeEnum.Easy;
                }
            });

            m_SoloPlayerData.m_Medium = m_SoloPlayerData.m_Root.SearchComponent<Toggle>("Medium");
            m_SoloPlayerData.m_Medium.GetComponentInChildren<Image>(true).raycastTarget = true;
            m_SoloPlayerData.m_Medium.onValueChanged.AddListener((state) =>
            {
                if (state == true)
                {
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = DifficultyModeEnum.Medium;
                }
            });

            m_SoloPlayerData.m_Hard = m_SoloPlayerData.m_Root.SearchComponent<Toggle>("Hard");
            m_SoloPlayerData.m_Hard.GetComponentInChildren<Image>(true).raycastTarget = true;
            m_SoloPlayerData.m_Hard.onValueChanged.AddListener((state) =>
            {
                if (state == true)
                {
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = DifficultyModeEnum.Hard;
                }
            });

            var songListTransform = m_SoloPlayerData.m_Root.SearchComponent<Transform>("SongList");
            InitiliseVerticalData(m_SoloPlayerData.m_VerticalData, songListTransform);
            FillInSongList(m_SoloPlayerData.m_VerticalData, null);


            m_SoloPlayerData.m_LaunchSolo = m_SoloPlayerData.m_Root.SearchComponent<Button>("LaunchSolo");
            m_SoloPlayerData.m_LaunchSolo.onClick.AddListener(() =>
            {
                MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.Solo;
                MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef = AllSongData.Find(e => e.SongName == MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName);
                MenuManager.Instance.LaptopMenuManagerRef.UpdateMenuData();
                SongAndDataLoadedAndReadyToStart(() =>
                {
                    InternalReceiveStartSession();
                    ConsoleScreenManager.Instance.BootGlitchScreensRef.StopBoot();
                });
            });
        }

        private void InitiliseMultiplayerChoiceData()
        {
            m_AllScreens.Add(ScreenMenu.MultiplayerChoice, m_MultiplayerChoiceData);
            m_MultiplayerChoiceData.m_Root = ConsoleCanvasRoot.Search("MultiplayerChoice");
            AddBasics(m_MultiplayerChoiceData);

            m_MultiplayerChoiceData.m_JoinRandomLobby = m_MultiplayerChoiceData.m_Root.SearchComponent<Button>("JoinRandomLobby");
            m_MultiplayerChoiceData.m_JoinRandomLobby.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerChoiceData.m_JoinRandomLobby.onClick.AddListener(() =>
            {
                if (m_CurrentNotPrivateLobbyRooms.Count != 0)
                {
                    var room = m_CurrentNotPrivateLobbyRooms.GetRandom();
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode = room.Name;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.LobbyCode = room.Name;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.ActorNumber = Core.PhotonMultiplayerRef.MySelf.ActorNumber;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.Nickname = Core.PhotonMultiplayerRef.MySelf.NickName;
                    Core.PhotonMultiplayerRef.ChangeRoom($"{room.Name}");
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.MultiplayerGuest;
                    Debug.LogError("send message to room owner, for details");
                    ForwardToScreen(ScreenMenu.MultiplayerLobbyGuest);
                }
                else
                {
                    CreateHost();
                }

            });

            m_MultiplayerChoiceData.m_SetupLobby = m_MultiplayerChoiceData.m_Root.SearchComponent<Button>("Setup Lobby");
            m_MultiplayerChoiceData.m_SetupLobby.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerChoiceData.m_SetupLobby.onClick.AddListener(() =>
            {
                CreateHost();
            });



            m_MultiplayerChoiceData.m_EnterLobbyCode = m_MultiplayerChoiceData.m_Root.SearchComponent<Button>("EnterLobbyCode");
            m_MultiplayerChoiceData.m_EnterLobbyCode.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerChoiceData.m_EnterLobbyCode.onClick.AddListener(() =>
            {
                MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.MultiplayerGuest;
                ForwardToScreen(ScreenMenu.LobbyCode);
            });

            InitiliseToggleSwap(m_MultiplayerChoiceData.m_MakePrivate, m_MultiplayerChoiceData.m_Root.SearchComponent<Toggle>("MakePrivate"));
            m_MultiplayerChoiceData.m_MakePrivate.m_Toggle.onValueChanged.AddListener((state) =>
            {
                MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PrivateLobby = state;
            });


            void CreateHost()
            {
                ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
                customRoomProperties[PRIVATE_ROOM] = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PrivateLobby;
                Core.PhotonMultiplayerRef.RoomOptionsRef.CustomRoomProperties = customRoomProperties;
                NetworkMessagesManager.Instance.CreateRoom(out string roomNumber);
                MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode = roomNumber;
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.LobbyCode = roomNumber;
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.ActorNumber = Core.PhotonMultiplayerRef.MySelf.ActorNumber;
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.Nickname = Core.PhotonMultiplayerRef.MySelf.NickName;
                MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.MultiplayerHost;

                ////MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected[PlayersEnum.Player1] = null;
                ////MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected[PlayersEnum.Player2] = null;
                ////MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected[PlayersEnum.Player3] = null;
                ////MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected[PlayersEnum.Player4] = null;
                ForwardToScreen(ScreenMenu.MultiplayerLobbyHost);
            }

        }

        private void InitiliseLobbyCodeData()
        {
            m_AllScreens.Add(ScreenMenu.LobbyCode, m_LobbyCodeData);
            m_LobbyCodeData.m_Root = ConsoleCanvasRoot.Search("LobbyCode");
            AddBasics(m_LobbyCodeData);

            for (int i = 0; i < 10; i++)
            {
                int index = i;
                Button button = m_LobbyCodeData.m_Root.SearchComponent<Button>($"Button{index}");
                button.GetComponent<Image>().raycastTarget = true;
                button.onClick.AddListener(() =>
                {
                    m_LobbyCodeData.m_LobbyCodeText.text += index;
                    CheckLobbyCode();
                });
                m_LobbyCodeData.m_NumberButtons.Add(button);
            }

            m_LobbyCodeData.m_CloseButton = m_LobbyCodeData.m_Root.SearchComponent<Button>("CloseButton");
            m_LobbyCodeData.m_CloseButton.GetComponent<Image>().raycastTarget = true;
            m_LobbyCodeData.m_CloseButton.onClick.AddListener(() => BackPressed());

            m_LobbyCodeData.m_TickButton = m_LobbyCodeData.m_Root.SearchComponent<Button>("Tick");
            m_LobbyCodeData.m_TickButton.GetComponent<Image>().raycastTarget = true;
            m_LobbyCodeData.m_TickButton.onClick.AddListener(() =>
            {
                Debug.LogError("check rooms number");
                RoomInfo room = m_CurrentLobbyRooms.Find(e => e.Name == m_LobbyCodeData.m_LobbyCodeText.text);
                if (room != null)
                {
                    Core.PhotonMultiplayerRef.ChangeRoom($"{room.Name}");
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.MultiplayerGuest;
                    Debug.LogError("send message to room owner, for details");
                    ForwardToScreen(ScreenMenu.MultiplayerLobbyGuest);
                }
                else
                {
                    Debug.LogError("room Not Found");
                    BackPressed();
                }
            });

            m_LobbyCodeData.m_Delete = m_LobbyCodeData.m_Root.SearchComponent<Button>("Delete");
            m_LobbyCodeData.m_Delete.GetComponent<Image>().raycastTarget = true;
            m_LobbyCodeData.m_Delete.onClick.AddListener(() =>
            {
                m_LobbyCodeData.m_LobbyCodeText.text = m_LobbyCodeData.m_LobbyCodeText.text.Remove(m_LobbyCodeData.m_LobbyCodeText.text.Length - 1);
                CheckLobbyCode();
            });
            m_LobbyCodeData.m_LobbyCodeText = m_LobbyCodeData.m_Root.SearchComponent<TextMeshProUGUI>("LobbyCodeText");

            m_LobbyCodeData.m_LobbyCodeText.text = "";
            CheckLobbyCode();

        }

        private void CheckLobbyCode()
        {
            m_LobbyCodeData.m_Delete.interactable = (m_LobbyCodeData.m_LobbyCodeText.text.Length > 0);
            m_LobbyCodeData.m_NumberButtons.ForEach(e => e.interactable = (m_LobbyCodeData.m_LobbyCodeText.text.Length < 4));
        }

        private void InitiliseMultiplayerLobbyHostData()
        {
            m_AllScreens.Add(ScreenMenu.MultiplayerLobbyHost, m_MultiplayerLobbyHostData);
            m_MultiplayerLobbyHostData.m_Root = ConsoleCanvasRoot.Search("MultiplayerLobbyHost");
            AddBasics(m_MultiplayerLobbyHostData);

            ToggleGroup group = m_MultiplayerLobbyHostData.m_Root.SearchComponent<ToggleGroup>("Difficulty");
            m_MultiplayerLobbyHostData.m_Easy = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Toggle>("Easy");
            m_MultiplayerLobbyHostData.m_Easy.GetComponentInChildren<Image>(true).raycastTarget = true;
            m_MultiplayerLobbyHostData.m_Easy.group = group;
            m_MultiplayerLobbyHostData.m_Easy.onValueChanged.AddListener((state) =>
            {
                if (state == true)
                {
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = DifficultyModeEnum.Easy;
                    Debug.LogError($"CurrentDifficulty Changed: {MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty}");
                    SendHostData();
                }
            });

            m_MultiplayerLobbyHostData.m_Medium = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Toggle>("Medium");
            m_MultiplayerLobbyHostData.m_Medium.GetComponentInChildren<Image>(true).raycastTarget = true;
            m_MultiplayerLobbyHostData.m_Medium.group = group;
            m_MultiplayerLobbyHostData.m_Medium.onValueChanged.AddListener((state) =>
            {
                if (state == true)
                {
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = DifficultyModeEnum.Medium;
                    Debug.LogError($"CurrentDifficulty Changed: {MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty}");
                    SendHostData();
                }
            });

            m_MultiplayerLobbyHostData.m_Hard = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Toggle>("Hard");
            m_MultiplayerLobbyHostData.m_Hard.GetComponentInChildren<Image>(true).raycastTarget = true;
            m_MultiplayerLobbyHostData.m_Hard.group = group;
            m_MultiplayerLobbyHostData.m_Hard.onValueChanged.AddListener((state) =>
            {
                if (state == true)
                {
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = DifficultyModeEnum.Hard;
                    Debug.LogError($"CurrentDifficulty Changed: {MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty}");
                    SendHostData();
                }
            });


            m_MultiplayerLobbyHostData.m_SwapTeamsButton = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Button>("SwapTeamsButton");
            m_MultiplayerLobbyHostData.m_SwapTeamsButton.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerLobbyHostData.m_SwapTeamsButton.onClick.AddListener(() =>
            {
                SwapTeams(Core.PhotonMultiplayerRef.MySelf.ActorNumber);
                OrderTheDisplays(m_MultiplayerLobbyHostData);
            });


            m_MultiplayerLobbyHostData.m_LobbyCodeString = m_MultiplayerLobbyHostData.m_Root.SearchComponent<TextMeshProUGUI>("LobbyCodeString");
            m_MultiplayerLobbyHostData.m_LobbyCodeString.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode;



            m_MultiplayerLobbyHostData.m_LaunchMultiplayer = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Button>("LaunchMultiplayer");
            m_MultiplayerLobbyHostData.m_LaunchMultiplayer.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerLobbyHostData.m_LaunchMultiplayer.onClick.AddListener(() =>
            {
                ForwardToScreen(ScreenMenu.Loading);
                Core.PhotonMultiplayerRef.CurrentRoom.IsOpen = false;
                NetworkMessagesManager.Instance.SendMuliplayerload(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode);
                NetworkMessagesManager.Instance.SendPlayerPrefsData(ref MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef);

            });

            m_MultiplayerLobbyHostData.m_CloseButton = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Button>("CloseButton");
            m_MultiplayerLobbyHostData.m_CloseButton.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerLobbyHostData.m_CloseButton.onClick.AddListener(() => ForwardToScreen(ScreenMenu.LeaveLobby));

            var songListTransform = m_MultiplayerLobbyHostData.m_Root.SearchComponent<Transform>("SongList");
            InitiliseVerticalData(m_MultiplayerLobbyHostData.m_VerticalData, songListTransform);
            FillInSongList(m_MultiplayerLobbyHostData.m_VerticalData, () =>
            {
                ConsoleExtra.Log($"Song Changed: {MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName}", null, ConsoleExtraEnum.EDebugType.DebugLogging);
                SendHostData();
            });

            m_MultiplayerLobbyHostData.m_Root.SearchComponent<Button>("CloseButton");

            InitilisePlayerData(m_MultiplayerLobbyHostData);

            void SendHostData()
            {
                this.WaitUntil(1, () => Core.PhotonMultiplayerRef.PhotonViewOwnerRef != null, () =>
                {
                    PlayerChoiceData baseItem = MonitorTrainerRoot.Instance.PlayerChoiceDataRef;
                    NetworkMessagesManager.Instance.SendFullRoomDetails(baseItem);
                });
            }

            NetworkMessagesManager.Instance.ReceivePlayerReady((playerReady) =>
            {
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle == PlayStyleEnum.MultiplayerHost && MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode == playerReady.m_LobbyCode)
                {
                    var player = m_MultiplayerLobbyHostData.m_PlayerData.Find(e => e.m_ActorNumber == playerReady.m_OwnerActorNr);
                    if (player != null)
                    {
                        player.m_PlayerReadyIcon.SetActive(playerReady.m_IsReady);
                    }
                }
            });

            NetworkMessagesManager.Instance.ReceiveSongFullyLoaded((playerReady) =>
            {
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle == PlayStyleEnum.MultiplayerHost && MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode == playerReady.m_LobbyCode)
                {
                    var player = m_MultiplayerLobbyHostData.m_PlayerData.Find(e => e.m_ActorNumber == playerReady.m_OwnerActorNr);
                    if (player != null)
                    {
                        player.m_FullyLoaded = true;
                    }
                }

                var FullyLoaded = m_MultiplayerLobbyHostData.m_PlayerData.FindAll(e => e.m_FullyLoaded == true);
                var playerUsed = m_MultiplayerLobbyHostData.m_PlayerData.FindAll(e => e.m_PlayerName.text != "");
                if (FullyLoaded.Count == playerUsed.Count)
                {
                    NetworkMessagesManager.Instance.SendStartSession();
                }
            });


        }


        private void InitilisePlayerData(HostGuest hostGuest)
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                hostGuest.m_PlayerData.Add(new PlayerData());

                string backGround = $"Slot{i + 1}Background";
                string slot = $"Slot{i + 1}NameString";
                hostGuest.m_PlayerData[i].m_Background = hostGuest.m_Root.SearchComponent<Image>(backGround, false);
                if (hostGuest.m_PlayerData[i].m_Background == null)
                {
                    hostGuest.m_PlayerData[i].m_Background = hostGuest.m_Root.SearchComponent<Image>("Guest4Background");
                }
                hostGuest.m_PlayerData[i].m_PlayerName = hostGuest.m_PlayerData[i].m_Background.transform.SearchComponent<TextMeshProUGUI>(slot);
                hostGuest.m_PlayerData[i].m_PlayerName.autoSizeTextContainer = true;
                hostGuest.m_PlayerData[i].m_KickPlayerButton = hostGuest.m_PlayerData[i].m_Background.transform.SearchComponent<Button>("Button_KickPlayer", false);

                if (hostGuest.m_PlayerData[i].m_KickPlayerButton != null)
                {
                    hostGuest.m_PlayerData[i].m_KickPlayerButton.onClick.AddListener(() =>
                    {
                        Debug.LogError("m_MultiplayerLobbyHostData.m_PlayerData[i].m_KickPlayerButton");
                    });
                }

                hostGuest.m_PlayerData[i].m_PlayerReadyIcon = hostGuest.m_PlayerData[i].m_Background.transform.SearchComponent<Image>("PlayerReadyIcon");
            }
        }


        private void InitiliseMultiplayerLobbyGuestData()
        {
            m_AllScreens.Add(ScreenMenu.MultiplayerLobbyGuest, m_MultiplayerLobbyGuestData);
            m_MultiplayerLobbyGuestData.m_Root = ConsoleCanvasRoot.Search("MultiplayerLobbyGuest");
            AddBasics(m_MultiplayerLobbyGuestData);

            m_MultiplayerLobbyGuestData.m_MarkReady = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<Toggle>("Toggle");
            m_MultiplayerLobbyGuestData.m_MarkReady.GetComponentInChildren<Image>().raycastTarget = true;
            m_MultiplayerLobbyGuestData.m_MarkReady.onValueChanged.AddListener((state) =>
            {
                MenuManager.PlayerReady ready = new PlayerReady();
                ready.m_IsReady = state;
                ready.m_LobbyCode = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode;
                ready.m_OwnerActorNr = Core.PhotonMultiplayerRef.PhotonViewOwnerRef.OwnerActorNr;
                NetworkMessagesManager.Instance.SendPlayerReady(ready);
                Debug.LogError("need to send meeage to host");
            });

            m_MultiplayerLobbyGuestData.m_CloseButton = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<Button>("CloseButton");
            m_MultiplayerLobbyGuestData.m_CloseButton.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerLobbyGuestData.m_CloseButton.onClick.AddListener(() => ForwardToScreen(ScreenMenu.LeaveLobby));

            m_MultiplayerLobbyGuestData.m_SwapTeamsButton = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<Button>("SwapTeamsButton");
            m_MultiplayerLobbyGuestData.m_SwapTeamsButton.GetComponent<Image>().raycastTarget = true;
            m_MultiplayerLobbyGuestData.m_SwapTeamsButton.onClick.AddListener(() =>
            {
                Debug.LogError("Swap Button pressed");
                GuestSwap guestSwap = new GuestSwap();
                guestSwap.ActorNumber = Core.PhotonMultiplayerRef.MySelf.ActorNumber;
                NetworkMessagesManager.Instance.SendGuestSwap(guestSwap);
            });



            m_MultiplayerLobbyGuestData.m_LobbyCode = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<TextMeshProUGUI>("LobbyCode");
            m_MultiplayerLobbyGuestData.m_SongTitle = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<TextMeshProUGUI>("SongTitle");
            m_MultiplayerLobbyGuestData.m_SongTitle.autoSizeTextContainer = true;
            m_MultiplayerLobbyGuestData.m_ArtistName = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<TextMeshProUGUI>("ArtistName");
            m_MultiplayerLobbyGuestData.m_ArtistName.autoSizeTextContainer = true;
            m_MultiplayerLobbyGuestData.m_SongLength = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<TextMeshProUGUI>("SongLength");

            m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Easy = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<Image>("LobbyDifficultyIcon_Easy");
            m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Medium = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<Image>("LobbyDifficultyIcon_Medium");
            m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Hard = m_MultiplayerLobbyGuestData.m_Root.SearchComponent<Image>("LobbyDifficultyIcon_Hard");

            InitilisePlayerData(m_MultiplayerLobbyGuestData);
        }

        private void InitiliseLeaveLobbyData()
        {
            m_AllScreens.Add(ScreenMenu.LeaveLobby, m_LeaveLobbyData);
            m_LeaveLobbyData.m_Root = ConsoleCanvasRoot.Search("LeaveLobby");
            AddBasics(m_LeaveLobbyData);

            m_LeaveLobbyData.m_LeaveLobby = m_LeaveLobbyData.m_Root.SearchComponent<Button>("LeaveLobby");
            m_LeaveLobbyData.m_LeaveLobby.GetComponent<Image>().raycastTarget = true;
            m_LeaveLobbyData.m_LeaveLobby.onClick.AddListener(() =>
            {
                Core.PhotonMultiplayerRef.LeaveRoom();
                m_StackScreenMenu.Clear();
                if (m_Destination != null)
                {
                    ForwardToScreen((ScreenMenu)m_Destination);
                }
                else
                {
                    ForwardToScreen(ScreenMenu.MultiplayerChoice);
                }
                m_Destination = null;
            });

            m_LeaveLobbyData.m_Stay = m_LeaveLobbyData.m_Root.SearchComponent<Button>("Stay");
            m_LeaveLobbyData.m_Stay.GetComponent<Image>().raycastTarget = true;
            m_LeaveLobbyData.m_Stay.onClick.AddListener(() => BackPressed());
        }

        private void InitiliseCustomiseData()
        {
            m_AllScreens.Add(ScreenMenu.Customise, m_CustomiseData);
            m_CustomiseData.m_Root = ConsoleCanvasRoot.Search("Customise");
            AddBasics(m_CustomiseData);

            m_CustomiseData.m_AddDecals = m_CustomiseData.m_Root.SearchComponent<Button>("AddDecals");
            m_CustomiseData.m_AddDecals.GetComponent<Image>().raycastTarget = true;
            m_CustomiseData.m_AddDecals.onClick.AddListener(() => ForwardToScreen(ScreenMenu.AddDecals));

            m_CustomiseData.m_CustomisePod = m_CustomiseData.m_Root.SearchComponent<Button>("CustomisePod");
            m_CustomiseData.m_CustomisePod.GetComponent<Image>().raycastTarget = true;
            m_CustomiseData.m_CustomisePod.onClick.AddListener(() => ForwardToScreen(ScreenMenu.CustomisePod));

            m_CustomiseData.m_AlterMaterials = m_CustomiseData.m_Root.SearchComponent<Button>("AlterMaterials");
            m_CustomiseData.m_AlterMaterials.GetComponent<Image>().raycastTarget = true;
            m_CustomiseData.m_AlterMaterials.onClick.AddListener(() => ForwardToScreen(ScreenMenu.AlterMaterials));
        }

        private void InitiliseAddDecalsData()
        {
            m_AllScreens.Add(ScreenMenu.AddDecals, m_AddDecalsData);
            m_AddDecalsData.m_Root = ConsoleCanvasRoot.Search("AddDecals");
            AddBasics(m_AddDecalsData);

            InitiliseButtonAmount(m_AddDecalsData.m_MonitorButton, m_AddDecalsData.m_Root.SearchComponent<Button>("MonitorButton"));
            m_AddDecalsData.m_MonitorButton.m_Button.onClick.AddListener(() =>
            {
                CloseLists();
                m_AddDecalsData.m_SelectableItems_Monitor.m_Root.SetActive(true);
            });


            InitiliseButtonAmount(m_AddDecalsData.m_FlightCasesButton, m_AddDecalsData.m_Root.SearchComponent<Button>("FlightCasesButton"));
            m_AddDecalsData.m_FlightCasesButton.m_Button.onClick.AddListener(() =>
            {
                CloseLists();
                m_AddDecalsData.m_SelectableItems_FlightCases.m_Root.SetActive(true);
            });

            InitiliseButtonAmount(m_AddDecalsData.m_LaptopButton, m_AddDecalsData.m_Root.SearchComponent<Button>("LaptopButton"));
            m_AddDecalsData.m_LaptopButton.m_Button.onClick.AddListener(() =>
            {
                CloseLists();
                m_AddDecalsData.m_SelectableItems_Laptop.m_Root.SetActive(true);
            });

            InitiliseButtonAmount(m_AddDecalsData.m_LampButton, m_AddDecalsData.m_Root.SearchComponent<Button>("LampButton"));
            m_AddDecalsData.m_LampButton.m_Button.onClick.AddListener(() =>
            {
                CloseLists();
                m_AddDecalsData.m_SelectableItems_Lamp.m_Root.SetActive(true);
            });


            void CloseLists()
            {
                m_AddDecalsData.m_SelectableItems_Monitor.m_Root.SetActive(false);
                m_AddDecalsData.m_SelectableItems_FlightCases.m_Root.SetActive(false);
                m_AddDecalsData.m_SelectableItems_Laptop.m_Root.SetActive(false);
                m_AddDecalsData.m_SelectableItems_Lamp.m_Root.SetActive(false);
            }
            InitiliseHorizontalData(m_AddDecalsData.m_SelectableItems_Monitor, m_AddDecalsData.m_Root.SearchComponent<Transform>("MonitorHorizontalScroll"));
            InitiliseHorizontalData(m_AddDecalsData.m_SelectableItems_FlightCases, m_AddDecalsData.m_Root.SearchComponent<Transform>("FlightCasesHorizontalScroll"));
            InitiliseHorizontalData(m_AddDecalsData.m_SelectableItems_Laptop, m_AddDecalsData.m_Root.SearchComponent<Transform>("LaptopHorizontalScroll"));
            InitiliseHorizontalData(m_AddDecalsData.m_SelectableItems_Lamp, m_AddDecalsData.m_Root.SearchComponent<Transform>("LampHorizontalScroll"));


            FillInHorizontalDecalList(m_AddDecalsData.m_SelectableItems_Monitor, LocalPlayerData.MONITOR_LOOKUP, (itemString) =>
            {
                if (m_IsInitialised == true)
                {
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.Monitor = itemString;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
                    TurnOnMenuDecals();
                }
            });

            FillInHorizontalDecalList(m_AddDecalsData.m_SelectableItems_FlightCases, LocalPlayerData.FLIGHTCASE_LOOKUP, (itemString) =>
            {
                if (m_IsInitialised == true)
                {
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.FlightCase = itemString;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
                    TurnOnMenuDecals();
                }
            });

            FillInHorizontalDecalList(m_AddDecalsData.m_SelectableItems_Laptop, LocalPlayerData.LAPTOP_LOOKUP, (itemString) =>
            {
                if (m_IsInitialised == true)
                {
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.LapTop = itemString;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
                    TurnOnMenuDecals();
                }
            });

            FillInHorizontalDecalList(m_AddDecalsData.m_SelectableItems_Lamp, LocalPlayerData.LAMP_LOOKUP, (itemString) =>
            {
                if (m_IsInitialised == true)
                {
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.Lamp = itemString;
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
                    TurnOnMenuDecals();
                }
            });

            m_AddDecalsData.m_MonitorButton.m_ButtonAmountTotal.text = m_AddDecalsData.m_SelectableItems_Monitor.m_TogglePool.GetPublicList().Count.ToString();
            m_AddDecalsData.m_FlightCasesButton.m_ButtonAmountTotal.text = m_AddDecalsData.m_SelectableItems_FlightCases.m_TogglePool.GetPublicList().Count.ToString();
            m_AddDecalsData.m_LaptopButton.m_ButtonAmountTotal.text = m_AddDecalsData.m_SelectableItems_Laptop.m_TogglePool.GetPublicList().Count.ToString();
            m_AddDecalsData.m_LampButton.m_ButtonAmountTotal.text = m_AddDecalsData.m_SelectableItems_Lamp.m_TogglePool.GetPublicList().Count.ToString();
        }

        private void InitiliseCustomisePodData()
        {
            m_AllScreens.Add(ScreenMenu.CustomisePod, m_CustomisePodData);
            m_CustomisePodData.m_Root = ConsoleCanvasRoot.Search("CustomisePod");
            AddBasics(m_CustomisePodData);

            InitiliseButtonAmount(m_CustomisePodData.m_InstrumentsButton, m_CustomisePodData.m_Root.SearchComponent<Button>("InstrumentsButton"));
            InitiliseButtonAmount(m_CustomisePodData.m_ConsumablesButton, m_CustomisePodData.m_Root.SearchComponent<Button>("ConsumablesButton"));
            InitiliseButtonAmount(m_CustomisePodData.m_WeaponsButton, m_CustomisePodData.m_Root.SearchComponent<Button>("WeaponsButton"));
            InitiliseButtonAmount(m_CustomisePodData.m_MiscButton, m_CustomisePodData.m_Root.SearchComponent<Button>("MiscButton"));

            InitiliseHorizontalData(m_CustomisePodData.m_SelectableItems_Instruments, m_CustomisePodData.m_Root.SearchComponent<Transform>("InstrumentsHorizontalScroll"));
            InitiliseHorizontalData(m_CustomisePodData.m_SelectableItems_Consumables, m_CustomisePodData.m_Root.SearchComponent<Transform>("ConsumablesHorizontalScroll"));
            InitiliseHorizontalData(m_CustomisePodData.m_SelectableItems_Weapons, m_CustomisePodData.m_Root.SearchComponent<Transform>("WeaponsHorizontalScroll"));
            InitiliseHorizontalData(m_CustomisePodData.m_SelectableItems_Misc, m_CustomisePodData.m_Root.SearchComponent<Transform>("MiscHorizontalScroll"));
        }

        private void InitiliseAlterMaterialsData()
        {
            m_AllScreens.Add(ScreenMenu.AlterMaterials, m_AlterMaterialsData);
            m_AlterMaterialsData.m_Root = ConsoleCanvasRoot.Search("AlterMaterials");
            AddBasics(m_AlterMaterialsData);
        }

        private void InitiliseTierProgressionData()
        {
            m_AllScreens.Add(ScreenMenu.TierProgression, m_TierProgressionData);
            m_TierProgressionData.m_Root = ConsoleCanvasRoot.Search("TierProgression");
            AddBasics(m_TierProgressionData);
            PlatformManager.Leaderboards.HighScoreLeaderboardUpdatedCallback = CollectedHighScore;
            PlatformManager.Leaderboards.LeaderboardsCheckForUpdatesGlobal();

            m_TierProgressionData.m_ChallengesButton = m_TierProgressionData.m_Root.SearchComponent<Button>("ChallengesButton");
            m_TierProgressionData.m_TierProgressionButton = m_TierProgressionData.m_Root.SearchComponent<Button>("TierProgressionButton");
            m_TierProgressionData.m_ChallengesButton.onClick.AddListener(() =>
            {
                ForwardToScreen(ScreenMenu.Challenges);
            });
        }

        private void CollectedHighScore(SortedDictionary<int, LeaderboardEntry> entries)
        {

        }

        private void InitiliseChallengesData()
        {
            m_AllScreens.Add(ScreenMenu.Challenges, m_ChallengesData);
            m_ChallengesData.m_Root = ConsoleCanvasRoot.Search("Challenges");
            AddBasics(m_ChallengesData);

            m_ChallengesData.m_ChallengesButton = m_ChallengesData.m_Root.SearchComponent<Button>("ChallengesButton");
            m_ChallengesData.m_TierProgressionButton = m_ChallengesData.m_Root.SearchComponent<Button>("TierProgressionButton");
            m_ChallengesData.m_TierProgressionButton.onClick.AddListener(() =>
            {
                ForwardToScreen(ScreenMenu.TierProgression);
            });

            InitiliseHorizontalData(m_ChallengesData.m_SelectableItems_Challenges, m_ChallengesData.m_Root.SearchComponent<Transform>("ChallengesHorizontalScroll"));

            UpdateChallenges();

            PlatformManager.Achievements.CheckForAchievmentUpdates();
            PlatformManager.Achievements.m_OnAchievementsUpdated += OnAchievementsUpdated;
        }

        private void OnAchievementsUpdated()
        {
            UpdateMainRootScreenChallenges();
            UpdateChallenges();
        }

        private void UpdateMainRootScreenChallenges()
        {
            System.Random rand = new System.Random();
            var item1 = PlatformManager.Achievements.AchievementResults.ToList()[rand.Next(PlatformManager.Achievements.AchievementResults.Count)];
            var item2 = item1;
            while (item1.Key == item2.Key)
            {
                item2 = PlatformManager.Achievements.AchievementResults.ToList()[rand.Next(PlatformManager.Achievements.AchievementResults.Count)];
            }
            Dictionary<string, AchievementData> frontData = new Dictionary<string, AchievementData>();

            frontData.Add(item1.Key.ToString(), item1.Value);
            frontData.Add(item2.Key.ToString(), item2.Value);

            int index = 0;
            foreach (var item in frontData)
            {
                Transform rootObject = null;
                if (index % 2 == 0)
                {
                    rootObject = m_MainRootScreenData.m_ChallengeBoxTop;
                }
                else
                {
                    rootObject = m_MainRootScreenData.m_ChallengeBoxBottom;
                }

                rootObject.transform.SearchComponent<TextMeshProUGUI>("Challenge Text").text = item.Key.Replace("_", " ");
                rootObject.transform.SearchComponent<TextMeshProUGUI>("ChallengeAmountPlayer").text = $"{item.Value.Count.ToString()}";
                rootObject.transform.SearchComponent<TextMeshProUGUI>("ChallengeAmountTotal").text = $"/{item.Value.UnlockAt.ToString()}";

                float amount = (float)item.Value.Count;
                amount /= (float)item.Value.UnlockAt;
                rootObject.transform.SearchComponent<Slider>("Challenge Progress Bar").value = amount;
                index++;
            }
        }

        private void UpdateChallenges()
        {
            m_ChallengesData.m_SelectableItems_Challenges.m_TogglePool.DeleteAll();
            int index = 0;
            foreach (var item in PlatformManager.Achievements.AchievementResults)
            {
                Transform rootObject = null;
                Toggle spawn = null;
                if (index % 2 == 0)
                {
                    spawn = m_ChallengesData.m_SelectableItems_Challenges.m_TogglePool.SpawnObject();
                    rootObject = spawn.transform.SearchComponent<Transform>("ChallengeBoxTop");
                    var bottom = spawn.transform.SearchComponent<Transform>("ChallengeBoxBottom");
                    bottom.SetActive(false);
                }
                else
                {
                    spawn = m_ChallengesData.m_SelectableItems_Challenges.m_TogglePool.GetPublicList().Last();
                    rootObject = spawn.transform.SearchComponent<Transform>("ChallengeBoxBottom");
                    rootObject.SetActive(true);
                }

                rootObject.transform.SearchComponent<TextMeshProUGUI>("ChallengeTextString").text = item.Key.ToString().Replace("_", " ");
                rootObject.transform.SearchComponent<TextMeshProUGUI>("ChallengeTextAmountStringVariable").text = $"{item.Value.Count.ToString()}";
                rootObject.transform.SearchComponent<TextMeshProUGUI>("ChallengeTextAmountTotal").text = $"/{item.Value.UnlockAt.ToString()}";

                float amount = (float)item.Value.Count;
                amount /= (float)item.Value.UnlockAt;
                rootObject.transform.SearchComponent<Slider>("Challenge Progress Bar").value = amount;
                index++;
            }
        }

        private void InitiliseSettingsMainMenuData()
        {
            m_AllScreens.Add(ScreenMenu.SettingsMainMenu, m_SettingsMainMenuData);
            m_SettingsMainMenuData.m_Root = ConsoleCanvasRoot.Search("SettingsMainMenu");
            AddBasics(m_SettingsMainMenuData);

            m_SettingsMainMenuData.m_MasterVolumeSlider = m_SettingsMainMenuData.m_Root.SearchComponent<Slider>("MasterVolumeSlider");
            m_SettingsMainMenuData.m_MasterVolumeSlider.transform.SearchComponent<Image>("Handle").raycastTarget = true;
            m_SettingsMainMenuData.m_MasterVolumeSlider.onValueChanged.AddListener((amount) =>
            {
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.MasterVolume = amount;
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
            });

            m_SettingsMainMenuData.m_MusicVolumeSlider = m_SettingsMainMenuData.m_Root.SearchComponent<Slider>("MusicVolumeSlider");
            m_SettingsMainMenuData.m_MusicVolumeSlider.transform.SearchComponent<Image>("Handle").raycastTarget = true;
            m_SettingsMainMenuData.m_MusicVolumeSlider.onValueChanged.AddListener((amount) =>
            {
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.MusicVolume = amount;
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
            });

            m_SettingsMainMenuData.m_ChatVolumeSlider = m_SettingsMainMenuData.m_Root.SearchComponent<Slider>("ChatVolumeSlider");
            m_SettingsMainMenuData.m_ChatVolumeSlider.transform.SearchComponent<Image>("Handle").raycastTarget = true;
            m_SettingsMainMenuData.m_ChatVolumeSlider.onValueChanged.AddListener((amount) =>
            {
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.ChatVolume = amount;
                MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveData();
            });


            InitiliseToggleSwap(m_SettingsMainMenuData.m_VignetteToggle, m_SettingsMainMenuData.m_Root.SearchComponent<Toggle>("VignetteToggle"));
            m_SettingsMainMenuData.m_VignetteToggle.m_Toggle.onValueChanged.AddListener((state) =>
            {
                Debug.LogError("m_VignetteToggle");
            });

            InitiliseToggleSwap(m_SettingsMainMenuData.m_SnapsToggle, m_SettingsMainMenuData.m_Root.SearchComponent<Toggle>("SnapsToggle"));
            m_SettingsMainMenuData.m_SnapsToggle.m_Toggle.onValueChanged.AddListener((state) =>
            {
                Debug.LogError("m_SnapsToggle");
            });

            m_SettingsMainMenuData.m_CreditsButton = m_SettingsMainMenuData.m_Root.SearchComponent<Button>("CreditsButton");
            m_SettingsMainMenuData.m_CreditsButton.transform.GetComponent<Image>().raycastTarget = true;
            m_SettingsMainMenuData.m_CreditsButton.onClick.AddListener(() => ForwardToScreen(ScreenMenu.Credits));
        }

        private void InitiliseCreditsData()
        {
            m_AllScreens.Add(ScreenMenu.Credits, m_CreditsData);
            m_CreditsData.m_Root = ConsoleCanvasRoot.Search("Credits");
            AddBasics(m_CreditsData);

            m_CreditsData.m_CreditsTitleText = m_CreditsData.m_Root.SearchComponent<TextMeshProUGUI>("CreditsTitleText");
            m_CreditsData.m_CreditsTitleText.text = $"Credits  ver({Application.version})";
        }

        private void InitiliseEndGameMulitplayerMenuData()
        {
            m_AllScreens.Add(ScreenMenu.EndGameMenuMultiPlayer, m_EndGameMenuMultiPlayerData);
            m_EndGameMenuMultiPlayerData.m_Root = ConsoleCanvasEndRoot.Search("EndGameMenu");
            AddBasics(m_EndGameMenuMultiPlayerData);

            m_EndGameMenuMultiPlayerData.m_SongTitleString = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("SongTitleString");
            m_EndGameMenuMultiPlayerData.m_ArtistString = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("ArtistString");
            m_EndGameMenuMultiPlayerData.m_SongLengthString = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("SongLengthString");


            m_EndGameMenuMultiPlayerData.m_TeamOnePoints = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("Team1Score");
            m_EndGameMenuMultiPlayerData.m_TeamTwoPoints = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("Team2Score");


            m_EndGameMenuMultiPlayerData.m_PlayerName1 = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerName1");
            m_EndGameMenuMultiPlayerData.m_PlayerOneTasks = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerOneTasks");
            m_EndGameMenuMultiPlayerData.m_PlayerOnePoints = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerOnePoints");

            m_EndGameMenuMultiPlayerData.m_PlayerName2 = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerName2");
            m_EndGameMenuMultiPlayerData.m_PlayerTwoTasks = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerTwoTasks");
            m_EndGameMenuMultiPlayerData.m_PlayerTwoPoints = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerTwoPoints");

            m_EndGameMenuMultiPlayerData.m_PlayerName3 = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerName3");
            m_EndGameMenuMultiPlayerData.m_PlayerThreeTasks = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerThreeTasks");
            m_EndGameMenuMultiPlayerData.m_PlayerThreePoints = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerThreePoints");

            m_EndGameMenuMultiPlayerData.m_PlayerName4 = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerName4");
            m_EndGameMenuMultiPlayerData.m_PlayerFourTasks = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerFourTasks");
            m_EndGameMenuMultiPlayerData.m_PlayerFourPoints = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerFourPoints");


            m_EndGameMenuMultiPlayerData.m_LeaveLobby = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<Button>("LeaveLobby");
            m_EndGameMenuMultiPlayerData.m_LeaveLobby.onClick.AddListener(() =>
            {
                m_LastTeleport = null;
                Teleport(MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start);
                m_StackScreenMenu.Clear();
                ForwardToScreen(ScreenMenu.MainRootScreen);
            });

            m_EndGameMenuMultiPlayerData.m_Rematch = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<Button>("Rematch");
            m_EndGameMenuMultiPlayerData.m_Rematch.onClick.AddListener(() =>
            {
                MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.Menu;
                NetworkMessagesManager.Instance.SendMuliplayerload(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode);
                NetworkMessagesManager.Instance.SendPlayerPrefsData(ref MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef);
            });
            m_EndGameMenuMultiPlayerData.m_Continue = m_EndGameMenuMultiPlayerData.m_Root.SearchComponent<Button>("Continue");
            m_EndGameMenuMultiPlayerData.m_Continue.onClick.AddListener(() =>
            {
                NetworkMessagesManager.Instance.SendEndContinue();
                EndContinue();
            });
        }

        private void InitiliseEndGameSinglePlayerMenuData()
        {
            m_AllScreens.Add(ScreenMenu.EndGameMenuSinglePlayer, m_EndGameMenuSinglePlayerData);
            m_EndGameMenuSinglePlayerData.m_Root = ConsoleCanvasEndRoot.Search("SinglePlayer_EndGameMenu");
            AddBasics(m_EndGameMenuSinglePlayerData);

            m_EndGameMenuSinglePlayerData.m_SongTitleString = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<TextMeshProUGUI>("SongTitleString");
            m_EndGameMenuSinglePlayerData.m_ArtistString = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<TextMeshProUGUI>("ArtistString");
            m_EndGameMenuSinglePlayerData.m_SongLengthString = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<TextMeshProUGUI>("SongLengthString");

            m_EndGameMenuSinglePlayerData.m_PlayerName1 = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerName1");
            m_EndGameMenuSinglePlayerData.m_PlayerOneTasks = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerOneTasks");
            m_EndGameMenuSinglePlayerData.m_PlayerOnePoints = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<TextMeshProUGUI>("PlayerOnePoints");


            m_EndGameMenuSinglePlayerData.m_LeaveLobby = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<Button>("LeaveLobby");
            m_EndGameMenuSinglePlayerData.m_LeaveLobby.onClick.AddListener(() =>
            {
                m_LastTeleport = null;
                Teleport(MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start);
                m_StackScreenMenu.Clear();
                ForwardToScreen(ScreenMenu.MainRootScreen);
            });
            m_EndGameMenuSinglePlayerData.m_Rematch = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<Button>("Rematch");
            m_EndGameMenuSinglePlayerData.m_Rematch.onClick.AddListener(() =>
            {
                MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.Menu;
                SongAndDataLoadedAndReadyToStart(() =>
                {
                    InternalReceiveStartSession();
                });

            });

            m_EndGameMenuSinglePlayerData.m_Continue = m_EndGameMenuSinglePlayerData.m_Root.SearchComponent<Button>("Continue");
            m_EndGameMenuSinglePlayerData.m_Continue.onClick.AddListener(() =>
            {
                m_LastTeleport = null;
                Teleport(MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start);
                m_StackScreenMenu.Clear();
                ForwardToScreen(ScreenMenu.SoloPlayer);
            });
        }

        private void InitiliseLoadingData()
        {
            m_AllScreens.Add(ScreenMenu.Loading, m_LoadingDataData);
            m_LoadingDataData.m_Root = ConsoleCanvasRoot.Search("Loading");
            AddBasics(m_LoadingDataData);
        }

        private void InitiliseNetworkErrorData()
        {
            m_AllScreens.Add(ScreenMenu.NetworkErrorScreen, m_NetworkErrorData);
            m_NetworkErrorData.m_Root = ConsoleCanvasRoot.Search("ErrorPage");
            
            if (m_NetworkErrorData.m_Root != null)
            {
                AddBasics(m_NetworkErrorData);
                m_NetworkErrorData.m_Continue = m_NetworkErrorData.m_Root.SearchComponent<Button>("ReturnToMenu");
                m_NetworkErrorData.m_Continue.GetComponent<Image>().raycastTarget = true;
                m_NetworkErrorData.m_Continue.onClick.AddListener(() =>
                {
                    m_StackScreenMenu.Clear();
                    ForwardToScreen(ScreenMenu.MainRootScreen);
                });
            }
            else
            {
                DebugBeep.LogError($"Screen needs to be added", DebugBeep.MessageLevel.High);
            }
        }

        #endregion   Initilise



        private void UpdateLobbyDisplayRooms(List<RoomInfo> rooms)
        {
            ////m_MultiplayerLobby.m_RoomInfos = rooms;
            ////m_MultiplayerLobby.m_ButtonPool.DeSpawnAll();
            ////foreach (var item in rooms)
            ////{
            ////    RoomInfo localInfo = item;
            ////    var obj = m_MultiplayerLobby.m_ButtonPool.SpawnObject();
            ////    obj.gameObject.transform.ClearLocals();
            ////    var text = obj.GetComponentInChildren<TextMeshProUGUI>();
            ////    text.text = $"Room {localInfo.PlayerCount}/{localInfo.MaxPlayers}";
            ////    obj.name = item.Name;
            ////    obj.onClick.RemoveAllListeners();
            ////    obj.onClick.AddListener(() =>
            ////    {
            ////        Core.PhotonMultiplayerRef.ChangeRoom($"{obj.name}");
            ////    });
            ////}
        }


        private void InternalReceiveStartSession()
        {
            var currentPlayersEnum = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum;
            TaskBarManager.Instance.SetVisible(true);
            MuliplayerData currentPlayerData = MonitorTrainerConsts.MULIPLYER_DATA[currentPlayersEnum];


            PhysicalConsole.Instance.TurnOnCorrectInteractive(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum);

            var found = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.ActorNumber);
            if (found != null)
            {
                found = MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef;
            }
            else
            {
                MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Add(MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef);
            }
            TurnOnPlayerDecal(currentPlayersEnum);

            MonitorTrainerRoot.Instance.transform.position = currentPlayerData.GUIStartPosition;
            var lapTopGui = MonitorTrainerRoot.Instance.transform.SearchComponent<Transform>(currentPlayerData.GuiLaptopHolderName);


            lapTopGui.localPosition = currentPlayerData.GUILapTopPosition;
            lapTopGui.localRotation = currentPlayerData.GUILapTopRotation;

            OppositeSide.Instance.transform.position = currentPlayerData.RootGameobjectPosition;
            OppositeSide.Instance.transform.rotation = currentPlayerData.RootGameobjectRotation;

            PhysicalConsole.Instance.transform.position = currentPlayerData.RootGameobjectPosition;
            PhysicalConsole.Instance.transform.rotation = currentPlayerData.RootGameobjectRotation;


            var itemFound = Core.Scene.GetSpawnedVrInteraction(currentPlayerData.ConsoleName, currentPlayerData.RootName);
            MonitorTrainerRoot.Instance.transform.SetParent(itemFound.transform);
            MonitorTrainerRoot.Instance.transform.ClearLocals();
            MonitorTrainerRoot.Instance.transform.localPosition = currentPlayerData.GUIStartPosition;
            MonitorTrainerRoot.Instance.transform.localRotation = currentPlayerData.GUIStartRotation;
            MonitorTrainerRoot.Instance.transform.SetParent(null);

            m_GuiEndHolder.transform.SetParent(MonitorTrainerRoot.Instance.ConsoleCanvasRoot);
            m_GuiEndHolder.transform.ClearPosRotLocals();
            m_GuiEndHolder.transform.position += -(m_GuiEndHolder.transform.forward * 0.01f);
            m_GuiEndHolder.transform.SetParent(null);


            Debug.LogError($"current player {currentPlayersEnum}, pos {currentPlayerData.GUIStartPosition.ToAccurateString()} , console name {itemFound}");


            // move phone manager to correct position
            var phone = Core.Scene.GetSpawnedVrInteraction(currentPlayerData.PhoneName);
            PhoneManager.Instance.transform.position = phone.transform.position;
            PhoneManager.Instance.transform.rotation = phone.transform.rotation;
            PhoneManager.Instance.SetPhone(phone.gameObject);

            ResetAllMovableObjects();

            // the m_Amp is set to 2d sound
            // search for it and will find
            SoundMixer.Instance.m_Amp.transform.position = currentPlayerData.StartPosition + new Vector3(0f, 3.5f, 0);
            //SoundMixer.Instance.m_Amp.transform.ClearLocals();

            PhysicalAmp.Instance.Setup(Core.Scene.GetSpawnedVrInteraction(currentPlayerData.AmpName));

            PhysicalAmp.Instance.SetupUSB(Core.Scene.GetSpawnedVrInteraction(currentPlayerData.USbName));


            foreach (var item in MonitorTrainerConsts.MULIPLYER_DATA)
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>(item.Value.TeleportName).SetActive(false);
            }

            PhysicalConsole.Instance.transform.SearchComponent<Transform>(currentPlayerData.TeleportName).SetActive(true);

            switch (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty)
            {
                case DifficultyModeEnum.Easy:
                case DifficultyModeEnum.Medium:
                case DifficultyModeEnum.Hard:
                    MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.Stackable;
                    MonitorTrainerRoot.Instance.ResetConsoleButtonStates();
                    break;
                default:
                    break;
            }

            // TOM: Add any setup for specific scenes here
            //MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum
            CameraControllerVR.Instance.TeleportAvatar(PhysicalConsole.Instance.gameObject.scene, currentPlayerData.StartPosition, null);

        }


        private void ResetAllMovableObjects()
        {
            var allItems = Core.Scene.GetAllSpawnedVrInteraction();
            foreach (var item in allItems)
            {
                if (item != null)
                {
                    item.ResetToOriginalState();
                    if (item is VrInteractionPickUp)
                    {
                        ((VrInteractionPickUp)item).SetVolumePercenatgeOfOriginal(DROP_VOLUME);
                    }
                    item.transform.SetParent(null); // so opposite side can be fixed
                }
            }
        }

        private void BasicNetworkMessages()
        {
            InitiliseLobbyAndPlayersCallbacks();


            NetworkMessagesManager.Instance.ReceiveMuliplayerload((roomNumber) =>
            {
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode == roomNumber)
                {
                    Debug.LogError($"ReceiveMuliplayerload {roomNumber} ");
                    ////NetworkMessagesManager.Instance.SendPrimaryColour(Core.PhotonMultiplayerRef.MySelf.ActorNumber, PlayerPrefsData.GetPlayerColour(PlayerPrefsData.PrimaryEnum.Primary));
                    ////NetworkMessagesManager.Instance.SendSecondaryColour(Core.PhotonMultiplayerRef.MySelf.ActorNumber, PlayerPrefsData.GetPlayerColour(PlayerPrefsData.PrimaryEnum.Secondary));
                    NetworkMessagesManager.Instance.SendPlayerPrefsData(ref MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef);


                    SongAndDataLoadedAndReadyToStart(() =>
                    {
                        MenuManager.PlayerReady ready = new PlayerReady();
                        ready.m_IsReady = true;
                        ready.m_LobbyCode = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode;
                        ready.m_OwnerActorNr = Core.PhotonMultiplayerRef.PhotonViewOwnerRef.OwnerActorNr;
                        ready.m_FullyLoaded = true;
                        NetworkMessagesManager.Instance.SendSongFullyLoaded(ready);
                    });
                }
            });


            // this also updated the scores
            NetworkMessagesManager.Instance.ReceivePlayerPrefsData((data) =>
            {
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode == data.LobbyCode)
                {
                    {
                        var found = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.ActorNumber == data.ActorNumber);
                        if (found == null)
                        {
                            MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Add(data);
                        }

                        TurnOnPlayerDecal((PlayersEnum)data.CurrentPlayersEnum);
                        MonitorTrainerRoot.Instance.LaptopMenuManagerRuntimeRef.UpdateRunTimeData();
                        LaptopMenuManagerRef.UpdateRunTimeData();
                    }
                }
            });


            NetworkMessagesManager.Instance.ReceiveStartSession(() =>
            {
                InternalReceiveStartSession();
            });

            NetworkMessagesManager.Instance.ReceiveAskRoomDetails(() =>
            {
                NetworkMessagesManager.Instance.SendFullRoomDetails(MonitorTrainerRoot.Instance.PlayerChoiceDataRef);
            });

            NetworkMessagesManager.Instance.ReceiveFullRoomDetails((data) =>
            {
                SetMultiplayerLobbyGuestData(data);
            });
        }

        private void SetMultiplayerLobbyGuestData(PlayerChoiceData received)
        {
            if (m_CurrentScreenMenu == ScreenMenu.MultiplayerLobbyGuest)
            {
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode == received.LobbyCode)
                {
                    Debug.LogError($"SetMultiplayerLobbyGuestData {received.CurrentSongChoiceName}");
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty = received.CurrentDifficulty;
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName = received.CurrentSongChoiceName;
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef = AllSongData.Find(e => e.SongName == MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName);
                    if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef == null)
                    {
                        Debug.LogError($"MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef  is null, cannot find CurrentSongChoiceName : {MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName}");
                    }
                    MenuManager.Instance.LaptopMenuManagerRef.UpdateMenuData();

                    m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Easy.SetActive(false);
                    m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Medium.SetActive(false);
                    m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Hard.SetActive(false);

                    m_MultiplayerLobbyGuestData.m_LobbyCode.text = received.LobbyCode;

                    switch (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty)
                    {
                        case DifficultyModeEnum.Easy:
                            m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Easy.SetActive(true);
                            break;
                        case DifficultyModeEnum.Medium:
                            m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Medium.SetActive(true);
                            break;
                        case DifficultyModeEnum.Hard:
                            m_MultiplayerLobbyGuestData.m_LobbyDifficultyIcon_Hard.SetActive(true);
                            break;
                        default:
                            break;
                    }

                    if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef != null && MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef != null)
                    {
                        TimeSpan time = TimeSpan.FromSeconds(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongLength);
                        m_MultiplayerLobbyGuestData.m_SongLength.text = time.ToString("mm':'ss");

                        m_MultiplayerLobbyGuestData.m_ArtistName.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.ArtistName;
                        m_MultiplayerLobbyGuestData.m_SongTitle.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongName;
                    }
                    else
                    {
                        Debug.LogError("MonitorTrainerRoot.Instance.PlayerChoiceDataRef is NULL");
                    }
                }
            }
        }

        #region ScreenChanges

        private void BackPressed()
        {
            if (m_StackScreenMenu.Count >= 2)
            {
                m_StackScreenMenu.Pop();
                var newState = m_StackScreenMenu.Peek();
                ForwardToScreen(newState, backPressed: true);
            }
        }


        public void ForwardToScreen(ScreenMenu menuEnum, bool backPressed = false)
        {
            if (m_CurrentScreenMenu != menuEnum)
            {
                menuEnum = ChangeToScreen(menuEnum, backPressed);
                // adds it if not same screen 
                if (m_StackScreenMenu.Count == 0 || m_StackScreenMenu.Peek() != menuEnum)
                {
                    m_StackScreenMenu.Push(menuEnum);
                }
            }
            m_CurrentScreenMenu = menuEnum;
        }


        private ScreenMenu ChangeToScreen(ScreenMenu menuEnum, bool backPressed = false)
        {
            this.transform.SetActive(true);
            foreach (var item in m_AllScreens)
            {
                if (item.Value.m_Root != null)
                {
                    item.Value.m_Root.SetActive(false);
                }
            }
            ConsoleExtra.Log($"ChangeToScreen: {menuEnum}", null, ConsoleExtraEnum.EDebugType.DebugLogging);
            switch (menuEnum)
            {
                case ScreenMenu.MainRootScreen:
                case ScreenMenu.Customise:
                case ScreenMenu.Challenges:
                case ScreenMenu.SettingsMainMenu:
                    if (Core.PhotonMultiplayerRef.IsOwnerInRoom == true)
                    {
                        m_Destination = menuEnum;
                        menuEnum = ScreenMenu.LeaveLobby;
                        Debug.LogError($"ChangeToScreen Force: {menuEnum}");
                    }
                    else
                    {
                        m_StackScreenMenu.Clear();
                    }
                    break;
            }

            if (m_AllScreens[menuEnum].m_Root != null)
            {
                m_AllScreens[menuEnum].m_Root.SetActive(true);
            }


            if (backPressed == false)
            {
                switch (menuEnum)
                {
                    case ScreenMenu.MainRootScreen:
                        MonitorTrainerRoot.Instance.MultiPlayerDataRef.ClearAll();
                        UpdateMainRootScreenChallenges();
                        PlatformManager.Achievements.CheckForAchievmentUpdates();
                        ShowCurrentSinglePlayer();
                        break;
                    case ScreenMenu.TutorialPage:
                        ShowCurrentSinglePlayer();
                        break;
                    case ScreenMenu.SoloPlayer:
                        // for testing ONLY //MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum = PlayersEnum.Player4;
                        m_SoloPlayerData.m_Easy.isOn = true;
                        m_SoloPlayerData.m_VerticalData.m_TogglePool.GetPublicList()[0].onValueChanged.Invoke(true);
                        ShowCurrentSinglePlayer();
                        break;
                    case ScreenMenu.MultiplayerChoice:
                        Core.PhotonMultiplayerRef.JoinLobby();
                        break;
                    case ScreenMenu.LobbyCode:
                        m_LobbyCodeData.m_LobbyCodeText.text = "";
                        ShowCurrentSinglePlayer();
                        break;
                    case ScreenMenu.MultiplayerLobbyHost:
                        // for testing ONLY //MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum = PlayersEnum.Player1;
                        MonitorTrainerRoot.Instance.MultiPlayerDataRef.ClearAll();
                        m_MultiplayerLobbyHostData.m_Easy.isOn = true;
                        m_MultiplayerLobbyHostData.m_VerticalData.m_TogglePool.GetPublicList()[0].onValueChanged.Invoke(true);
                        m_MultiplayerLobbyHostData.m_LobbyCodeString.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode;
                        m_MultiplayerLobbyHostData.m_PlayerData.ForEach(e => e.m_PlayerName.text = "");
                        m_MultiplayerLobbyHostData.m_PlayerData.ForEach(e => e.m_ActorNumber = -1);
                        m_MultiplayerLobbyHostData.m_PlayerData.ForEach(e => e.m_PhotonPlayer = null);
                        CurrentRoomCallbacks(PhotonNetwork.PlayerList.ToList());

                        this.WaitUntil(1, () => Core.PhotonMultiplayerRef.CurrentRoom != null, () =>
                        {
                            Core.PhotonMultiplayerRef.CurrentRoom.IsOpen = true;
                        });
                        ShowCurrentSinglePlayer();
                        break;
                    case ScreenMenu.MultiplayerLobbyGuest:
                        MonitorTrainerRoot.Instance.MultiPlayerDataRef.ClearAll();
                        m_MultiplayerLobbyGuestData.m_PlayerData.ForEach(e => e.m_PlayerName.text = "");
                        m_MultiplayerLobbyGuestData.m_PlayerData.ForEach(e => e.m_ActorNumber = -1);
                        m_MultiplayerLobbyGuestData.m_PlayerData.ForEach(e => e.m_PhotonPlayer = null);
                        SetCorrectPlayerIndex();

                        Teleport(MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start);

                        CurrentRoomCallbacks(PhotonNetwork.PlayerList.ToList());
                        m_MultiplayerLobbyGuestData.m_MarkReady.SetIsOnWithoutNotify(false);
                        break;
                    case ScreenMenu.LeaveLobby:
                        break;
                    case ScreenMenu.Customise:
                        break;
                    case ScreenMenu.AddDecals:
                        break;
                    case ScreenMenu.CustomisePod:
                        break;
                    case ScreenMenu.AlterMaterials:
                        break;
                    case ScreenMenu.TierProgression:
                        break;
                    case ScreenMenu.Challenges:
                        PlatformManager.Achievements.CheckForAchievmentUpdates();
                        break;
                    case ScreenMenu.SettingsMainMenu:
                        break;
                    case ScreenMenu.Credits:
                        break;
                    case ScreenMenu.EndGameMenuMultiPlayer:
                        m_EndGameMenuMultiPlayerData.m_SongTitleString.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongName;
                        m_EndGameMenuMultiPlayerData.m_ArtistString.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.ArtistName;

                        TimeSpan time = TimeSpan.FromSeconds(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongLength);
                        m_EndGameMenuMultiPlayerData.m_SongLengthString.text = time.ToString("mm':'ss");

                        var data1 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum == PlayersEnum.Player1);
                        if (data1 != null)
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName1.text = data1.Nickname;
                            m_EndGameMenuMultiPlayerData.m_PlayerOneTasks.text = data1.TasksCompleted.ToString();
                            m_EndGameMenuMultiPlayerData.m_PlayerOnePoints.text = data1.XP.ToString();
                        }
                        else
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName1.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerOneTasks.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerOnePoints.text = "";
                        }

                        var data2 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum == PlayersEnum.Player2);
                        if (data2 != null)
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName2.text = data2.Nickname;
                            m_EndGameMenuMultiPlayerData.m_PlayerTwoTasks.text = data2.TasksCompleted.ToString();
                            m_EndGameMenuMultiPlayerData.m_PlayerTwoPoints.text = data2.XP.ToString();
                        }
                        else
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName2.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerTwoTasks.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerTwoPoints.text = "";
                        }

                        var data3 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum == PlayersEnum.Player3);
                        if (data3 != null)
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName3.text = data3.Nickname;
                            m_EndGameMenuMultiPlayerData.m_PlayerThreeTasks.text = data3.TasksCompleted.ToString();
                            m_EndGameMenuMultiPlayerData.m_PlayerThreePoints.text = data3.XP.ToString();
                        }
                        else
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName3.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerThreeTasks.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerThreePoints.text = "";
                        }

                        var data4 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum == PlayersEnum.Player3);
                        if (data4 != null)
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName4.text = data4.Nickname;
                            m_EndGameMenuMultiPlayerData.m_PlayerFourTasks.text = data4.TasksCompleted.ToString();
                            m_EndGameMenuMultiPlayerData.m_PlayerFourPoints.text = data4.XP.ToString();

                        }
                        else
                        {
                            m_EndGameMenuMultiPlayerData.m_PlayerName4.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerFourTasks.text = "";
                            m_EndGameMenuMultiPlayerData.m_PlayerFourPoints.text = "";
                        }
                        m_EndGameMenuMultiPlayerData.m_Rematch.SetActive(Core.PhotonMultiplayerRef.IsOwner);
                        break;

                    case ScreenMenu.EndGameMenuSinglePlayer:
                        m_EndGameMenuSinglePlayerData.m_SongTitleString.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongName;
                        m_EndGameMenuSinglePlayerData.m_ArtistString.text = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.ArtistName;

                        TimeSpan singleTime = TimeSpan.FromSeconds(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongLength);
                        m_EndGameMenuSinglePlayerData.m_SongLengthString.text = singleTime.ToString("mm':'ss");
                        m_EndGameMenuSinglePlayerData.m_PlayerName1.text = Core.PhotonMultiplayerRef.MySelf.NickName;
                        var single = MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef;
                        m_EndGameMenuSinglePlayerData.m_PlayerOneTasks.text = single.TasksCompleted.ToString();
                        m_EndGameMenuSinglePlayerData.m_PlayerOnePoints.text = single.XP.ToString();

                        break;

                    case ScreenMenu.Loading:
                        MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.XP = 0;
                        break;
                    default:
                        break;
                }
            }
            return menuEnum;
        }

        private void ShowCurrentSinglePlayer()
        {
            var CurrentSinglePlayer= MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum;
            //SetCorrectPlayerIndex();
            Core.Mono.WaitUntil(1, () => MenuManager.Instance.m_MenuArea.ContainsKey(PlayersEnum.Player4), () =>
           {
               if (m_LocalLapTopGUIPosition == null)
               {
                   var laptopParent = m_LaptopGUIHolder.transform.parent;
                   m_LaptopGUIHolder.transform.SetParent(MenuManager.Instance.m_MenuArea[CurrentSinglePlayer].m_LapTop);
                   m_LocalLapTopGUIPosition = m_LaptopGUIHolder.transform.localPosition;
                   m_LocalLapTopGUIRotation = m_LaptopGUIHolder.transform.localRotation;
                   m_LaptopGUIHolder.SetParent(laptopParent);

                   var lconsoleParent = m_GuiHolder.transform.parent;
                   m_GuiHolder.transform.SetParent(MenuManager.Instance.m_MenuArea[CurrentSinglePlayer].m_Console);
                   m_LocalMainGUIPosition = m_GuiHolder.transform.localPosition;
                   m_LocalMainGUIRotation = m_GuiHolder.transform.localRotation;
                   m_LaptopGUIHolder.SetParent(lconsoleParent);
               }

               MenuManager.Instance.m_MenuArea[PlayersEnum.Player1].TurnOn(false);
               MenuManager.Instance.m_MenuArea[PlayersEnum.Player2].TurnOn(false);
               MenuManager.Instance.m_MenuArea[PlayersEnum.Player3].TurnOn(false);
               MenuManager.Instance.m_MenuArea[PlayersEnum.Player4].TurnOn(false);

               MenuManager.Instance.m_MenuArea[CurrentSinglePlayer].TurnOn(true);
               Teleport(MenuManager.Instance.m_MenuArea[CurrentSinglePlayer].m_Start);
           });
        }

        private Transform m_LastTeleport = null;
        #endregion ScreenChanges


        private void InitiliseLobbyAndPlayersCallbacks()
        {
            NetworkMessagesManager.Instance.PhotonMultiplayerRef.OnLobbyListChanged((allRooms, useable) => LobbyRoomsCallbacks(allRooms, useable));
            NetworkMessagesManager.Instance.PhotonMultiplayerRef.OnRoomPlayersChanged((players) => CurrentRoomCallbacks(players));
            NetworkMessagesManager.Instance.PhotonMultiplayerRef.OnFail((fail) => FailCallbacks(fail));
        }


        private void FailCallbacks(PhotonMultiplayer.NetworkFail fail)
        {
            m_StackScreenMenu.Clear();
            Debug.LogError($"FailCallbacks: {fail}");
            ForwardToScreen(ScreenMenu.NetworkErrorScreen);
        }

        private void LobbyRoomsCallbacks(List<RoomInfo> allRooms, List<RoomInfo> useableRooms)
        {
            Debug.LogError($"LobbyRoomsCallbacks count :{useableRooms.Count}");
            for (int i = 0; i < useableRooms.Count; i++)
            {
                Debug.LogError($"room name :{useableRooms[i].Name}, PlayerCount : {useableRooms[i].PlayerCount} ");
            }
            m_CurrentLobbyRooms = useableRooms;
            m_CurrentNotPrivateLobbyRooms.Clear();
            foreach (var item in m_CurrentLobbyRooms)
            {
                if (item.CustomProperties != null && item.CustomProperties.ContainsKey(PRIVATE_ROOM) == true)
                {
                    bool isPrivate = (bool)item.CustomProperties[PRIVATE_ROOM];
                    if (isPrivate == false)
                    {
                        m_CurrentNotPrivateLobbyRooms.Add(item);
                    }
                }
                else
                {
                    m_CurrentNotPrivateLobbyRooms.Add(item);
                }
            }
            CheckLobbyCode();
        }

        private void CurrentRoomCallbacks(List<Player> players)
        {
            SetCorrectPlayerIndex();
            m_CurrentRoomPlayers = players;
            List<Player> toAdd = new List<Player>();

            InternalAssignData(m_MultiplayerLobbyHostData);
            InternalAssignData(m_MultiplayerLobbyGuestData);

            void InternalAssignData(HostGuest hostGuest)
            {
                List<int> actorNumbers = new List<int>();
                foreach (var item in players)
                {
                    actorNumbers.Add(item.ActorNumber);
                }
                actorNumbers.Sort();

                for (int i = 0; i < MAX_PLAYERS; i++)
                {
                    hostGuest.m_PlayerData[i].m_PlayerName.text = "";
                    hostGuest.m_PlayerData[i].m_ActorNumber = -1;
                    hostGuest.m_PlayerData[i].m_FullyLoaded = false;
                    hostGuest.m_PlayerData[i].m_PlayerReadyIcon.SetActive(false);
                    hostGuest.m_PlayerData[i].m_PhotonPlayer = null;
                }


                for (int i = 0; i < actorNumbers.Count; i++)
                {
                    var player = players.Find(e => e.ActorNumber == actorNumbers[i]);
                    if (player != null)
                    {
                        hostGuest.m_PlayerData[i].m_PhotonPlayer = players[i];
                        hostGuest.m_PlayerData[i].m_PlayerName.text = players[i].NickName;
                        hostGuest.m_PlayerData[i].m_ActorNumber = players[i].ActorNumber;
                    }
                }
            }


            if (m_CurrentScreenMenu == ScreenMenu.MultiplayerLobbyGuest)
            {
                NetworkMessagesManager.Instance.SendAskRoomDetails(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.LobbyCode);
            }
            CheckLobbyCode();
        }



        private void FillInSongList(VerticalData verticalData, Action LoadedAll = null)
        {
            // loop through song list 
            Core.AssetBundlesRef.TextAssetBundleRef.GetItemList(Core.Mono, MonitorTrainerConsts.SONG_DATA_JSON, (dataList) =>
            {

                AllSongData = new List<SongData>();
                AllSongData.Clear();


                string highScore = "highScore List:\n";
                TaskAction task = new TaskAction(dataList.Count, () =>
                {
                    LoadedAll?.Invoke();
                });

                foreach (var item in dataList)
                {
                    Core.AssetBundlesRef.TextAssetBundleRef.GetItem(Core.Mono, item, (data) =>
                    {
                        task.Increment();
                        if (data != null)
                        {
                            SongData songData = Json.JsonNet.ReadFromText<SongData>(data.text);

                            AllSongData.Add(songData);

                            string spawnString = songData.SongName;

                            // assign name
                            var spawnItem = verticalData.m_TogglePool.SpawnObject();
                            spawnItem.name = spawnString;
                            spawnItem.transform.SearchComponent<TMPro.TextMeshProUGUI>("SongTitleText").text = songData.SongName;
                            spawnItem.transform.SearchComponent<TMPro.TextMeshProUGUI>("ArtistNameText").text = songData.ArtistName;
                            TimeSpan time = TimeSpan.FromSeconds(songData.SongLength);
                            spawnItem.transform.SearchComponent<TMPro.TextMeshProUGUI>("SongLengthText").text = time.ToString("mm':'ss");
                            spawnItem.transform.ClearLocals();
                            // click item
                            spawnItem.onValueChanged.AddListener((state) =>
                            {
                                if (state == true)
                                {
                                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName = spawnString;
                                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef = AllSongData.Find(e => e.SongName == MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName);
                                    MenuManager.Instance.LaptopMenuManagerRef.UpdateMenuData();
                                }
                            });
                        }
                    });
                };
            });
        }

        private void FillInHorizontalDecalList(HorizontalData verticalData, string startsWith, Action<string> itemChoice)
        {
            var nonSpawn = verticalData.m_TogglePool.SpawnObject();
            nonSpawn.onValueChanged.RemoveAllListeners();
            nonSpawn.onValueChanged.AddListener((clicked) =>
            {
                if (clicked == true)
                {
                    nonSpawn.name = "None";
                    var text = nonSpawn.transform.SearchComponent<TextMeshProUGUI>("ItemTextString");
                    text.text = nonSpawn.name;

                    itemChoice?.Invoke(nonSpawn.name);
                }
            });


            var children = m_MenuDecals;
            var valid = children.FindAll(e => e.name.CaseInsensitiveContains(startsWith));

            List<string> names = new List<string>();
            foreach (var item in valid)
            {
                if (names.Contains(item.name) == true)
                {
                    Debug.LogError($"Duplicate name in {startsWith}  and its : {item.name}");
                }
                names.Add(item.name);
            }

            foreach (var item in valid)
            {
                var localItem = item;
                var spawnItem = verticalData.m_TogglePool.SpawnObject();
                var back = spawnItem.transform.SearchComponent<Image>("Background");
                var image = spawnItem.transform.SearchComponent<Image>("Image");
                var text = spawnItem.transform.SearchComponent<TextMeshProUGUI>("ItemTextString");
                text.text = item.name;
                spawnItem.name = item.name;

                spawnItem.onValueChanged.RemoveAllListeners();
                spawnItem.onValueChanged.AddListener((clicked) =>
                {
                    if (clicked == true)
                    {
                        itemChoice?.Invoke(text.text);
                    }
                });
            }

        }



        private void FillInColourList()
        {
            //////var allButtons = m_ColourChoice.m_Root.GetComponentsInChildren<Button>().ToList();
            //////foreach (var item in allButtons)
            //////{
            //////    if (item.name.StartsWith("Primary") || item.name.StartsWith("Secondary"))
            //////    {
            //////        ColourChoiceData.IndidualColourChoiceData data = new ColourChoiceData.IndidualColourChoiceData();
            //////        data.m_Button = item;
            //////        data.m_Color = item.GetComponent<Image>().color;
            //////        if (item.name.StartsWith("Primary"))
            //////        {
            //////            data.m_PrimaryEnum = PlayerColourData.PrimaryEnum.Primary;
            //////        }
            //////        else
            //////        {
            //////            data.m_PrimaryEnum = PlayerColourData.PrimaryEnum.Secondary;
            //////        }
            //////        m_ColourChoice.m_Buttons.Add(data);
            //////        data.m_Button.onClick.AddListener(() =>
            //////        {
            //////            MonitorTrainerRoot.Instance.PlayerDataRef.ChangeColour(PlayerChoiceDataRef.CurrentPlayersEnum, data.m_PrimaryEnum, data.m_Color);
            //////            MenuManager.Instance.PlayerDataRef.ChangeColour(PlayersEnum.Player1, data.m_PrimaryEnum, data.m_Color);
            //////        });
            //////    }
            //////}
        }

        private void SongAndDataLoadedAndReadyToStart(Action callback)
        {
            if (string.IsNullOrEmpty(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName) == false)
            {
                ForwardToScreen(ScreenMenu.Loading);

                TaskAction task = new TaskAction(3, () =>
                {
                    // makes sure it all loaded
                    ConsoleData.Instance.InitialiseSongCreateSongInputData();//
                    ConsoleScreenManager.Instance.InitialiseSongData();

                    callback?.Invoke();
                });

                SetCorrectPlayerIndex();

                SoundMixer.Instance.InitialiseSongLoadAudio(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName, task.Increment);
                BandManager.Instance.InitialiseSongLoadBandTiming(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName, task.Increment);
                PhoneManager.Instance.InitialiseSongLoadMessage(MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentSongChoiceName, task.Increment);
                ///MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Count
                Debug.LogError("Set up all MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef here");

            }
            else
            {
                ForwardToScreen(ScreenMenu.Loading);
            }
        }


        private void SetCorrectPlayerIndex()
        {
            if (Core.PhotonMultiplayerRef.CurrentRoom != null)
            {
                if (CurrentScreen == ScreenMenu.MultiplayerLobbyHost)
                {
                    if (Core.PhotonMultiplayerRef.IsOwner == false)
                    {
                        m_StackScreenMenu.Clear();
                        ChangeToScreen(ScreenMenu.MultiplayerLobbyGuest);
                        MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.MultiplayerGuest;
                        return;
                    }
                }
                else if (CurrentScreen == ScreenMenu.MultiplayerLobbyGuest)
                {
                    if (Core.PhotonMultiplayerRef.IsOwner == true)
                    {
                        m_StackScreenMenu.Clear();
                        ChangeToScreen(ScreenMenu.MultiplayerLobbyHost);
                        MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle = PlayStyleEnum.MultiplayerHost;
                        return;
                    }
                }
            }

            switch (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle)
            {
                case PlayStyleEnum.Solo:
                    MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum = PlayersEnum.Player1;
                    ShowCurrentSinglePlayer();
                    break;
                case PlayStyleEnum.MultiplayerGuest:
                case PlayStyleEnum.MultiplayerHost:
                    ShowCurrentSinglePlayer();

                    if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle == PlayStyleEnum.MultiplayerHost)
                    {
                        MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum = PlayersEnum.Player1;
                    }
                    ShowCurrentSinglePlayer();
                    Debug.LogError($"PlayerChoiceDataRef.CurrentPlayersEnum {MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum}");
                    break;
                default:
                    break;
            }
        }

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.SongFinishedCompleted:
                    m_StackScreenMenu.Clear();
                    if (Core.PhotonMultiplayerRef.IsOwnerInRoom == false)
                    {
                        ForwardToScreen(ScreenMenu.EndGameMenuSinglePlayer);
                    }
                    else
                    {
                        ForwardToScreen(ScreenMenu.EndGameMenuMultiPlayer);
                    }
                    break;
                case ScenarioEnum.Stackable:
                    LaptopMenuManagerRef.UpdateMaxXP();
                    MonitorTrainerRoot.Instance.LaptopMenuManagerRuntimeRef.UpdateMaxXP();
                    this.SetActive(false);
                    break;
                case ScenarioEnum.Menu:
                    this.SetActive(true);
                    m_StackScreenMenu.Clear();
                    ForwardToScreen(ScreenMenu.MainRootScreen);
                    break;
            }
            IsPaused = false;
        }


        private void TurnOnMenuToggles()
        {
            MonitorTrainerRoot.Instance.LocalPlayerDataRef.ReadData();
            var data = MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef;
            var monitor = m_AddDecalsData.m_SelectableItems_Monitor.m_TogglePool.GetPublicList().Find(e => e.name == MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.Monitor);
            if (monitor != null)
            {
                monitor.SetIsOnWithoutNotify(true);
            }

            var flight = m_AddDecalsData.m_SelectableItems_FlightCases.m_TogglePool.GetPublicList().Find(e => e.name == MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.FlightCase);
            if (flight != null)
            {
                flight.SetIsOnWithoutNotify(true);
            }

            var laptop = m_AddDecalsData.m_SelectableItems_Laptop.m_TogglePool.GetPublicList().Find(e => e.name == MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.LapTop);
            if (laptop != null)
            {
                laptop.SetIsOnWithoutNotify(true);
            }

            var lamp = m_AddDecalsData.m_SelectableItems_Lamp.m_TogglePool.GetPublicList().Find(e => e.name == MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.Lamp);
            if (lamp != null)
            {
                lamp.SetIsOnWithoutNotify(true);
            }
        }
        private void TurnOnMenuDecals()
        {
            MonitorTrainerRoot.Instance.LocalPlayerDataRef.ReadData();
            var data = MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef;
            m_MenuDecals.ForEach(e => e.SetActive(false));
            var monitor = m_MenuDecals.Find(e => e.name == data.SendDataRef.Monitor);
            if (monitor != null)
            {
                monitor.SetActive(true);
            }

            var flightCase = m_MenuDecals.Find(e => e.name == data.SendDataRef.FlightCase);
            if (flightCase != null)
            {
                flightCase.SetActive(true);
            }

            var lapTop = m_MenuDecals.Find(e => e.name == data.SendDataRef.LapTop);
            if (lapTop != null)
            {
                lapTop.SetActive(true);
            }

            var lamp = m_MenuDecals.Find(e => e.name == data.SendDataRef.Lamp);
            if (lamp != null)
            {
                lamp.SetActive(true);
            }
        }

        private void TurnOnPlayerDecal(PlayersEnum player)
        {
            var decals = MonitorTrainerConsts.MULIPLYER_DATA[player].DecalList;
            decals.ForEach(e => e.SetActive(false));

            var data = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum == player);
            var monitor = decals.Find(e => e.name == data.Monitor);
            if (monitor != null)
            {
                monitor.SetActive(true);
            }

            var flightCase = decals.Find(e => e.name == data.FlightCase);
            if (flightCase != null)
            {
                flightCase.SetActive(true);
            }

            var lapTop = decals.Find(e => e.name == data.LapTop);
            if (lapTop != null)
            {
                lapTop.SetActive(true);
            }

            var lamp = decals.Find(e => e.name == data.Lamp);
            if (lamp != null)
            {
                lamp.SetActive(true);
            }
        }

    }
}
