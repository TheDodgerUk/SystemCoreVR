using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;
using static VrInteractionBaseButton;

namespace MonitorTrainer
{
    public class PhysicalConsole : MonoBehaviour
    {     
        public static PhysicalConsole Instance;

        private TransformData m_PlayerOne = new TransformData();
        private TransformData m_PlayerTwo = new TransformData();

        public class Channel
        {
            public GameObject MusicianImageRoot;
            public Renderer MusicianImageRenderer;
            public Renderer MusicianColourImageRenderer;
            public Renderer MusicianColourRenderer;
            public VrInteractionBaseSlider Fader;
            public VrInteractionBaseButton Mute;
            public VrInteractionBaseButton Solo;
        }

        private const float MOVEMENT_TIME = 0.25f /2; // Adam requested double speed
        private const float AMOUNT = -0.006f;
        private readonly Color RED = new Color(255f/255f, 76f / 255f, 76f / 255f);
        private readonly Color ORANGE = new Color(255f / 255f, 76f / 255f, 0);
        private int m_PowerOnCount = 0;

        public List<Channel> ChannelList = new List<Channel>();
        public VrInteractionBaseButton MuteGroupAll;
        public VrInteractionBaseButton MuteGroupDrums;
        public VrInteractionBaseButton MuteGroupGuitar;
        public VrInteractionBaseButton MuteGroupVox;
        public VrInteractionBaseButton Power; 


        public GameObject ConsoleDummy;

        public Dictionary<ConsoleSliderGroupEnum, VrInteractionBaseSlider> m_GroupSliderDictonary = new Dictionary<ConsoleSliderGroupEnum, VrInteractionBaseSlider>();
        public Dictionary<ConsoleButtonEnum, VrInteractionBaseButton> m_GroupButtonDictonary = new Dictionary<ConsoleButtonEnum, VrInteractionBaseButton>();

        public bool HasInitialised { get; private set; }
        public bool IsPowerOn() => (Power.ButtonState == ButtonStateEnum.Down);
        private AudioSource m_PowerOn;
        private AudioSource m_PowerOff;
        private AudioSource m_PowerLoop;
        private AudioSource m_ButtonClickUp;
        private AudioSource m_ButtonClickDown;

        private AudioSource m_SliderDrag;

        private VrInteraction m_RootInteraction;
        private GameObject m_AudioSourceHolder;

        private GameObject m_InteractiveHolder_Player1And2;
        private List<VrInteraction> m_InteractiveHolder_Player1And2VrInteraction;
        private GameObject m_InteractiveHolder_Player3And4;
        private List<VrInteraction> m_InteractiveHolder_Player3And4VrInteraction;
        public void Initialise()
        {
            Instance = this;
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }


        private void OnEnvironmentLoadingComplete()
        {
            for(int i = 0; i < this.transform.childCount; i++)
            {
                var child = this.transform.GetChild(i);
                child.transform.SetStaticRecursivelyStartsWithStatic();
            }

            m_InteractiveHolder_Player1And2 = this.transform.Search("InteractiveHolder_Player1And2").gameObject;
            m_InteractiveHolder_Player3And4 = this.transform.Search("InteractiveHolder_Player3And4").gameObject;
            m_InteractiveHolder_Player1And2VrInteraction = m_InteractiveHolder_Player1And2.GetComponentsInChildren<VrInteraction>(true).ToList();
            m_InteractiveHolder_Player3And4VrInteraction = m_InteractiveHolder_Player3And4.GetComponentsInChildren<VrInteraction>(true).ToList();

            m_InteractiveHolder_Player1And2.SetActive(true);
            m_InteractiveHolder_Player3And4.SetActive(false);

            var player1 = Core.Scene.GetSpawnedVrInteraction(MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player1].ConsoleName);
            m_PlayerOne = player1.gameObject.GetTransformData();
            var player2 = Core.Scene.GetSpawnedVrInteraction(MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player2].ConsoleName);
            m_PlayerTwo = player2.gameObject.GetTransformData();

