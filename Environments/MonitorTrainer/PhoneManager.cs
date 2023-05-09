using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class PhoneManager : MonoBehaviour
    {

        public static PhoneManager Instance;

        private const string PHONE_MESSAGES = "_PHONE_MESSAGES";

        private VisualEffect m_EmojiParticleSystem;
        private Material m_PhoneMaterial;

        private Dictionary<RatingEnum, SpriteRenderer> m_Emoji = new Dictionary<RatingEnum, SpriteRenderer>();
        private AudioClip m_TextNotification;
        private AudioClip m_TweetNotification;
        private AudioSource m_PhoneAudioSource;

        private TextMeshProUGUI m_Title;
        private TextMeshProUGUI m_Body;


        private Animator m_PhoneAnimation;
        private readonly int EMJ_TEXTURE = Shader.PropertyToID("EmojiTexture");

        public class Message
        {
            public string Title;
            public string Body;
            public RatingEnum ratingEnum = RatingEnum.Amazing;
        }
        public List<Message> m_Messages = new List<Message>();

        public void Initialise()
        {
            Instance = this;

            var emojHolder = transform.gameObject.SearchComponent<Transform>("PhoneEmoji");
            emojHolder.SetActive(false);
            m_Emoji.Add(RatingEnum.Angry, emojHolder.gameObject.SearchComponent<SpriteRenderer>("Emoji_Angry"));
            m_Emoji.Add(RatingEnum.Amazing, emojHolder.gameObject.SearchComponent<SpriteRenderer>("Emoji_Heart"));
            m_Emoji.Add(RatingEnum.Happy, emojHolder.gameObject.SearchComponent<SpriteRenderer>("Emoji_Heart"));
            m_Emoji.Add(RatingEnum.Sad, emojHolder.gameObject.SearchComponent<SpriteRenderer>("Emoji_Sad"));
            m_Emoji.Add(RatingEnum.Neutral, emojHolder.gameObject.SearchComponent<SpriteRenderer>("Emoji_ThumbsUp"));

            SetUpAudio();

            m_Title = transform.gameObject.SearchComponent<TextMeshProUGUI>("Text_Title");
            m_Body = transform.gameObject.SearchComponent<TextMeshProUGUI>("Text_Body");

            m_PhoneAnimation = transform.gameObject.SearchComponent<Animator>("Canvas");

            ToggleEmission(false);

            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;

        }


        private void DebugWriteFile()
        {
            Message newMes1 = new Message();
            newMes1.Title = "Fred";
            newMes1.Body = "dddd";
            m_Messages.Add(newMes1);

            Message newMes2 = new Message();
            newMes2.Title = "dsdsdss";
            newMes2.Body = "xxxxxxxx";
            newMes2.ratingEnum = RatingEnum.Neutral;
            m_Messages.Add(newMes2);

            string file = Application.streamingAssetsPath + "\\m_Messages";
            Debug.LogError($"file {file}");
            Json.FullSerialiser.WriteToFile(m_Messages, file, true);
        }

        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;

            var original = this.gameObject.SearchComponent<Transform>("VFX_EmojiParticleSystem");
            m_EmojiParticleSystem = original.GetComponent<VisualEffect>();
            m_EmojiParticleSystem.Stop();

            // reset position
            var message = this.gameObject.SearchComponent<Transform>("GUI_PhoneMessages");
            message.transform.localPosition = new Vector3(0f, message.transform.localPosition.y, 0f);
            original.transform.localPosition = new Vector3(0f, original.transform.localPosition.y, 0f);
        }


        public void InitialiseSongLoadMessage(string songName, Action callback)
        {
            m_Messages.Clear();

            string fileName = $"{songName}{PHONE_MESSAGES}";
            Core.AssetBundlesRef.TextAssetBundleRef.GetItem<List<Message>>(Core.Mono, fileName, (messages) =>
            {
                m_Messages = messages;
                if (m_Messages == null)
                {
                    DebugBeep.LogError($"no band tweetes for :{fileName}", DebugBeep.MessageLevel.High);
                }
                callback?.Invoke();
            });
        }


        public void SetPhone(GameObject phone)
        {
            m_PhoneMaterial = phone.GetComponentInChildren<Renderer>().material;
            ToggleEmission(false);
        }

        private void SetUpAudio()
        {
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "Text_Notification", (soundclip) =>
            {
                m_TextNotification = soundclip;
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "Tweet_Notification", (soundclip) =>
            {
                m_TweetNotification = soundclip;
            });

            m_PhoneAudioSource = gameObject.ForceComponent<AudioSource>();
            m_PhoneAudioSource.playOnAwake = false;
            m_PhoneAudioSource.spatialBlend = 1;
            m_PhoneAudioSource.clip = m_TextNotification;
            m_PhoneAudioSource.volume = 1f;
            m_PhoneAudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_PhoneAudioSource.minDistance = 0.1f;
            m_PhoneAudioSource.maxDistance = 2f;
        }

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Blank:
                    this.StopAllCoroutines();
                    break;
                case ScenarioEnum.Menu:
                    this.StopAllCoroutines();
                    break;

                case ScenarioEnum.TutorialPart1:
                    break;

                case ScenarioEnum.Stackable:
                    StartCoroutine(BeginNotifications());
                    break;
            }
        }

        private IEnumerator BeginNotifications()
        {


            List<Message> values = m_Messages.Clone();

            System.Random rand = new System.Random();
            while (values.Count > 0)
            {
                //Wait between 30 and 90 seconds to show the next message
                float currentTime = 0;
                int delay = rand.Next(30, 90);
                while (currentTime < delay) 
                {
#if UNITY_EDITOR
                    yield return null;
#else
                    yield return new WaitForEndOfFrame();
#endif
                    if (MenuManager.Instance.IsPaused == false)
                    {
                        currentTime += Time.deltaTime;
                    }
                }

                var msg = GetRandomMessage(values);
                ShowMessage(msg);
                values.Remove(msg);
            }
        }


        private Message GetRandomMessage(List<Message> values)
        {
            System.Random rand = new System.Random();
            var item = values[rand.Next(values.Count)];
            return item;
        }

        [EditorButton]
        private void Debug_ShowMessage() => ShowMessage(m_Messages[0]);

        private void ShowMessage(Message msg)
        {
            CrowdStateChange(msg.ratingEnum);
            if (msg.ToString().Contains("Tweet"))
            {
                m_PhoneAudioSource.clip = m_TweetNotification;
            }
            else
            {
                m_PhoneAudioSource.clip = m_TextNotification;
            }
            m_PhoneAudioSource.Play();

            m_Title.text = msg.Title;
            m_Body.text = msg.Body;
            m_PhoneAnimation.Play("Open"); // it will hide itself after a 4.25 seconds
            ToggleEmission(true);
            this.WaitFor(4.25f, () => 
            {
                m_PhoneAnimation.Play("Close"); // it will hide itself after a 4.25 seconds
            });
            this.WaitFor(5.25f, () =>
            {
                ToggleEmission(false);
            });
        }

        private void ToggleEmission(bool state)
        {
            if (m_PhoneMaterial != null)
            {
                //emission 1.5f for screen on and -8 for screen off
                if (state)
                {
                    m_PhoneMaterial.SetColor("_EmissionColor", m_PhoneMaterial.color * 1.5f);
                }
                else
                {
                    m_PhoneMaterial.SetColor("_EmissionColor", m_PhoneMaterial.color * -8f);
                }
            }
        }

        public void CrowdStateChange(RatingEnum ratingEnum)
        {
#if UNITY_EDITOR
            if(m_EmojiParticleSystem.HasTexture(EMJ_TEXTURE) == false)
            {
                Debug.LogError($"not found item");
            }
#endif
            m_EmojiParticleSystem.SetTexture(EMJ_TEXTURE, m_Emoji[ratingEnum].sprite.texture);


            PlayEmojiParticles();
        }

        [InspectorButton]
        public void PlayEmojiParticles()
        {
            m_EmojiParticleSystem.Play();
        }

        public void StopEmojiParticles()
        {
            m_EmojiParticleSystem.Stop();
        }
    }
}