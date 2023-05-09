using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class InputChannelsDetails : MonoBehaviour
    {
        [SerializeField] private CanvasGroup m_ChannelsIcons;
        [SerializeField] private ChannelIconButton[] m_IconicBtns = new ChannelIconButton[CHANNEL_GROUP_SIZE];

        [SerializeField] private Image m_TopBar;
        [SerializeField] private TextMeshProUGUI m_OwnerName;
        [SerializeField] private Image m_Head;
        [SerializeField] private Image m_HeadBackground;
        [SerializeField] private Toggle m_Tgl40V;
        [SerializeField] private Toggle m_TglPhase;

        //TODO: figure out a way to change these two with vr input, and they dont do anything right now
        [SerializeField] private Slider m_Trim;

        [SerializeField] private Slider m_Gain;

        [SerializeField] private Slider m_LevelSlider;
        [SerializeField] private Image m_ChannelLevel;

        private Animator m_Animator;
        private AuxEnum m_CurrentAuxEnum = AuxEnum.Blank1;
        private TrackDataMusician m_TrackDataMusicianMaster;
        private AudioSampling m_AudioSampling;
        private CharacterDataClass m_CharacterDataClass;

        private bool m_CurrentState = false;

        internal void Init(GameObject iconsParent, Animator animator)
        {
            m_ChannelsIcons = iconsParent.GetComponent<CanvasGroup>();
            m_Animator = animator;
            m_IconicBtns[0] = iconsParent.transform.Find("CH_Iconic 1").gameObject.ForceComponent<ChannelIconButton>();
            m_IconicBtns[1] = iconsParent.transform.Find("CH_Iconic 2").gameObject.ForceComponent<ChannelIconButton>();
            m_IconicBtns[2] = iconsParent.transform.Find("CH_Iconic 3").gameObject.ForceComponent<ChannelIconButton>();
            m_IconicBtns[3] = iconsParent.transform.Find("CH_Iconic 4").gameObject.ForceComponent<ChannelIconButton>();

            foreach (var item in m_IconicBtns)
            {
                item.Init();
            }

            m_TopBar = transform.Find("TopBar/Background").GetComponent<Image>();
            m_OwnerName = transform.Find("TopBar/OwnerName").GetComponent<TextMeshProUGUI>();

            m_Head = transform.Find("Icon/Head").GetComponent<Image>();
            m_HeadBackground = transform.Find("Icon/IconBackground").GetComponent<Image>();

            m_Tgl40V = transform.Find("Left/Tgl40V").GetComponent<Toggle>();
            m_TglPhase = transform.Find("Left/TglPhase").GetComponent<Toggle>();

            m_Trim = transform.Find("Mid_NewTrimAndGain/Background/Trim/Slider_Trim").gameObject.GetComponent<Slider>();

            m_Trim.onValueChanged.RemoveAllListeners();
            m_Trim.onValueChanged.AddListener((amount) =>
            {
                if (m_TrackDataMusicianMaster != null)
                {
                    m_TrackDataMusicianMaster.m_Trim = amount;
                }
            });

            m_Gain = transform.Find("Mid_NewTrimAndGain/Background/Gain/Slider_Gain").gameObject.GetComponent<Slider>();

            m_Gain.onValueChanged.RemoveAllListeners();
            m_Gain.onValueChanged.AddListener((amount) =>
            {
                if (m_TrackDataMusicianMaster != null)
                {
                    m_TrackDataMusicianMaster.m_Gain = amount;
                }
            });

            m_LevelSlider = transform.Find("Right/Slider").GetComponent<Slider>();
            m_ChannelLevel = transform.Find("Right/CH_Large/Level").GetComponent<Image>();
            m_LevelSlider.interactable = false;
        }

        private void SetChannelDetails(bool state, AuxEnum auxEnum)
        {
            bool newState = (m_CurrentState && state) ? false : state;

            m_Animator.SetBool("Selected", newState);

            foreach (ChannelIconButton item in m_IconicBtns)
            {
                item.ToggleVisual(newState);
            }

            if (newState)
            {
                m_CurrentAuxEnum = auxEnum;
                m_TrackDataMusicianMaster = ConsoleData.Instance.m_GlobalInputData.FindLast((e) => e.m_AuxEnum == auxEnum);

                m_AudioSampling = MusicSoundManager.Instance.m_MusicianAudioSamplings.Get((MusicianTypeEnum)auxEnum);
                m_CharacterDataClass = MonitorTrainerConsts.GetCharacterData(auxEnum);

                if (m_Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Selected")
                {
                    m_Animator.SetInteger("RefreshStep", 1);

                    this.WaitForFrames(4, () =>
                    {
                        m_TopBar.color = m_CharacterDataClass.Colour;
                        m_OwnerName.SetText(m_CharacterDataClass.CharacterName);
                        m_HeadBackground.color = m_CharacterDataClass.Colour;
                        m_Head.sprite = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == m_CharacterDataClass.CharacterName.ToString().ToLower());

                        m_Animator.SetInteger("RefreshStep", 2);
                    });
                }
                else
                {
                    m_TopBar.color = m_CharacterDataClass.Colour;
                    m_OwnerName.SetText(m_CharacterDataClass.CharacterName);
                    m_HeadBackground.color = m_CharacterDataClass.Colour;
                    m_Head.sprite = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == m_CharacterDataClass.CharacterName.ToString().ToLower());
                }

                SetChannelSlider(m_TrackDataMusicianMaster.GetPercentageLevel());
                SetPhantonPower(m_TrackDataMusicianMaster.m_PhantomPower40V);
                SetPhase(m_TrackDataMusicianMaster.m_Phase);
                SetTrimLevel(m_TrackDataMusicianMaster.GetPercentageLevel());
                SetGainLevel(m_TrackDataMusicianMaster.m_Gain);
                int index = ConsoleData.Instance.GetCurrentInputAuxData().LastIndexOf(m_TrackDataMusicianMaster);
                if (index == -1)
                {
                    var aux = ConsoleData.Instance.GetCurrentInputAuxData();
                    Debug.LogError($"index not found count:, {aux.Count} : m_AuxEnum: {m_TrackDataMusicianMaster.m_AuxEnum}");
                }
                else
                {
                    PhysicalConsole.Instance.SetSliderInstantDb((ConsoleSliderGroupEnum)index, m_TrackDataMusicianMaster.GetDBLevel());
                }
                m_Tgl40V.onValueChanged.RemoveAllListeners();
                m_Tgl40V.onValueChanged.AddListener((value) =>
                {
                    m_TrackDataMusicianMaster.m_PhantomPower40V = value;
                });

                m_TglPhase.onValueChanged.RemoveAllListeners();
                m_TglPhase.onValueChanged.AddListener((value) =>
                {
                    m_TrackDataMusicianMaster.m_Phase = value;
                });

            }

            m_CurrentState = newState;
        }

        //Only for initial state
        public void SetDetailsActive(bool state)
        {
            bool newState = (m_CurrentState && state) ? false : state;

            m_Animator.SetBool("Selected", newState);

            foreach (ChannelIconButton item in m_IconicBtns)
            {
                item.ToggleVisual(newState);
            }
            m_CurrentState = newState;
        }

        public void SetIcons(int channelGroup)
        {
            for (int i = 0; i < m_IconicBtns.Length; i++)
            {
                int targetChannel = (channelGroup * CHANNEL_GROUP_SIZE) + i;

                AuxEnum auxEnum = ConsoleData.Instance.m_GlobalInputData[targetChannel].m_AuxEnum;
                string auxName = auxEnum.ToString();

                CharacterDataClass characterData = MonitorTrainerConsts.GetCharacterData(auxEnum);

                Sprite spriteUI = null;
                if (auxEnum.IsMusician() == false)
                {
                    spriteUI = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == (characterData.CharacterName).ToLower());
                    if (spriteUI == null)
                    {
                        Debug.LogError($"Cannot find sprite :{characterData.CharacterName} , AND");
                    }
                }
                else
                {
                    spriteUI = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == (characterData.Abbreviation + TXT_EXTENSION).ToLower());
                    if (spriteUI == null)
                    {
                        Debug.LogError($"Cannot find sprite :{characterData.Abbreviation + TXT_EXTENSION}  AND");
                    }
                }



                Sprite spriteConsole = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == (characterData.Abbreviation + TXT_EXTENSION).ToLower());
                if (spriteConsole == null)
                {
                    Debug.LogError($"Cannot find headName: {characterData.Abbreviation + TXT_EXTENSION}");
                }
                PhysicalConsole.Channel item = PhysicalConsole.Instance.ChannelList[i];

                if (spriteConsole != null)
                {
                    item.MusicianImageRenderer.gameObject.SetActive(true);
                    item.MusicianImageRenderer.material.mainTexture = spriteConsole.texture;
                    item.MusicianColourRenderer.gameObject.SetActive(true);
                    item.MusicianColourRenderer.material.SetColor("_Color", characterData.Colour);
                }
                else
                {
                    item.MusicianImageRenderer.gameObject.SetActive(false);
                    item.MusicianColourRenderer.gameObject.SetActive(false);
                }
                m_IconicBtns[i].SetIconBtnData(characterData, spriteUI, targetChannel);
                m_IconicBtns[i].InitCallback(auxEnum, SetChannelDetails);
            }

            int vcaIndex = 1;
            for (int i = CHANNEL_GROUP_SIZE; i < TOTAL_PHYSICAL_SLIDERS; i++)
            {
                string headName = $"VCA{vcaIndex}{TXT_EXTENSION}";
                Sprite sprite = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == (headName).ToLower());

                PhysicalConsole.Channel item = PhysicalConsole.Instance.ChannelList[i];
                item.MusicianImageRenderer.gameObject.SetActive(true);

                if (sprite == null)
                {
                    Debug.LogError($"Cannot find headName: {headName}, trying backup: {headName}");
                }
                item.MusicianImageRenderer.material.mainTexture = sprite.texture;



                item.MusicianColourRenderer.gameObject.SetActive(false);

                string colourName = $"VCA{vcaIndex}_Bg";
                Sprite spriteColour = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == (colourName).ToLower());
                if (spriteColour == null)
                {
                    Debug.LogError($"Cannot find colourName: {colourName}");
                }
                else
                {
                    item.MusicianColourImageRenderer.material.mainTexture = spriteColour.texture;
                }

                item.MusicianColourImageRenderer.gameObject.SetActive(true);

                vcaIndex++;
            }

            ConsoleData.Instance.ChangeInputDataChannelGroup((ChannelGroupEnum)channelGroup);
        }

        public void SetIconsActive(bool state)
        {
            m_ChannelsIcons.VisibleAndInteractive(state);
        }

        public void UpdateData()
        {
            if ((null != ConsoleData.Instance) && (null != m_TrackDataMusicianMaster))
            {
                if (ConsoleData.Instance.GetCurrentInputAuxData().Count >= CHANNEL_GROUP_SIZE)
                {
                    int index = ConsoleData.Instance.GetCurrentInputAuxData().FindLastIndex(e => e.m_AuxEnum == m_CurrentAuxEnum);
                    if (index != -1)
                    {
                        SetInputData(ConsoleData.Instance.GetCurrentInputAuxData()[index]);
                    }
                    else
                    {
                        SetInputDataMinimum();
                    }
                }
            }
        }

        private void SetPhantonPower(bool state) => m_Tgl40V.isOn = state;
        private void SetPhase(bool state) => m_TglPhase.isOn = state;

        private void SetTrimLevel(float level) => m_Trim.value = level;
        private void SetGainLevel(float level) => m_Gain.value = level;

        private void SetChannelFillerLevel(float level) => m_ChannelLevel.fillAmount = level;
        private void SetChannelSlider(float level) => m_LevelSlider.value = level;

        private void SetInputData(TrackDataMusician data) => SetChannelSlider(data.GetPercentageLevel());
        private void SetInputDataMinimum() => SetChannelSlider(0);


        private void Update() // TODO make it manual in part and loop though
        {
            if (null != m_AudioSampling && null != m_TrackDataMusicianMaster)
            {
                SetChannelFillerLevel(m_AudioSampling.FreqBandTotal * m_TrackDataMusicianMaster.GetPercentageLevel());
            }
        }
    }
}