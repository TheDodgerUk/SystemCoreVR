#if VR_INTERACTION

using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TimeLine
{
    public class TimeLineRoot : MonoBehaviour
    {
        private readonly Vector3 OFFSET = new Vector3(0f, 100f, 0f);
        private const float DISTANCE = 200f;
        private const float MOVEMENT_SPEED = 5f;

        int sceneIndex = 0;
        List<string> garageScenes = new List<string>() { "Garage1977", "GarageTransition", "Garage1988" };

        private GameObject m_Garage1977;
        private GameObject m_GarageTransition;
        private GameObject m_Garage1988;


        public void Initialise()
        {
            var PhotonSetupRef = Core.PhotonMultiplayerRef;
            PhotonSetupRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayer);
            PhotonSetupRef.ChangeRoom($"{nameof(TimeLineRoot)}{Application.version}");
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
#if VR_INTERACTION && UNITY_ANDROID
            OVRManager manager = FindObjectOfType<OVRManager>();
            //https://github.com/dilmerv/OculusPassthroughDemos
            ///manager.isInsightPassthroughEnabled = false;

#if VR_HAND_TRACKING
            OVRPassthroughLayer passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();
            Debug.LogError($"passthroughLayer.hidden  {passthroughLayer.hidden}");
            passthroughLayer.textureOpacity = 1f;
#endif
#endif
        }

        [EditorButton]
        private void DEBUG_SwapScenes() => InteranalSwap();

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            CameraControllerVR.Instance.TeleportAvatar(this.gameObject.scene, new Vector3(0.37f, 1.48f, 0.818f), null);

            //UnityEngine.SceneManagement.SceneManager.SetActiveScene(this.gameObject.scene);

            //CameraControllerVR.Instance.TeleporterRight.Add(InputManagerVR.Instance.AnySubscription.BtnSecondary);
            //InputManagerVR.Instance.AnySubscription.BtnPrimary.Begin += LoggerData;
            InputManagerVR.Instance.AnySubscription.BtnSecondary.Begin += SwapScenes;
            sceneIndex = garageScenes.Count - 1;
            InteranalSwap();

            // Load the garage scenes.
            Core.Environment.LoadEnvironmentOptional(garageScenes, () =>
            {
                //SceneManager.SetActiveScene());
                m_Garage1977 = GameObject.Find("Garage1977_Static");
                m_GarageTransition = GameObject.Find("GarageTransition_Static");
                m_GarageTransition.SetActive(false);
                m_Garage1988 = GameObject.Find("Garage1988_Static");
                m_Garage1988.SetActive(false);

                SetActiveScene(garageScenes[0]);

                //StartCoroutine(TestTransition());
            });


        }

        public void TriggerTransition()
        {
            StartCoroutine(TransitionGarageScenes());
        }

        private IEnumerator TransitionGarageScenes()
        {
            m_Garage1977.SetActive(false);
            m_GarageTransition.SetActive(true);

            SetActiveScene(garageScenes[1]);

            // Wait for transition animation.
            yield return new WaitForSeconds(9.39f);

            m_GarageTransition.SetActive(false);
            m_Garage1988.SetActive(true);

            SetActiveScene(garageScenes[2]);
        }

        private void SetActiveScene(string sName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (true == SceneManager.GetSceneAt(i).name.Contains(sName))
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
                    break;
                }
            }
        }

        private void SwapScenes(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
            InteranalSwap();
        }

        private void InteranalSwap()
        {
            CameraControllerVR.Instance.ToggleBlink(true, () =>
            {
                sceneIndex = sceneIndex.WrapIncrement(garageScenes);
                Core.Environment.LoadEnvironmentOptionalAndRemoveOld(new List<string> { garageScenes[sceneIndex] }, () =>
                {
                    CameraControllerVR.Instance.SnapTeleport(Vector3.zero);
                    CameraControllerVR.Instance.ToggleBlink(false, () =>
                    {
                        // nothing needed
                    });
                    
                });
            });
        }

        private void LoggerData(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
            Core.VisualLoggerRef.Toggle();
        }




        
    }
}
#endif