#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorTrainer;
using static MonitorTrainer.MonitorTrainerConsts;
using UnityEngine.UI;
using static MonitorTrainer.MenuManager;

[CustomEditor(typeof(MonitorTrainer.MenuManager))]
public class MenuManagerEditor : MenuEditorBase
{

    public override void OnInspectorGUI()
    {
        MonitorTrainer.MenuManager menu = (MonitorTrainer.MenuManager)target;


        switch (menu.m_CurrentScreenMenu)
        {
            case MenuManager.ScreenMenu.MainRootScreen:
                MainRootScreen(menu);
                break;
            case MenuManager.ScreenMenu.SoloPlayer:
                SoloPlayer(menu);
                break;
            case MenuManager.ScreenMenu.LobbyCode:
                LobbyCode(menu);
                break;
            case MenuManager.ScreenMenu.MultiplayerChoice:
                MultiplayerChoice(menu);
                break;
            case MenuManager.ScreenMenu.MultiplayerLobbyGuest:
                MultiplayerLobbyGuest(menu);
                break;
            case MenuManager.ScreenMenu.MultiplayerLobbyHost:
                MultiplayerLobbyHost(menu);
                break;
            case MenuManager.ScreenMenu.LeaveLobby:
                LeaveLobby(menu);
                break;

            case MenuManager.ScreenMenu.Customise:
                Customise(menu);
                break;
            case MenuManager.ScreenMenu.AddDecals:
                AddDecals(menu);
                break;

            case MenuManager.ScreenMenu.TierProgression:
                TierProgression(menu);
                break;

            case MenuManager.ScreenMenu.Challenges:
                Challenges(menu);
                break;
            case MenuManager.ScreenMenu.EndGameMenuMultiPlayer:
                EndGameMenuMultiPlayer(menu);
                break;
            case MenuManager.ScreenMenu.EndGameMenuSinglePlayer:
                EndGameMenuSinglePlayer(menu);
                break;
            case MenuManager.ScreenMenu.NetworkErrorScreen:
                NetworkError(menu);
                break;
        }

        GUILayout.Space(20);
        MenuManager.BaseChoice baseData = menu.m_AllScreens[menu.m_CurrentScreenMenu];

        Display(baseData.m_BasePlayStyle);
        Display(baseData.m_BaseCustomise);
        Display(baseData.m_BaseStats);
        Display(baseData.m_BaseSettings);
    }

