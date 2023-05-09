using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using UnityEngine;
using UnityEngine.Playables;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    [Serializable]
    public class BandState
    {
        public string m_sTime = "00:00:00";
        public MusicianTypeEnum m_Musician = MusicianTypeEnum.Vocals;
        public string m_sState = "";

        [NonSerialized]
        public DateTime m_DateTime = new DateTime();
    }

    [Serializable]
    public class SongBandStates
    {

        public List<BandState> m_BandStates = new List<BandState>
        {
            new BandState
            {
                m_sState = "Test"
            },
            new BandState
            {
                m_sTime = "00:01:13",
                m_Musician = MusicianTypeEnum.Bass,
                m_sState = "Play"
            }
        };
    }

    public class BandManager : MonoBehaviour
    {
        private const string BAND_TIMING = "_BAND_TIMING";


        public static BandManager Instance;

        public class BandGrouping
        {
            public MusicianTypeEnum m_MusicianTypeEnum;
            public Transform m_Transform;

            public BandGrouping (MusicianTypeEnum musicianTypeEnum, Transform trans)
            {
                m_MusicianTypeEnum = musicianTypeEnum;
                m_Transform = trans;
            }
        }

        //private List<BandGrouping> m_BandMembers = new List<BandGrouping>();

        private Dictionary<MusicianTypeEnum, Animator> m_MusicianAnimators = new Dictionary<MusicianTypeEnum, Animator>();


        private List<BandState> m_BandStates;

        private Coroutine m_BandStatesCoroutine = null;

        private DateTime m_Timer;

        private float m_fCrossfadeTime = 0.5f;


        public void Initialise()
        {
            Instance = this;

            // Dev, create example JSON
            //Json.FullSerialiser.WriteToFile(m_SongBandStates, Application.streamingAssetsPath + "\\Band States Test", true);

            // TODO: Find and turn on the correct musicians
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Bass, this.transform.SearchComponent<Transform>("BassMan")));
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.LeadGuitar, this.transform.SearchComponent<Transform>("GuitarMan")));

            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Vocals, this.transform.SearchComponent<Transform>("Singer")));
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Vocals, this.transform.SearchComponent<Transform>("Quest_Aidan_Singer")));
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Vocals, this.transform.SearchComponent<Transform>("Quest_Ken_Singer")));

            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Synth1, this.transform.SearchComponent<Transform>("KeyboardLady")));
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Synth2, this.transform.SearchComponent<Transform>("KeyboardLady")));
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.RhythmGuitar, this.transform.SearchComponent<Transform>("GuitarLady")));
            //m_BandMembers.Add(new BandGrouping(MusicianTypeEnum.Drums, this.transform.SearchComponent<Transform>("Drummer")));

            //TurnOffBand();

            // Add the animators for the states!
            //////m_MusicianAnimators = new Dictionary<MusicianTypeEnum, Animator>
            //////{
            //////    { MusicianTypeEnum.Vocals, this.transform.SearchComponent<Transform>("Quest_Aidan_Singer").GetComponent<Animator>() },
            //////    { MusicianTypeEnum.Saxaphone, this.transform.SearchComponent<Transform>("Quest_SaxophonePlayer").GetComponent<Animator>() },
            //////    { MusicianTypeEnum.LeadGuitar, this.transform.SearchComponent<Transform>("Quest_GuitarPlayer").GetComponent<Animator>() },
            //////    { MusicianTypeEnum.Bass, this.transform.SearchComponent<Transform>("Quest_BassPlayer").GetComponent<Animator>() },
            //////    { MusicianTypeEnum.Synth1, this.transform.SearchComponent<Transform>("Zoe_Keyboard").GetComponent<Animator>() },
            //////    { MusicianTypeEnum.Drums, this.transform.SearchComponent<Transform>("Ken_Drummer").GetComponent<Animator>() }
            //////};

        }

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            Debug.Log($"ChangeToScenario {Scenario.ToString()}");

            // TODO: Load up and setup the correct config for the song/band.
            switch (Scenario)
            {
                case ScenarioEnum.Menu:
                case ScenarioEnum.Blank:
                case ScenarioEnum.TutorialPart1:
                    TurnOffBand();
                    break;

                case ScenarioEnum.Stackable:
                    Debug.LogError("Disabled turning on band for now");
                    TurnOffBand();
                    TurnOnCorrectBandMembers();
                    StartBandPlaying();
                    break;
            }
        }


        public void InitialiseSongLoadBandTiming(string songName, Action callback)
        {
            m_MusicianAnimators.Clear();

            foreach (var item in MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers)
            {
                var musician = this.transform.SearchComponent<Transform>(item.MusicianModel);
                if (musician != null)
                {
                    m_MusicianAnimators.Add((MusicianTypeEnum)item.AuxEnum, musician.GetComponent<Animator>());
                }
                else
                {
                    Debug.LogError($"Cannot find :  {item.MusicianModel}");
                }
            }


            Core.AssetBundlesRef.TextAssetBundleRef.GetItem<SongBandStates>(Core.Mono, $"{songName}{BAND_TIMING}", (songBandStates) =>
            {
                if (songBandStates != null)
                {
                    m_BandStates = songBandStates.m_BandStates;

                    // Get the actual times
                    foreach (BandState item in m_BandStates)
                    {
                        item.m_DateTime = DateTime.Parse(item.m_sTime);
                    }

                    // Order the list in ascending order
                    m_BandStates = m_BandStates.OrderBy(x => x.m_DateTime.Ticks).ToList();
                }
                else
                {
                    m_BandStates = new List<BandState>();
                    Debug.LogError($"no band animations for :{songName}");
                }
                callback?.Invoke();
            });
        }

        private void TurnOffBand()
        {
            var list = this.transform.GetDirectChildren();
            foreach (var item in list)
            {
                item.SetActive(false);
            }
        }


        private void TurnOnCorrectBandMembers()
        {
            var allRootBandMembers = this.transform.GetDirectChildren();

            var data = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef;
            Debug.LogError("TurnOnCorrectBandMembers needs doing");
            foreach (var item in data.BandMembers)
            {
                var musician = allRootBandMembers.Find(e => e.name == item.MusicianModel);
                if (musician != null)
                {
                    musician.SetActive(true);
                    musician.localPosition = item.Position;
                    musician.transform.localRotation = Quaternion.Euler(item.Rotation);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.MusicianModel) == false)
                    {
                        Debug.LogError($"Cannot find: {item.MusicianModel}");
                    }
                }
            }
        }

        // Animation states for each band member that can currently be set.
        // SINGER: Quest_Aidan_Singer
        // - Walking
        // - Standing
        // - Singing Walking
        // - Singing
        //
        // Saxaphone Player: Quest_SaxophonePlayer
        // - Idle
        // - Playing
        // - Standing
        //
        // Guitar/Bass Player: Quest_GuitarPlayer Quest_BassPlayer
        // - Play Guitar
        // - Throw Guitar
        // - Standing
        //
        // Drummer: Ken_Drummer
        // - Idle
        // - Play Drums
        //
        // Synth 1: Zoe_Keyboard
        // - Idle
        // - Play Keyboard

        public void StartBandPlaying()
        {
            // Turn off the old band states coroutine.
            if (null != m_BandStatesCoroutine)
            {
                StopCoroutine(m_BandStatesCoroutine);
                m_BandStatesCoroutine = null;
            }

            Debug.LogError("<color=red>Band Manager: TriggerBandStates needs to be triggered at the point where the music starts playing!!!</color>", this);
            m_BandStatesCoroutine = StartCoroutine(TriggerBandStates());
        }


        private IEnumerator TriggerBandStates()
        {
            yield return new WaitForSeconds(DELAY_TO_STOP_SONG_SYNC_ISSUES);
            m_Timer = new DateTime();
            WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

            while (m_BandStates.Count > 0)
            {
                // Figure out what needs to be played.
                while ((m_BandStates[0].m_DateTime.Minute <= m_Timer.Minute) && (m_BandStates[0].m_DateTime.Second <= m_Timer.Second))
                {
                    Debug.Log($"Band State - Time: {m_BandStates[0].m_sTime} Musician: {m_BandStates[0].m_Musician} Animation: {m_BandStates[0].m_sState}");

                    // Should add error checking for musicians that aren't setup yet.
                    if(m_MusicianAnimators.ContainsKey(m_BandStates[0].m_Musician))
                    {
                        m_MusicianAnimators[m_BandStates[0].m_Musician].CrossFadeInFixedTime(m_BandStates[0].m_sState, m_fCrossfadeTime);
                    }
                    else
                    {
                        Debug.LogError($"m_MusicianAnimators does not contain: {m_BandStates[0].m_Musician}");
                    }

                    m_BandStates.RemoveAt(0);
                }

                // Wait a frame then add the frame time to the timer.
                yield return waitFrame;
                m_Timer = m_Timer.AddSeconds(Time.deltaTime);
            }

            m_BandStatesCoroutine = null;
        }
    }
}
