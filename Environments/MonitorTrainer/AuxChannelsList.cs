using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class AuxChannelsList : MonoBehaviour
    {
        [SerializeField] private Button m_BtnChannelsPrevPage;
        [SerializeField] private Button m_BtnChannelsNextPage;
        [SerializeField] private TextMeshProUGUI m_PageLabel;
        private Animator m_Animator;
        private Animator m_FlipAnimator;
        [SerializeField] private List<CanvasGroup> m_ChannelsPages = new List<CanvasGroup>();
        private List<ChannelLarge> m_Channels = new List<ChannelLarge>();
        private int m_CurrentPage = 0;
        public AuxEnum m_CurrentAuxEnum;

        [EditorButton]
        private void DEBUG_Next() => m_BtnChannelsNextPage.onClick.Invoke();

        [EditorButton]
        private void DEBUG_Prev() => m_BtnChannelsPrevPage.onClick.Invoke();

        internal void Init()
        {
            if (m_BtnChannelsPrevPage == null)
            {
                m_Animator = GetComponent<Animator>();
                m_FlipAnimator = transform.Find("Flip").GetComponent<Animator>();
                m_BtnChannelsPrevPage = transform.Find("TopBar/PgPrevious").GetComponent<Button>();
                m_BtnChannelsNextPage = transform.Find("TopBar/PgNext").GetComponent<Button>();
                m_PageLabel = transform.Find("TopBar/GroupName").GetComponent<TextMeshProUGUI>();

                m_BtnChannelsPrevPage.onClick.AddListener(() => ShowPreviousPage());
                m_BtnChannelsNextPage.onClick.AddListener(() => ShowNextPage());


                var directChildren = transform.GetDirectChildren();

                m_ChannelsPages.Clear();
                for (int i = 0; i < directChildren.Count; i++)
                {
                    if (directChildren[i].name.StartsWith("Channels"))
                    {
                        CanvasGroup page = directChildren[i].GetComponent<CanvasGroup>();
                        page.VisibleAndInteractive(false);
                        m_ChannelsPages.Add(page);
                    }
                }

                var allChildren = transform.GetAllChildren();

                for (int k = 0; k < allChildren.Count; k++)
                {
                    if (allChildren[k].name.StartsWith("CH_Large"))
                    {
                        ChannelLarge item = allChildren[k].gameObject.ForceComponent<ChannelLarge>();
                        item.Init();
                        m_Channels.Add(item);
                    }
                }
            }
            //this should be replaced by :
            //Core.Environment.OnEnvironmentLoadingComplete += UpdateAllInitDataCoroutine;
            //and while loops removed
            InitialiseSongUpdateAllInitData();
        }

        public void SetActive(AuxEnum auxEnum, bool state)
        {
            m_Animator.SetBool("Visible", state);

            m_CurrentAuxEnum = auxEnum;

            if (state)
            {
                ShowPage(0);
            }
        }


        //Thee three functions are only to be used for manual updating when the Flip switch is off. This is because any changes on flip off are reverted when its turned back on.
        public void UpdateChannelSlider(int index, float percentage)
        {
            int auxStartIndex = (m_CurrentPage * CHANNEL_GROUP_SIZE) + index;
            auxStartIndex = Mathf.Clamp(auxStartIndex, 0, TOTAL_CHANNELS);
            m_Channels[auxStartIndex].SetSliderWithoutNotify(percentage);

            var data = ConsoleData.Instance.GetCurrentInputAuxData()[index];
            data.SetPercentageLevel(percentage);

        }

        public void UpdateChannelMute(int index, bool mute)
        {
            int auxStartIndex = (m_CurrentPage * CHANNEL_GROUP_SIZE) + index;
            auxStartIndex = Mathf.Clamp(auxStartIndex, 0, TOTAL_CHANNELS);
            m_Channels[auxStartIndex].SetMuteWithoutNotify(mute);

            var data = ConsoleData.Instance.GetCurrentInputAuxData()[index];
            data.m_Mute = mute;
        }

        public void UpdateChannelSolo(int index, bool solo)
        {
            int auxStartIndex = (m_CurrentPage * CHANNEL_GROUP_SIZE) + index;
            auxStartIndex = Mathf.Clamp(auxStartIndex, 0, TOTAL_CHANNELS);
            m_Channels[auxStartIndex].SetSoloWithoutNotify(solo);

            var data = ConsoleData.Instance.GetCurrentInputAuxData()[index];
            data.m_Solo = solo;
        }

        public void SetAuxData(int index, int consoleIndex, TrackDataMusician data, TrackDataMusician masterData)
        {
            m_Channels[index].InitCallBacks(data, masterData, (value) =>
            {
                if (IsValidFlipOnState() == true)
                {
                    data.SetPercentageLevel(value);
                    PhysicalConsole.Instance.SetSliderInstantDb((ConsoleSliderGroupEnum)consoleIndex, data.GetDBLevel());
                }
                else
                {
                    PhysicalConsole.Instance.SetSliderInstantPercentage((ConsoleSliderGroupEnum)consoleIndex, value);
                }
            }, (value) =>
            {
                if (IsValidFlipOnState() == true)
                {
                    data.m_Mute = value;
                }
                int muteIndex = (int)ConsoleButtonEnum.Mute1;
                muteIndex += consoleIndex;
                PhysicalConsole.Instance.SetButtonInstant((ConsoleButtonEnum)muteIndex, value ? VrInteractionButtonLatched.ButtonStateEnum.Down : VrInteractionButtonLatched.ButtonStateEnum.Up);
            }, (value) =>
            {
                if (IsValidFlipOnState() == true)
                {
                    data.m_Solo = value;
                }
                int soloIndex = (int)ConsoleButtonEnum.Solo1;
                soloIndex += consoleIndex;
                PhysicalConsole.Instance.SetButtonInstant((ConsoleButtonEnum)soloIndex, value ? VrInteractionButtonLatched.ButtonStateEnum.Down : VrInteractionButtonLatched.ButtonStateEnum.Up);
            });
        }

        public void ToggleFlip(bool state)
        {
            if (m_FlipAnimator != null)
            {
                m_FlipAnimator.SetBool("Flipped", state);
            }
        }



        private void ShowPreviousPage()
        {
            int newPage = m_CurrentPage.ClampDecrement(m_ChannelsPages.Count);
            ShowPage(newPage);
        }

        private void ShowNextPage()
        {
            int newPage = m_CurrentPage.ClampIncrement(m_ChannelsPages.Count);
            ShowPage(newPage);
        }

        private void ShowPage(int newPage)
        {
            m_ChannelsPages[m_CurrentPage].VisibleAndInteractive(false);
            m_ChannelsPages[newPage].VisibleAndInteractive(true);


            m_PageLabel.SetText(m_ChannelsPages[newPage].gameObject.name);

            m_CurrentPage = newPage;
            UpdateData();
        }

        public void UpdateData()
        {
            int auxStartIndex = m_CurrentPage * CHANNEL_GROUP_SIZE;
            auxStartIndex = Mathf.Clamp(auxStartIndex, 0, TOTAL_CHANNELS);
            ConsoleData.Instance.ChangeAuxDataChannelGroup(m_CurrentAuxEnum, (ChannelGroupEnum)m_CurrentPage);
            if (ConsoleData.Instance.GetCurrentInputAuxData().Count >= CHANNEL_GROUP_SIZE)
            {
                for (int i = 0; i < CHANNEL_GROUP_SIZE; i++)
                {
                    var data = ConsoleData.Instance.GetCurrentInputAuxData()[i];
                    CharacterDataClass characterData = MonitorTrainerConsts.GetCharacterData(data.m_AuxEnum);
                    Sprite sprite = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name == (characterData.CharacterName.ToString() + TXT_EXTENSION));

                    PhysicalConsole.Channel item = PhysicalConsole.Instance.ChannelList[i];
                    if (null != sprite)
                    {
                        item.MusicianImageRenderer.gameObject.SetActive(true);
                        item.MusicianImageRenderer.material.mainTexture = sprite.texture;
                        item.MusicianColourRenderer.gameObject.SetActive(true);
                        item.MusicianColourRenderer.material.SetColor("_Color", characterData.Colour);
                    }
                    else
                    {
                        item.MusicianImageRenderer.gameObject.SetActive(false);
                        item.MusicianColourRenderer.gameObject.SetActive(false);
                    }

                    SetAuxData(auxStartIndex + i, i, data, ConsoleData.Instance.m_GlobalInputData[auxStartIndex + i]);
                }


                int vcaIndex = 1;
                for (int i = CHANNEL_GROUP_SIZE; i < TOTAL_PHYSICAL_SLIDERS; i++)
                {
                    Sprite sprite = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == ($"VCA{vcaIndex}{TXT_EXTENSION}".ToLower()));

                    PhysicalConsole.Channel item = PhysicalConsole.Instance.ChannelList[i];
                    item.MusicianImageRenderer.gameObject.SetActive(true);
                    item.MusicianImageRenderer.material.mainTexture = sprite.texture;
                    item.MusicianColourRenderer.gameObject.SetActive(false);

                    Sprite spriteColour = ConsoleScreenManager.Instance.m_AllIcons.FindLast(e => e.name.ToLower() == ($"VCA{vcaIndex}_Bg".ToLower()));
                    item.MusicianColourImageRenderer.material.mainTexture = spriteColour.texture;
                    item.MusicianColourImageRenderer.gameObject.SetActive(true);

                    vcaIndex++;
                }

            }
        }


        private void UpdateAllData()
        {
            int startPage = m_CurrentPage;
            for (int i = 0; i < m_ChannelsPages.Count; i++)
            {
                m_CurrentPage = i;
                if (i != startPage)
                {
                    UpdateData();
                }
            }
            m_CurrentPage = startPage;
        }

        public void InitialiseSongUpdateAllInitData()
        {
            ConsoleData.Instance.SetStartData();
            UpdateAllData();
            ShowPage(0);
        }
    }
}