    private void MainRootScreen(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_MainRootScreenData.m_Solo);
        Display(menu.m_MainRootScreenData.m_Multiplayer);
        Display(menu.m_MainRootScreenData.m_Tutorial);
    }

    private void SoloPlayer(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_SoloPlayerData.m_Easy);
        Display(menu.m_SoloPlayerData.m_Medium);
        Display(menu.m_SoloPlayerData.m_Hard);
        Display(menu.m_SoloPlayerData.m_LaunchSolo);

        GUILayout.Space(10);
        var list = menu.m_SoloPlayerData.m_VerticalData.m_TogglePool.GetPublicList();
        foreach (var item in list)
        {
            Display(item);
        }
    }
    
    private void MultiplayerChoice(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_MultiplayerChoiceData.m_JoinRandomLobby);
        Display(menu.m_MultiplayerChoiceData.m_SetupLobby);
        Display(menu.m_MultiplayerChoiceData.m_EnterLobbyCode);
    }

    private void LobbyCode(MonitorTrainer.MenuManager menu)
    {
        foreach (var item in menu.m_LobbyCodeData.m_NumberButtons)
        {
            if (GUILayout.Button(item.name))
            {
                item.onClick.Invoke();
            }
        }
        Display(menu.m_LobbyCodeData.m_TickButton);
        Display(menu.m_LobbyCodeData.m_CloseButton);
    }

    private void MultiplayerLobbyGuest(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_MultiplayerLobbyGuestData.m_MarkReady);
        Display(menu.m_MultiplayerLobbyGuestData.m_CloseButton);
        Display(menu.m_MultiplayerLobbyGuestData.m_SwapTeamsButton);
    }
    private void LeaveLobby(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_LeaveLobbyData.m_Stay);
        Display(menu.m_LeaveLobbyData.m_LeaveLobby);
    }

    private void MultiplayerLobbyHost(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_MultiplayerLobbyHostData.m_LaunchMultiplayer);
        Display(menu.m_MultiplayerLobbyHostData.m_Easy);
        Display(menu.m_MultiplayerLobbyHostData.m_Medium);
        Display(menu.m_MultiplayerLobbyHostData.m_Hard);
        Display(menu.m_MultiplayerLobbyHostData.m_CloseButton);

        Display(menu.m_MultiplayerLobbyHostData.m_SwapTeamsButton);

        GUILayout.Space(20);
        var list = menu.m_MultiplayerLobbyHostData.m_VerticalData.m_TogglePool.GetPublicList();
        foreach (var item in list)
        {
            Display(item);
        }
    }

    private void Customise(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_CustomiseData.m_AddDecals);
        Display(menu.m_CustomiseData.m_CustomisePod);
        Display(menu.m_CustomiseData.m_AlterMaterials);
    }

    private void AddDecals(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_AddDecalsData.m_MonitorButton.m_Button);
        Display(menu.m_AddDecalsData.m_FlightCasesButton.m_Button);
        Display(menu.m_AddDecalsData.m_LaptopButton.m_Button);
        Display(menu.m_AddDecalsData.m_LampButton.m_Button);

        Display(menu.m_AddDecalsData.m_SelectableItems_Monitor);
        Display(menu.m_AddDecalsData.m_SelectableItems_FlightCases);
        Display(menu.m_AddDecalsData.m_SelectableItems_Laptop);
        Display(menu.m_AddDecalsData.m_SelectableItems_Lamp);
    }

    private void TierProgression(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_TierProgressionData.m_ChallengesButton);
        Display(menu.m_TierProgressionData.m_TierProgressionButton);
    }

    private void Challenges(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_ChallengesData.m_ChallengesButton);
        Display(menu.m_ChallengesData.m_TierProgressionButton);
    }

    private void EndGameMenuMultiPlayer(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_EndGameMenuMultiPlayerData.m_LeaveLobby);
        Display(menu.m_EndGameMenuMultiPlayerData.m_Rematch);
        Display(menu.m_EndGameMenuMultiPlayerData.m_Continue);
    }

    private void EndGameMenuSinglePlayer(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_EndGameMenuSinglePlayerData.m_LeaveLobby);
        Display(menu.m_EndGameMenuSinglePlayerData.m_Rematch);
        Display(menu.m_EndGameMenuSinglePlayerData.m_Continue);
    }

    private void NetworkError(MonitorTrainer.MenuManager menu)
    {
        Display(menu.m_NetworkErrorData.m_Continue);
    }

    ////private void RootMenu(MonitorTrainer.MenuManager menu)
    ////{
    ////    if (GUILayout.Button(menu.m_RootMenu.m_PlayStyle.name))
    ////    {
    ////        menu.m_RootMenu.m_PlayStyle.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_RootMenu.m_Customise.name))
    ////    {
    ////        menu.m_RootMenu.m_Customise.onClick.Invoke();
    ////    }


    ////    if (GUILayout.Button(menu.m_RootMenu.m_Stats.name))
    ////    {
    ////        menu.m_RootMenu.m_Stats.onClick.Invoke();
    ////    }


    ////    if (GUILayout.Button(menu.m_RootMenu.m_Settings.name))
    ////    {
    ////        menu.m_RootMenu.m_Settings.onClick.Invoke();
    ////    }            
    ////}


    ////private void SongChoiceMenu(MonitorTrainer.MenuManager menu)
    ////{
    ////    foreach (var item in menu.m_SongDifficultyChoice.m_ButtonPool.GetPublicList())
    ////    {
    ////        if (GUILayout.Button(item.name))
    ////        {
    ////            item.onClick.Invoke();
    ////        }
    ////    }


    ////    if (GUILayout.Button(menu.m_SongDifficultyChoice.m_Advanced.name))
    ////    {
    ////        menu.m_SongDifficultyChoice.m_Advanced.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_SongDifficultyChoice.m_Normal.name))
    ////    {
    ////        menu.m_SongDifficultyChoice.m_Normal.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_SongDifficultyChoice.m_Start.name))
    ////    {
    ////        menu.m_SongDifficultyChoice.m_Start.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_SongDifficultyChoice.m_BaseBack.name))
    ////    {
    ////        menu.m_SongDifficultyChoice.m_BaseBack.onClick.Invoke();
    ////    }
    ////}

    ////private void MultiplayerLobbyMenu(MonitorTrainer.MenuManager menu)
    ////{
    ////    if (GUILayout.Button(menu.m_MultiplayerLobby.m_CreateRoom.name))
    ////    {
    ////        menu.m_MultiplayerLobby.m_CreateRoom.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_MultiplayerLobby.m_BaseBack.name))
    ////    {
    ////        menu.m_MultiplayerLobby.m_BaseBack.onClick.Invoke();
    ////    }

    ////    var allRooms = menu.m_MultiplayerLobby.m_ButtonPool.GetPublicList();
    ////    foreach (var room in allRooms)
    ////    {
    ////        if (GUILayout.Button(room.name))
    ////        {
    ////            room.onClick.Invoke();
    ////        }
    ////    }
    ////}

    ////private void PlayStyle(MonitorTrainer.MenuManager menu)
    ////{
    ////    if (GUILayout.Button(menu.m_PlayerStyleData.m_Solo.name))
    ////    {
    ////        menu.m_PlayerStyleData.m_Solo.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_PlayerStyleData.m_Multiplayer.name))
    ////    {
    ////        menu.m_PlayerStyleData.m_Multiplayer.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_PlayerStyleData.m_BaseBack.name))
    ////    {
    ////        menu.m_PlayerStyleData.m_BaseBack.onClick.Invoke();
    ////    }
    ////}

    ////private void MultiplayerRoomMenu(MonitorTrainer.MenuManager menu)
    ////{
    ////    if (GUILayout.Button(menu.m_MultiplayerLobby.m_CreateRoom.name))
    ////    {
    ////        menu.m_MultiplayerRoom.m_BaseBack.onClick.Invoke();
    ////    }
    ////}

    ////private void ColourButton(MonitorTrainer.MenuManager menu)
    ////{
    ////    foreach (var item in menu.m_ColourChoice.m_Buttons)
    ////    {
    ////        if (GUILayout.Button(item.m_Button.name))
    ////        {
    ////            item.m_Button.onClick.Invoke();
    ////        }
    ////    }

    ////    if (GUILayout.Button(menu.m_ColourChoice.m_BaseBack.name))
    ////    {
    ////        menu.m_ColourChoice.m_BaseBack.onClick.Invoke();
    ////    }
    ////}
    ////private void DifficultyChoiceMenu(MonitorTrainer.MenuManager menu)
    ////{
    ////    if (GUILayout.Button(menu.m_SongDifficultyChoice.m_Advanced.name))
    ////    {
    ////        menu.m_SongDifficultyChoice.m_Advanced.onClick.Invoke();
    ////    }

    ////    if (GUILayout.Button(menu.m_SongDifficultyChoice.m_BaseBack.name))
    ////    {
    ////        menu.m_SongDifficultyChoice.m_BaseBack.onClick.Invoke();
    ////    }
    ////}

}
#endif
