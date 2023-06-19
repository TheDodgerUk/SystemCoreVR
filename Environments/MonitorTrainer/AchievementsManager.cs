namespace MonitorTrainer
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;
    using Oculus.Platform.Models;

    public class AchievementsManager
    {
        public class AchievementDebugData
        {
            public AchievementsManager.AchievementsEnum AchievEnum;
        }


        public enum AchievementsEnum
        {
            CompleteFirstSong,
            Complete10Songs,
            Complete15Songs,
            BasketBall10Times,
        }


        // enum and unlock at value
        public static readonly Dictionary<AchievementsEnum, int> AchievementsEnumValues = new Dictionary<AchievementsEnum, int>()
        {
            { AchievementsEnum.CompleteFirstSong, 1 },
            { AchievementsEnum.Complete10Songs, 10 },
            { AchievementsEnum.Complete15Songs, 15 },
            { AchievementsEnum.BasketBall10Times, 10 },
        };


        public Action m_OnAchievementsUpdated;


        public Dictionary<string, AchievementData> AchievementResults = new Dictionary<string, AchievementData>();

        private List<string> m_AchievementNames = new List<string>();
        public class AchievementData
        {
            public AchievementData(int unlockAt)
            {
                UnlockAt = (ulong)unlockAt;
            }
            public bool Unlocked = false;
            public ulong Count = 0;
            public ulong UnlockAt = 0;
        }

        public AchievementsManager()
        {
            foreach (var item in AchievementsEnumValues)
            {
                AchievementResults.Add(item.Key.ToString(), new AchievementData(item.Value));
            }

            foreach (var item in AchievementResults)
            {
                m_AchievementNames.Add(item.Key.ToString());
            }

            Core.PhotonGenericRef.CollectAchievmentDataMessage<AchievementDebugData>((data) =>
            {
                PlatformManager.Achievements.IncrementCount(data.AchievEnum);
                PlatformManager.Achievements.CheckForAchievmentUpdates();
            });
        }


        public void IncrementCount(AchievementsEnum name)
        {
            ulong count = AchievementResults[name.ToString()].Count;
            count++;
            Debug.LogError($"IncrementCount {name}  , count: {count}");
            Oculus.Platform.Achievements.AddCount(name.ToString(), (ulong)count);
        }

        public void CheckForAchievmentUpdates()
        {
            if(Oculus.Platform.Core.IsInitialized() == false)
            {
                Debug.LogError($"Core.IsInitialized()  {Oculus.Platform.Core.IsInitialized()}");
            }
            Debug.LogError($"CheckForAchievmentUpdates PlatformManager.MyID {PlatformManager.MyID}   MyOculusID :{PlatformManager.MyOculusID}");
            try
            {
                Oculus.Platform.Achievements.GetAllProgress().OnComplete(InteranlCheckForAchievmentUpdates);
            }
            catch (Exception e) 
            {
                Debug.LogError($"Error CheckForAchievmentUpdates {e.Message}");
            }
        }


        private void InteranlCheckForAchievmentUpdates(Oculus.Platform.Message<AchievementProgressList> msg)
        {
            List<string> itemsToUnlock = new List<string>();
            if (msg.Data != null)
            {
                Debug.LogError($"Achievment: msg.Data.Count {msg.Data.Count}");

                foreach (var dataFromServer in msg.Data)
                {
                    Debug.LogError($"Achievment: dataFromServer: {dataFromServer.Name}");
                    if (AchievementResults.TryGetValue(dataFromServer.Name, out AchievementData data) == true)
                    {
                        data.Unlocked = dataFromServer.IsUnlocked;
                        data.Count = dataFromServer.Count;
                        Debug.LogError($"Achievment: {dataFromServer.Name}, count {data.Count}, unlock at {data.UnlockAt}");
                        if (data.Count >= data.UnlockAt && data.Unlocked == false)
                        {
                            itemsToUnlock.Add(dataFromServer.Name);
                            Debug.LogError($"Achievment: UNLOCK {dataFromServer.Name}, count {data.Count}, unlock at {data.UnlockAt}");

                        }
                    }
                    else
                    {
                        Debug.LogError($"Cannot find Achievment: {dataFromServer.Name}");
                    }
                }

                if (itemsToUnlock.Count > 0)
                {
                    TaskAction action = new TaskAction(itemsToUnlock.Count, () =>
                    {
                        Debug.LogError("Unlocked Achievements");
                        m_OnAchievementsUpdated?.Invoke();
                    });
                    foreach (var item in itemsToUnlock)
                    {
                        UnlockAchievement(item, action);
                    }
                }
                else
                {
                    Debug.LogError("Update shown values"); // once no more items to unlock, sho it all 
                }
            }
            else
            {
                Debug.LogError($"Achievment: msg.Data == null");

            }
        }


        private void UnlockAchievement(string achievementName, TaskAction action)
        {
            Oculus.Platform.Achievements.Unlock(achievementName).OnComplete((msg) =>
            {
                action.Increment();
            });
        }
    }
}

