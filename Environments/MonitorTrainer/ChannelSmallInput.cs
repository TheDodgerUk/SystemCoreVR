using System.Collections;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ChannelSmallInput : ChannelSmall
    {
        public void InitAsInputChannel(int channel, float defaultLevel = 0f)
        {
            base.Init();

            m_Channel = channel;
            SetLevel(defaultLevel);
        }

        public void SetData(CharacterDataClass data)
        {
            m_CharacterDataClass = data;
            SetupData();
        }

        public override void ManualUpdate()
        {
            if (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Input)
            {
                if (null != m_TrackDataMusicianInput && null != m_AudioSampling)
                {
                    SetLevel(m_TrackDataMusicianInput.GetPercentageLevel() * m_AudioSampling.FreqBandTotal);
                }
            }
        }
    }
}