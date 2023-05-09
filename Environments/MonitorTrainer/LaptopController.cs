using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnvironmentHelpers;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System;

namespace MonitorTrainer
{
    public class LaptopController : MonoBehaviour
    {
        [SerializeField] private VideoPlayer m_VideoPlayer;
        [SerializeField] private GameObject m_LaptopScreen;
        public VrInteractionClickCallBack Laptop { get; private set; }
        
        public static LaptopController Instance;
        public void Initialise()
        {
            Instance = this;

            Scene sc = this.gameObject.scene;
            var list = sc.GetRootGameObjects().ToList();

            GameObject laptopModelRoot = list.FindLast(e => e.name == "Models");
            if (null != laptopModelRoot)
            {
                GameObject laptop = laptopModelRoot.transform.Search("Laptop").gameObject;
                m_LaptopScreen = laptop.transform.Find("VideoPlayerScreen").gameObject;
                Laptop = laptop.ForceComponent<VrInteractionClickCallBack>();
                Laptop.AddCallback((amount) =>
                {
                    LaptopColliderPressed();
                });
            }
            else
            {
                Debug.LogError("Cannot find Models");
            }
            m_VideoPlayer = m_LaptopScreen.ForceComponent<VideoPlayer>();
            SetupVideoPlayer();
        }

        public void ReassignCallBack()
        {
            Laptop.AddCallback((amount) =>
            {
                LaptopColliderPressed();
            });
        }

        [InspectorButton]
        private void LaptopColliderPressed()
        {
            PlayVideo();
        }

        private void SetupVideoPlayer()
        {
            Debug.LogError("needs fixing");
            return;
            m_VideoPlayer.targetMaterialRenderer = m_LaptopScreen.GetComponent<MeshRenderer>();
            m_VideoPlayer.isLooping = false;
            m_VideoPlayer.playOnAwake = false;
            m_VideoPlayer.skipOnDrop = false;
            m_VideoPlayer.aspectRatio = VideoAspectRatio.Stretch;
            m_VideoPlayer.source = VideoSource.VideoClip;
            Core.AssetBundlesRef.VideoClipAssetBundleRef.GetItem(this, "MidasIntro", (item) =>
            {
                m_VideoPlayer.clip = item;
            });

            m_VideoPlayer.loopPointReached += EndVideoReached;
            ResetVideo(m_VideoPlayer);
        }

        public void PlayVideo()
        {
            if(m_VideoPlayer.isPlaying == false)
            {
                m_VideoPlayer.Play();
            }
            else
            {
                ResetVideo(m_VideoPlayer);
            }
        }
        private void ResetVideo(UnityEngine.Video.VideoPlayer videoplayer)
        {
            videoplayer.time = 0;
            videoplayer.Play();
            videoplayer.Pause();
        }

        void EndVideoReached(UnityEngine.Video.VideoPlayer videoplayer)
        {
            ResetVideo(videoplayer);
        }
    }
}

