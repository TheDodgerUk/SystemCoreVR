#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorTrainer;
using static MonitorTrainer.MonitorTrainerConsts;
using static MonitorTrainer.MenuManagerDebug;
using System;
using static MonitorTrainer.AchievementsManager;
using static MonitorTrainer.LeaderboardManager;

namespace MonitorTrainer
{
    [CustomEditor(typeof(MonitorTrainer.PlatformManager))]
    public class PlatformManagerEditor : MenuEditorBase
    {
        private int m_LeaderBoardScore;
        private Dictionary<string, ulong> m_Achievement = new Dictionary<string, ulong>();

        private void Awake()
        {
            
        }
        public override void OnInspectorGUI()
        {


            foreach (var item in AchievementsEnumValues)
            {
                if (GUILayout.Button($"Achievements: {item.Key.ToString()}"))
                {
                    AchievementDebugData newData = new AchievementDebugData();

                    newData.AchievEnum = item.Key;
                    Core.PhotonGenericRef.SendAchievmentData<AchievementDebugData>(newData);
                }
            }

            if (GUILayout.Button($"LeaderBoard CURRENT_XP:"))
            {
                LeaderBoardDebugData newData = new LeaderBoardDebugData();
                newData.LeaderEnum = LeaderboardDataEnum.CURRENT_XP;
                newData.LeaderBoardScore = m_LeaderBoardScore;
                Core.PhotonGenericRef.SendLeaderBoardData<LeaderBoardDebugData>(newData);

            }
            m_LeaderBoardScore = EditorGUILayout.IntField("LeaderBoard Score :", m_LeaderBoardScore);


        }
    }
}

#endif
