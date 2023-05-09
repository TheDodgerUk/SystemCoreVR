using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using static MonitorTrainer.MonitorTrainerConsts;
using EPOOutline;
using static VrInteractionBaseButton;
using System;

namespace MonitorTrainer
{
    public class PhysicalAmp : MonoBehaviour
    {
        public static PhysicalAmp Instance;

        private const float SPEED = 0.25f;
        private const float AMOUNT = -0.006032702f; // this is because the button scale is not 1

        public VrInteractionBaseButton Button2 { get; private set; }
        public VrInteractionBaseButton PowerAmp { get; private set; }
        public VrInteractionBaseButton PowerMic { get; private set; }

        public VrInteractionPickUpCable LooseCable { get; private set; }
        public VrInteractionPickUpSocket USB { get; private set; }

        private Color RED = new Color(255f/255f, 76f / 255f, 76f / 255f);
        private AudioSource m_ButtonClickUp;
        private AudioSource m_ButtonClickDown;



        public void Initialise()
        {
            Instance = this;
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "ButtonClickUp", (audio) =>
            {
                SetClip(ref m_ButtonClickUp, audio);
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "ButtonClickDown", (audio) =>
            {
                SetClip(ref m_ButtonClickDown, audio);
            });


        }

        public void SetupUSB(VrInteraction root)
        {
            USB = (VrInteractionPickUpSocket)root;
        }
        public void Setup(VrInteraction root)
        {
            Button2 = (VrInteractionBaseButton)root.GetVrInteractionFromRoot(MetaDataType.ContentButton, "Button_Mute_Amp");
            Button2.ButtonState = ButtonStateEnum.Down;
            Button2.ClearAddCallback((state) => PlayButtonClick(Button2, state));

            PowerAmp = (VrInteractionBaseButton)root.GetVrInteractionFromRoot(MetaDataType.ContentButton, "Button_Power_Amp");
            PowerAmp.ButtonState = ButtonStateEnum.Down;
            PowerAmp.ClearAddCallback((state) => PlayButtonClick(PowerAmp, state));

            PowerMic = (VrInteractionBaseButton)root.GetVrInteractionFromRoot(MetaDataType.ContentButton, "Button_Power_Hub");
            PowerMic.ButtonState = ButtonStateEnum.Down;
            PowerMic.ClearAddCallback((state) => PlayButtonClick(PowerMic, state));

            LooseCable = (VrInteractionPickUpCable)root.GetVrInteractionFromRoot(MetaDataType.ContentPickUpCable).GetRandom();
            LooseCable.ClearAddCallback(null);
        }

        private void PlayButtonClick(VrInteractionBaseButton item, ButtonStateEnum state)
        {

            if (state == ButtonStateEnum.Up)
            {
                AudioSource.PlayClipAtPoint(m_ButtonClickUp.clip, item.transform.position, m_ButtonClickUp.volume);
                ////// this moves the GUI the script is acctahed to MonitorTrainerRoot , ClickUp.transform.position = item.transform.position;
                ////// m_ButtonClickUp.Play();
            }
            else
            {
                AudioSource.PlayClipAtPoint(m_ButtonClickDown.clip, item.transform.position, m_ButtonClickDown.volume);
                ////// this moves the GUI the script is acctahed to MonitorTrainerRoot , m_ButtonClickDown.transform.position = item.transform.position;
                ////// m_ButtonClickDown.Play();
            }
        }

        private void SetClip(ref AudioSource audioSource, AudioClip clip)
        {
            audioSource = this.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.8f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0.01f;
            audioSource.maxDistance = 2f;
            audioSource.clip = clip;
        }
    }
}