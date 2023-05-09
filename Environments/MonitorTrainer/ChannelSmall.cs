using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ChannelSmall : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI m_ChannelTitle;
        [SerializeField] protected Image m_Level;
        [SerializeField] protected Image m_MusicianColour;
        [SerializeField] protected Toggle m_Tgl;
        public int ChannelNum => m_Channel;
        protected int m_Channel = -1;

        public string ChannelName => m_ChannelName;
        protected string m_ChannelName = "";

        protected CharacterDataClass m_CharacterDataClass;
        protected TrackDataMusician m_TrackDataMusicianInput;
        protected AudioSampling m_AudioSampling;

        protected void Init()
        {
            m_Level = transform.Find("Level").GetComponent<Image>();
            m_MusicianColour = transform.Find("Colour").GetComponent<Image>();
            m_MusicianColour.gameObject.SetActive(false);

            SetLevel(0);
        }

        public void SetupData()
        {

            if (null != m_CharacterDataClass && m_CharacterDataClass.AuxEnum.IsMusician() == true)
            {
                if(MusicSoundManager.Instance.m_MusicianAudioSamplings.Count == 0)
                {
                    Debug.LogError($"Error MusicSoundManager.Instance.m_MusicianAudioSamplings == 0");
                }


                m_TrackDataMusicianInput = ConsoleData.Instance.m_GlobalInputData.FindLast((e) => e.m_AuxEnum == m_CharacterDataClass.AuxEnum);
                m_AudioSampling = MusicSoundManager.Instance.m_MusicianAudioSamplings.Get((MusicianTypeEnum)m_CharacterDataClass.AuxEnum);
                m_MusicianColour.color = m_CharacterDataClass.Colour;
                m_MusicianColour.gameObject.SetActive(true);

                InitialiseData();
            }
        }

        protected virtual void InitialiseData()
        { 
        
        }
        public void SetLevel(float amount)
        {
            m_Level.fillAmount = amount;
        }

        public virtual void ManualUpdate()
        {
        }
    }
}