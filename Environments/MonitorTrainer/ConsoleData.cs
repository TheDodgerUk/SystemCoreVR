using System;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ConsoleData
    {
        public static ConsoleData Instance;

        private MonoBehaviour m_Mono;
        public InputAuxTypeEnum m_InputAuxType = InputAuxTypeEnum.Input;
        public SimpleDictionaryList<AuxEnum, TrackDataMusician> m_GlobalAuxData = new SimpleDictionaryList<AuxEnum, TrackDataMusician>();
        public List<TrackDataMusician> m_GlobalInputData = new List<TrackDataMusician>();
        public Dictionary<ConsoleButtonEnum, bool> m_ConsoleMuteButtons = new Dictionary<ConsoleButtonEnum, bool>();
        private List<TrackDataMusician> m_CurrentInputAuxData = new List<TrackDataMusician>();
        private Dictionary<MusicianTypeEnum, bool> m_ForcedMuteButtons = new Dictionary<MusicianTypeEnum, bool>();

        public List<TrackDataMusician> GetCurrentInputAuxData() => m_CurrentInputAuxData;

        private List<TrackDataMusician> m_DVAInputAuxData = new List<TrackDataMusician>();

        public ConsoleData(MonoBehaviour mono)
        {
            Instance = this;

            m_Mono = mono;

            for (int i = (int)ConsoleButtonEnum.MuteGroupAll; i < Enum.GetNames(typeof(ConsoleButtonEnum)).Length; i++)
            {
                m_ConsoleMuteButtons[(ConsoleButtonEnum)i] = false;
            }
            CreatAuxData();
            ClearData();

            m_DVAInputAuxData.Clear();
            m_DVAInputAuxData.Add(new TrackDataMusician(AuxEnum.Blank1));
            m_DVAInputAuxData.Add(new TrackDataMusician(AuxEnum.Blank2));
            foreach (AuxEnum auxChannel in Enum.GetValues(typeof(AuxEnum)))
            {
                if (auxChannel.IsMusician() == true)
                {
                    m_ForcedMuteButtons.Add((MusicianTypeEnum)auxChannel, false);
                }
            }
        }

        public void SetForcedMuteTasks(MusicianTypeEnum musicianTypeEnum, bool mute)
        {
            if (m_ForcedMuteButtons.ContainsKey(musicianTypeEnum) == true)
            {
                m_ForcedMuteButtons[musicianTypeEnum] = mute;
                MusicSoundManager.Instance.CheckTrackDataAndMute();
            }
        }
        public bool GetForcedMuteTasks(MusicianTypeEnum musicianTypeEnum) =>m_ForcedMuteButtons[musicianTypeEnum];

        public void ClearForcedMuteTasks()
        {
            foreach (AuxEnum auxChannel in Enum.GetValues(typeof(AuxEnum)))
            {
                if (auxChannel.IsMusician() == true)
                {
                    m_ForcedMuteButtons[(MusicianTypeEnum)auxChannel] = false;
                }
            }
        }

        public void SetStartData()
        {
            ChangeAuxDataChannelGroup(AuxEnum.Vocals, ChannelGroupEnum.Channel1_4);
        }

        public void ClearData()
        {
            if(m_GlobalInputData.Count == 0)
            {
                Debug.LogError($"ERROR, m_GlobalInputData == 0");
            }
            foreach (var item in m_GlobalInputData)
            {
                float level = UnityEngine.Random.Range(MIN_MAX_RANDOM_PERCENTAGE_START.x, MIN_MAX_RANDOM_PERCENTAGE_START.y);

                item.SetPercentageLevel(level);
                item.m_Gain = UnityEngine.Random.Range(MIN_MAX_RANDOM_PERCENTAGE_START.x, MIN_MAX_RANDOM_PERCENTAGE_START.y);
                item.m_Trim = UnityEngine.Random.Range(MIN_MAX_RANDOM_PERCENTAGE_START.x, MIN_MAX_RANDOM_PERCENTAGE_START.y);
                item.m_Mute = false;
                item.m_Solo = false;
            }

            var keys = m_GlobalAuxData.GetKeys();
            foreach (var key in keys)
            {
                if (true == key.IsMusician())
                {
                    var values = m_GlobalAuxData[key];
                    foreach (var item in values)
                    {
                        float level = UnityEngine.Random.Range(MIN_MAX_RANDOM_PERCENTAGE_START.x, MIN_MAX_RANDOM_PERCENTAGE_START.y);
                        item.SetPercentageLevel(level);
                        item.m_Mute = false;
                        item.m_Solo = false;
                    }
                }
            }
        }

        public void RandomiseData()
        {
            foreach(var item in m_GlobalInputData)
            {              
                item.m_Solo = false;
                if (true == item.m_AuxEnum.IsMusician())
                {
                    item.m_Mute = false;
                    item.SetPercentageLevel(UnityEngine.Random.Range(0.45f, 0.55f));
                    item.m_Gain = UnityEngine.Random.Range(MIN_MAX_RANDOM_PERCENTAGE_START.x, MIN_MAX_RANDOM_PERCENTAGE_START.y);
                    item.m_Trim = UnityEngine.Random.Range(MIN_MAX_RANDOM_PERCENTAGE_START.x, MIN_MAX_RANDOM_PERCENTAGE_START.y);
                }
                else
                {
                    item.m_Mute = true;
                    item.SetPercentageLevel(0);
                    item.m_Gain = MIN_MAX_RANDOM_PERCENTAGE_START.x;
                    item.m_Trim = MIN_MAX_RANDOM_PERCENTAGE_START.x;
                }
            }

            foreach (var key in m_GlobalAuxData.GetKeys())
            {
                if (true == key.IsMusician())
                {
                    foreach (var item in m_GlobalAuxData[key])
                    {                       
                        item.m_Solo = false;
                        if (true == item.m_AuxEnum.IsMusician())
                        {
                            item.m_Mute = false;
                            item.SetPercentageLevel(UnityEngine.Random.Range(0.45f, 0.55f));
                        }
                        else
                        {
                            item.m_Mute = true;
                            item.SetPercentageLevel(0);
                        }
                    }
                }
            }
        }



        public void InitialiseSongCreateSongInputData()
        {
            m_GlobalInputData.Clear();
            for (int i = 0; i < TOTAL_CHANNELS; i++)
            {
                m_GlobalInputData.Add(new TrackDataMusician(AuxEnum.Blank1));
            }
            foreach(var item in MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers)
            {
                if (item.InputIndex < m_GlobalInputData.Count)
                {
                    m_GlobalInputData[item.InputIndex] = new TrackDataMusician(item.AuxEnum);
                }
            }

            ClearData();
        }


        private void CreatAuxData()
        {
            m_GlobalAuxData.ClearAll();
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.Vocals));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.LeadGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.Bass));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.Drums));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.Synth1));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.Synth2));
            m_GlobalAuxData.AddToList(AuxEnum.Vocals, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.LeadGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.Bass));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.Synth1));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.Synth2));
            m_GlobalAuxData.AddToList(AuxEnum.LeadGuitar, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Vocals));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Drums));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Synth1));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Synth2));
            m_GlobalAuxData.AddToList(AuxEnum.RhythmGuitar, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Vocals));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Bass));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Drums));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Bass, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.Blank1, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.Vocals));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.LeadGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.Bass));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.Drums));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Drums, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.Blank1, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.Vocals));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.LeadGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.Synth1));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.Synth2));
            m_GlobalAuxData.AddToList(AuxEnum.Synth1, new TrackDataMusician(AuxEnum.Blank1));

            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.Vocals));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.LeadGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.RhythmGuitar));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.Blank1));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.Synth1));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.Synth2));
            m_GlobalAuxData.AddToList(AuxEnum.Synth2, new TrackDataMusician(AuxEnum.Blank1));

            foreach (AuxEnum item in Enum.GetValues(typeof(AuxEnum)))
            {
                int count = m_GlobalAuxData.GetValuesList(item).Count;
                for (int i = count; i < TOTAL_CHANNELS; i++)
                {
                    m_GlobalAuxData.AddToList(item, new TrackDataMusician(AuxEnum.Blank1));
                }
            }        
        }

        public void ChangeAuxDataChannelGroup(AuxEnum aux, ChannelGroupEnum group)
        {
            List<TrackDataMusician> data = m_GlobalAuxData.GetValuesList(aux);
            int start = (int)group * CHANNEL_GROUP_SIZE;
            SetCurrentInputAuxData(data.GetRange(start, CHANNEL_GROUP_SIZE));
        }

        public void ChangeInputDataChannelGroup(ChannelGroupEnum group)
        {
            List<TrackDataMusician> data = m_GlobalInputData;
            int start = (int)group * CHANNEL_GROUP_SIZE;
            SetCurrentInputAuxData(data.GetRange(start, CHANNEL_GROUP_SIZE));
        }

        public List<TrackDataMusician> GetAuxDataChannel(AuxEnum aux)
        {
            return m_GlobalAuxData.GetValuesList(aux);
        }

        public List<TrackDataMusician> GetInputData()
        {
            return m_GlobalInputData;
        }

        public TrackDataMusician GetCurrentAuxData(MusicianTypeEnum musicianType)
        {
            foreach (var item in GetCurrentInputAuxData())
            {
                if (item.m_AuxEnum == (AuxEnum)musicianType)
                {
                    return item;
                }
            }
            return null;
        }

        public void SetCurrentInputAuxData(List<TrackDataMusician> data)
        {
            // added channel 5 and 6 
            if(data.Count == MonitorTrainerConsts.CHANNEL_GROUP_SIZE)
            {
                data.AddRange(m_DVAInputAuxData);              
            }

            List<GroupData> oldData = new List<GroupData>();
            if (GetCurrentInputAuxData().Count >= CHANNEL_GROUP_SIZE)
            {
                for (int i = 0; i < PHYSICAL_PERSON_SLIDERS; i++)
                {
                    oldData.Add(new GroupData(GetCurrentInputAuxData()[i].GetPercentageLevel(), GetCurrentInputAuxData()[i].m_Mute, GetCurrentInputAuxData()[i].m_Solo));
                }
                AssignAndMoveToNewData(data, oldData);
            }
            else
            {
                // safety check
                m_CurrentInputAuxData = data;
            }
        }

        public void AssignAndMoveToNewData(List<TrackDataMusician> data, List<GroupData> oldData)
        {
            List<float> newSliderPercentageLevels = new List<float>();
            for (int i = 0; i < data.Count; i++)
            {
                newSliderPercentageLevels.Add(data[i].GetPercentageLevel());
            }
            m_CurrentInputAuxData = data;

            m_Mono.CancelAllTweens(); // cancel any previous SetSoundDataAndLerpPhysicalDB

            int min = Mathf.Min(oldData.Count, data.Count);
            for (int i = 0; i < min; i++)
            {
                ConsoleButtonEnum buttonSolo = (ConsoleButtonEnum)(((int)ConsoleButtonEnum.Solo1) + i);
                ConsoleButtonEnum buttonMute = (ConsoleButtonEnum)(((int)ConsoleButtonEnum.Mute1) + i);
                ConsoleSliderGroupEnum slider = (ConsoleSliderGroupEnum)i;

                SetSoundLevelPercentage(slider, newSliderPercentageLevels[i]);
                SetSoundDataAndLerpPhysicalPercentage(slider, oldData[i].Slider, newSliderPercentageLevels[i]);
                ConsoleScreenManager.Instance.UpdateSpecificAuxSlider(i, newSliderPercentageLevels[i]);

                SetConsoleButtonData(buttonSolo, data[i].m_Solo);
                PhysicalConsole.Instance.SetButtonInstant(buttonSolo, data[i].m_SoloState);
                ConsoleScreenManager.Instance.UpdateSpecificAuxSolo(i, data[i].m_Solo);

                SetConsoleButtonData(buttonMute, data[i].m_Mute);
                PhysicalConsole.Instance.SetButtonInstant(buttonMute, data[i].m_MuteState);
                ConsoleScreenManager.Instance.UpdateSpecificAuxMute(i, data[i].m_Mute);

            }
            MusicSoundManager.Instance.CheckTrackDataAndMute();
            MusicSoundManager.Instance.CheckTrackDataAndSolo();
        }

        public void SetMute(ConsoleSliderGroupEnum consoleSliderGroup, bool mute)
        {
            if ((int)consoleSliderGroup < GetCurrentInputAuxData().Count)
            {
                GetCurrentInputAuxData()[(int)consoleSliderGroup].m_Mute = mute;
                MusicSoundManager.Instance.CheckTrackDataAndMute();
                MusicSoundManager.Instance.CheckTrackDataAndSolo();
            }
            else
            {
                Debug.LogError($"CurrentInputAuxData not found  { (int)consoleSliderGroup}");
            }
        }

        public void SetSolo(ConsoleSliderGroupEnum consoleSliderGroup, bool solo)
        {
            if ((int)consoleSliderGroup < GetCurrentInputAuxData().Count)
            {
                GetCurrentInputAuxData()[(int)consoleSliderGroup].m_Solo = solo;
                MusicSoundManager.Instance.CheckTrackDataAndMute();
                MusicSoundManager.Instance.CheckTrackDataAndSolo();
            }
            else
            {
                Debug.LogError($"CurrentInputAuxData not found  { (int)consoleSliderGroup}");
            }
        }

        public void SetSoundLevelPercentage(ConsoleSliderGroupEnum consoleSliderGroup, float percentage)
        {
            if ((int)consoleSliderGroup < GetCurrentInputAuxData().Count)
            {
                GetCurrentInputAuxData()[(int)consoleSliderGroup].SetPercentageLevel(percentage);
                MusicSoundManager.Instance.CheckTrackDataAndMute();
                MusicSoundManager.Instance.CheckTrackDataAndSolo();
            }
            else
            {
                Debug.LogError($"CurrentInputAuxData not found  { (int)consoleSliderGroup}");
            }
        }

        public void SetConsoleButtonData(ConsoleButtonEnum consoleButton, bool mute)
        {
            m_ConsoleMuteButtons[consoleButton] = mute;
            MusicSoundManager.Instance.CheckTrackDataAndMute();
        }

        // real sound is instant, but Physical sliders need to move slower
        public void SetSoundDataAndLerpPhysicalPercentage(ConsoleSliderGroupEnum consoleSlider, float from, float to)
        {
            m_Mono.Create<ValueTween>(LERP_PERCENTAGE_TIMMING, EaseType.SineInOut, () =>
            {
                PhysicalConsole.Instance.SetSliderInstantPercentageNoCallback(consoleSlider, to);
            }).Initialise(from, to, (f) =>
            {
                PhysicalConsole.Instance.SetSliderInstantPercentageNoCallback(consoleSlider, f);
            });
        }
    }
}