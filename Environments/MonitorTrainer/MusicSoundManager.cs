using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class MusicSoundManager : MonoBehaviour
    {
        public static MusicSoundManager Instance;

        public SimpleDictionaryList<MusicianTypeEnum, AudioSource> m_Tracks = new SimpleDictionaryList<MusicianTypeEnum, AudioSource>();
        public Dictionary<MusicianTypeEnum, AudioSampling> m_MusicianAudioSamplings = new Dictionary<MusicianTypeEnum, AudioSampling>();
        private SoundMixer m_SoundMixer;

        public bool HasInitialised { get; private set; }

        private IEnumerator m_SongFinishedCoroutine;

        public void Initialise()
        {
            Instance = this;

            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;
            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        }

        #region setup

        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;
            m_SoundMixer = new SoundMixer(this, m_MusicianAudioSamplings, () =>
            {
                HasInitialised = true;
            });
            
        
        }

        public void InitialiseSongTrackData()
        {
            List<TrackDataMusician> data = new List<TrackDataMusician>();
            foreach (ConsoleSliderGroupEnum consoleSlider in Enum.GetValues(typeof(ConsoleSliderGroupEnum)))
            {
                data.Add(new TrackDataMusician(AuxEnum.Bass));
            }
            InitialiseSetCurrentInputAuxData(data);        
        }

        private void InitialiseSetCurrentInputAuxData(List<TrackDataMusician> data)
        {
            ConsoleData.Instance.SetCurrentInputAuxData(data);
            SetMasterMixerAmpToMin();
        }

        private void SetMasterMixerAmpToMin()
        {
            ConsoleData.Instance.m_InputAuxType = InputAuxTypeEnum.Input;
            foreach (ConsoleSliderGroupEnum musicianType in Enum.GetValues(typeof(ConsoleSliderGroupEnum)))
            {
                ConsoleData.Instance.SetSoundLevelPercentage(musicianType, 0);
            }
        }

        private void ScenarioBlank()
        {
            foreach (var item in m_Tracks.GetKeys())
            {
                foreach (var source in m_Tracks.GetList(item))
                {
                    source.Stop();
                }
            }
        }

        private void Scenario1()
        {
            foreach (var item in m_Tracks.GetKeys())
            {
                foreach (var source in m_Tracks.GetList(item))
                {
                    source.Stop();
                }
            }
        }

        private void StartSong()
        {
            m_SoundMixer.m_MasterMixerLeft.SetFloat(MASTER_CHANNEL, SPEAKER_DECIBEL_LEVEL);
            m_SoundMixer.m_MasterMixerRight.SetFloat(MASTER_CHANNEL, SPEAKER_DECIBEL_LEVEL);
            //m_SoundMixer.m_MasterMixerRight.SetFloat(MASTER_CHANNEL, DECIBEL_MIN);

            this.WaitForReal(DELAY_TO_STOP_SONG_SYNC_ISSUES, () =>
            {
                List<AudioSource> allSounds = m_Tracks.GetListAll();
                foreach (var source in allSounds)
                {
                    source.Stop();
                    source.time = 0;
                    source.Play();
                    m_SongFinishedCoroutine = SongFinished();
                    StartCoroutine(m_SongFinishedCoroutine);
                }                
            });
        }


        public enum SourceState
        {
            NotStarted,
            Started,
            Finished,
        }

        private IEnumerator SongFinished()
        {
            yield return new WaitForSeconds(5);
            List<AudioSource> allSounds = m_Tracks.GetListAll();
            List<SourceState> started = new List<SourceState>();
            foreach (var item in allSounds)
            {
                started.Add(SourceState.NotStarted);
            }
            while(MonitorTrainerRoot.Instance.CurrentScenario == ScenarioEnum.Stackable)
            {
                int finishedCount = 0;
                yield return new WaitForSeconds(1);
                for(int i = 0; i < allSounds.Count; i++)
                {
                    switch (started[i])
                    {
                        case SourceState.NotStarted:
                            if(allSounds[i].time != 0)
                            {
                                started[i] = SourceState.Started;
                            }
                            break;
                        case SourceState.Started:
                            if (allSounds[i].time == 0)
                            {
                                started[i] = SourceState.Finished;
                            }
                            break;
                        case SourceState.Finished:
                            finishedCount++;
                            break;

                        default:
                            break;
                    }
                    if(finishedCount == allSounds.Count)
                    {
                        MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.SongFinishedCompleted;
                    }
                }

            }
        }

        public void PauseItem(bool pause)
        {
            List<AudioSource> allSounds = m_Tracks.GetListAll();
            foreach (var source in allSounds)
            {
                if(pause == true)
                {
                    source.Pause();
                }
                else
                {
                    source.UnPause();
                }
            }
        }

        #endregion setup

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Blank:
                case ScenarioEnum.Menu:
                    ScenarioBlank();
                    StopSongFinishedCoroutine();                    
                    break;

                case ScenarioEnum.TutorialPart1:
                    ConsoleData.Instance.ClearForcedMuteTasks();
                    ScenarioBlank();
                    break;

                case ScenarioEnum.Stackable:
                    ConsoleData.Instance.ClearForcedMuteTasks();
                    StartSong();
                    break;
            }
        }

        private void StopSongFinishedCoroutine()
        {
            if(m_SongFinishedCoroutine != null)
            {
                StopCoroutine(m_SongFinishedCoroutine);
                m_SongFinishedCoroutine = null;
            }
        }

        public void CheckTrackDataAndMute()
        {
            if(HasInitialised == false)
            {
                return;
            }
            if (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Input)
            {
                CheckInputTrackDataAndSetMuteSoloEct();
            }
            else
            {
                foreach (var item in ConsoleData.Instance.m_ConsoleMuteButtons)
                {
                    switch (item.Key)
                    {
                        case ConsoleButtonEnum.MuteGroupAll:
                            if (true == item.Value)
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_CHANNEL, DECIBEL_MIN);
                            }
                            else
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_CHANNEL, 0);
                            }
                            break;

                        case ConsoleButtonEnum.MuteGroupVox:
                            if (true == item.Value)
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_VOX, DECIBEL_MIN);
                            }
                            else
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_VOX, 0);
                            }
                            break;

                        case ConsoleButtonEnum.MuteGroupGuitar:
                            if (true == item.Value)
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_GUITAR, DECIBEL_MIN);
                            }
                            else
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_GUITAR, 0);
                            }
                            break;

                        case ConsoleButtonEnum.MuteGroupDrum:
                            if (true == item.Value)
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_DRUM, DECIBEL_MIN);
                            }
                            else
                            {
                                m_SoundMixer.m_MasterMixerAmp.SetFloat(MASTER_DRUM, 0);
                            }
                            break;

                        default:
                            break;
                    }
                }
                CheckInputTrackDataAndSetMuteSoloEct();
            }
            CheckForcedMuteTasks();
        }

        public void CheckTrackDataAndSolo()
        {
            if (HasInitialised == false)
            {
                return;
            }

            bool containsSolo = false;
            foreach (MusicianTypeEnum musicianType in Enum.GetValues(typeof(MusicianTypeEnum)))
            {
                var trackData = ConsoleData.Instance.GetCurrentAuxData(musicianType);
                if ((null != trackData) && (true == trackData.m_Solo))
                {
                    containsSolo = true;
                }
            }

            if (true == containsSolo)
            {
                foreach (MusicianTypeEnum musicianType in Enum.GetValues(typeof(MusicianTypeEnum)))
                {
                    var trackData = ConsoleData.Instance.GetCurrentAuxData(musicianType);
                    if (null != trackData)
                    {
                        CheckAuxTrackDataAndSetMuteSoloEct(musicianType, (false == trackData.m_Solo));
                    }
                }
            }
        }

        private void CheckInputTrackDataAndSetMuteSoloEct()
        {
            if (HasInitialised == false)
            {
                return;
            }
            foreach (var item in ConsoleData.Instance.m_GlobalInputData)
            {
                string masterChannel = $"{MASTER_CONTROL}{(int)item.m_AuxEnum + 1}"; // has to be Control1 or Control12 if i name them MasterVocals it goies weird
                if (item.m_AuxEnum.IsMusician() == true)
                {
                    if ((true == item.m_Mute))
                    {
                        m_SoundMixer.m_MasterMixerAmp.SetFloat(masterChannel, DECIBEL_MIN);
                    }
                    else
                    {
                        if (false == m_SoundMixer.m_MasterMixerAmp.SetFloat(masterChannel, item.GetDBLevel()))
                        {
                            var auxEnum = item.m_AuxEnum;
                            int aux = (int)item.m_AuxEnum + 1;
                            Debug.LogError($"AudioMixer: {m_SoundMixer.m_MasterMixerAmp.name}  cannot set mix amount {masterChannel}  {item.m_AuxEnum}");
                        }
                    }
                }
            }
        }

        private void CheckForcedMuteTasks()
        {
            foreach (AuxEnum auxChannel in Enum.GetValues(typeof(AuxEnum)))
            {
                if (auxChannel.IsMusician() == true)
                {
                    if(ConsoleData.Instance.GetForcedMuteTasks((MusicianTypeEnum)auxChannel) == true)
                    {
                        string channel = ((MusicianTypeEnum)auxChannel).ToString();
                        m_SoundMixer.m_MasterMixerAmp.SetFloat(channel, 0);
                        m_SoundMixer.m_MasterMixerLeft.SetFloat(channel, 0);
                        m_SoundMixer.m_MasterMixerRight.SetFloat(channel, 0);
                    }
                }
            }
        }

        private void CheckAuxTrackDataAndSetMuteSoloEct(MusicianTypeEnum musicianType, bool overrideMute)
        {
            if (HasInitialised == false)
            {
                return;
            }
            var auxData = ConsoleData.Instance.GetCurrentAuxData(musicianType);
            if (null != auxData)
            {
                if ((true == auxData.m_Mute) || (true == overrideMute))
                {
                    m_SoundMixer.m_MasterMixerAmp.SetFloat(musicianType.ToString(), DECIBEL_MIN);
                }
                else
                {
                    m_SoundMixer.m_MasterMixerAmp.SetFloat(musicianType.ToString(), auxData.GetDBLevel());
                }
            }
        }

        #region debugging

        private float m_SpeakersBD = SPEAKER_DECIBEL_LEVEL;
        private float m_ConsoleBD = SPEAKER_DECIBEL_LEVEL;

        [InspectorButton]
        private void ChangeConsoleUp()
        {
            m_ConsoleBD += 1;
            ChangeConsoleAmp();
        }

        [InspectorButton]
        private void ChangeConsoleDown()
        {
            m_ConsoleBD -= 1;
            ChangeConsoleAmp();
        }

        private void ChangeConsoleAmp()
        {
            Debug.LogError($"m_ConsoleBD  {m_ConsoleBD}");

            foreach (ConsoleSliderGroupEnum musicianType in Enum.GetValues(typeof(ConsoleSliderGroupEnum)))
            {
                ConsoleData.Instance.SetSoundLevelPercentage(musicianType, m_ConsoleBD);
            }
        }

        [InspectorButton]
        private void ChangeSpeakersUp()
        {
            m_SpeakersBD += 1;
            ChangeSpeakers();
        }

        [InspectorButton]
        private void ChangeSpeakersDown()
        {
            m_SpeakersBD -= 1;
            ChangeSpeakers();
        }

        private void ChangeSpeakers()
        {
            Debug.LogError($"m_SpeakersBD  {m_SpeakersBD}");
            m_SoundMixer.m_MasterMixerLeft.SetFloat(MASTER_CHANNEL, m_SpeakersBD);
            m_SoundMixer.m_MasterMixerRight.SetFloat(MASTER_CHANNEL, m_SpeakersBD);
            m_SoundMixer.m_MasterMixerRight.SetFloat(MASTER_CHANNEL, DECIBEL_MIN);
        }

        #endregion debugging
    }
}