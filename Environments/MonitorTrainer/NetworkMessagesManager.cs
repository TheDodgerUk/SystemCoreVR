using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class NetworkMessagesManager : MonoBehaviour
    {
        private const string ASK_ROOM_DETAILS = "ASK_ROOM_DETAILS";
        private const string FULL_ROOM_DETAILS = "FULL_ROOM_DETAILS";
        private const string PLAYER_READY = "PLAYER_READY";
        private const string MULIPLAYER_LOAD = "MULIPLAYER_LOAD";
        private const string SONG_LOADED = "SONG_LOADED";

        private const string TASK_SPECIAL_FOR_ALL_TIMING = "TASK_SPECIAL_FOR_ALL_TIMING";
        private const string TASK_SPECIAL_FOR_ALL_HIT = "TASK_SPECIAL_FOR_ALL_HIT";

        public const string START_SESSION = "START_SESSION";


        public const string PLAYER_SEND_DATA = "PLAYER_SEND_DATA";

        public const string PLAYER_ORDER = "PLAYER_ORDER";
        public const string GUEST_SWAP = "GUEST_SWAP";
        public const string END_CONTINUE = "END_CONTINUE";


        public static NetworkMessagesManager Instance;

        public PhotonMultiplayer PhotonMultiplayerRef;


        public List<Player> Players => PhotonNetwork.PlayerList.ToList();

        public List<RoomInfo> m_RoomInfos = new List<RoomInfo>();


        private RoomState m_RoomState = RoomState.Waiting;
        public enum RoomState
        {
            Waiting,
            Voting,
            Playing,           
        }


        public RoomState RoomStateRef
        {
            get { return m_RoomState; }
            set
            {
                if (Core.PhotonMultiplayerRef.IsOwner == true)
                {
                    if (m_RoomState != value)
                    {
                        m_RoomState = value;
                        Core.PhotonMultiplayerRef.CurrentRoom.IsOpen = (m_RoomState == RoomState.Waiting || m_RoomState == RoomState.Voting);
                    }
                }
            }
        }




        void Awake()
        {
            Instance = this;

            PhotonMultiplayerRef = Core.PhotonMultiplayerRef;
            
            PhotonMultiplayerRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayer);
            PhotonMultiplayerRef.RoomOptionsRef.MaxPlayers = 4;
            PhotonMultiplayerRef.RoomOptionsRef.EmptyRoomTtl = Convert.ToInt32(false); // hardcore way to set to zero ALWAYS
            PhotonMultiplayerRef.RoomOptionsRef.PlayerTtl = Convert.ToInt32(false); // hardcore way to set to zero ALWAYS
            PhotonMultiplayerRef.RoomOptionsRef.IsVisible = true;
            PhotonMultiplayerRef.RoomOptionsRef.IsOpen = true;
            PhotonMultiplayerRef.RoomOptionsRef.CleanupCacheOnLeave = true;

            if (MenuManager.Instance != null && MenuManager.Instance.m_MenuArea.ContainsKey(PlayersEnum.Player1) == true)
            { 
                CrowdAndGenericRockSoundManager.Instance.transform.position = MenuManager.Instance.m_MenuArea[PlayersEnum.Player1].m_Start.transform.position;
            }
            else
            {
                CrowdAndGenericRockSoundManager.Instance.transform.position = Vector3.zero;
            }

            PhotonMultiplayerRef.OnOwnerLoaded((playerOwner) =>
            {

            });
        }

        public void CreateRoom(out string roomNumber)
        {
            for (; ; )
            {
                roomNumber = UnityEngine.Random.Range(0, 9999).ToString("0000");
                string test = roomNumber;
                var foundRoom = m_RoomInfos.Find(e => e.Name == test);
                if (foundRoom == null)
                {
                    PhotonMultiplayerRef.ChangeRoom($"{roomNumber}");

                    break;
                }
            }
        }


        public void SendAskRoomDetails(string lobbyCode)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData(ASK_ROOM_DETAILS, lobbyCode);
        }

        public void ReceiveAskRoomDetails(Action owner)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(ASK_ROOM_DETAILS, (roomCode) =>
                {
                    if (PhotonMultiplayerRef.IsOwnerInRoom && PhotonMultiplayerRef.CurrentRoom.Name == roomCode)
                    {
                        owner?.Invoke();
                    }
                });
        }


        public void SendTaskSpecialForAllTimings(List<TaskSpecialForAllTimings> items)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData<List<TaskSpecialForAllTimings>>(TASK_SPECIAL_FOR_ALL_TIMING, items);
        }

        public void ReceiveTaskSpecialForAllTimings(Action<List<TaskSpecialForAllTimings>> items)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<List<TaskSpecialForAllTimings>>(TASK_SPECIAL_FOR_ALL_TIMING, (rawData) =>
            {
                items?.Invoke(rawData);
            });
        }

        public void SendTaskSpecialForAllHitCompleted(string nickNameOfCompleatedPlayer)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData<string>(TASK_SPECIAL_FOR_ALL_HIT, nickNameOfCompleatedPlayer);
        }

        public void ReceiveTaskSpecialForAllHitCompleted(Action<string> nickNameOfCompleatedPlayer)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(TASK_SPECIAL_FOR_ALL_HIT, (rawData) =>
            {
                nickNameOfCompleatedPlayer?.Invoke(rawData);
            });
        }


        public void SendFullRoomDetails(MonitorTrainerConsts.PlayerChoiceData baseItem)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData< MonitorTrainerConsts.PlayerChoiceData>(FULL_ROOM_DETAILS, baseItem);
        }

        public void ReceiveFullRoomDetails(Action<MonitorTrainerConsts.PlayerChoiceData> baseItem)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MonitorTrainerConsts.PlayerChoiceData>(FULL_ROOM_DETAILS, (rawData) =>
            {
                baseItem?.Invoke(rawData);
            });
        }

        public void SendMuliplayerload(string roomNumber)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData(MULIPLAYER_LOAD, roomNumber);
        }

        public void ReceiveMuliplayerload(Action<string> roomNumber)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(MULIPLAYER_LOAD, (receiveRoomNumber) =>
            {
                roomNumber?.Invoke(receiveRoomNumber);
            });
        }

        public void SendPlayerReady(MenuManager.PlayerReady playerReady)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData< MenuManager.PlayerReady>(PLAYER_READY, playerReady);
        }

        public void ReceivePlayerReady(Action<MenuManager.PlayerReady> playerReady)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MenuManager.PlayerReady>(PLAYER_READY, (rawData) =>
            {
                playerReady?.Invoke(rawData);
            });
        }


        /// <summary>
        /// //////////////////////////////////////////////////////////
        /// </summary>

        public void SendSongFullyLoaded(MenuManager.PlayerReady playerReady)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData<MenuManager.PlayerReady>(SONG_LOADED, playerReady);
        }

        public void ReceiveSongFullyLoaded(Action<MenuManager.PlayerReady> playerReady)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MenuManager.PlayerReady>(SONG_LOADED, (rawData) =>
            {
                playerReady?.Invoke(rawData);
            });
        }


        /// <summary>
        /// ///////////////////////////////////////////////////////
        /// </summary>
        /// 
        public void SendStartSession()
        {
            Core.PhotonGenericRef.SendGenericHeader(START_SESSION);
        }

        public void ReceiveStartSession(Action callback)
        {
            Core.PhotonGenericRef.CollectGenericHeader(START_SESSION, callback);
        }


        /////////////////////////////////////////////////////////
        ///
        public void SendPlayerPrefsData(ref MonitorTrainerConsts.LocalPlayerData.SaveDataClass.SendData data)
        {
            data.ActorNumber = Core.PhotonMultiplayerRef.MySelf.ActorNumber;
            data.Nickname = Core.PhotonMultiplayerRef.MySelf.NickName;

            Core.PhotonGenericRef.SendGenericHeaderAndData<MonitorTrainerConsts.LocalPlayerData.SaveDataClass.SendData>(PLAYER_SEND_DATA, data);
        }


        public void ReceivePlayerPrefsData(Action<MonitorTrainerConsts.LocalPlayerData.SaveDataClass.SendData> callback)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MonitorTrainerConsts.LocalPlayerData.SaveDataClass.SendData>(PLAYER_SEND_DATA, (callback));
        }

        public void SendPlayerOrder(List<LocalPlayerData.SaveDataClass.SendData> playerReady)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData<List<LocalPlayerData.SaveDataClass.SendData>>(PLAYER_ORDER, playerReady);
        }

        public void ReceivePlayerOrder(Action<List<LocalPlayerData.SaveDataClass.SendData>> playerReady)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<List<LocalPlayerData.SaveDataClass.SendData>>(PLAYER_ORDER, (rawData) =>
            {
                playerReady?.Invoke(rawData);
            });
        }

        public void SendGuestSwap(GuestSwap playerReady)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData<GuestSwap>(GUEST_SWAP, playerReady);
        }

        public void ReceiveGuestSwap(Action<GuestSwap> playerReady)
        {
            Core.PhotonGenericRef.CollectGenericHeaderAndData<GuestSwap>(GUEST_SWAP, (rawData) =>
            {
                playerReady?.Invoke(rawData);
            });
        }

        public void SendEndContinue()
        {
            Core.PhotonGenericRef.SendGenericHeader(END_CONTINUE);
        }

        public void ReceiveEndContinue(Action receive)
        {
            Core.PhotonGenericRef.CollectGenericHeader(END_CONTINUE, () =>
            {
                receive?.Invoke();
            });
        }

    }
}
