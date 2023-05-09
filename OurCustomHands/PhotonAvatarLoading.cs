// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using ExitGames.Client.Photon;
    using Oculus.Avatar2;
    using Photon.Realtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Photon Networking/PhotonAvatar")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonAvatarLoading : MonoBehaviour
    {
        public static string AVATAR_CHANGED = "AVATAR_CHANGED";
        public enum MessageType
        {
            AvatarChanged
        }

        private bool m_HasCdnAvatarBeenCalled = false;

        private SampleAvatarEntity m_SampleAvatarEntity;
        private PhotonView m_PhotonView;

        private Transform m_DebugInfo;
        private Text m_Info1;
        private Text m_Info2;
        private Text m_Info3;
        private Text m_Info4;
        private Text m_Info5;


        private int m_SentCount = 0;
        private int m_RecieveCount = 0;

        private List<MonoBehaviourPun> m_MonoBehaviourPuns;

        public class LoadingData
        {
            public ulong _userId;
            public int ActorNumber;

        }

        public void Awake()
        {
            m_SampleAvatarEntity = this.GetComponent<SampleAvatarEntity>();
            m_PhotonView = this.GetComponent<PhotonView>();

            m_DebugInfo = this.transform.SearchComponent<Transform>("DebugInfo");
            m_Info1 = this.transform.SearchComponent<Text>("Info1");
            m_Info2 = this.transform.SearchComponent<Text>("Info2");
            m_Info3 = this.transform.SearchComponent<Text>("Info3");
            m_Info4 = this.transform.SearchComponent<Text>("Info4");
            m_Info5 = this.transform.SearchComponent<Text>("Info5");
            m_SampleAvatarEntity.OnDefaultAvatarLoadedEvent.AddListener(BasicAvatarLoaded);
            m_SampleAvatarEntity.OnUserAvatarLoadedEvent.AddListener(BasicAvatarLoaded);
            m_DebugInfo.SetActive(false);

            m_MonoBehaviourPuns = this.GetComponentsInChildren<MonoBehaviourPun>().ToList();
            if (CameraControllerVR.Instance.NetworkPlayerRef == null && m_PhotonView.IsMine)
            {
                CameraControllerVR.Instance.NetworkPlayerRef = this;
            }
        }





        private void Update()
        {
            m_DebugInfo.LookAt(CameraControllerVR.Instance.transform);
            m_Info1.text = $"Sent count {m_SentCount} , LocalPlayer number, {PhotonNetwork.LocalPlayer.ActorNumber.ToString()}, own number {m_PhotonView.Owner.ActorNumber}";
            m_Info2.text = $"ownNumber {m_PhotonView.Owner.ActorNumber}";
            m_Info3.text = $"recieveCount  {m_RecieveCount}, loadedID {m_SampleAvatarEntity._userId}   m_HasCdnAvatarBeenCalled: {m_HasCdnAvatarBeenCalled}";
            ///m_Info4.text = $"m_ErrorMessage,   {m_SampleAvatarEntity.m_ErrorMessage}";
        }
     
        private void BasicAvatarLoaded(OvrAvatarEntity arg0)
        {
            if (m_PhotonView.IsMine)
            {
                if (m_HasCdnAvatarBeenCalled == false)
                {
                    m_HasCdnAvatarBeenCalled = true;
                    m_SampleAvatarEntity.OnUserAvatarLoadedEvent.AddListener(OwnAvatarLoaded);
                    m_SampleAvatarEntity.LoadRemoteUserCdnAvatar(0);
                }
            }
            else
            {
                // this placed here , so it has to load the BasicAvatarLoaded before changing otherwise it can break it all 
                Core.PhotonGenericRef.CollectGenericHeaderAndData<LoadingData>(AVATAR_CHANGED, (loadingData) =>
                {
                    int localActorNumber = loadingData.ActorNumber;
                   //// Debug.LogError($"localActorNumber String: {localActorNumber} m_OwnActorNumber Int: {m_PhotonView.Owner.ActorNumber} localActorNumberString int {localActorNumber}");
                    if (m_PhotonView.IsMine == false)
                    {
                        if (m_PhotonView.Owner.ActorNumber == localActorNumber)
                        {
                            m_RecieveCount++;                            
                            if (m_HasCdnAvatarBeenCalled == false)
                            {
                                // this is so it is only done the once, and you will never get the Ghost weird avatar
                                m_HasCdnAvatarBeenCalled = true;
                                m_SampleAvatarEntity.LoadRemoteUserCdnAvatar(loadingData._userId);
                            }
                        }
                    }
                });
            }
        }

        private void OwnAvatarLoaded(OvrAvatarEntity arg0)
        {
            Core.VisualLoggerRef.SetInformation(VisualLogger.InformationType.Avatars, VisualLogger.Information.Information_1, $"My ActorNumber {PhotonNetwork.LocalPlayer.ActorNumber.ToString()}, ActorNumber {m_PhotonView.Owner.ActorNumber}");
            StartCoroutine(SendAvatar(arg0._userId));
        }

        // send all the time, so when you join it keeps sending 
        // not  the best way to do it though
        private IEnumerator SendAvatar(ulong id)
        {
            while (true)
            {
                yield return new WaitForSeconds(4f);
                m_SentCount++;
                Core.VisualLoggerRef.SetInformation(VisualLogger.InformationType.Avatars, VisualLogger.Information.Information_2, $"My SentCount {m_SentCount}");

                LoadingData send = new LoadingData
                {
                    ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
                    _userId = id,
                };
                Core.PhotonGenericRef.SendGenericHeaderAndData<LoadingData>(AVATAR_CHANGED, send);
            }
        }
    }
}