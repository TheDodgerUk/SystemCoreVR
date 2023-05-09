using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class TaskSettings : Singleton<TaskSettings>
    {
        public DificultySettings CurrentDificultySettings { get; private set; }
        private Dictionary<DifficultyModeEnum, DificultySettings> DificultySettingsRef = new Dictionary<DifficultyModeEnum, DificultySettings>();

        private const float MIN_SCORE_MULTIPLYER = 0.1f;
        public void SetCurrentData()
        {
            CurrentDificultySettings = DificultySettingsRef[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentDifficulty];
        }

        public void CreateData()
        {
            ///////////////////////////////////////////////////////////////////////
            DificultySettings easy = new DificultySettings();

            easy.TasksPerMin.Add(DifficultyModeEnum.Easy, 3);//3
            easy.TasksPerMin.Add(DifficultyModeEnum.Medium, 3);//3
            easy.TasksPerMin.Add(DifficultyModeEnum.Hard, 2);//2

            easy.MinTimeBetweenTasks.Add(DifficultyModeEnum.Easy, 4);
            easy.MinTimeBetweenTasks.Add(DifficultyModeEnum.Medium, 4);
            easy.MinTimeBetweenTasks.Add(DifficultyModeEnum.Hard, 8);

            easy.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Easy, 10);
            easy.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Medium, 10);
            easy.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Hard, 15);

            easy.TimeAtMaxScore.Add(DifficultyModeEnum.Easy, 4);
            easy.TimeAtMaxScore.Add(DifficultyModeEnum.Medium, 4);
            easy.TimeAtMaxScore.Add(DifficultyModeEnum.Hard, 8);

            easy.TimeAtMinScore.Add(DifficultyModeEnum.Easy, 10);
            easy.TimeAtMinScore.Add(DifficultyModeEnum.Medium, 10);
            easy.TimeAtMinScore.Add(DifficultyModeEnum.Hard, 15);

            easy.MaxScore.Add(DifficultyModeEnum.Easy, 50);
            easy.MaxScore.Add(DifficultyModeEnum.Medium, 50);
            easy.MaxScore.Add(DifficultyModeEnum.Hard, 200);
            easy.MaxScore.Add(DifficultyModeEnum.Global, 200);

            DificultySettingsRef.Add(DifficultyModeEnum.Easy, easy);

            ///////////////////////////////////////////////////////////////////////
            DificultySettings medium = new DificultySettings();

            medium.TasksPerMin.Add(DifficultyModeEnum.Easy, 3);
            medium.TasksPerMin.Add(DifficultyModeEnum.Medium, 3);
            medium.TasksPerMin.Add(DifficultyModeEnum.Hard, 2);

            medium.MinTimeBetweenTasks.Add(DifficultyModeEnum.Easy, 4);
            medium.MinTimeBetweenTasks.Add(DifficultyModeEnum.Medium, 4);
            medium.MinTimeBetweenTasks.Add(DifficultyModeEnum.Hard, 8);

            medium.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Easy, 10);
            medium.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Medium, 10);
            medium.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Hard, 15);

            medium.TimeAtMaxScore.Add(DifficultyModeEnum.Easy, 4);
            medium.TimeAtMaxScore.Add(DifficultyModeEnum.Medium, 4);
            medium.TimeAtMaxScore.Add(DifficultyModeEnum.Hard, 8);

            medium.TimeAtMinScore.Add(DifficultyModeEnum.Easy, 10);
            medium.TimeAtMinScore.Add(DifficultyModeEnum.Medium, 10);
            medium.TimeAtMinScore.Add(DifficultyModeEnum.Hard, 15);

            medium.MaxScore.Add(DifficultyModeEnum.Easy, 50);
            medium.MaxScore.Add(DifficultyModeEnum.Medium, 50);
            medium.MaxScore.Add(DifficultyModeEnum.Hard, 200);
            medium.MaxScore.Add(DifficultyModeEnum.Global, 200);
            DificultySettingsRef.Add(DifficultyModeEnum.Medium, medium);

            ///////////////////////////////////////////////////////////////////////
            DificultySettings hard = new DificultySettings();

            hard.TasksPerMin.Add(DifficultyModeEnum.Easy, 3);
            hard.TasksPerMin.Add(DifficultyModeEnum.Medium, 3);
            hard.TasksPerMin.Add(DifficultyModeEnum.Hard, 2);

            hard.MinTimeBetweenTasks.Add(DifficultyModeEnum.Easy, 4);
            hard.MinTimeBetweenTasks.Add(DifficultyModeEnum.Medium, 4);
            hard.MinTimeBetweenTasks.Add(DifficultyModeEnum.Hard, 8);

            hard.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Easy, 10);
            hard.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Medium, 10);
            hard.MaxTimeBetweenTasks.Add(DifficultyModeEnum.Hard, 15);

            hard.TimeAtMaxScore.Add(DifficultyModeEnum.Easy, 4);
            hard.TimeAtMaxScore.Add(DifficultyModeEnum.Medium, 4);
            hard.TimeAtMaxScore.Add(DifficultyModeEnum.Hard, 8);

            hard.TimeAtMinScore.Add(DifficultyModeEnum.Easy, 10);
            hard.TimeAtMinScore.Add(DifficultyModeEnum.Medium, 10);
            hard.TimeAtMinScore.Add(DifficultyModeEnum.Hard, 15);

            hard.MaxScore.Add(DifficultyModeEnum.Easy, 50);
            hard.MaxScore.Add(DifficultyModeEnum.Medium, 50);
            hard.MaxScore.Add(DifficultyModeEnum.Hard, 200);
            hard.MaxScore.Add(DifficultyModeEnum.Global, 200);
            DificultySettingsRef.Add(DifficultyModeEnum.Hard, hard);
        }


        public void UpdateScores(List<MusicianRequestData> data, MusicianRequestData newOne)
        {
            var currentSettings = CurrentDificultySettings;
            int completedCount = 0;
            float totalXp = 0f;
            float newOneXP = 0f;
            foreach (var item in data)
            {
                if(item.m_RuntimeTypeEnum != MusicianRequestData.RuntimeTypeEnum.None)
                {
                    // these are not real ones
                    continue;
                }

                float itemXp = 0;
                if (item.m_CanAutoRemove == true)
                {
                    if (item.m_CurrentTime < item.m_LifespanSeconds)
                    {
                        itemXp = currentSettings.MaxScore[item.m_Difficulty];
                        if (item.m_CurrentTime < currentSettings.TimeAtMaxScore[item.m_Difficulty])
                        {
                            itemXp = currentSettings.MaxScore[item.m_Difficulty];
                        }
                        else
                        {
                            var percentage = Mathf.InverseLerp(currentSettings.TimeAtMinScore[item.m_Difficulty], currentSettings.TimeAtMaxScore[item.m_Difficulty], item.m_CurrentTime);
                            percentage = Mathf.Clamp01(percentage);
                            itemXp = Mathf.Lerp(currentSettings.MaxScore[item.m_Difficulty], currentSettings.MaxScore[item.m_Difficulty] * MIN_SCORE_MULTIPLYER, percentage);
                        }
                        completedCount++;
                    }
                }
                else if (item.m_RequestType == MusicianRequestData.RequestTypeEnum.SpecialForAll)
                {
                    if (item.m_SpecialForAllNickName == Core.PhotonMultiplayerRef.MySelf.NickName)
                    {
                        itemXp += currentSettings.MaxScore[DifficultyModeEnum.Global];
                        completedCount++;
                    }
                }
                else
                {
                    itemXp = currentSettings.MaxScore[item.m_Difficulty];
                    if (item.m_CurrentTime < currentSettings.TimeAtMaxScore[item.m_Difficulty])
                    {
                        itemXp = currentSettings.MaxScore[item.m_Difficulty];
                    }
                    else
                    {
                        var percentage = Mathf.InverseLerp(currentSettings.TimeAtMinScore[item.m_Difficulty], currentSettings.TimeAtMaxScore[item.m_Difficulty], item.m_CurrentTime);
                        percentage = Mathf.Clamp01(percentage);
                        itemXp = Mathf.Lerp(currentSettings.MaxScore[item.m_Difficulty], currentSettings.MaxScore[item.m_Difficulty] * MIN_SCORE_MULTIPLYER, percentage);


                        // testing 
                        var p0 = Mathf.InverseLerp(3, 6, 1.99f);
                        p0 = Mathf.Clamp01(p0);
                        var xp0 = Mathf.Lerp(10, 10 * MIN_SCORE_MULTIPLYER, p0);

                        var p1 = Mathf.InverseLerp(3, 6, 5.99f);
                        p1 = Mathf.Clamp01(p1);
                        var xp1 = Mathf.Lerp(10, 10 * MIN_SCORE_MULTIPLYER, p1);

                        var p2 = Mathf.InverseLerp(3, 6, 3.11f);
                        p2 = Mathf.Clamp01(p2);
                        var xp2 = Mathf.Lerp(10, 10 * MIN_SCORE_MULTIPLYER, p2);

                        var p3 = Mathf.InverseLerp(3, 6, 4.11f);
                        p3 = Mathf.Clamp01(p3);
                        var xp3 = Mathf.Lerp(10, 10 * MIN_SCORE_MULTIPLYER, p3);

                        var p4 = Mathf.InverseLerp(3, 6, 7.11f);
                        p4 = Mathf.Clamp01(p4);
                        var xp4 = Mathf.Lerp(10, 10 * MIN_SCORE_MULTIPLYER, p4);

                        int ff = 0;
                    }


                    completedCount++;
                    Debug.Log($" max xp :{currentSettings.MaxScore[item.m_Difficulty]} is now :{itemXp}, descrp {item.m_Description}");
                }
                if(item ==newOne)
                {
                    newOneXP = itemXp;
                }
                totalXp += itemXp;
            }
            MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.XP = (int)totalXp;
            MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.TasksCompleted = completedCount;
            if (newOne.m_RuntimeTypeEnum == MusicianRequestData.RuntimeTypeEnum.None)
            {
                int intXP = (int)newOneXP;
                if (intXP > 0)
                {
                    MonitorTrainerRoot.Instance.FloatingPointsGUIManagerRef.PlayAmount(intXP);
                }
                else
                {
                    Debug.LogError($"Xp was ZERO {newOne.m_DescriptionForReport}");
                }
            }

            NetworkMessagesManager.Instance.SendPlayerPrefsData(ref MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef);
            Debug.LogError($"UpdateScores : {totalXp}");
            var player = MonitorTrainer.MonitorTrainerRoot.Instance.PlayerChoiceDataRef;
            PlatformManager.Leaderboards.SubmitSongHighScore(player.SongDataRef.SongName, player.CurrentDifficulty, (long)totalXp);

        }
    }
}
