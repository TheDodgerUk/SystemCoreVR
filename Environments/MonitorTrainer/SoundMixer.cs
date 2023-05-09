using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class SoundMixer
    {
        public static SoundMixer Instance;
        public AudioMixer m_MasterMixerLeft { get; private set; }
        public AudioMixer m_MasterMixerRight { get; private set; }
        public AudioMixer m_MasterMixerAmp { get; private set; }

        private MusicSoundManager m_MusicSoundManager;

        private GameObject m_SpeakerLeft;
        private GameObject m_SpeakerRight;
        public GameObject m_Amp;

        private List<GameObject> m_AddedSounds = new List<GameObject>();

        private Dictionary<MusicianTypeEnum, AudioSampling> m_MusicianAudioSamplings;

        private enum MixerEnum
        {
           /// removed to cut down on the double play Left,
            Right,
            Amp,
        }

        public SoundMixer(MusicSoundManager mono, Dictionary<MusicianTypeEnum, AudioSampling> musicianAudioSamplings, Action callback)
        {
            Instance = this;
            m_MusicSoundManager = mono;
            m_MusicianAudioSamplings = musicianAudioSamplings;
            GetGameObjects();
            LoadMixerFromResources(callback);
            //LoadMixerFromAssetBundle();
        }

        private void LoadMixerFromResources(Action callback)
        {
            m_MasterMixerLeft = Resources.Load<AudioMixer>("MonitorTrainerSpeakerLeft");
            m_MasterMixerRight = Resources.Load<AudioMixer>("MonitorTrainerSpeakerRight");
            m_MasterMixerAmp = Resources.Load<AudioMixer>("MonitorTrainerAmp");
            callback?.Invoke();
        }

        private void GetGameObjects()
        {
            var directChildren = m_MusicSoundManager.transform.GetDirectChildren();
            foreach (var child in directChildren)
            {
                if (child.name == "SpeakerLeft")
                {
                    m_SpeakerLeft = child.gameObject;
                }
                else if (child.name == "SpeakerRight")
                {
                    m_SpeakerRight = child.gameObject;
                }
                else if (child.name == "MonitorAmp")
                {
                    m_Amp = child.gameObject;
                }
            }
        }

        public void InitialiseSongLoadAudio(string songMane, Action callback)
        {
            for(int i = 0; i < m_AddedSounds.Count; i++)
            {
                if (m_AddedSounds[i] != null)
                {
                    var clip = m_AddedSounds[i].GetComponent<AudioClip>();
                    if (clip != null)
                    {
                        AudioClip.Destroy(clip);
                    }
                    UnityEngine.Object.Destroy(m_AddedSounds[i].gameObject);
                }
            }
            m_AddedSounds.Clear();

            var allAudioSources = m_MusicSoundManager.m_Tracks.GetListAll();

            for (int i = 0; i < allAudioSources.Count; i++)
            {
                if (allAudioSources[i].clip != null)
                {
                    AudioClip.Destroy(allAudioSources[i].clip);
                }
            }

            m_MusicSoundManager.m_Tracks.ClearAll();
            m_MusicianAudioSamplings.Clear();

            Core.Mono.WaitForFrames(2, () =>
            {
                Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItemList(m_MusicSoundManager, songMane, (items) =>
                {
                    if(items == null || items.Count == 0)
                    {
                        Debug.LogError($"Cannot find tracks starting with : {songMane}");
                    }
                    TaskAction task = new TaskAction(items.Count, () =>
                    {
                        callback?.Invoke();
                    });
                    foreach (var item in items)
                    {
                        Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, item, (clip) =>
                        {

                            //////Create(clip, m_MasterMixerLeft, m_SpeakerLeft, MixerEnum.Left);
                            Create(clip, m_MasterMixerRight, m_SpeakerRight, MixerEnum.Right);
                            Create(clip, m_MasterMixerAmp, m_Amp, MixerEnum.Amp);
                            task.Increment();
                        });
                    }

                });
            });
        }

        private void Create(AudioClip clip, AudioMixer mixer, GameObject parent, MixerEnum mixerEnum)
        {
            GameObject newItem = new GameObject(clip.name);
            m_AddedSounds.Add(newItem);
            AudioSource audioSourceRef = newItem.ForceComponent<AudioSource>();
            audioSourceRef.playOnAwake = false;
            audioSourceRef.Stop();
            audioSourceRef.time = 0;
            audioSourceRef.Stop();

            audioSourceRef.clip = clip;
            audioSourceRef.spatialBlend = 1; // sets it fade when going away from it
            if(mixerEnum == MixerEnum.Amp)
            {
                audioSourceRef.spatialBlend = 0; // this sets it to all around you 
            }
            audioSourceRef.rolloffMode = AudioRolloffMode.Linear;
            if (mixerEnum == MixerEnum.Amp)
            {
                audioSourceRef.maxDistance = 10;
                audioSourceRef.bypassReverbZones = true;
            }
            else
            {
                audioSourceRef.maxDistance = 500;
                audioSourceRef.bypassReverbZones = false;
            }
            newItem.transform.SetParent(parent.transform);
            newItem.transform.ClearLocals();
            AssignSound(audioSourceRef, mixer, mixerEnum);
        }

        private void AssignSound(AudioSource audioSourceRef, AudioMixer mixer, MixerEnum mixerEnum)
        {
            foreach (MusicianTypeEnum musicianTypeEnum in Enum.GetValues(typeof(MusicianTypeEnum)))
            {
                String[] splited = audioSourceRef.clip.name.Split('_');
                string realSoundName = splited[splited.Length - 1];

                if (realSoundName == musicianTypeEnum.ToString())
                {
                    m_MusicSoundManager.m_Tracks.AddToList(musicianTypeEnum, audioSourceRef);
                    var rr = mixer.FindMatchingGroups(musicianTypeEnum.ToString());
                    m_MusicSoundManager.m_Tracks[musicianTypeEnum][(int)mixerEnum].outputAudioMixerGroup = mixer.FindMatchingGroups(musicianTypeEnum.ToString())[0];

                    if (mixerEnum == MixerEnum.Amp)
                    {
                        AudioSampling audioSamplingRef = audioSourceRef.gameObject.ForceComponent<AudioSampling>();
                        audioSamplingRef.Init(audioSourceRef);
                        m_MusicianAudioSamplings.Add(musicianTypeEnum, audioSamplingRef);
                    }
                }
            }
        }
    }
}