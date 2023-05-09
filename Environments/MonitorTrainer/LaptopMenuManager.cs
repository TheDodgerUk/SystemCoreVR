using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class LaptopMenuManager : MonoBehaviour
    {

        private static float m_MaxXP = 0; 
        public class BandInfo
        {
            public Transform m_Root;
            public Image m_BandImage;
            public TextMeshProUGUI m_BandName;
            public TextMeshProUGUI m_SongName;
            public TextMeshProUGUI m_BandInfo;

            public Button m_Apple;
            public Button m_Spotify;
            public Button m_Amazon;
            public Button m_YouTube;
        }

        public class InMultiPlayerGameScore
        {
            public Transform m_Root;
            public Team m_Team1;
            public Team m_Team2;

            public class Team
            {
                public Transform m_TeamRoot;
                public Image m_ProgressBar;
                public TextMeshProUGUI m_TeamName;
                public TextMeshProUGUI m_PlayerOneName;
                public TextMeshProUGUI m_PlayerTwoName;
                public TextMeshProUGUI m_TotalTeamScore;

                private float m_ProgressAmount;
                public float ProgressAmount
                {
                    get { return m_ProgressAmount; }
                    set
                    {
                        m_ProgressAmount = value;
                        m_ProgressBar.fillAmount = Mathf.Lerp(0.03f, 0.97f, m_ProgressAmount);
                    }
                }
            }


        }

        public class InSinglePlayerGameScore
        {
            public Transform m_Root;

            public Image m_ProgressBar;

            public TextMeshProUGUI m_TotalTeamScore;

            private float m_ProgressAmount;
            public float ProgressAmount
            {
                get { return m_ProgressAmount; }
                set
                {
                    m_ProgressAmount = value;
                    m_ProgressBar.fillAmount = Mathf.Lerp(0.03f, 0.97f, m_ProgressAmount);
                }
            }
        }


        public List<BandInfo> m_BandPages = new List<BandInfo>();
        public InMultiPlayerGameScore m_InMultiPlayerGameScore = new InMultiPlayerGameScore();
        public InSinglePlayerGameScore m_InSinglePlayerGameScore = new InSinglePlayerGameScore();

        void Start()
        {
            BandInfo band1 = new BandInfo();
            band1.m_Root = this.transform.SearchComponent<Transform>("BandPage1");
            m_BandPages.Add(band1);

            BandInfo band2 = new BandInfo();
            band2.m_Root = this.transform.SearchComponent<Transform>("BandPage2");
            m_BandPages.Add(band2);

            BandInfo band3 = new BandInfo();
            band3.m_Root = this.transform.SearchComponent<Transform>("BandPage3");
            m_BandPages.Add(band3);

            BandInfo band4 = new BandInfo();
            band4.m_Root = this.transform.SearchComponent<Transform>("BandPage4");
            m_BandPages.Add(band4);

            InitiliseBandLinks();
            InitiliseSinglePlayerGameRunTime();
            InitiliseMultiPlayerPlayerGameRunTime();
            OpenRandomPage();
        }

        private void InitiliseMultiPlayerPlayerGameRunTime()
        {
            m_InMultiPlayerGameScore.m_Root = this.transform.SearchComponent<Transform>("InMultiPlayerGameScore");

            m_InMultiPlayerGameScore.m_Team1 = new InMultiPlayerGameScore.Team();
            m_InMultiPlayerGameScore.m_Team1.m_TeamRoot = m_InMultiPlayerGameScore.m_Root.transform.SearchComponent<Transform>("Team1");
            m_InMultiPlayerGameScore.m_Team1.m_ProgressBar = m_InMultiPlayerGameScore.m_Team1.m_TeamRoot.SearchComponent<Image>("Fill");
            m_InMultiPlayerGameScore.m_Team1.m_TeamName = m_InMultiPlayerGameScore.m_Team1.m_TeamRoot.SearchComponent<TextMeshProUGUI>("Team Name");

            m_InMultiPlayerGameScore.m_Team1.m_PlayerOneName = m_InMultiPlayerGameScore.m_Team1.m_TeamRoot.SearchComponent<TextMeshProUGUI>("PlayerOneName");
            m_InMultiPlayerGameScore.m_Team1.m_PlayerTwoName = m_InMultiPlayerGameScore.m_Team1.m_TeamRoot.SearchComponent<TextMeshProUGUI>("PlayerTwoName");
            m_InMultiPlayerGameScore.m_Team1.m_TotalTeamScore = m_InMultiPlayerGameScore.m_Team1.m_TeamRoot.SearchComponent<TextMeshProUGUI>("TotalTeamOneScore");


            m_InMultiPlayerGameScore.m_Team2 = new InMultiPlayerGameScore.Team();
            m_InMultiPlayerGameScore.m_Team2.m_TeamRoot = m_InMultiPlayerGameScore.m_Root.transform.SearchComponent<Transform>("Team2");
            m_InMultiPlayerGameScore.m_Team2.m_ProgressBar = m_InMultiPlayerGameScore.m_Team2.m_TeamRoot.SearchComponent<Image>("Fill");
            m_InMultiPlayerGameScore.m_Team2.m_TeamName = m_InMultiPlayerGameScore.m_Team2.m_TeamRoot.SearchComponent<TextMeshProUGUI>("Team Name");

            m_InMultiPlayerGameScore.m_Team2.m_PlayerOneName = m_InMultiPlayerGameScore.m_Team2.m_TeamRoot.SearchComponent<TextMeshProUGUI>("PlayerThreeName");
            m_InMultiPlayerGameScore.m_Team2.m_PlayerTwoName = m_InMultiPlayerGameScore.m_Team2.m_TeamRoot.SearchComponent<TextMeshProUGUI>("PlayerFourName");
            m_InMultiPlayerGameScore.m_Team2.m_TotalTeamScore = m_InMultiPlayerGameScore.m_Team2.m_TeamRoot.SearchComponent<TextMeshProUGUI>("TotalTeamTwoScore");

        }

        private void InitiliseSinglePlayerGameRunTime()
        {
            m_InSinglePlayerGameScore.m_Root = this.transform.SearchComponent<Transform>("InSinglePlayerGameScore");
            m_InSinglePlayerGameScore.m_ProgressBar = m_InSinglePlayerGameScore.m_Root.SearchComponent<Image>("Fill");
            m_InSinglePlayerGameScore.m_TotalTeamScore = m_InSinglePlayerGameScore.m_Root.SearchComponent<TextMeshProUGUI>("TotalTeamOneScore");
        }


        private void InitiliseBandLinks()
        {
            foreach (var item in m_BandPages)
            {
                item.m_BandImage = item.m_Root.SearchComponent<Image>("BandImage");
                item.m_BandName = item.m_Root.SearchComponent<TextMeshProUGUI>("BandName");
                item.m_SongName = item.m_Root.SearchComponent<TextMeshProUGUI>("SongName");
                item.m_BandInfo = item.m_Root.SearchComponent<TextMeshProUGUI>("BandInfo");


                item.m_Apple = item.m_Root.SearchComponent<Transform>("MusicLink_One").GetComponentInChildren<Button>(true);
                item.m_Apple.onClick.AddListener(() =>
                {
                    var songData = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef;
                    Application.OpenURL(songData.AppleLink);
                });

                item.m_Spotify = item.m_Root.SearchComponent<Transform>("MusicLink_Two").GetComponentInChildren<Button>(true);
                item.m_Spotify.onClick.AddListener(() =>
                {
                    var songData = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef;
                    Application.OpenURL(songData.SpotifyLink);
                });

                item.m_Amazon = item.m_Root.SearchComponent<Transform>("MusicLink_Three").GetComponentInChildren<Button>(true);
                item.m_Amazon.onClick.AddListener(() =>
                {
                    var songData = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef;
                    Application.OpenURL(songData.AmazonLink);
                });

                item.m_YouTube = item.m_Root.SearchComponent<Transform>("MusicLink_Four").GetComponentInChildren<Button>(true);
                item.m_YouTube.onClick.AddListener(() =>
                {
                    var songData = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef;
                    Application.OpenURL(songData.YouTubeLink);
                });
            }


        }

        private void OpenRandomPage()
        {
            m_BandPages.ForEach(e => e.m_Root.SetActive(false));
            m_BandPages.GetRandom().m_Root.SetActive(true);
            m_InMultiPlayerGameScore.m_Root.SetActive(false);
            m_InSinglePlayerGameScore.m_Root.SetActive(false);
        }


        public void UpdateRunTimeData()
        {

            this.WaitUntil(1, () => m_InMultiPlayerGameScore != null && m_InMultiPlayerGameScore.m_Root != null, () =>
            {
                m_BandPages.ForEach(e => e.m_Root.SetActive(false));


                m_InMultiPlayerGameScore.m_Team1.m_PlayerOneName.text = "";
                m_InMultiPlayerGameScore.m_Team1.m_PlayerTwoName.text = "";
                m_InMultiPlayerGameScore.m_Team2.m_PlayerOneName.text = "";
                m_InMultiPlayerGameScore.m_Team2.m_PlayerTwoName.text = "";

                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.PlayStyle == PlayStyleEnum.Solo)
                {
                    m_InMultiPlayerGameScore.m_Root.SetActive(false);
                    m_InSinglePlayerGameScore.m_Root.SetActive(true);

                    var player1 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.Find(e => e.CurrentPlayersEnum ==  PlayersEnum.Player1);
                    if (player1 != null)
                    {
                        m_InSinglePlayerGameScore.m_TotalTeamScore.text = player1.XP.ToString();
                        m_InSinglePlayerGameScore.ProgressAmount = player1.XP / m_MaxXP;
                    }
                }
                else
                {
                    m_InMultiPlayerGameScore.m_Root.SetActive(true);
                    m_InSinglePlayerGameScore.m_Root.SetActive(false);

                    float team1Score = 0;
                    float team2Score = 0;


                    m_InMultiPlayerGameScore.m_Team1.m_PlayerOneName.text = "";
                    m_InMultiPlayerGameScore.m_Team1.m_PlayerTwoName.text = "";

                    m_InMultiPlayerGameScore.m_Team2.m_PlayerOneName.text = "";
                    m_InMultiPlayerGameScore.m_Team2.m_PlayerTwoName.text = "";

                    var team1 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team1);
                    if (team1.Count != 0)
                    {
                        if(team1.Count == 1)
                        {
                            m_InMultiPlayerGameScore.m_Team1.m_PlayerOneName.text = team1[0].Nickname;
                            team1Score += team1[0].XP;
                        }
                        else
                        {
                            m_InMultiPlayerGameScore.m_Team1.m_PlayerOneName.text = team1[0].Nickname;
                            m_InMultiPlayerGameScore.m_Team1.m_PlayerTwoName.text = team1[1].Nickname;
                            team1Score += team1[0].XP + team1[1].XP;
                        }
                    }

                    var team2 = MonitorTrainerRoot.Instance.MultiPlayerDataRef.m_MultiPlayerPrefsSelected.FindAll(e => e.CurrentTeamEnum == TeamEnum.Team2);
                    if (team2.Count != 0)
                    {
                        if (team2.Count == 1)
                        {
                            m_InMultiPlayerGameScore.m_Team2.m_PlayerOneName.text = team2[0].Nickname;
                            team2Score += team2[0].XP;
                        }
                        else
                        {
                            m_InMultiPlayerGameScore.m_Team2.m_PlayerOneName.text = team2[0].Nickname;
                            m_InMultiPlayerGameScore.m_Team2.m_PlayerTwoName.text = team2[1].Nickname;
                            team2Score += team2[0].XP + team2[1].XP;
                        }
                    }

                    m_InMultiPlayerGameScore.m_Team1.ProgressAmount = team1Score / (m_MaxXP * 2);
                    m_InMultiPlayerGameScore.m_Team2.ProgressAmount = team2Score / (m_MaxXP * 2);

                }
            });

        }

        public void UpdateMaxXP()
        {
            TaskSettings.Instance.SetCurrentData();
            m_MaxXP = TaskSettings.Instance.CurrentDificultySettings.MaxScore[DifficultyModeEnum.Global];
            foreach (var task in TaskManager.Instance.GenerateGenericTasksRef.m_TaskTimings)
            {
                m_MaxXP += TaskSettings.Instance.CurrentDificultySettings.MaxScore[task.Difficulty];
            }

            m_InSinglePlayerGameScore.ProgressAmount = 0;
            m_InMultiPlayerGameScore.m_Team1.ProgressAmount = 0;
            m_InMultiPlayerGameScore.m_Team2.ProgressAmount = 0;

            m_InSinglePlayerGameScore.m_TotalTeamScore.text = 0.ToString();
            m_InMultiPlayerGameScore.m_Team1.m_TotalTeamScore.text = 0.ToString();
            m_InMultiPlayerGameScore.m_Team2.m_TotalTeamScore.text = 0.ToString();
        }

        public void UpdateMenuData()
        {
            this.WaitUntil(1, () => m_InMultiPlayerGameScore != null && m_InMultiPlayerGameScore.m_Root != null, () =>
            {
                OpenRandomPage();
                var songData = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef;
                if (songData != null)
                {
                    foreach (var item in m_BandPages)
                    {
                        try
                        {
                            item.m_BandName.text = songData.ArtistName;
                            item.m_SongName.text = songData.SongName;
                            item.m_BandInfo.text = songData.BandInfo;

                            item.m_Apple.interactable = (string.IsNullOrEmpty(songData.AppleLink) == false);
                            item.m_Amazon.interactable = (string.IsNullOrEmpty(songData.AmazonLink) == false);
                            item.m_Spotify.interactable = (string.IsNullOrEmpty(songData.SpotifyLink) == false);
                            item.m_YouTube.interactable = (string.IsNullOrEmpty(songData.YouTubeLink) == false);

                            Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItemSprite(Core.Mono, songData.SongName, (sprite) =>
                            {
                                item.m_BandImage.sprite = sprite;
                            });
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"errors: {e.Message}");
                        }
                    }
                }
                else
                {
                    foreach (var item in m_BandPages)
                    {
                        item.m_BandName.text = "";
                        item.m_SongName.text = "";
                        item.m_BandInfo.text = "";

                        item.m_Apple.interactable = false;
                        item.m_Amazon.interactable = false;
                        item.m_Spotify.interactable = false;
                        item.m_YouTube.interactable = false;
                        Debug.LogError("MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef is NULL");
                    }
                }
            });

        }

    }
}

