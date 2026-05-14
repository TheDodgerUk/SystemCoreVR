#if Photon
using Photon.Pun;
using Photon.Realtime;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
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

#if Photon
        public PhotonMultiplayer PhotonMultiplayerRef;

        public List<Player> Players => PhotonNetwork.PlayerList.ToList();

        public List<RoomInfo> m_RoomInfos = new List<RoomInfo>();
#else
        public List<object> Players => new List<object>();
#endif

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
#if Photon
                if (Core.PhotonMultiplayerRef != null && Core.PhotonMultiplayerRef.IsOwner == true)
                {
                    if (m_RoomState != value)
                    {
                        m_RoomState = value;

                        if (Core.PhotonMultiplayerRef.CurrentRoom != null)
                        {
                            Core.PhotonMultiplayerRef.CurrentRoom.IsOpen =
                                m_RoomState == RoomState.Waiting || m_RoomState == RoomState.Voting;
                        }
                    }
                }
#else
                m_RoomState = value;
#endif
            }
        }

        private void Awake()
        {
            Instance = this;

#if Photon
            PhotonMultiplayerRef = Core.PhotonMultiplayerRef;

            PhotonMultiplayerRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayer);
            PhotonMultiplayerRef.RoomOptionsRef.MaxPlayers = 4;
            PhotonMultiplayerRef.RoomOptionsRef.EmptyRoomTtl = Convert.ToInt32(false);
            PhotonMultiplayerRef.RoomOptionsRef.PlayerTtl = Convert.ToInt32(false);
            PhotonMultiplayerRef.RoomOptionsRef.IsVisible = true;
            PhotonMultiplayerRef.RoomOptionsRef.IsOpen = true;
            PhotonMultiplayerRef.RoomOptionsRef.CleanupCacheOnLeave = true;

            PhotonMultiplayerRef.OnOwnerLoaded((playerOwner) =>
            {
            });
#endif

            if (MenuManager.Instance != null &&
                MenuManager.Instance.m_MenuArea.ContainsKey(PlayersEnum.Player1) == true)
            {
                CrowdAndGenericRockSoundManager.Instance.transform.position =
                    MenuManager.Instance.m_MenuArea[PlayersEnum.Player1].m_Start.transform.position;
            }
            else
            {
                CrowdAndGenericRockSoundManager.Instance.transform.position = Vector3.zero;
            }
        }

        public void CreateRoom(out string roomNumber)
        {
            roomNumber = string.Empty;

#if Photon
            for (;;)
            {
                roomNumber = UnityEngine.Random.Range(0, 9999).ToString("0000");
                string test = roomNumber;

                var foundRoom = m_RoomInfos.Find(e => e.Name == test);
                if (foundRoom == null)
                {
                    PhotonMultiplayerRef.ChangeRoom(roomNumber);
                    break;
                }
            }
#endif
        }

        public void SendAskRoomDetails(string lobbyCode)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(ASK_ROOM_DETAILS, lobbyCode);
#endif
        }

        public void ReceiveAskRoomDetails(Action owner)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(ASK_ROOM_DETAILS, (roomCode) =>
            {
                if (PhotonMultiplayerRef.IsOwnerInRoom &&
                    PhotonMultiplayerRef.CurrentRoom != null &&
                    PhotonMultiplayerRef.CurrentRoom.Name == roomCode)
                {
                    owner?.Invoke();
                }
            });
#endif
        }

        public void SendTaskSpecialForAllTimings(List<TaskSpecialForAllTimings> items)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(TASK_SPECIAL_FOR_ALL_TIMING, items);
#endif
        }

        public void ReceiveTaskSpecialForAllTimings(Action<List<TaskSpecialForAllTimings>> items)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<List<TaskSpecialForAllTimings>>(
                TASK_SPECIAL_FOR_ALL_TIMING,
                (rawData) => { items?.Invoke(rawData); });
#endif
        }

        public void SendTaskSpecialForAllHitCompleted(string nickNameOfCompleatedPlayer)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(TASK_SPECIAL_FOR_ALL_HIT, nickNameOfCompleatedPlayer);
