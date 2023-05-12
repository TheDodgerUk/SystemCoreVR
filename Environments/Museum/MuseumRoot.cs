using Autohand;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Museum
{

    public class Data
    {
        public AudioClip m_AudioClip;
        public string m_BodyText;
        public string m_ShortName;
        public ArFeaturesMetaData m_ArData;
        public Vector3 m_Position;
        public Quaternion m_Rotation;
        public XrayProduct m_XrayProduct;
        public ExplodeProduct m_ExplodeProduct;
        public VideoClip m_VideoClip;
    }

    public class MuseumRoot : MonoBehaviour
    {
        private InteractiveMenu m_InteractiveMenu;
        private Dictionary<VrInteraction, Data> m_CollectedData = new Dictionary<VrInteraction, Data>();

        public void Initialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.PhotonMultiplayerRef.InitialiseForApp();
            Core.PhotonMultiplayerRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayer);
            Core.PhotonMultiplayerRef.RoomOptionsRef.MaxPlayers = 4;
            Core.PhotonMultiplayerRef.RoomOptionsRef.EmptyRoomTtl = Convert.ToInt32(false); // hardcore way to set to zero ALWAYS
            Core.PhotonMultiplayerRef.RoomOptionsRef.PlayerTtl = Convert.ToInt32(false); // hardcore way to set to zero ALWAYS
            Core.PhotonMultiplayerRef.RoomOptionsRef.IsVisible = true;
            Core.PhotonMultiplayerRef.RoomOptionsRef.IsOpen = true;
            Core.PhotonMultiplayerRef.RoomOptionsRef.CleanupCacheOnLeave = true;

            Core.PhotonMultiplayerRef.JoinLobby();
            Core.PhotonMultiplayerRef.ChangeRoom($"{nameof(Museum.MuseumRoot)}");

            var ui = GameObject.Find("UI_Stuff");
            m_InteractiveMenu = ui.AddComponent<InteractiveMenu>();

#if VR_INTERACTION
            CameraControllerVR.Instance.TeleportAvatar(this.gameObject.scene, new Vector3(-2f, 0f, 0f), null);
#endif

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItemList(Core.Mono, (allAudio) =>
            {

                List<VrInteraction> allStanding = Core.Scene.GetSpawnedVrInteractionGUID("8012d8c2-5112-48d7-a263-2864eec37da0"); // standing
                List<VrInteraction> allLargeFlat = Core.Scene.GetSpawnedVrInteractionGUID("c3e45577-8be4-46ce-82eb-3b9cc2c39535"); // standing
                List<VrInteraction> allSmallFlat = Core.Scene.GetSpawnedVrInteractionGUID("059d13ec-5511-490a-b507-e141d37f6db5"); // standing

                if(allStanding .Count == 0)
                {
                    DebugBeep.LogError("allStanding is zero", DebugBeep.MessageLevel.Low);
                }

                if (allLargeFlat.Count == 0)
                {
                    DebugBeep.LogError("allLargeFlat is zero", DebugBeep.MessageLevel.Low);
                }

                if (allSmallFlat.Count == 0)
                {
                    DebugBeep.LogError("allSmallFlat is zero", DebugBeep.MessageLevel.Low);
                }
                List <VrInteraction> allItems = new List<VrInteraction>();
                allItems.AddRange(allStanding);
                allItems.AddRange(allLargeFlat);
                allItems.AddRange(allSmallFlat);

                foreach (var item in allItems)
                {

                    var mainPrefabList = item.gameObject.transform.parent.GetComponentsInChildren<VrInteraction>(true).ToList();
                    var localItem = item;
                    mainPrefabList.RemoveAll(e => e.IsRootGameObject == false);
                    var mainPrefab = mainPrefabList.Find(e => (e.name != localItem.name) &&  e.CatalogueEntryRef.CatalogueFilters!= null && e.CatalogueEntryRef.CatalogueFilters.Count > 0);
                    if (mainPrefab == null)
                    {
                        DebugBeep.LogError($"Not found sibling {localItem.name}", DebugBeep.MessageLevel.Medium,  localItem.gameObject);
                    }
                    else
                    {
                        Data newData = new Data();
                        newData.m_ShortName = mainPrefab.CatalogueEntryRef.ShortName;
                        newData.m_Position = localItem.transform.position + new Vector3(0f, 1f, 0f);
                        newData.m_Rotation = localItem.transform.rotation * Quaternion.Euler(0, 90, 0);

                        var menuPosition = item.gameObject.transform.parent.SearchComponent<Transform>("Menu Position", false);
                        if(menuPosition != null)
                        {
                            newData.m_Position = menuPosition.transform.position;
                            newData.m_Rotation = menuPosition.transform.rotation;
                        }
                        m_CollectedData.Add(mainPrefab, newData);

                        ClipAndData(mainPrefab, allAudio, newData);

                        var ar = mainPrefab.GetMetaDataTypeFromRoot(MetaDataType.ArFeatures);
                        if (ar.Count != 0)
                        {
                            newData.m_ArData = (ArFeaturesMetaData)ar[0];
                            newData.m_XrayProduct = mainPrefab.GetComponent<XrayProduct>();
                            newData.m_ExplodeProduct = mainPrefab.GetComponent<ExplodeProduct>();
                        }
                        if (localItem.name == mainPrefab.name)
                        {
                            DebugBeep.LogError($"Not found sibling {localItem.name} {mainPrefab.name}", DebugBeep.MessageLevel.Medium, localItem.gameObject);
                        }
                    }

                    foreach (var col in item.ColliderList)
                    {
                        var touchEvent = col.AddComponent<Autohand.HandTouchEvent>();
                        touchEvent.HandStartTouch = new Autohand.UnityHandEvent();
                        touchEvent.HandStartTouch.AddListener((hand) =>
                        {
                            if (m_CollectedData.ContainsKey(mainPrefab) == true)
                            {
                                m_InteractiveMenu.SetData(m_CollectedData[mainPrefab]);
                            }
                        });
                    }
                    


                }
                var first = m_CollectedData.First();
                m_InteractiveMenu.SetData(m_CollectedData[first.Key]);

                var all = Core.Scene.GetAllSpawnedVrInteraction();
                foreach (var item in all) 
                {
                    var pickup = item.GetVrInteractionFromRoot(MetaDataType.ContentPickUp);
                    if(pickup.Count != 0)
                    {
#if VR_INTERACTION
                        ReleaseGrip(null, pickup[0].GetComponent<Grabbable>());
                        CameraControllerVR.Instance.HandLeftRef.OnBeforeGrabbed += GrabGrip;
                        CameraControllerVR.Instance.HandRightRef.OnBeforeGrabbed += GrabGrip;

                        CameraControllerVR.Instance.HandLeftRef.OnReleased += ReleaseGrip;
                        CameraControllerVR.Instance.HandRightRef.OnReleased += ReleaseGrip;
#endif
                    }
                }
            });


            var all = Core.Scene.GetAllSpawnedVrInteraction();
            foreach (var item in all)
            {
                LODGroup lod = item.GetComponentInChildren<LODGroup>();
                if (lod != null)
                {
                    lod.enabled = false;
                }
            }

            string CorrectAudioName(List<string> allAudio, string productcode)
            {
                foreach (var item in allAudio)
                {
                    var splits = item.Split('_');
                    if(splits.Length ==2)
                    {
                        if(splits[1].ToLower() == productcode.ToLower())
                        {
                            return item;
                        }
                    }
                }
                return null;
            }


            void ClipAndData(VrInteraction mainPrefab, List<string> allAudio, Data newData)
            {
                if (string.IsNullOrEmpty(mainPrefab.CatalogueEntryRef.ReferenceNumber) == false)
                {
                    string correct = CorrectAudioName(allAudio, mainPrefab.CatalogueEntryRef.ReferenceNumber);
                    if (string.IsNullOrEmpty(correct) == false)
                    {
                        Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, correct, (audio) =>
                        {
                            newData.m_AudioClip = audio;
                        });

                        Core.AssetBundlesRef.VideoClipAssetBundleRef.GetItem(Core.Mono, correct, (video) =>
                        {
                            newData.m_VideoClip = video;
                        });
                    }

                    Core.Catalogue.ProductInfoManagerRef.GetProductInfo(mainPrefab, (data) =>
                    {
                        if (data != null)
                        {
                            newData.m_BodyText = data.headline;
                        }
                    });
                }
            }
        }

        private void GrabGrip(Hand hand, Grabbable grabbable)
        {
            var vr = grabbable.GetComponent<VrInteraction>();
            if (vr != null)
            {
                VrInteractionPickUp pick = (VrInteractionPickUp)vr;
                pick.Body.isKinematic = false;
            }
        }

        private void ReleaseGrip(Hand hand, Grabbable grabbable)
        {
            var vr = grabbable.GetComponent <VrInteraction> ();
            if(vr != null) 
            {
                Core.Mono.WaitFor(1f, () =>
                {
                    if(grabbable.IsHeld() == false)
                    {
                        VrInteractionPickUp pick = (VrInteractionPickUp)vr;
                        pick.ResetToOriginalState();
                    }
                });
                
            }
        }


    }
}
