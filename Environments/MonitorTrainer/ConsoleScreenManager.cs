using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ConsoleScreenManager : MonoBehaviour
    {
        public static ConsoleScreenManager Instance;

        public List<Sprite> m_AllIcons = new List<Sprite>();

        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private GameObject m_Logo;

        [SerializeField] private Toggle m_TglInputs;
        [SerializeField] private Toggle m_TglAux;

        [SerializeField] private Toggle m_TglFlip;

        [SerializeField] private CanvasGroup m_LeftSidePanel;
        [SerializeField] private CanvasGroup m_RightSidePanel;

        [SerializeField] private CanvasGroup m_InputsContent;
        [SerializeField] private CanvasGroup m_AuxContent;
        [SerializeField] private TextMeshProUGUI m_DateTime;

        private VideoPlayer m_VideoPlayer;
        private FileManager m_FileManager;
        private AuxChannelsList m_AuxChannelsList;
        private InputChannelsDetails m_InputChannelsDetails;
        private Transform m_AuxGroups;

        private List<InputsNavGroup> m_InputsNavGroups = new List<InputsNavGroup>();
        private Dictionary<AuxEnum, ChannelSmallAux> m_AuxNavGroups = new Dictionary<AuxEnum, ChannelSmallAux>();
        private InputsNavGroup m_CurrentlySelectedInput = null;
        private AuxEnum m_CurrentlySelectedAux = AuxEnum.Bass;
        public bool FlipOn { get; private set; }


        public BootGlitchScreens BootGlitchScreensRef { get; private set; }


        [EditorButton]
        private void DEBUG_Inputs() => m_TglInputs.onValueChanged.Invoke(true);
        [EditorButton]
        private void DEBUG_Aux() => m_TglAux.onValueChanged.Invoke(true);

        [EditorButton]
        private void DEBUG_AuxForward() => m_TglAux.onValueChanged.Invoke(true);


        [EditorButton]
        private void DEBUG_PlayBoot() => BootGlitchScreensRef.PlayBoot();
        [EditorButton]
        private void DEBUG_PlayGlictch() => BootGlitchScreensRef.PlayGlitch();


        internal void Initialise(Dictionary<string, GameObject> content)
        {
            Instance = this;
            FlipOn = false;
            m_CanvasGroup = GetComponent<CanvasGroup>();

            m_VideoPlayer = transform.Find("Background").GetComponent<VideoPlayer>();
            m_DateTime = transform.Find("TopBar/DateTime").GetComponent<TextMeshProUGUI>();
            m_Logo = transform.Find("Logo").gameObject;
            m_FileManager = transform.Find("FileManager").gameObject.ForceComponent<FileManager>();
            m_FileManager.Init(content, LoadSongAndChangeToScenario1);

            BootGlitchScreensRef = this.AddComponent<BootGlitchScreens>();
            BootGlitchScreensRef.Init(transform.Find("BootUp Screens").gameObject, transform.Find("GlitchScreen").gameObject);
 

            Transform leftSide = transform.Find("LeftSide");
            Transform rightSide = transform.Find("RightSide");

            m_LeftSidePanel = leftSide.GetComponent<CanvasGroup>();
            m_LeftSidePanel.VisibleAndInteractiveInitilise();
            m_RightSidePanel = rightSide.GetComponent<CanvasGroup>();
            m_RightSidePanel.VisibleAndInteractiveInitilise();

            m_TglInputs = rightSide.Find("NavBar/TglInputs").GetComponent<Toggle>();
            m_TglAux = rightSide.Find("NavBar/TglAux").GetComponent<Toggle>();


            m_InputsContent = rightSide.SearchComponent<CanvasGroup>("InputsContent");
            m_InputsContent.VisibleAndInteractiveInitilise();
            m_AuxContent = rightSide.SearchComponent<CanvasGroup>("AuxContent");
            m_AuxContent.VisibleAndInteractiveInitilise();

            m_TglInputs.onValueChanged.RemoveAllListeners();
            m_TglAux.onValueChanged.RemoveAllListeners();

            m_TglInputs.onValueChanged.AddListener((value) => ToggleInputsTab(value));
            m_TglAux.onValueChanged.AddListener((value) => ToggleAuxTab(value));


            m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
            m_CanvasGroup.VisibleAndInteractiveInitilise();
            m_TglFlip = m_AuxContent.transform.Find("TglFlip").GetComponent<Toggle>();
            m_TglFlip.onValueChanged.AddListener((value) =>
            {
                m_AuxChannelsList.ToggleFlip(value);
                FlipOn = value;
                if (FlipOn == true)
                {
                    PhysicalConsole.Instance.MoveInvalidSlidersToCorrectPlace();
                }
            });



            TaskAction allLoaded = new TaskAction(1, () =>
            {

                var channelsList = leftSide.transform.Find("ChannelsList");
                m_AuxChannelsList = channelsList.gameObject.ForceComponent<AuxChannelsList>();


                var detailsParent = leftSide.transform.Find("Mask/ChannelDetails").gameObject;
                detailsParent.transform.localPosition = new Vector3(0f, -1274f, 0f);
                Debug.Log("Moved by code as i not sure who Lubo did this, This just sets it to the correct in asset bundles");

                var iconsParent = leftSide.transform.Find("BottomQuads").gameObject;
                m_InputChannelsDetails = detailsParent.ForceComponent<InputChannelsDetails>();
                Animator leftSideAnimator = leftSide.GetComponent<Animator>();
                m_InputChannelsDetails.Init(iconsParent, leftSideAnimator);

                SetupBackgroundVideo();
                ShowSplashScreen();

            });

            Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItemList(Core.Mono, (list) =>
            {
                TaskAction allItemsLoaded = new TaskAction(list.Count, () => allLoaded.Increment());
                foreach (var item in list)
                {
                    Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItemSprite(Core.Mono, item, (sprite) =>
                    {
                        if (sprite != null)
                        {
                            m_AllIcons.Add(sprite);
                        }
                        allItemsLoaded.Increment();
                    });
                }
            });

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        public void InitialiseSongData()
        {
            m_AuxChannelsList.Init();
            SetupInputsGroups();
            SetupAuxGroups();
            m_InputsNavGroups.ForEach(e => e.InitiliseSongData());
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Mono.WaitForFrames(5, () =>
            {
                Debug.Log("Get rid of the bloody button Send On");
                m_TglFlip.isOn = true;
                m_TglFlip.SetActive(false); // remove it 
            });
        }

        public void SetVisible(bool state)
        {
            m_CanvasGroup.VisibleAndInteractive(state);
        }

        private void SetupBackgroundVideo()
        {
            if (m_VideoPlayer != null)
            {
                Core.AssetBundlesRef.VideoClipAssetBundleRef.GetItem(this, "Background", (clip) =>
                {
                    m_VideoPlayer.source = VideoSource.VideoClip;
                    m_VideoPlayer.clip = clip;
                    m_VideoPlayer.Prepare();
                    m_VideoPlayer.Play();
                });
            }
        }

        private void SetupInputsGroups()
        {
            m_InputsNavGroups.Clear();

            var children = m_InputsContent.transform.GetChild(0).GetDirectChildren();
            for (int i = 0; i < children.Count; i++)
            {
                var grp = children[i].gameObject.ForceComponent<InputsNavGroup>();
                grp.Init((state, inputGroup) =>
                {
                    ToggleIconicButtons(state);
                    m_InputChannelsDetails.SetIcons(inputGroup - 1);
                    m_CurrentlySelectedInput = grp;
                }, i + 1);

                m_InputsNavGroups.Add(grp);
            }
            //m_InputChannelsDetails.SetIcons(0);
        }


        private void SetupAuxGroups()
        {
            m_AuxNavGroups.Clear();

            m_AuxGroups = m_AuxContent.transform.Find("AuxGroups");

            var auxChildren = m_AuxGroups.GetDirectChildren();
            for (int k = 0; k < auxChildren.Count; k++)
            {
                ChannelSmallAux channel = auxChildren[k].Find("CH_Small").gameObject.ForceComponent<ChannelSmallAux>();
                string channelName = auxChildren[k].name.SubstringToEnd(" ");

                if (false == Enum.TryParse(channelName, out AuxEnum auxEnum))
                {
                    Debug.LogError($"Could not parse channel : {channelName}");
                }
                if (false == m_AuxNavGroups.ContainsKey(auxEnum))
                {
                    m_AuxNavGroups.Add(auxEnum, channel);
                }
                else
                {
                    Debug.LogError("Failed to add Aux group, the names must be unique");
                }

                channel.InitAsAuxChannel((state) =>
                {
                    m_AuxChannelsList.SetActive(auxEnum, state);
                    m_CurrentlySelectedAux = auxEnum;
                }, channelName);
            }
        }

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Blank:
                    ShowSplashScreen();
                    break;
                case ScenarioEnum.Stackable:
                    ShowMainMenu();
                    m_TglInputs.onValueChanged.Invoke(true);
                    break;
                case ScenarioEnum.SongFinishedCompleted:
                    ShowNothing();
                    break;
            }
        }

        public void UpdateSpecificAuxSlider(int index,float percentage)
        {
            m_AuxChannelsList.UpdateChannelSlider(index,percentage);
        }

        public void UpdateSpecificAuxMute(int index, bool mute)
        {
            m_AuxChannelsList.UpdateChannelMute(index, mute);
        }

        public void UpdateSpecificAuxSolo(int index, bool solo)
        {
            m_AuxChannelsList.UpdateChannelSolo(index, solo);
        }

        public void UpdateData()
        {
            if (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Aux)
            {
                m_AuxChannelsList.UpdateData();
            }
            else
            {
                m_InputChannelsDetails.UpdateData();
            }
        }


        public void PlugUSB()
        {
            m_Logo.gameObject.SetActive(false);
            m_FileManager.VisibleAndInteractive(true);

            ToggleMainMenuPanels(false);
        }

        private void ShowSplashScreen()
        {
            m_Logo.SetActive(true);
            m_FileManager.VisibleAndInteractive(false);

            ToggleMainMenuPanels(false);
        }

        private void ShowNothing()
        {
            m_Logo.SetActive(false);
            m_FileManager.VisibleAndInteractive(false);

            ToggleMainMenuPanels(false);
        }

        private void LoadSongAndChangeToScenario1()
        {
            ConsoleData.Instance.RandomiseData();
            ShowMainMenu();
            MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.TutorialPart2;
        }

        public void LoadSong()
        {
            ConsoleData.Instance.RandomiseData();
            ShowMainMenu();           
        }

        private void ShowMainMenu()
        {
            m_Logo.SetActive(false);
            m_FileManager.VisibleAndInteractive(false);

            ToggleMainMenuPanels(true);

            m_TglInputs.isOn = true;
            m_InputsNavGroups[0].m_Tgl.onValueChanged.Invoke(true);
        }

        private void ToggleMainMenuPanels(bool state)
        {
            m_LeftSidePanel.VisibleAndInteractive(state);
            m_RightSidePanel.VisibleAndInteractive(state);
        }

        private void ToggleInputsTab(bool state)
        {
            m_InputsContent.VisibleAndInteractive(state);
            m_AuxContent.VisibleAndInteractive(!state);

            ToggleIconicButtons(state);

            if (state)
            {
                if (m_CurrentlySelectedInput != null)
                {
                    m_CurrentlySelectedInput.m_Tgl.onValueChanged.Invoke(true);
                }
                ConsoleData.Instance.m_InputAuxType = InputAuxTypeEnum.Input;
            }
        }

        private void ToggleAuxTab(bool state)
        {
            m_AuxContent.VisibleAndInteractive(state);
            m_InputsContent.VisibleAndInteractive(!state);

            if (state)
            {
                m_AuxChannelsList.SetActive(m_CurrentlySelectedAux, state);
                ConsoleData.Instance.m_InputAuxType = InputAuxTypeEnum.Aux;
            }
        }

        private void ToggleIconicButtons(bool state)
        {
            if (state)
            {
                m_AuxNavGroups[m_AuxChannelsList.m_CurrentAuxEnum].Deselect();
                m_AuxChannelsList.SetActive(AuxEnum.Bass, false);
                m_InputChannelsDetails.SetIconsActive(true);
                m_InputChannelsDetails.SetDetailsActive(false);
            }
        }


        internal void ManualUpdate()
        {
            if (m_DateTime != null)
            {
                m_DateTime.SetText(DateTime.Now.ToString("ddd dd MMM HH:mm"));
            }

            foreach (var item in m_AuxNavGroups.Values)
            {
                item.ManualUpdate();
            }
            foreach (var item in m_InputsNavGroups)
            {
                item.ManualUpdate();
            }
        }
    }
}