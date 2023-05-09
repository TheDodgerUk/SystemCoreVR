using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Museum
{

    public class InteractiveMenu : MonoBehaviour
    {
        public class Item
        {
            public Canvas m_ItemCanvas;
            public Button m_Info;
            public Button m_Xray;
            public Button m_Explode;
            public Button m_Video;
            public TextMeshProUGUI m_Title;
            public TextMeshProUGUI m_Body;
            public AudioSource m_ItemAudioSource;
            public AudioSource AudioSourceButtonPing;
            public Transform m_MainPage;
        }

        public class Video
        {
            public Transform m_VideoRoot;
            public VideoPlayer m_VideoPlayer;
            public Button m_Back;
            public Button m_Play;
            public Button m_Stop;
        }

        public class MainAudio
        {
            public Button m_MainPlay;
            public Button m_MainStop;
            public AudioSource m_MainAudioSource;
        }

        private Item m_Item = new Item();
        private Video m_Video = new Video();
        private MainAudio m_MainAudio = new MainAudio();

        private Museum.Data m_Data;

        void Start()
        {
            InitItemsMenu();
            InitVideoMenu();
            InitMainMenu();
        }

        public void SetData(Museum.Data data)
        {
            m_Data = data;
            m_Item.m_MainPage.SetActive(true);
            m_Item.m_ItemAudioSource.Stop();
            SetItemItemsMenu();
            SetVideoMenu();
        }

        private void InitVideoMenu()
        {
            m_Video.m_VideoRoot = this.gameObject.SearchComponent<Transform>("VideoPage");
            m_Video.m_VideoPlayer = this.gameObject.SearchComponent<VideoPlayer>("Video Player");
            m_Video.m_Back = this.gameObject.SearchComponent<Button>("Button_Back");
            m_Video.m_Play = this.gameObject.SearchComponent<Button>("Button_Play");
            m_Video.m_Stop = this.gameObject.SearchComponent<Button>("Button_Stop");
        }

        private void InitItemsMenu()
        {
            var item_Canvas = this.gameObject.SearchComponent<Transform>("Item_Canvas");
            item_Canvas.gameObject.ForceComponent<VRUICanvas>();

            m_Item.m_Info = this.gameObject.SearchComponent<Button>("Button_Info");
            m_Item.m_Xray = this.gameObject.SearchComponent<Button>("Button_Xray");
            m_Item.m_Explode = this.gameObject.SearchComponent<Button>("Button_Explode");
            m_Item.m_Video = this.gameObject.SearchComponent<Button>("Button_Video");

            m_Item.m_Title = this.gameObject.SearchComponent<TextMeshProUGUI>("Text_Title");
            m_Item.m_Body = this.gameObject.SearchComponent<TextMeshProUGUI>("Text_Body");
            m_Item.m_Title.enableAutoSizing = true;
            m_Item.m_Body.enableAutoSizing = true;

            m_Item.m_ItemCanvas = this.gameObject.SearchComponent<Canvas>("Item_Canvas");
            m_Item.m_MainPage = this.gameObject.SearchComponent<Transform>("MainPage");
            m_Item.m_ItemAudioSource = m_Item.m_ItemCanvas.AddComponent<AudioSource>();
            m_Item.m_ItemAudioSource.playOnAwake = false;
            m_Item.m_ItemAudioSource.spatialBlend = 1f;
            m_Item.m_ItemAudioSource.maxDistance = 10f;


            m_Item.AudioSourceButtonPing = m_Item.m_ItemCanvas.AddComponent<AudioSource>();
            m_Item.AudioSourceButtonPing.playOnAwake = false;
            m_Item.AudioSourceButtonPing.spatialBlend = 1f;
            m_Item.AudioSourceButtonPing.maxDistance = 10f;
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, "Ping", (clip) =>
            {
                m_Item.AudioSourceButtonPing.clip = clip;
            });
        }

        private void InitMainMenu()
        {
            var startAudio_Canvas = this.gameObject.SearchComponent<Transform>("StartAudio_Canvas");
            startAudio_Canvas.gameObject.ForceComponent<VRUICanvas>();

            m_MainAudio.m_MainPlay = this.gameObject.SearchComponent<Button>("Button_Play Audio");
            m_MainAudio.m_MainStop = this.gameObject.SearchComponent<Button>("Button_Stop Audio");


            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, "Behringer Description", (audio) =>
            {
                m_MainAudio.m_MainAudioSource = startAudio_Canvas.AddComponent<AudioSource>();
                m_MainAudio.m_MainAudioSource.playOnAwake = false;
                m_MainAudio.m_MainAudioSource.clip = audio;
                m_MainAudio.m_MainAudioSource.spatialBlend = 1f;
                m_MainAudio.m_MainAudioSource.maxDistance = 10f;
            });

            m_MainAudio.m_MainPlay.onClick.AddListener(() =>
            {
                m_MainAudio.m_MainAudioSource.Play();
            });
            m_MainAudio.m_MainStop.onClick.AddListener(() =>
            {
                m_MainAudio.m_MainAudioSource.Stop();
            });
        }


        [EditorButton]
        private void VideoBack() => m_Video.m_Back.onClick.Invoke();
        [EditorButton]
        private void VideoPlay() => m_Video.m_Play.onClick.Invoke();
        [EditorButton]
        private void VideoStop() => m_Video.m_Stop.onClick.Invoke();

        private void SetVideoMenu()
        {
            m_Video.m_VideoPlayer.Stop();
            m_Video.m_VideoPlayer.clip = m_Data.m_VideoClip;
            m_Video.m_Back.onClick.RemoveAllListeners();
            m_Video.m_Play.onClick.RemoveAllListeners();
            m_Video.m_Stop.onClick.RemoveAllListeners();

            m_Video.m_Back.onClick.AddListener(() =>
            {
                m_Item.m_MainPage.SetActive(true);
                m_Video.m_VideoRoot.SetActive(false);
            });

            m_Video.m_Play.onClick.AddListener(() =>
            {
                m_Video.m_VideoPlayer.Play();
            });

            m_Video.m_Stop.onClick.AddListener(() =>
            {
                m_Video.m_VideoPlayer.Pause();
            });
        }


        [EditorButton]
        private void ItemInfo() => m_Item.m_Info.onClick.Invoke();
        [EditorButton]
        private void ItemXray() => m_Item.m_Xray.onClick.Invoke();
        [EditorButton]
        private void ItemExplode() => m_Item.m_Explode.onClick.Invoke();
        [EditorButton]
        private void ItemVideo() => m_Item.m_Video.onClick.Invoke();

        private void SetItemItemsMenu()
        {
            m_Item.m_ItemCanvas.transform.position = m_Data.m_Position;
            m_Item.m_ItemCanvas.transform.rotation = m_Data.m_Rotation;

            if (m_Data.m_ArData != null)
            {
                m_Item.m_Xray.interactable = m_Data.m_ArData.m_bSupportsXRay;
                m_Item.m_Explode.interactable = m_Data.m_ArData.m_bSupportsExplosion;
            }
            else
            {
                m_Item.m_Xray.interactable = false;
                m_Item.m_Explode.interactable = false;
            }


            m_Item.m_Info.onClick.RemoveAllListeners();
            m_Item.m_Xray.onClick.RemoveAllListeners();
            m_Item.m_Explode.onClick.RemoveAllListeners();
            m_Item.m_Video.onClick.RemoveAllListeners();

            m_Item.m_ItemAudioSource.Stop();
            m_Item.m_Info.interactable = (m_Data.m_AudioClip != null);
            m_Item.m_ItemAudioSource.clip = m_Data.m_AudioClip;
            m_Item.m_Info.onClick.AddListener(() =>
            {
                if(m_Item.m_ItemAudioSource.isPlaying == false)
                {
                    m_Item.m_ItemAudioSource.Play();
                }
                else
                {
                    m_Item.m_ItemAudioSource.Stop();
                    m_Item.AudioSourceButtonPing.PlayOneShot(m_Item.AudioSourceButtonPing.clip);
                }

            });


            m_Item.m_Xray.onClick.AddListener(() =>
            {
                m_Data.m_XrayProduct.ToggleXray();
                m_Item.AudioSourceButtonPing.PlayOneShot(m_Item.AudioSourceButtonPing.clip);
            });


            m_Item.m_Explode.onClick.AddListener(() =>
            {
                m_Data.m_ExplodeProduct.ToggleExplodeProduct();
                m_Item.AudioSourceButtonPing.PlayOneShot(m_Item.AudioSourceButtonPing.clip);
            });


            m_Item.m_Title.text = m_Data.m_ShortName;
            m_Item.m_Body.text = m_Data.m_BodyText;


            // video
            m_Video.m_VideoRoot.SetActive(false);
            m_Item.m_Video.interactable = (m_Data.m_VideoClip != null);
            m_Item.m_Video.onClick.AddListener(() =>
            {
                m_Item.m_ItemAudioSource.Stop();
                m_Item.m_MainPage.SetActive(false);
                m_Video.m_VideoRoot.SetActive(!m_Video.m_VideoRoot.gameObject.activeSelf);
                m_Video.m_VideoPlayer.Play();
            });

        }

    }
}