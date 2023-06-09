﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class IntroMenuManager : MonoBehaviour
    {
        public static IntroMenuManager Instance;

        [SerializeField] private GameObject m_IntroMessage;
        [SerializeField] private GameObject m_EpilepsyMessage;
        [SerializeField] private GameObject m_DifficultyMessage;
        [SerializeField] private GameObject m_OutroMessage;

        [SerializeField] private Button m_StartButton;

        [SerializeField] private Button m_NormalButton;
        [SerializeField] private Button m_AdvancedButton;
        [SerializeField] private Button m_ContinueButton;

        [SerializeField] private List<ParticleSystem> m_ConfettiCannonsList;
        [SerializeField] private List<AudioSource> m_ConfettiAudioSourceList;
        [SerializeField] private AudioClip m_ConfettiPopSound;

        private AudioClip m_CompletedCheerSound;
        private AudioSource m_CompletedAudioSource;

        private CanvasGroup m_CanvasGroup;

        public void Initialise()
        {
            Instance = this;
            m_CanvasGroup = GetComponent<CanvasGroup>();

            m_StartButton = transform.Search("StartButton").GetComponent<Button>();
            m_NormalButton = transform.Search("NormalButton").GetComponent<Button>();
            m_AdvancedButton = transform.Search("AdvancedButton").GetComponent<Button>();
            m_ContinueButton = transform.Search("ContinueButton").GetComponent<Button>();

            m_IntroMessage = transform.Search("IntroMessage").gameObject;
            m_EpilepsyMessage = transform.Search("EpilepsyMessage").gameObject;
            m_DifficultyMessage = transform.Search("DifficultyMessage").gameObject;
            m_OutroMessage = transform.Search("OutroMessage").gameObject;

            SetupConfettiEffect();

            m_StartButton.onClick.AddListener(() =>
            {
                ToggleIntroMessage(false);
                ToggleEpilepsyWarning(true);
            });

            /////m_NormalButton.onClick.AddListener(NormalModeSelection);
            /////m_AdvancedButton.onClick.AddListener(AdvancedModeSelection);

            m_ContinueButton.onClick.AddListener(() =>
            {
                ToggleEpilepsyWarning(false);
                ToggleDifficultySelection(true);
            });

            SetupOutroAudio();

            ToggleIntroMessage(true);
            ToggleEpilepsyWarning(false);
            ToggleDifficultySelection(false);

            m_OutroMessage.SetActive(false);
            SetVisible(true);
            TaskBarManager.Instance.SetVisible(false);

#if UNITY_ANDROID
            //this.WaitFor(3, () =>
            //{
            //    Debug.LogError("Force AdvancedModeSelection for android");
            //    AdvancedModeSelection();
            //});
#endif
        }

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Stackable:
                    SetVisible(false);
                    break;
                case ScenarioEnum.SongFinishedCompleted:
                    ShowOutroMessage();
                    break;
            }
        }

        public void SetVisible(bool state)
        {
            m_CanvasGroup.VisibleAndInteractive(state);
        }

        private void SetupOutroAudio()
        {
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, "FinishedCheer", (soundclip) =>
            {
                m_CompletedCheerSound = soundclip;
            });

            m_CompletedAudioSource = gameObject.ForceComponent<AudioSource>();
            m_CompletedAudioSource.playOnAwake = false;
            m_CompletedAudioSource.spatialBlend = 1;
            m_CompletedAudioSource.clip = m_CompletedCheerSound;
            m_CompletedAudioSource.volume = 0.7f;
            m_CompletedAudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_CompletedAudioSource.minDistance = 0.1f;
            m_CompletedAudioSource.maxDistance = 2f;
        }

        private void ToggleIntroMessage(bool state)
        {
            m_IntroMessage.SetActive(state);
            m_StartButton.gameObject.SetActive(state);
        }

        [InspectorButton]
        public void ShowOutroMessage()
        {
            m_CompletedAudioSource.clip = m_CompletedCheerSound;
            m_CompletedAudioSource.Play();

            ShootConfetti();
            TaskBarManager.Instance.SetVisible(false);
            SetVisible(true);

            m_OutroMessage.SetActive(true);
            m_StartButton.gameObject.SetActive(false);
            m_NormalButton.gameObject.SetActive(false);
            m_AdvancedButton.gameObject.SetActive(false);
        }

        private void ToggleDifficultySelection(bool state)
        {
            m_DifficultyMessage.SetActive(state);
            m_NormalButton.gameObject.SetActive(state);
            m_AdvancedButton.gameObject.SetActive(state);
        }

        private void ToggleEpilepsyWarning(bool state)
        {
            m_EpilepsyMessage.SetActive(state);
            m_ContinueButton.gameObject.SetActive(state);
        }

        ////////////private void NormalModeSelection()
        ////////////{
        ////////////    MonitorTrainerRoot.Instance.Normal();
        ////////////    TaskBarManager.Instance.SetVisible(true);
        ////////////    SetVisible(false);
        ////////////}

        ////////////private void AdvancedModeSelection()
        ////////////{
        ////////////    MonitorTrainerRoot.Instance.Advanced();
        ////////////    TaskBarManager.Instance.SetVisible(true);
        ////////////    SetVisible(false);
        ////////////}

        private void PlayConfettiAnimation()
        {
            m_ConfettiCannonsList.ForEach(e => e.Play());
        }

        private void PlayConfettiSound()
        {
            m_ConfettiAudioSourceList.ForEach(e => e.clip = m_ConfettiPopSound);
            m_ConfettiAudioSourceList.ForEach(e => e.volume = 0.7f);
            m_ConfettiAudioSourceList.ForEach(e => e.Play());
        }

        private void ShootConfetti()
        {
            PlayConfettiAnimation();
            PlayConfettiSound();
        }

        private void SetupConfettiEffect()
        {
            LoadConfettiPopSound();
            SetupConfettiAudioSource();
            SetupConfettiParticleSystems();
        }

        private void SetupConfettiParticleSystems()
        {
            var cannon = transform.parent.FindSibling("ConfettiParticles").gameObject;
            m_ConfettiCannonsList = cannon.GetComponentsInChildren<ParticleSystem>().ToList();
        }

        private void SetupConfettiAudioSource()
        {
            var cannon = transform.parent.FindSibling("ConfettiParticles").gameObject;
            m_ConfettiAudioSourceList = cannon.GetComponentsInChildren<AudioSource>().ToList();
        }

        private void LoadConfettiPopSound()
        {
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, "ConfettiSoundEffect", (sound) =>
            {
                m_ConfettiPopSound = sound;
            });
        }
    }
}