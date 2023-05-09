using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class InputsNavGroup : MonoBehaviour
    {
        [SerializeField] public Toggle m_Tgl;
        private List<ChannelSmallInput> m_InputChannels = new List<ChannelSmallInput>();
        private int m_Group = 0;

        internal void Init(Action<bool, int> selection, int group)
        {
            m_Group = group;
            ChannelSmallInput channel_A = transform.Find("CH_Small 1").gameObject.ForceComponent<ChannelSmallInput>();
            ChannelSmallInput channel_B = transform.Find("CH_Small 2").gameObject.ForceComponent<ChannelSmallInput>();
            ChannelSmallInput channel_C = transform.Find("CH_Small 3").gameObject.ForceComponent<ChannelSmallInput>();
            ChannelSmallInput channel_D = transform.Find("CH_Small 4").gameObject.ForceComponent<ChannelSmallInput>();

            channel_A.InitAsInputChannel(((group - 1) * 4) + 1);
            channel_B.InitAsInputChannel(((group - 1) * 4) + 2);
            channel_C.InitAsInputChannel(((group - 1) * 4) + 3);
            channel_D.InitAsInputChannel(((group - 1) * 4) + 4);

            m_InputChannels.Add(channel_A);
            m_InputChannels.Add(channel_B);
            m_InputChannels.Add(channel_C);
            m_InputChannels.Add(channel_D);

            m_Tgl = transform.Find("TglGroup").GetComponent<Toggle>();
            if (m_Tgl != null)
            {
                m_Tgl.onValueChanged.RemoveAllListeners();
                m_Tgl.onValueChanged.AddListener((value) => selection(value, group));
            }

        }

        [EditorButton]
        private void DEBUG_Select() => m_Tgl.onValueChanged.Invoke(true);

        public void InitiliseSongData()
        {
            for (int i = 0; i < CHANNEL_GROUP_SIZE; i++)
            {
                int index = i;
                SetupData(index, m_Group);
            }
        }

        private void SetupData(int i, int group)
        {
            int index = ((group - 1) * CHANNEL_GROUP_SIZE) + i;
            AuxEnum auxEnum = ConsoleData.Instance.m_GlobalInputData[index].m_AuxEnum;
            
            var data = MonitorTrainerConsts.GetCharacterData(auxEnum);
            m_InputChannels[i].SetData(data);
        }

        public void ManualUpdate()
        {
            foreach (var item in m_InputChannels)
            {
                item.ManualUpdate();
            }
        }
    }
}