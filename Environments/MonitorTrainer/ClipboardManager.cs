using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonitorTrainer
{
    public class ClipboardManager : MonoBehaviour
    {
        private enum ListItems
        {
            Console,
            InEarSystem,
            PersonalSpeaker,
            MicrophoneSplitter,
            Laptop,
            USB,
            RackAmpBox
        }

        public static ClipboardManager Instance;
        private GameObject m_Clipboard;
        private Dictionary<ListItems, GameObject> m_CheckList = new Dictionary<ListItems, GameObject>();
        private Dictionary<ListItems, GameObject> m_SceneObjects = new Dictionary<ListItems, GameObject>();
        private Dictionary<ListItems, VrInteractionClickCallBack> m_CallBackItems = new Dictionary<ListItems, VrInteractionClickCallBack>();
        private Action m_OnCompleted;
        private ControllerData m_LeftHandController;

        private AudioClip m_ScratchSound;
        private AudioSource m_ClipboardAudioSource;

        private float MAX_AUDIO_DISTANCE = 2f;
        private float MIN_AUDIO_DISTANCE = 0.1f;


        public void Initialise()
        {
            Instance = this;
            LoadScratchSound();
            Core.Environment.OnEnvironmentLoadingComplete += CreateVrInteractionObjects;           
        }

        private void CreateVrInteractionObjects()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= CreateVrInteractionObjects;
            StartCoroutine(Internal());
        }

        private IEnumerator Internal()
        {
            while (null == PhysicalConsole.Instance || PhysicalConsole.Instance.HasInitialised == false)
            {
                yield return new WaitForSeconds(MonitorTrainerConsts.INITIALISE_COROUTINE);
            }
            Scene sc = this.gameObject.scene;
            var list = sc.GetRootGameObjects().ToList();

            foreach (var item in list)
            {
                if (item.name == "Models")
                {
                    m_SceneObjects.Add(ListItems.InEarSystem, item.transform.SearchChildrenWithNameContains($"Equipment_InEarCase_LOD0_Dummy").gameObject);
                    m_SceneObjects.Add(ListItems.PersonalSpeaker, item.transform.SearchChildrenWithNameContains($"Behringer Truth-B2031A_LOD0_Dummy").gameObject);
                    m_SceneObjects.Add(ListItems.MicrophoneSplitter, item.transform.SearchChildrenWithNameContains($"MonitorTrainer_MicBox_Dummy").gameObject);
                }
                if (item.name == "MT_Amp")
                {
                    m_SceneObjects.Add(ListItems.RackAmpBox, item.transform.SearchChildrenWithNameContains($"MT_Amp_Box_Dummy").gameObject);
                }
                if (item.name == "MonitorTrainer_Clipboard")
                {
                    m_Clipboard = item.gameObject;
                }
            }

            SetUpAudioSource();
            var scratches = m_Clipboard.transform.GetDirectChildren();
            m_CheckList.Add(ListItems.Console, scratches[0].gameObject);
            m_CheckList.Add(ListItems.InEarSystem, scratches[1].gameObject);
            m_CheckList.Add(ListItems.PersonalSpeaker, scratches[2].gameObject);
            m_CheckList.Add(ListItems.MicrophoneSplitter, scratches[3].gameObject);
            m_CheckList.Add(ListItems.Laptop, scratches[4].gameObject);
            m_CheckList.Add(ListItems.USB, scratches[5].gameObject);
            m_CheckList.Add(ListItems.RackAmpBox, scratches[6].gameObject);

            foreach (GameObject item in m_CheckList.Values)
            {
                item.SetActive(false);
            }
            m_Clipboard.SetActive(false);

            //////PhysicalConsole.Instance.ConsoleDummy.SetActive(true);
            //////PhysicalConsole.Instance.ConsoleDummy.ForceComponent<VrInteractionClickCallBack>();
            //////m_SceneObjects.Add(ListItems.Console, PhysicalConsole.Instance.ConsoleDummy.gameObject);
            ////m_SceneObjects.Add(ListItems.USB, PhysicalConsole.Instance.USBKey.gameObject);
            m_SceneObjects.Add(ListItems.Laptop, LaptopController.Instance.Laptop.gameObject);

            foreach (KeyValuePair<ListItems, GameObject> pair in m_SceneObjects)
            {
                var vr = pair.Value.ForceComponent<VrInteractionClickCallBack>();
                vr.AddCallback((amount) => 
                { 
                    OnItemSelected(pair.Key); 
                });
                m_CallBackItems.Add(pair.Key, vr);
            }

            ToggleClipboard(false);
            InputManagerVR.Instance.IterateControllers(OnControllerAdded);
            InputManagerVR.Instance.ControllerAdded -= OnControllerAdded;
            InputManagerVR.Instance.ControllerAdded += OnControllerAdded;

            this.WaitForFrames(3, () =>
            {
                foreach (var item in m_CallBackItems)
                {
                    if(item.Key != ListItems.Laptop)
                    {
                        item.Value.SetState(VrInteraction.StateEnum.OffIncludingRenderer);
                    }
                }
                ////PhysicalConsole.Instance.USBKey.SetState(VrInteraction.StateEnum.On);
            });
        }

        public void BeginClipboardTask(Action onCompleted)
        {
            m_OnCompleted = onCompleted;
            foreach (var item in m_CallBackItems.Values)
            {
                item.SetState(VrInteraction.StateEnum.On);
            }

            ControllerData controller = InputManagerVR.Instance.GetController(Handedness.Left);
            OnControllerAdded(controller);


            ToggleClipboard(true);
        }

        private void OnControllerAdded(ControllerData controller)
        {
            var tracked = controller as TrackedControllerData;
            if(tracked != null)
            {
                if (controller.Hand == Handedness.Left)
                {
                    m_LeftHandController = controller;
                    var gfx = m_LeftHandController.GetGfx<VRControllerGraphics>();

                    controller.AddModelAttachmentToRaycastRoot(m_Clipboard, new Vector3(0.07f, 0.0047f, 0.02f), Quaternion.Euler(new Vector3(-1.175f, 179.803f, -3.676f)));
                }
            }
        }

        private void ToggleClipboard(bool state)
        {
            if (m_LeftHandController != null || m_Clipboard != null)
            {
                m_Clipboard.SetActive(state);
            }
            else
            {
                Debug.LogError("m_LeftHandController is null or m_Clipboard is null");
            }
        }


        private void OnItemSelected(ListItems item)
        {
            if (m_Clipboard.activeSelf == true)
            {
                if (m_CheckList[item].activeSelf == false)
                {
                    InputManagerVR.Instance.GetController(Handedness.Left).Vibrate(0.5f, m_ClipboardAudioSource.clip.length);
                    PlayScratchSound();
                    m_CheckList[item].SetActive(true);
                }
                m_CallBackItems.TryGetValue(item, out VrInteractionClickCallBack vrInteraction);
                if (vrInteraction != null)
                {

                    if (item == ListItems.Laptop)
                    {
                        LaptopController.Instance.ReassignCallBack();
                        LaptopController.Instance.PlayVideo();
                        //vrInteraction.SetState(VrInteraction.StateEnum.On);
                    }
                    else if (item == ListItems.USB)
                    {
                        vrInteraction.SetState(VrInteraction.StateEnum.OffWithColliderOff);
                    }
                    else
                    {
                        vrInteraction.SetState(VrInteraction.StateEnum.OffIncludingRenderer);
                    }
                }


                if (IsChecklistCompleted() == true) //This will be called when the last item is selected
                {
                    this.WaitFor(m_ClipboardAudioSource.clip.length, () =>
                    {
                        RemoveClipBoardItems();
                        //LaptopController.Instance.Laptop.SetState(VrInteraction.StateEnum.On);
                    });
                }
            }
            else
            {
                if (item == ListItems.Laptop)
                {
                    LaptopController.Instance.PlayVideo();
                }
            }
        }

        public void RemoveClipBoardItems()
        {
            ToggleClipboard(false);
            InputManagerVR.Instance.ControllerAdded -= OnControllerAdded;

            m_OnCompleted?.Invoke();
        }

        private bool IsChecklistCompleted()
        {
            foreach (var item in m_CheckList.Values)
            {
                if (!item.activeSelf)
                    return false;
            }
            return true;
        }

        private void SetUpAudioSource()
        {
            m_ClipboardAudioSource = m_Clipboard.gameObject.ForceComponent<AudioSource>();
            m_ClipboardAudioSource.playOnAwake = false;
            m_ClipboardAudioSource.spatialBlend = 1;
            m_ClipboardAudioSource.clip = m_ScratchSound;
            m_ClipboardAudioSource.volume = 1f;
            m_ClipboardAudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_ClipboardAudioSource.minDistance = MIN_AUDIO_DISTANCE;
            m_ClipboardAudioSource.maxDistance = MAX_AUDIO_DISTANCE;
        }

        private void LoadScratchSound()
        {
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "Scratch", (soundclip) =>
            {
                m_ScratchSound = soundclip;
            });
        }

        private void PlayScratchSound()
        {
            m_ClipboardAudioSource.Play();
        }
    }
}