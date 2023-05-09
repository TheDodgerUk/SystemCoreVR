using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class CrowdAndGenericRockSoundManager : MonoBehaviour
    {
        public static CrowdAndGenericRockSoundManager Instance;

        private const string CROWD = "crowd";
        private List<Transform> m_Positions = new List<Transform>();
        private List<AudioSource> m_CheerOneShot = new List<AudioSource>();
        private List<AudioSource> m_CheerOneShotGroup = new List<AudioSource>();


        private List<AudioSource> m_BooOneShotGroup = new List<AudioSource>();
        private List<AudioSource> m_ClapOneShotGroup = new List<AudioSource>();

        private AudioSource m_ChatterLoud = new AudioSource();
        private AudioSource m_GenericRock = new AudioSource();
        private bool m_IsGenericRockPlaying = true;
        private bool m_ChatterLoudPlaying = true;

        private Dictionary<CrowdEnum, AudioSource> m_CrowdGroup = new Dictionary<CrowdEnum, AudioSource>();

        private IEnumerator m_PlayClapBuildUp;
        private IEnumerator m_Scenario1Group;
        private IEnumerator m_Scenario1Single;

        private IEnumerator m_Scenario2Cheer;
        private IEnumerator m_Scenario2Boo;

        [SerializeField] private readonly Vector2 CROWD_NOISE_LEVEL_SCENARIO_1 = new Vector2(0.2f, 0.5f);
        [SerializeField] private readonly Vector2 CROWD_NOISE_LEVEL_SCENARIO_2 = new Vector2(0.93f, 1f);
        [SerializeField] private const float GenericRockVolume = 0.5f;
        [SerializeField] private const float CrowdChatterVolume = 1f;
        private Vector2 m_CurrentCrowdNoiseLevel;

        private float m_ChanceOfCheer = 100;
        private float m_ChanceOfBoo = 0;


        [SerializeField] private RatingEnum m_CurrentRatingEnum = RatingEnum.Amazing;
        [SerializeField] private RatingEnum m_PrevRatingEnum = RatingEnum.Amazing;

        public AudioMixer m_CrowdMixer { get; private set; }

        private enum CrowdEnum
        {
            AmazingToNeutral,
            AngryToNeutral,
            NeutralToAmazing,
            NeutralToAngry    
        }


        public void Initialise()
        {
            Instance = this;
            m_CurrentCrowdNoiseLevel = CROWD_NOISE_LEVEL_SCENARIO_1;
            m_Positions = this.transform.GetDirectChildren();
            m_Positions.RemoveAll(e => !e.name.Contains("Position"));

            m_CrowdMixer = Resources.Load<AudioMixer>("MonitorTrainerCrowd");
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItemList(this, CROWD, (crowdItems) =>
            {
                foreach (var crowd in crowdItems)
                {
                    Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, crowd, (clip) =>
                    {
                        GameObject newItem = new GameObject(crowd);
                        AudioSource audioSourceRef = newItem.ForceComponent<AudioSource>();
                        audioSourceRef.clip = clip;
                        audioSourceRef.spatialBlend = 1; // make it 3d
                                                         // better a standard audioSourceRef.rolloffMode = AudioRolloffMode.Linear;
                        audioSourceRef.minDistance = 5;
                        audioSourceRef.maxDistance = 10;
                        var mixer = m_CrowdMixer.FindMatchingGroups(CROWD);
   
                        if (mixer.Length > 0)
                        {
                            audioSourceRef.outputAudioMixerGroup = mixer[0];
                        }
                        else
                        {
                            Debug.LogError($"Could not find , {CROWD} ", this);
                        }
                        
                        newItem.transform.ClearLocals();
                        newItem.transform.SetParent(this.transform);
                        AssignSound(audioSourceRef);
                    });

                }
            });
        }

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Blank:
                    ScenarioNothing();
                    break;
                case ScenarioEnum.Menu:
                    this.transform.position = MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start.transform.position;
                    StartBackgroundNoise();
                    break;

                case ScenarioEnum.TutorialPart1:
                    StartBackgroundNoise();
                    break;

                case ScenarioEnum.TutorialPart2:
                    break;

                case ScenarioEnum.Stackable:
                    this.transform.position = Vector3.zero;
                    CrowdChatterToZero();
                    GenericRockToZero();
                    break;

                case ScenarioEnum.SongFinishedCompleted:
                    StartBackgroundNoise();
                    break;
                default:
                    break;

            }
        }

        private void ScenarioNothing()
        {
            m_ChatterLoud.Stop();
            m_GenericRock.Stop();
            if (null != m_Scenario1Group)
            {
                StopCoroutine(m_Scenario1Group);
            }
            if (null != m_Scenario1Single)
            {
                StopCoroutine(m_Scenario1Single);
            }

            if (null != m_Scenario2Cheer)
            {
                StopCoroutine(m_Scenario2Cheer);
            }

            if (null != m_Scenario2Boo)
            {
                StopCoroutine(m_Scenario2Boo);
            }
        }


        private void CrowdChatterToZero()
        {
            float crowdStartLevel = m_ChatterLoud.volume;
            this.Create<ValueTween>(SCENARIO_PRELUDE_TIMING, () =>
            {
            }).Initialise(crowdStartLevel, 0.2f, (f) =>
            {
                m_ChatterLoud.volume  = f;
            });


        }

        private void GenericRockToZero()
        {
            float genericRockLevel = GenericRockVolume;
            this.Create<ValueTween>(SCENARIO_PRELUDE_TIMING, () =>
            {
            }).Initialise(genericRockLevel, 0, (f) =>
            {
                m_GenericRock.volume = f;
            });
        }

        public void PauseItem(bool pause)
        {
            if(pause == true)
            {
                m_ChatterLoud.volume = CrowdChatterVolume / 3f;                
            }
            else
            {
                m_ChatterLoud.volume = CrowdChatterVolume;
            }
        }

        private void StartCrowdBuildCalmDown()
        {
            float crowdStartLevel = m_ChatterLoud.volume;
            this.Create<ValueTween>(SCENARIO_PRELUDE_TIMING, () =>
            {
            }).Initialise(crowdStartLevel, 0.2f, (f) =>
            {
                m_ChatterLoud.volume = f;
            });

        }


        private void StartBackgroundNoise()
        {
            if (false == m_GenericRock.isPlaying)
            {
                m_CurrentCrowdNoiseLevel = CROWD_NOISE_LEVEL_SCENARIO_1;
                m_ChatterLoud.volume = CrowdChatterVolume;
                m_ChatterLoud.loop = true;
                m_ChatterLoud.Play();

                m_GenericRock.volume = GenericRockVolume;
                m_GenericRock.loop = true;
                m_GenericRock.Play();

                m_Scenario1Group = Scenario1CoCheerGroup();
                m_Scenario1Single = Scenario1CoCheerSingle();
                StartCoroutine(m_Scenario1Group);
                StartCoroutine(m_Scenario1Single);
                SetRating(RatingEnum.Amazing);
            }
        }

        private void Scenario2()
        {
            m_GenericRock.Stop();
            m_CurrentCrowdNoiseLevel = CROWD_NOISE_LEVEL_SCENARIO_2;

            m_Scenario2Cheer = Scenario2CoCheerGroup();
            m_Scenario2Boo = Scenario2CoBooGroup();
            StartCoroutine(m_Scenario2Cheer);
            StartCoroutine(m_Scenario2Boo);

            this.WaitFor(5, () =>
            {
                if (null != m_Scenario1Group)
                {
                    StopCoroutine(m_Scenario1Group);
                }
                if (null != m_Scenario1Single)
                {
                    StopCoroutine(m_Scenario1Single);
                }
            });
        }

        public void SetRating(RatingEnum ratingEnum)
        {
            m_PrevRatingEnum = m_CurrentRatingEnum;
            m_CurrentRatingEnum = ratingEnum;
            ChangeAudienceRating();
            ChangeRatingChances();          
        }

        [InspectorButton]
        private void RatingDebug()
        {
            ChangeAudienceRating();
            ChangeRatingChances();
        }


        public void ChangeAudienceRating()
        {
            if(MenuManager.Instance.IsPaused == true)
            {
                return;
            }
            if(m_PrevRatingEnum == RatingEnum.Amazing && (m_CurrentRatingEnum == RatingEnum.Happy || m_CurrentRatingEnum == RatingEnum.Neutral))
            {
                m_CrowdGroup[CrowdEnum.AmazingToNeutral].Play();
            }

            if (m_PrevRatingEnum == RatingEnum.Angry && (m_CurrentRatingEnum == RatingEnum.Sad || m_CurrentRatingEnum == RatingEnum.Neutral))
            {
                m_CrowdGroup[CrowdEnum.AngryToNeutral].Play();
            }

            if ((m_PrevRatingEnum == RatingEnum.Neutral || m_PrevRatingEnum == RatingEnum.Sad) && (m_CurrentRatingEnum == RatingEnum.Angry))
            {
                m_CrowdGroup[CrowdEnum.NeutralToAngry].Play();
            }

            if ((m_PrevRatingEnum == RatingEnum.Neutral || m_PrevRatingEnum == RatingEnum.Happy) && (m_CurrentRatingEnum == RatingEnum.Amazing))
            {
                m_CrowdGroup[CrowdEnum.NeutralToAmazing].Play();
            }
        }

        public void ChangeRatingChances()
        {
            switch (m_CurrentRatingEnum)
            {
                case RatingEnum.Amazing:
                    m_ChanceOfCheer = 100;
                    m_ChanceOfBoo = 0;
                    break;
                case RatingEnum.Happy:
                    m_ChanceOfCheer = 50;
                    m_ChanceOfBoo = 0;
                    break;
                case RatingEnum.Neutral:
                    m_ChanceOfCheer = 10;
                    m_ChanceOfBoo = 0;
                    break;
                case RatingEnum.Sad:
                    m_ChanceOfCheer = 0;
                    m_ChanceOfBoo = 50;
                    break;
                case RatingEnum.Angry:
                    m_ChanceOfCheer = 0;
                    m_ChanceOfBoo = 100;
                    break;
                default:
                    break;
            }
        }

        private IEnumerator Scenario1CoCheerGroup()
        {
            while(true)
            {
                float waitTime = Random.Range(2f, 4f);
                yield return new WaitForSeconds(waitTime);
                PlayCheerOneShotGroup();
            }
        }

        private IEnumerator Scenario1CoCheerSingle()
        {
            while (true)
            {
                float waitTime = Random.Range(2f, 4f);
                yield return new WaitForSeconds(waitTime);
                PlayCheerOneShot();
            }
        }


        private IEnumerator Scenario2CoCheerGroup()
        {
            while (true)
            {
                float waitTime = Random.Range(2f, 4f);
                yield return new WaitForSeconds(waitTime);
                if (Random.Range(0, 100) > m_ChanceOfCheer)
                {
                    PlayCheerOneShot();
                }
            }
        }

        private IEnumerator Scenario2CoBooGroup()
        {
            while (true)
            {
                float waitTime = Random.Range(2f, 4f);
                yield return new WaitForSeconds(waitTime);
                if (Random.Range(0, 100) > m_ChanceOfBoo)
                {
                    PlayBooOneShotGroup();
                }
            }
        }


        private void PlayOneShotGeneral(List<AudioSource> audioSource)
        {
            int safetyCheck = 10;
            int index = audioSource.GetRandomIndex();
            do
            {
                safetyCheck--;
                index = audioSource.GetRandomIndex();
            } while ((true == audioSource[index].isPlaying) && (safetyCheck > 0));

            float currentVolume = Random.Range(m_CurrentCrowdNoiseLevel.x, m_CurrentCrowdNoiseLevel.y);
            Transform randomTransform = m_Positions.GetRandom();
            audioSource[index].transform.position = randomTransform.position;
            audioSource[index].volume = currentVolume;
            audioSource[index].Play();

        }

        [InspectorButton] private void PlayCheerOneShotGroup() => PlayOneShotGeneral(m_CheerOneShotGroup);
        [InspectorButton] private void PlayCheerOneShot() => PlayOneShotGeneral(m_CheerOneShot);
        [InspectorButton] private void PlayBooOneShotGroup() => PlayOneShotGeneral(m_BooOneShotGroup);
        [InspectorButton] private void PlayClapOneShotGroup() => PlayOneShotGeneral(m_ClapOneShotGroup);



        #region PlayClapBuildUp
        [InspectorButton]
        private void PlayClapBuildUp()
        {
            m_PlayClapBuildUp = PlayClapBuildUpCo();
            StartCoroutine(m_PlayClapBuildUp);
        }

        [InspectorButton]
        private void StopClapBuildUp()
        {
            if (null != m_PlayClapBuildUp)
            {
                StopCoroutine(m_PlayClapBuildUp);
                m_PlayClapBuildUp = null;
            }
        }

        private IEnumerator PlayClapBuildUpCo()
        {
            float time = 1.3f;
            while(true)
            {
                yield return new WaitForSeconds(time);
                PlayClapOneShotGroup();
                if(time> 0.3f)
                {
                    time -= 0.1f;
                }
            }
        }
        #endregion PlayClapBuildUp

        private void AssignSound(AudioSource audioSourceRef)
        {
            if (audioSourceRef.clip.name.Contains("Chatter_Large"))
            {
                m_ChatterLoud = audioSourceRef;
            }

            if (audioSourceRef.clip.name.Contains("GenericRock"))
            {
                m_GenericRock = audioSourceRef;
            }

            if (audioSourceRef.clip.name.Contains("close_one_shots"))
            {
                m_CheerOneShot.Add(audioSourceRef);
            }

            if (audioSourceRef.clip.name.Contains("cheer_sm"))
            {
                m_CheerOneShotGroup.Add(audioSourceRef);
            }

            if (audioSourceRef.clip.name.Contains("boo_sm"))
            {
                m_BooOneShotGroup.Add(audioSourceRef);
            }

            if (audioSourceRef.clip.name.Contains("group_clap"))
            {
                m_ClapOneShotGroup.Add(audioSourceRef);
            }

            if (audioSourceRef.clip.name.Contains("bad_to_med"))
            {
                m_CrowdGroup[CrowdEnum.AngryToNeutral] = audioSourceRef;
            }

            if (audioSourceRef.clip.name.Contains("good_to_medium"))
            {
                m_CrowdGroup[CrowdEnum.AmazingToNeutral] = audioSourceRef;
            }

            if (audioSourceRef.clip.name.Contains("med_to_bad"))
            {
                m_CrowdGroup[CrowdEnum.NeutralToAngry] = audioSourceRef;
            }

            if (audioSourceRef.clip.name.Contains("med_to_good"))
            {
                m_CrowdGroup[CrowdEnum.NeutralToAmazing] = audioSourceRef;
            }

        }

    }


}
