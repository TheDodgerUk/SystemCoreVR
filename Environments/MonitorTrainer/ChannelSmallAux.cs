using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ChannelSmallAux : ChannelSmall
    {
        private List<TrackDataMusician> m_TrackDataMusicians = new List<TrackDataMusician>();


        private void OnEnable()
        {
            // this gets turn off a lot
            if (m_Tgl != null)
            {
                m_Tgl.graphic.raycastTarget = true;
            }
        }

        public void InitAsAuxChannel(Action<bool> onTglChanged, string channelName)
        {
            base.Init();

            var toggle = transform.FindSibling("Toggle");
            m_Tgl = toggle.GetComponent<Toggle>();
            m_Tgl.onValueChanged.RemoveAllListeners();
            m_Tgl.onValueChanged.AddListener((value) => onTglChanged(value));

            m_Tgl.graphic.raycastTarget = true;
            Transform nameObj = transform.FindSibling("GrpName");
            m_ChannelTitle = nameObj.GetComponent<TextMeshProUGUI>();

            var data = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.FindLast(e => e.AuxEnum.ToString() == channelName);
            m_CharacterDataClass = data;


            m_ChannelName = channelName;

            SetupData();
        }

        protected override void InitialiseData()
        {
            var data = ConsoleData.Instance.m_GlobalAuxData.GetList(m_CharacterDataClass.AuxEnum);
            foreach (var item in data)
            {
                if (item.m_AuxEnum.IsMusician() == true)
                {
                    m_TrackDataMusicians.Add(item);
                }
            }
            m_ChannelTitle.fontSize = 35f;
            m_ChannelTitle.SetText(m_CharacterDataClass.Abbreviation);
        }

        public override void ManualUpdate()
        {
            if (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Aux)
            {
                if (null != m_TrackDataMusicianInput && null != m_AudioSampling)
                {
                    SetLevel(CollectAverageData() * m_AudioSampling.FreqBandTotal);
                }
            }
        }

        private float CollectAverageData()
        {
            float average = 0;
            for (int i = 0; i < m_TrackDataMusicians.Count; i++)
            {
                average += m_TrackDataMusicians[i].GetPercentageLevel();
            }
            average /= m_TrackDataMusicians.Count;
            return average;
        }


        public void Deselect()
        {
            m_Tgl.SetIsOnWithoutNotify(false);
        }
    }
}