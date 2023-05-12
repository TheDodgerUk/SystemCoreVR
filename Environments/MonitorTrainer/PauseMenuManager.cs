using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class PauseMenuManager : MonoBehaviour
    {

        private Transform m_Root;

        public class PauseData
        {
            public Button m_Button_Return;
            public Button m_Button_Settings;
            public Button m_Button_LeaveGame;
        }

        public class SettingsData
        {
            public Button m_Button_Back;
            public Toggle m_VignetteToggle;
            public Toggle m_SnapTurnsToggle;
            public Toggle m_ForceReachToggle;
        }

        public PauseData PauseDataRef = new PauseData();
        public SettingsData SettingsDataRef = new SettingsData();

        public void Show(bool enable)
        {
            m_Root.SetActive(enable);
            m_Root.gameObject.PlaceInfrontMainCamera(0.2f);
            if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle == MonitorTrainerConsts.PlayStyleEnum.Solo)
            {
                MenuManager.Instance.IsPaused = enable;
            }
        }


        void Start()
        {
            m_Root = this.transform.SearchComponent<Transform>("PauseMenuGUIHolder");
            Canvas can = m_Root.gameObject.GetComponentInChildren<Canvas>();
            can.AddComponent<VRUICanvas>();
            m_Root.transform.SetParent(null); // needed otherwise , when not show base (menuManager) then this will not show
            InitilisePauseData();
            InitiliseSettingsData();
            Show(false);
        }

        public void ToggleShow()
        {
            if (MonitorTrainerRoot.Instance.CurrentScenario == ScenarioEnum.Stackable)
            {
                m_Root.gameObject.SetActive(!m_Root.gameObject.activeInHierarchy);
                m_Root.gameObject.PlaceInfrontMainCamera(0.2f);
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle == MonitorTrainerConsts.PlayStyleEnum.Solo)
                {
                    MenuManager.Instance.IsPaused = m_Root.gameObject.activeInHierarchy;
                    Debug.LogError($"Pause if in solo ONLY,   paused : {MenuManager.Instance.IsPaused}");
                }
            }
        }

        public bool IsToggleShown() => m_Root.gameObject.activeInHierarchy;

        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Stackable:
                    break;
                default:
                    if(IsToggleShown() == true)
                    {
                        ToggleShow();
                    }
                    break;
            }

        }

        private void InitiliseSettingsData()
        {
            //throw new NotImplementedException();
        }

        private void InitilisePauseData()
        {
            PauseDataRef.m_Button_Return = m_Root.transform.SearchComponent<Button>("Button_Return");
            PauseDataRef.m_Button_Settings = m_Root.transform.SearchComponent<Button>("Button_Settings");
            PauseDataRef.m_Button_LeaveGame = m_Root.transform.SearchComponent<Button>("Button_LeaveGame");

            PauseDataRef.m_Button_Return.onClick.AddListener(() =>
            {
                Debug.LogError("m_Button_Return");
                Debug.LogError("UNPause if in solo ONLY");
                MenuManager.Instance.IsPaused = false;
                Show(false);
            });

            PauseDataRef.m_Button_Settings.onClick.AddListener(() =>
            {
                Debug.LogError("m_Button_Settings");
            });

            PauseDataRef.m_Button_LeaveGame.onClick.AddListener(() =>
            {
                Debug.LogError("m_Button_LeaveGame");
                MonitorTrainerRoot.Instance.CurrentScenario = ScenarioEnum.Menu;
                MenuManager.Instance.IsPaused = false;
#if VR_INTERACTION
                CameraControllerVR.Instance.TeleportAvatar(MenuManager.Instance.gameObject.scene, MenuManager.Instance.m_MenuArea[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum].m_Start, null);
#endif
            });
        }
    }
}

