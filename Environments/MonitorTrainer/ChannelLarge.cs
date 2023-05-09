using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ChannelLarge : MonoBehaviour
    {
        [SerializeField] private Toggle m_TglMute;
        [SerializeField] private Toggle m_TglSolo;
        [SerializeField] private Image m_Level;
        [SerializeField] private Slider m_Slider;
        [SerializeField] private Image m_DetailsBackground;
        [SerializeField] private TextMeshProUGUI m_DetailsLabel;
        private AudioSampling m_AudioSampling;
        private TrackDataMusician m_TrackDataMusician;
        private TrackDataMusician m_TrackDataMusicianMaster;

        private int m_Channel;
        internal void Init()
        {
            m_TglMute = transform.Search("TglMute").GetComponent<Toggle>();
            m_TglSolo = transform.Search("TglSolo").GetComponent<Toggle>();

            m_Level = transform.Search("Level").GetComponent<Image>();
            m_Slider = transform.Search("Slider").GetComponent<Slider>();

            m_DetailsBackground = transform.Search("Details").GetComponent<Image>();
            m_DetailsLabel = transform.Search("Details/Label").GetComponent<TextMeshProUGUI>();
        }


        internal void InitCallBacks(TrackDataMusician data, TrackDataMusician masterData, Action<float> onSliderChanged, Action<bool> tglMute, Action<bool> tglSolo)
        {
            m_TrackDataMusician = data;
            m_TrackDataMusicianMaster = masterData;
            m_AudioSampling = MusicSoundManager.Instance.m_MusicianAudioSamplings.Get((MusicianTypeEnum)data.m_AuxEnum);

            m_TglMute.onValueChanged.RemoveAllListeners();
            m_TglSolo.onValueChanged.RemoveAllListeners();
            m_Slider.onValueChanged.RemoveAllListeners();

            CharacterDataClass characterData = MonitorTrainerConsts.GetCharacterData(data.m_AuxEnum);
            if (characterData != null)
            {
                if (characterData.Abbreviation.ContainsLetters() == true)
                {
                    m_DetailsBackground.SetActive(true);
                    SetDetails(characterData.Colour, characterData.Abbreviation);
                }
                else
                {
                    m_DetailsBackground.SetActive(false);
                }
            }
            else
            {
                m_DetailsBackground.SetActive(false);
            }

            SetSoloWithoutNotify(data.m_Solo);
            SetMuteWithoutNotify(data.m_Mute);
            SetSliderWithoutNotify(data.GetPercentageLevel());

            m_Slider.onValueChanged.RemoveAllListeners();
            m_TglMute.onValueChanged.RemoveAllListeners();
            m_TglSolo.onValueChanged.RemoveAllListeners();

            m_Slider.onValueChanged.AddListener((value) => onSliderChanged(value));
            m_TglMute.onValueChanged.AddListener((value) => tglMute(value));
            m_TglSolo.onValueChanged.AddListener((value) => tglSolo(value));

            if(null == m_AudioSampling)
            {
                m_Level.fillAmount = 0;
            }
        }

        public void SetSliderWithoutNotify(float val) =>m_Slider.SetValueWithoutNotify(val);
        public void SetMuteWithoutNotify(bool val) => m_TglMute.SetIsOnWithoutNotify(val);
        public void SetSoloWithoutNotify(bool val) => m_TglSolo.SetIsOnWithoutNotify(val);

        public void SetDetails(Color backgroundCol, string channel)
        {
            m_DetailsBackground.color = backgroundCol;
            m_DetailsLabel.SetText(channel);
        }
        private void Update()
        {
            if (null != m_AudioSampling)
            {
                float fillLevel = m_TrackDataMusicianMaster.GetPercentageLevel() * m_TrackDataMusician.GetPercentageLevel();
                m_Level.fillAmount = fillLevel * m_AudioSampling.FreqBandTotal;
            }
        }
    }
}

