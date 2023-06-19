using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static MonitorTrainer.MonitorTrainerConsts;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class MonitorTrainerRoot : MonoBehaviour
    {
        public static MonitorTrainerRoot Instance;


        private DifficultyModeEnum DifficultyMode = DifficultyModeEnum.Easy;

        private ScenarioEnum m_CurrentScenario = ScenarioEnum.Blank;

        public PlayerPos PlayerPosRef = PlayerPos.PlayArea;
        public PlayerChoiceData PlayerChoiceDataRef = new PlayerChoiceData();
        public LocalPlayerData LocalPlayerDataRef = new LocalPlayerData();
        public MultiPlayerData MultiPlayerDataRef = new MultiPlayerData();

        public ScenarioEnum CurrentScenario
        {
            get { return m_CurrentScenario; }
            set
            {
                if (m_CurrentScenario != value)
                {
                    Debug.Log($"Scenario changed from {m_CurrentScenario} to {value}");
                    m_CurrentScenario = value;
                    TaskManager.Instance.ChangeToScenario(CurrentScenario, DifficultyMode);
                    MusicSoundManager.Instance.ChangeToScenario(CurrentScenario);
                    CrowdAndGenericRockSoundManager.Instance.ChangeToScenario(CurrentScenario);
                    m_ConsoleScreenManager.ChangeToScenario(CurrentScenario);
                    PhoneManager.Instance.ChangeToScenario(CurrentScenario);
                    // no longer neededd   BalloonManager.Instance.ChangeToScenario(CurrentScenario);
                    PhysicalConsole.Instance.ChangeToScenario(CurrentScenario);
                    ////StageLightManager.Instance.ChangeToScenario(CurrentScenario);
                    BandManager.Instance.ChangeToScenario(CurrentScenario);
                    IntroMenuManager.Instance.ChangeToScenario(CurrentScenario);
                    MenuManager.Instance.ChangeToScenario(CurrentScenario);
                    MenuManager.Instance.PauseMenuManagerRef.ChangeToScenario(CurrentScenario);
                    MonitorTrainerRoot.Instance.LaptopMenuManagerRuntimeRef.UpdateRunTimeData();
                    LaptopMenuManagerRuntimeRef.UpdateRunTimeData();
                }
            }
        }


        public bool IsPlaying()
        {
            switch (CurrentScenario)
            {
                case ScenarioEnum.Stackable:
                case ScenarioEnum.SongFinishedCompleted:
                    return true;
                    break;
                default:
                    break;
            }
            return false;
        }

        private Dictionary<string, GameObject> m_ContentTypes = new Dictionary<string, GameObject>();


        [SerializeField] public Transform ConsoleCanvasRoot;
        [SerializeField] private Transform TasksCanvasRoot;
        [SerializeField] private Transform IntroMenuRoot;
        [SerializeField] private Transform LaptopMenuRoot;
        [SerializeField] private Transform FloatingPointsGUI;
        
        [SerializeField] private ConsoleScreenManager m_ConsoleScreenManager;
        [SerializeField] private TaskBarManager m_TaskBarManager;
        [SerializeField] private TaskDescManager m_TaskDescriptionManager;

        public LaptopMenuManager LaptopMenuManagerRuntimeRef;
        public FloatingPointsGUIManager FloatingPointsGUIManagerRef;


        [InspectorButton]
        private void DEBUG_FINSH_SONG()
        {
            MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.SongFinishedCompleted;
        }

        public void Initialise()
        {
            Application.runInBackground = true;
            LocalPlayerDataRef.ReadData();
            Instance = this;
            new ConsoleData(this);
            this.AddComponent<PlatformManager>();
            this.AddComponent<NetworkMessagesManager>();

            this.gameObject.SearchComponent<Transform>("TaskMenu").SetActive(true);
            this.gameObject.SearchComponent<Transform>("ConsoleMenu").SetActive(true);
            GetContentTypes();

            SetupConsoleGUI();
            SetupTasksGUI();
            SetupTaskBar();
            SetupTaskDescription();
            SetupAchievementManager();
            SetupIntroMenuGUI();
            LaptopMenuRootMenuGUI();
            InitFloatingPointsGUI();

            var all = this.gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var item in all)
            {
                if(item.name.StartsWith("HighLight_", StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    item.SetActive(false);
                }
            }

            DebugBeep.LogError("Turned off all tutorail stuff here ", DebugBeep.MessageLevel.Medium);
            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        }


        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;


            Core.PhotonMultiplayerRef.InitialiseForApp();
            this.WaitForReal(3, () =>
            {
                CurrentScenario = ScenarioEnum.Menu;
                TaskManager.Instance.OnCompetedTaskItem += UpdateScores;
            });

            ////if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
            ////{
            ////    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Custom;
            ////}

#if VR_INTERACTION
            CameraControllerVR.Instance.TeleportAvatar(MenuManager.Instance.gameObject.scene, MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start, null);
            CameraControllerVR.Instance.TeleporterRight.Add(InputManagerVR.Instance.AnySubscription.BtnPrimary);
            InputManagerVR.Instance.AnySubscription.BtnStart.Begin += OnPauseMenu;

            CameraControllerVR.Instance.MainCamera.farClipPlane = 100f;
            CameraControllerVR.Instance.DistanceGrabber = false;
            CameraControllerVR.Instance.DistanceGrabHighLightInvalidTarget = false;
#endif

            var inter0 = Core.Scene.GetSpawnedVrInteraction(MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player1].PhoneName);
            if (inter0 != null)
            {
                var pick0 = (VrInteractionPickUp)inter0;
                if (pick0 != null)
                {
                    pick0.AllowPickup = false;
                }
            }

           

            var inter1 = Core.Scene.GetSpawnedVrInteraction(MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player2].PhoneName);
            if (inter1 != null)
            {
                var pick0 = (VrInteractionPickUp)inter1;
                if (pick0 != null)
                {
                    pick0.AllowPickup = false;
                }
            }


            // turn all raycastTarget off
            // IF NOT THE FPS IS GARBAGE
            var all = this.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var item in all)
            {
                item.raycastTarget = false;
            }


            var toggle = this.GetComponentsInChildren<ToggleGroup>(true);
            foreach (var item in toggle)
            {
                var image = this.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.raycastTarget = true;
                    image.transform.SetActive(true);
                }
            }

            var toggles = this.GetComponentsInChildren<Toggle>(true);
            foreach (var item in toggles)
            {
                // this part fixes the toggle slection
                if(item.graphic != null)
                {
                    item.graphic.raycastTarget = true;
                    item.graphic.transform.SetActive(true);
                }
                EnableTargetGraphic(item);
            }

            var buttons = this.GetComponentsInChildren<Button>(true);
            foreach (var item in buttons)
            {
                EnableTargetGraphic(item);
            }

            var sliders = this.GetComponentsInChildren<Slider>(true);
            foreach (var item in sliders)
            {
                EnableTargetGraphic(item);
            }

            var vol = GameObject.FindObjectsOfType<VLB.VolumetricLightBeam>(true);
            foreach (var item in vol)
            {
                item.trackChangesDuringPlaytime = false;
            }
        }

        private void EnableTargetGraphic(Selectable  item)
        {
            var image = item.targetGraphic;
            if (image != null)
            {
                image.raycastTarget = true;
                image.transform.SetActive(true);
            }
        }
        private void UpdateScores(List<MusicianRequestData> obj, MusicianRequestData newOne)
        {
            TaskSettings.Instance.UpdateScores(obj, newOne);
        }

        private void OnPauseMenu(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
            MenuManager.Instance.PauseMenuManagerRef.ToggleShow();
        }

        private void SetupConsoleGUI()
        {
            ConsoleCanvasRoot = transform.Search("ConsoleRoot");
            ConsoleCanvasRoot.gameObject.ForceComponent<VRUICanvas>();
            //ConsoleCanvasRoot.gameObject.ForceComponent<DebugOnDisable>();
            m_ConsoleScreenManager = ConsoleCanvasRoot.gameObject.ForceComponent<ConsoleScreenManager>();
            m_ConsoleScreenManager.Initialise(m_ContentTypes);
        }

        private void SetupIntroMenuGUI()
        {
            IntroMenuRoot = transform.Search("IntroMenu/IntroMenuRoot");
            IntroMenuRoot.gameObject.ForceComponent<VRUICanvas>();
            IntroMenuManager menu = IntroMenuRoot.gameObject.ForceComponent<IntroMenuManager>();
            menu.Initialise();
        }

        private void LaptopMenuRootMenuGUI()
        {
            LaptopMenuRoot = transform.SearchComponent<Transform>("LaptopGUIHolder");
            LaptopMenuManagerRuntimeRef = LaptopMenuRoot.gameObject.ForceComponent<LaptopMenuManager>();
            LaptopMenuManagerRuntimeRef.UpdateRunTimeData();
        }

        private void InitFloatingPointsGUI()
        {
            FloatingPointsGUI = transform.SearchComponent<Transform>("FloatingPointsGUI");
            FloatingPointsGUIManagerRef = FloatingPointsGUI.gameObject.ForceComponent<FloatingPointsGUIManager>();
            FloatingPointsGUI.transform.SetParent(LaptopMenuRoot);
        }

        private void SetupTasksGUI()
        {
            TasksCanvasRoot = transform.Search("TasksRoot");
            TasksCanvasRoot.gameObject.ForceComponent<VRUICanvas>();
        }

        private void SetupTaskBar()
        {
            var taskBar = transform.Search("TaskBar");
            m_TaskBarManager = taskBar.gameObject.ForceComponent<TaskBarManager>();
            m_TaskBarManager.Initialise(m_ContentTypes);
        }

        private void SetupTaskDescription()
        {
            var taskDesc = transform.Search("TaskDescription");
            m_TaskDescriptionManager = taskDesc.gameObject.ForceComponent<TaskDescManager>();
            m_TaskDescriptionManager.Initialise(m_ContentTypes);
        }

        private void SetupAchievementManager()
        {
            TaskManager item = this.gameObject.ForceComponent<TaskManager>();
            item.Initialise();
        }


        private void GetContentTypes()
        {
            Transform contentHolder = transform.Find("ContentTypes");

            List<Transform> children = contentHolder.GetDirectChildren();

            foreach (var child in children)
            {
                if (false == m_ContentTypes.ContainsKey(child.name))
                {
                    m_ContentTypes.Add(child.name, null);
                }

                m_ContentTypes[child.name] = child.gameObject;
                m_ContentTypes[child.name].SetActive(false);
            }
        }

        public void ResetConsoleButtonStates()
        {
            PhysicalConsole.Instance.Power.AllowPress = true;
            PhysicalConsole.Instance.Power.ButtonState = VrInteractionButtonLatched.ButtonStateEnum.Down;
            PhysicalConsole.Instance.MuteGroupAll.SetInstantStateWithoutCallback(VrInteractionButtonLatched.ButtonStateEnum.Up);
            PhysicalConsole.Instance.MuteGroupDrums.SetInstantStateWithoutCallback(VrInteractionButtonLatched.ButtonStateEnum.Up);
            PhysicalConsole.Instance.MuteGroupGuitar.SetInstantStateWithoutCallback(VrInteractionButtonLatched.ButtonStateEnum.Up);
            PhysicalConsole.Instance.MuteGroupVox.SetInstantStateWithoutCallback(VrInteractionButtonLatched.ButtonStateEnum.Up);
            ////PhysicalConsole.Instance.USBKey.SetState(VrInteraction.StateEnum.OffIncludingRenderer);
        }


        private void Update()
        {
            if(IsPlaying() == true)
            {
                m_ConsoleScreenManager.ManualUpdate();
            }
        }


        private void OnDestroy()
        {

        }

#region DisableColliders



#if UNITY_EDITOR
        private Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode> m_RendererShadowcast = new Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode>();

        [InspectorButton]
        private void ShadowsOff()
        {
            foreach (var item in m_RendererShadowcast)
            {
                item.Key.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        [InspectorButton]
        private void ShadowsOn()
        {
            foreach (var item in m_RendererShadowcast)
            {
                item.Key.shadowCastingMode = item.Value;
            }
        }

#endif

#endregion DisableColliders
    }
}