#endif
        }

        public void ReceiveTaskSpecialForAllHitCompleted(Action<string> nickNameOfCompleatedPlayer)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(
                TASK_SPECIAL_FOR_ALL_HIT,
                (rawData) => { nickNameOfCompleatedPlayer?.Invoke(rawData); });
#endif
        }

        public void SendFullRoomDetails(MonitorTrainerConsts.PlayerChoiceData baseItem)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(FULL_ROOM_DETAILS, baseItem);
#endif
        }

        public void ReceiveFullRoomDetails(Action<MonitorTrainerConsts.PlayerChoiceData> baseItem)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MonitorTrainerConsts.PlayerChoiceData>(
                FULL_ROOM_DETAILS,
                (rawData) => { baseItem?.Invoke(rawData); });
#endif
        }

        public void SendMuliplayerload(string roomNumber)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(MULIPLAYER_LOAD, roomNumber);
#endif
        }

        public void ReceiveMuliplayerload(Action<string> roomNumber)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(
                MULIPLAYER_LOAD,
                (receiveRoomNumber) => { roomNumber?.Invoke(receiveRoomNumber); });
#endif
        }

        public void SendPlayerReady(MenuManager.PlayerReady playerReady)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(PLAYER_READY, playerReady);
#endif
        }

        public void ReceivePlayerReady(Action<MenuManager.PlayerReady> playerReady)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MenuManager.PlayerReady>(
                PLAYER_READY,
                (rawData) => { playerReady?.Invoke(rawData); });
#endif
        }

        public void SendSongFullyLoaded(MenuManager.PlayerReady playerReady)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(SONG_LOADED, playerReady);
#endif
        }

        public void ReceiveSongFullyLoaded(Action<MenuManager.PlayerReady> playerReady)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<MenuManager.PlayerReady>(
                SONG_LOADED,
                (rawData) => { playerReady?.Invoke(rawData); });
#endif
        }

        public void SendStartSession()
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeader(START_SESSION);
#endif
        }

        public void ReceiveStartSession(Action callback)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeader(START_SESSION, callback);
#endif
        }

        public void SendPlayerPrefsData(ref MonitorTrainerConsts.LocalPlayerData.SaveDataClass.SendData data)
        {
#if Photon
            data.ActorNumber = Core.PhotonMultiplayerRef.MySelf.ActorNumber;
            data.Nickname = Core.PhotonMultiplayerRef.MySelf.NickName;

            Core.PhotonGenericRef.SendGenericHeaderAndData(PLAYER_SEND_DATA, data);
#endif
        }

        public void ReceivePlayerPrefsData(
            Action<MonitorTrainerConsts.LocalPlayerData.SaveDataClass.SendData> callback)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData(PLAYER_SEND_DATA, callback);
#endif
        }

        public void SendPlayerOrder(List<LocalPlayerData.SaveDataClass.SendData> playerReady)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(PLAYER_ORDER, playerReady);
#endif
        }

        public void ReceivePlayerOrder(Action<List<LocalPlayerData.SaveDataClass.SendData>> playerReady)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<List<LocalPlayerData.SaveDataClass.SendData>>(
                PLAYER_ORDER,
                (rawData) => { playerReady?.Invoke(rawData); });
#endif
        }

        public void SendGuestSwap(GuestSwap playerReady)
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeaderAndData(GUEST_SWAP, playerReady);
#endif
        }

        public void ReceiveGuestSwap(Action<GuestSwap> playerReady)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeaderAndData<GuestSwap>(
                GUEST_SWAP,
                (rawData) => { playerReady?.Invoke(rawData); });
#endif
        }

        public void SendEndContinue()
        {
#if Photon
            Core.PhotonGenericRef.SendGenericHeader(END_CONTINUE);
#endif
        }

        public void ReceiveEndContinue(Action receive)
        {
#if Photon
            Core.PhotonGenericRef.CollectGenericHeader(END_CONTINUE, () =>
            {
                receive?.Invoke();
            });
#endif
        }
    }
}