            StartCoroutine(Internal(player1, null));


            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "PowerOn", (audio) =>
            {
                SetClip(ref m_PowerOn, audio);
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "PowerOff", (audio) =>
            {
                SetClip(ref m_PowerOff, audio);
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "PowerLoop", (audio) =>
            {
                SetClip(ref m_PowerLoop, audio);
                m_PowerLoop.loop = true;
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "ButtonClickUp", (audio) =>
            {
                SetClip(ref m_ButtonClickUp, audio);
                SetClip(ref m_SliderDrag, audio);
                m_SliderDrag.volume = 0.3f;
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "ButtonClickDown", (audio) =>
            {
                SetClip(ref m_ButtonClickDown, audio);
            });
            var items = this.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true).ToList();
            foreach (var item in items)
            {
                var holderItem = item;
                holderItem.material.shader = Shader.Find(holderItem.material.shader.name);
            }


            var Decals_Player1 = this.transform.SearchComponent<Transform>("Decals_Player1");
            var Decals_Player2 = this.transform.SearchComponent<Transform>("Decals_Player2");

            MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player1].DecalList = Decals_Player1.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true).ToList();
            MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player2].DecalList = Decals_Player2.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true).ToList();
            MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player3].DecalList = Decals_Player1.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true).ToList();
            MonitorTrainerConsts.MULIPLYER_DATA[PlayersEnum.Player4].DecalList = Decals_Player2.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true).ToList();
        }

        public void TurnOnCorrectInteractive(PlayersEnum player)
        {
            m_InteractiveHolder_Player1And2.SetActive(player.IsMainSide() == true);
            m_InteractiveHolder_Player3And4.SetActive(player.IsMainSide() == false);

            OppositeSide.Instance.m_StaticHolder1and2.SetActive(!m_InteractiveHolder_Player1And2.activeInHierarchy); // th eopposite of 3And4
            OppositeSide.Instance.m_StaticHolder3and4.SetActive(!m_InteractiveHolder_Player3And4.activeInHierarchy); // th eopposite of 3And4
            m_InteractiveHolder_Player1And2VrInteraction.ForEach(e => e.SetActive(m_InteractiveHolder_Player1And2.activeInHierarchy));
            m_InteractiveHolder_Player3And4VrInteraction.ForEach(e => e.SetActive(m_InteractiveHolder_Player3And4.activeInHierarchy));
        }

        public void SetAudioSourcesHolder(GameObject obj)
        {
            if (m_AudioSourceHolder == null)
            {
                m_AudioSourceHolder = new GameObject("m_AudioSourceHolder");
            }
            m_AudioSourceHolder.transform.SetParent(obj.transform);
            m_AudioSourceHolder.transform.ClearLocals();

        }
        private void SetClip(ref AudioSource audioSource, AudioClip clip)
        {
            if(m_AudioSourceHolder == null)
            {
                m_AudioSourceHolder = new GameObject("m_AudioSourceHolder");
            }
            audioSource = m_AudioSourceHolder.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.8f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0.01f;
            audioSource.maxDistance = 2f;
            audioSource.clip = clip;
        }


        private void PlayButtonClick(ButtonStateEnum state)
        {
            if(state == ButtonStateEnum.Up)
            {
                m_ButtonClickUp.Play();
            }
            else
            {
                m_ButtonClickDown.Play();
            }
        }

        private void PlaySliderDrag()
        {
            m_SliderDrag.Play();
        }


        private IEnumerator Internal(VrInteraction original, Action callback)
        {
            m_RootInteraction = original;
            SetAudioSourcesHolder(original.gameObject);

            m_GroupButtonDictonary.Clear();
            m_GroupSliderDictonary.Clear();

            while (null == MusicSoundManager.Instance || MusicSoundManager.Instance.HasInitialised == false)
            {
                yield return new WaitForSeconds(INITIALISE_COROUTINE);
            }
            for (int i = 0; i < TOTAL_PHYSICAL_SLIDERS; i++)
            {
                int index = i;
                MusicianTypeEnum musicianType = (MusicianTypeEnum)index;
                Channel track = new Channel();
                ConsoleButtonEnum soloEnum = (ConsoleButtonEnum)((int)ConsoleButtonEnum.Solo1 + i);
                ConsoleButtonEnum muteEnum = (ConsoleButtonEnum)((int)ConsoleButtonEnum.Mute1 + i);
                ConsoleSliderGroupEnum sliderGroupEnum = (ConsoleSliderGroupEnum)((int)ConsoleSliderGroupEnum.Slider1 + i);


                track.Solo = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"Solo{i+1}Button");
                track.Solo.ClearAddCallback((buttonState) =>
               {
                   if (IsPowerOn())
                   {
                       PlayButtonClick(buttonState);
                       if (IsInputAuxType_INPUT())
                       {
                           ConsoleData.Instance.SetSolo(sliderGroupEnum, (buttonState == ButtonStateEnum.Down));
                           ConsoleScreenManager.Instance.UpdateData();
                       }
                       else
                       {
                           ConsoleScreenManager.Instance.UpdateSpecificAuxSolo(index, (buttonState == ButtonStateEnum.Down));
                       }
                   }
               });

                track.Mute = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"Mute{i + 1}Button");
                track.Mute.ClearAddCallback((buttonState) =>
                {
                    if (IsPowerOn())
                    {
                        PlayButtonClick(buttonState);
                        if (IsInputAuxType_INPUT())
                        {
                            ConsoleData.Instance.SetMute(sliderGroupEnum, (buttonState == ButtonStateEnum.Down));
                            ConsoleScreenManager.Instance.UpdateData();
                        }
                        else
                        {
                            ConsoleScreenManager.Instance.UpdateSpecificAuxMute(index, (buttonState == ButtonStateEnum.Down));
                        }
                    }
                });

                track.Fader = (VrInteractionBaseSlider)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentSlider, $"Slider{i + 1}");
                track.Fader.ClearAddCallback((percentage) =>
                {
                    if (IsPowerOn())
                    {
                        PlaySliderDrag();
                        if (IsInputAuxType_INPUT())
                        {
                            ConsoleData.Instance.SetSoundLevelPercentage(sliderGroupEnum, percentage);
                            ConsoleScreenManager.Instance.UpdateData();
                            
                            InputManagerVR.Instance.GetController(Handedness.Right).Vibrate(0.05f, 0.001f);
                        }
                        else
                        {
                            ConsoleScreenManager.Instance.UpdateSpecificAuxSlider(index, percentage);
                        }
                    }
                });

                m_GroupButtonDictonary.Add(soloEnum, track.Solo);
                m_GroupButtonDictonary.Add(muteEnum, track.Mute);
                m_GroupSliderDictonary.Add(sliderGroupEnum, track.Fader);

                track.MusicianImageRoot = m_RootInteraction.transform.SearchChildrenWithNameContains($"Dummy_Display{i + 1}").gameObject;
                track.MusicianImageRenderer = track.MusicianImageRoot.transform.Search("Image").GetComponent<Renderer>();
                track.MusicianImageRenderer.SetActive(true);
                track.MusicianColourRenderer = track.MusicianImageRoot.transform.Search("Colour").GetComponent<Renderer>();
                track.MusicianColourRenderer.SetActive(true);

                track.MusicianColourImageRenderer = null;
                Transform colourImage = track.MusicianImageRoot.transform.Search("ColourImage"); // these are only on 5 and 6 for the multicolor color
                if(null != colourImage)
                {
                    track.MusicianColourImageRenderer = colourImage.GetComponent<Renderer>();
                    colourImage.SetActive(true);
                }

                ChannelList.Add(track);
            }

            InitialiseSeperateButtons();

            ChangeOutlineColour();

            HasInitialised = true;
            callback?.Invoke();
        }



        private void InitialiseSeperateButtons()
        {
            MuteGroupAll = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"MonitorTrainer_Console-MuteAll_Button");
            MuteGroupAll.ClearAddCallback((buttonState) =>
            {
                if (IsPowerOn())
                {
                    PlayButtonClick(buttonState);
                    ConsoleData.Instance.SetConsoleButtonData(ConsoleButtonEnum.MuteGroupAll, (buttonState == ButtonStateEnum.Down));
                }
            });
            m_GroupButtonDictonary.Add(ConsoleButtonEnum.MuteGroupAll, MuteGroupAll);
            MuteGroupAll.ButtonState = ButtonStateEnum.Up;

            MuteGroupDrums = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"MonitorTrainer_Console-MuteDrums_Button");
            MuteGroupDrums.ClearAddCallback((buttonState) =>
            {
                if (IsPowerOn())
                {
                    PlayButtonClick(buttonState);
                    ConsoleData.Instance.SetConsoleButtonData(ConsoleButtonEnum.MuteGroupDrum, (buttonState == ButtonStateEnum.Down));
                }
            });
            m_GroupButtonDictonary.Add(ConsoleButtonEnum.MuteGroupDrum, MuteGroupDrums);
            MuteGroupDrums.ButtonState = ButtonStateEnum.Up;

            MuteGroupGuitar = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"MonitorTrainer_Console-MuteGuitar_Button");
            MuteGroupGuitar.ClearAddCallback((buttonState) =>
            {
                if (IsPowerOn())
                {
                    PlayButtonClick(buttonState);
                    ConsoleData.Instance.SetConsoleButtonData(ConsoleButtonEnum.MuteGroupGuitar, (buttonState == ButtonStateEnum.Down));
                }
            });
            m_GroupButtonDictonary.Add(ConsoleButtonEnum.MuteGroupGuitar, MuteGroupGuitar);
            MuteGroupGuitar.ButtonState = ButtonStateEnum.Up;

            MuteGroupVox = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"MonitorTrainer_Console-MuteVOX_Button");
            MuteGroupVox.ClearAddCallback((buttonState) =>
            {
                if (IsPowerOn())
                {
                    PlayButtonClick(buttonState);
                    ConsoleData.Instance.SetConsoleButtonData(ConsoleButtonEnum.MuteGroupVox, (buttonState == ButtonStateEnum.Down));
                }
            });
            m_GroupButtonDictonary.Add(ConsoleButtonEnum.MuteGroupVox, MuteGroupVox);
            MuteGroupVox.ButtonState = ButtonStateEnum.Up;

            Power = (VrInteractionBaseButton)m_RootInteraction.GetVrInteractionFromRoot(MetaDataType.ContentButton, $"MonitorTrainer_Console-PowerButton");
            Power.ClearAddCallback((buttonState) =>
            {
                PowerStateChanged();
            });
            Power.SetInstantStateWithoutCallback(ButtonStateEnum.Up);
            PowerStateChanged(false);
            m_GroupButtonDictonary.Add(ConsoleButtonEnum.Power, Power);
        }

        private void ChangeOutlineColour()
        {
            foreach (var button in m_GroupButtonDictonary.Values)
            {
                button.SetOutlineColour(EPOOutline.Outliner.EnumItemSelected.Valid);
            }
            foreach (var slider in m_GroupSliderDictonary.Values)
            {
                slider.SetOutlineColour(EPOOutline.Outliner.EnumItemSelected.Valid);
            }
        }


        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Stackable:
                    // safety to not trigger event
                    m_PowerOnCount = 100;
                    PhysicalConsole.Instance.Power.AllowPress = true;
                    break;

                case ScenarioEnum.SongFinishedCompleted:
                    // safety to not trigger event
                    m_PowerOnCount = 100;
                    PhysicalConsole.Instance.Power.ButtonState = ButtonStateEnum.Down;
                    PhysicalConsole.Instance.Power.AllowPress = false;
                    break;
            }
        }


        public void SetSliderInstantDb(ConsoleSliderGroupEnum consoleSlider, float DB)
        {
            float percentage = DecibelToPercentage(DB);
            m_GroupSliderDictonary[consoleSlider].SetPercentageCallbackAmount(percentage);
        }

        public void SetSliderInstantPercentage(ConsoleSliderGroupEnum consoleSlider, float percentage)
        {
            m_GroupSliderDictonary[consoleSlider].SetPercentageCallbackAmount(percentage);
        }

        public void SetSliderInstantPercentageNoCallback(ConsoleSliderGroupEnum consoleSlider, float percentage)
        {
            m_GroupSliderDictonary[consoleSlider].SetPercentageNoCallbackAmount(percentage);
        }

        public void SetButtonInstant(ConsoleButtonEnum button, ButtonStateEnum buttonState)
        {
            m_GroupButtonDictonary[button].SetInstantStateWithoutCallback(buttonState);
        }

        public void PowerStateChanged(bool playsound = true)
        {
            bool isPowerOn = IsPowerOn();

            if (isPowerOn == true && playsound == true)
            {
                // this covers the whole screen 
                ConsoleScreenManager.Instance.BootGlitchScreensRef.PlayBoot();
            }
            ConsoleScreenManager.Instance.BootGlitchScreensRef.StopGlicth();

            ConsoleScreenManager.Instance.SetVisible(isPowerOn);
            foreach (var item in ChannelList)
            {
                item.MusicianImageRoot.SetActive(isPowerOn);
            }

            MuteGroupAll.EnableEmmisive(isPowerOn);
            MuteGroupDrums.EnableEmmisive(isPowerOn);
            MuteGroupGuitar.EnableEmmisive(isPowerOn);
            MuteGroupVox.EnableEmmisive(isPowerOn);

            // reset it no matter what
            MuteGroupAll.SetInstantStateWithoutCallback(ButtonStateEnum.Down);
            MuteGroupDrums.SetInstantStateWithoutCallback(ButtonStateEnum.Up);
            MuteGroupGuitar.SetInstantStateWithoutCallback(ButtonStateEnum.Up);
            MuteGroupVox.SetInstantStateWithoutCallback(ButtonStateEnum.Up);

            foreach (var item in ChannelList)
            {
                item.Mute.EnableEmmisive(isPowerOn);
                item.Solo.EnableEmmisive(isPowerOn);
            }
            // when turned back on
            if (true == isPowerOn)
            {
                MoveInvalidSlidersToCorrectPlace();

                m_PowerOnCount++;
                // first time power on 
                if (m_PowerOnCount == 2)
                {
                    MuteGroupAll.SetInstantStateWithoutCallback(ButtonStateEnum.Up);
                }
            }
            if (playsound == true)
            {
                PlayPowerSound();
            }
        }



        private void PlayPowerSound()
        {
            if (true == IsPowerOn())
            {
                m_PowerOn.Play();
                this.WaitFor(m_PowerOn.clip.length, () =>
                {
                    m_PowerLoop.Play();
                });
            }
            else
            {
                m_PowerLoop.Stop();
                m_PowerOff.Play();
            }
        }

        public void MoveInvalidSlidersToCorrectPlace()
        {
            // this puts same data back in , 
            // it moves all the slider and buttons to the state it was before the power off
            // its instant, this looks bad though
            List<GroupData> currentData = new List<GroupData>();
            foreach (var item in ChannelList)
            {
               /////// currentData.Add(new GroupData(item.Fader.SliderPercentageValue, item.Mute.ButtonState, item.Solo.ButtonState));
            }

            ConsoleData.Instance.AssignAndMoveToNewData(ConsoleData.Instance.GetCurrentInputAuxData(), currentData);
        }

    }
}
