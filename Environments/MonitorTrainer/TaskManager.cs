using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;
using static MonitorTrainer.MusicianRequestData;

namespace MonitorTrainer
{
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance;

        public GeneratedTasks GenerateGenericTasksRef = new GeneratedTasks();
        private SpecialTasks SpecialTasksRef = new SpecialTasks();
        private TutorialTasks TutorialTasksRef = new TutorialTasks();


        public List<MusicianRequestData> m_MusicianRequestsCurrent = new List<MusicianRequestData>();
        public List<MusicianRequestData> m_MusicianRequestsCompleted = new List<MusicianRequestData>();
        public SimpleDictionaryList<RuntimeTypeEnum, MusicianRequestData> m_RuntimeErrors = new SimpleDictionaryList<RuntimeTypeEnum, MusicianRequestData>();
        private void AddRuntimeErrorsToList(MusicianRequestData item) => m_RuntimeErrors.AddToList(item.m_RuntimeTypeEnum, item);

        public List<MusicianRequestData> m_UniqueStackableMusicianRequestData = new List<MusicianRequestData>();
        private List<MusicianRequestData> m_TutorialMusicianRequestData = new List<MusicianRequestData>();


        private List<MusicianRequestData> m_LocalSpecialTasks;
        private List<MusicianRequestData> m_LocalSpecialForAllTasks;

        private List<TaskSpecialForAllTimings> m_TaskSpecialForAllTimings = new List<TaskSpecialForAllTimings>();


        public int? m_Scenario2Count = null;


        public Action<List<MusicianRequestData>, MusicianRequestData> OnCompetedTaskItem;


        private float m_CurrentSongTime = 0;

        private float MAX_TIMER_FOR_ERROR = 3f;
        private float m_ErrorTaskCheckTimer = 0;

        public const float SPECIALTASK_PROBABILITY = 0.2f; // 20%

        public void Initialise()
        {
            Instance = this;

            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
            ////TaskItemsRef = this.gameObject.ForceComponent<TaskItems>();
            GameObject specialTasksHitBox = new GameObject("SpecialTasksHitBox");
            SpecialTasksHitBox.CreateInstance();
            SpecialTasksHitBox.Instance.transform.SetParent(null);

        }


        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;
            TaskSettings.CreateInstance();
            TaskSettings.Instance.CreateData();
            NetworkMessagesManager.Instance.ReceiveTaskSpecialForAllTimings((data) =>
            {
                m_TaskSpecialForAllTimings = data;
            });

            NetworkMessagesManager.Instance.ReceiveTaskSpecialForAllHitCompleted((nickname) =>
            {
                var forAll = m_MusicianRequestsCurrent.Find(e => e.m_RequestType == RequestTypeEnum.SpecialForAll);
                if(forAll != null)
                {
                    forAll.m_SpecialForAllNickName = nickname;
                    forAll.m_IsCompleted = true;
                }
                Debug.LogError($"nickname {nickname}");
            });

        }

        public void ChangeToScenario(ScenarioEnum Scenario, DifficultyModeEnum DifficultyMode)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Blank:
                    m_TaskSpecialForAllTimings.Clear();
                    break;
                case ScenarioEnum.TutorialPart1:
                    ClearAllTaskData();
                    var tasks = TutorialTasksRef.CollectTutorialTasks();

                    foreach (var item in tasks)
                    {
                        item.m_OnComplete = NextTutorialTask;
                        AddTutorialTasks(item);             
                    }
                    break;
                case ScenarioEnum.TutorialPart2:
                    break;
                case ScenarioEnum.Menu:
                    m_TaskSpecialForAllTimings.Clear();
                    SpecialTasksHitBox.Instance.DeleteAllHitBoxs();
                    break;
                case ScenarioEnum.Stackable:
                    MonitorTrainerRoot.Instance.LocalPlayerDataRef.SaveDataRef.SendDataRef.XP = 0;
                    ClearAllTaskData();
                    SpecialTasksRef.DeleteTaskCreatedItems();
                    SpecialTasksHitBox.Instance.DeleteAllHitBoxs();
                    GenerateGenericTasksRef.CreateTaskTimings();
                    m_LocalSpecialTasks = SpecialTasksRef.CollectSpecial();
                    m_LocalSpecialForAllTasks = SpecialTasksRef.CollectSpecialForAll();

                    if ((Core.PhotonMultiplayerRef.CurrentRoom == null) || (Core.PhotonMultiplayerRef.IsOwner == true))
                    {
                        var items = SpecialTasksRef.CreateTimingsTaskSpecialForAll(m_LocalSpecialForAllTasks);
                        m_TaskSpecialForAllTimings = items;
                        NetworkMessagesManager.Instance.SendTaskSpecialForAllTimings(items);
                    }
                    var runtimeErrors = SpecialTasksRef.CollectRunTime();
                    foreach (var item in runtimeErrors)
                    {
                        AddRuntimeErrorsToList(item);
                    }
                    SpecialTasksHitBox.Instance.DeleteAllHitBoxs(); // needs to last
                    break;
                case ScenarioEnum.SongFinishedCompleted:
                    ClearAllTaskData();
                    SpecialTasksRef.DeleteTaskCreatedItems();
                    SpecialTasksHitBox.Instance.DeleteAllHitBoxs();
                    ConsoleScreenManager.Instance.BootGlitchScreensRef.StopBoot();
                    ConsoleScreenManager.Instance.BootGlitchScreensRef.StopGlicth();
                    break;
                default:
                    break;
            }
        }


        private void ClearAllTaskData()
        {
            TaskDescManager.Instance.ClearAllTasks();
            TaskBarManager.Instance.ClearAllTasks();
            m_CurrentSongTime = 0;

            foreach (var item in m_MusicianRequestsCurrent)
            {
                item.m_OnComplete?.Invoke(item);
                foreach (var objects in item.m_CreatedObjects)
                {
                    UnityEngine.Object.Destroy(objects.Value);
                }
                item.m_CreatedObjects.Clear();
            }

            m_MusicianRequestsCurrent.Clear();
            m_MusicianRequestsCompleted.Clear();
            m_RuntimeErrors.ClearAll();
        }

        private void AddTutorialTasks(MusicianRequestData Achievement)
        {
            m_TutorialMusicianRequestData.Add(Achievement);
        }

        public void StartTutorialTasks()
        {
            TaskManager.Instance.AddMusicianRequestToCurrentList(m_TutorialMusicianRequestData[0]);
        }

        private void NextTutorialTask(MusicianRequestData data)
        {
            m_TutorialMusicianRequestData.RemoveAt(0);
            this.WaitForEndOfFrame(() =>
            {
                if (m_TutorialMusicianRequestData.Count != 0)
                {
                    this.WaitFor(UnityEngine.Random.Range(3f, 5f), () =>
                    {
                        TaskManager.Instance.AddMusicianRequestToCurrentList(m_TutorialMusicianRequestData[0]);
                    });
                }
                else
                {
                    Debug.Log("NextSequential AtEnd");
                }
            });
        }

        private void CheckRuntimeErrors()
        {
            if (PhysicalConsole.Instance.IsPowerOn() == false) // this will overide everything 
            {
                AddRuntimeErrorMusicianRequestToCurrentList(MusicianRequestData.RuntimeTypeEnum.Power);
            }
            else
            {
                if (PhysicalConsole.Instance.MuteGroupAll.ButtonState == VrInteractionButtonLatched.ButtonStateEnum.Down)
                {
                    AddRuntimeErrorMusicianRequestToCurrentList(MusicianRequestData.RuntimeTypeEnum.MuteAll);
                }
            }
        }

        private void AddRuntimeErrorMusicianRequestToCurrentList(MusicianRequestData.RuntimeTypeEnum type)
        {
            var found = m_MusicianRequestsCurrent.FindLast(e => e.m_RuntimeTypeEnum == type);
            if (found == null)
            {
                var item = m_RuntimeErrors.GetRandomFromList(type);
                var itemClone = (MusicianRequestData)item.Clone();
                AddMusicianRequestToCurrentList(itemClone);
            }
        }


        private void Update()
        {
            if (MonitorTrainerRoot.Instance.CurrentScenario == ScenarioEnum.Stackable)
            {
                if (MenuManager.Instance.IsPaused == false)
                {
                    m_CurrentSongTime += Time.deltaTime;
                }

                CheckRuntimeErrors();
                AddGenerateTaskToSystem();
                /// not working AddPlayerErrorAuxSlider();
                AddSpecialTaskForAllToSystem();

                CheckIfTasksSolved();
            }
        }

        private void AddPlayerErrorAuxSlider()
        {
            m_ErrorTaskCheckTimer += Time.deltaTime;
            if (m_ErrorTaskCheckTimer > MAX_TIMER_FOR_ERROR)
            {
                m_ErrorTaskCheckTimer = 0;
                var errorTask = GenerateGenericTasksRef.CheckForBadPositionsOnAuxSliders();
                if (errorTask != null)
                {
                    AddMusicianRequestToCurrentList(errorTask);
                }
            }
        }


        private void CheckIfTasksSolved()
        {
            for (int index = 0; index < m_MusicianRequestsCurrent.Count; /*index++*/) // cant use as if remove item, it causes issues
            {
                var item = m_MusicianRequestsCurrent[index];
                item.m_CurrentTime += Time.deltaTime;
                if (item.m_CurrentTime > ACHIEVEMENT_MIN_TEST_TIME) // only start testing once its been there for 1+ seconds 
                {
                    if (true == HasIssueBeenSolved(item))
                    {
                        item.m_CompletedDelayTime += Time.deltaTime;
                        if (item.m_CompletedDelayTime > ACHIEVEMENT_MIN_COMPLETED_TIME)// complete if complete for more than 1 second 
                        {
                            TaskCompleted(item, item.m_Description);
                        }
                        else
                        {
                            index++;
                        }

                        break;
                    }
                    else
                    {
                        item.m_CompletedDelayTime = 0; // reset completed time 
                        index++;
                    }
                }
            }
        }


        private void AddSpecialTaskForAllToSystem()
        {
            var newTask = m_TaskSpecialForAllTimings.Find(e => e.Time < m_CurrentSongTime);
            if (newTask != null)
            {
                m_TaskSpecialForAllTimings.Remove(newTask);
                var specialTask = m_LocalSpecialForAllTasks[newTask.SpecialTaskIndex];
                specialTask.m_RequestType = RequestTypeEnum.SpecialForAll;
                if (specialTask.m_OnDelayStart != null)
                {
                    specialTask.m_OnDelayStart?.Invoke(specialTask);

                    Core.Mono.WaitFor(specialTask.m_DelayStartTime, () =>
                    {
                        AddMusicianRequestToCurrentList(specialTask);
                    });
                }
                else
                {
                    AddMusicianRequestToCurrentList(specialTask);
                }

            }
        }


        private void AddGenerateTaskToSystem()
        {
            var newTask = GenerateGenericTasksRef.m_TaskTimings.Find(e => e.Time < m_CurrentSongTime);
            if(newTask != null)
            {
                GenerateGenericTasksRef.m_TaskTimings.Remove(newTask);
                if ((m_LocalSpecialTasks.Count > 0) && (UnityEngine.Random.value < SPECIALTASK_PROBABILITY))
                {
                    float ff = TaskManager.SPECIALTASK_PROBABILITY;//20%// this is for refernce
                    var spec = m_LocalSpecialTasks.GetRandom();
                    spec.m_RequestType = RequestTypeEnum.Special;
                    m_LocalSpecialTasks.Remove(spec);
                    if (spec.m_OnDelayStart != null)
                    {
                        spec.m_OnDelayStart?.Invoke(spec);

                        Core.Mono.WaitFor(spec.m_DelayStartTime, () =>
                        {
                            AddMusicianRequestToCurrentList(spec);
                        });
                    }
                    else
                    {
                        AddMusicianRequestToCurrentList(spec);
                    }
                }
                else
                {
                    var generated = GenerateGenericTasksRef.MakeStackableTask(newTask.Difficulty);
                    generated.m_RequestType = RequestTypeEnum.Generated;
                    AddMusicianRequestToCurrentList(GenerateGenericTasksRef.MakeStackableTask(newTask.Difficulty));
                }
            }
        }

        public void AddMusicianRequestToCurrentList(MusicianRequestData musicianRequestRef)
        {
            if (musicianRequestRef.m_CompletedDelayTime != 0)
            {
                Debug.LogError($"{musicianRequestRef.m_DescriptionForReport}, m_CompletedDelayTime was set to {musicianRequestRef.m_CompletedDelayTime}, this should be zero");
                musicianRequestRef.m_CompletedDelayTime = 0;
            }


            if (musicianRequestRef.HasHitBox == true)
            {
                musicianRequestRef.HitBox.name = musicianRequestRef.m_DescriptionForReport;
            }


            Debug.LogError($"TaskAdded {musicianRequestRef.m_MainMusicianType} :{musicianRequestRef.m_RequestType} : {musicianRequestRef.m_Description}");
            AssignCurrentDBs(musicianRequestRef);
            musicianRequestRef.m_OnStart?.Invoke(musicianRequestRef);
            musicianRequestRef.m_OnSpecialTasksHitBox?.Invoke(musicianRequestRef);
            if (false == HasIssueBeenSolved(musicianRequestRef))
            {
                TaskBarManager.Instance.GenerateAndAddTask(musicianRequestRef);

                m_MusicianRequestsCurrent.Add(musicianRequestRef);
            }
            else
            {
                DebugBeep.LogError($"{musicianRequestRef.m_MainMusicianType} :{musicianRequestRef.m_Description} autoCompleted by mistake", DebugBeep.MessageLevel.High);
#if UNITY_EDITOR
                HasIssueBeenSolved(musicianRequestRef); // this is to go though the issues
                HasIssueBeenSolved(musicianRequestRef); // this is to go though the issues
                HasIssueBeenSolved(musicianRequestRef); // this is to go though the issues
                HasIssueBeenSolved(musicianRequestRef); // this is to go though the issues
#endif
                // do not add to complete because it add the score 
                ////musicianRequestRef.m_OnComplete?.Invoke();
                ////m_MusicianRequestsCompleted.Add(musicianRequestRef);
                ////OnCompetedTaskItem?.Invoke(m_MusicianRequestsCompleted);

            }
        }

        private static void AssignCurrentDBs(MusicianRequestData musicianRequestRef)
        {
            Debug.Log($"AddMusicianRequestToCurrentList : {musicianRequestRef.m_Description}, need to find the correct decibels for this");
            for (int i = 0; i < musicianRequestRef.m_MusicianRequestPersonList.Count; i++)
            {
                //find the correct decibels and assign 
                var item = musicianRequestRef.m_MusicianRequestPersonList[i];
                if (null != item.SliderState || null != item.GainState || null != item.TrimState)
                {
                    if (item.InputAuxType == InputAuxTypeEnum.Input)
                    {
                        var data = ConsoleData.Instance.m_GlobalInputData.FindLast(e => e.m_AuxEnum == (AuxEnum)item.MusicianType);
                        if (null != data)
                        {  
                            item.StartingDecibels = data.GetDBLevel();
                            item.StartingGain = data.m_Gain;
                            item.StartingTrim = data.m_Trim;
                            Debug.Log($"Input, StartingDecibels : {item.StartingDecibels}, starting Percentage {data.GetPercentageLevel()}");
                        }
                    }
                    else
                    {
                        var dataList = ConsoleData.Instance.m_GlobalAuxData.GetList((AuxEnum)musicianRequestRef.m_MainMusicianType);
                        if(null != musicianRequestRef.m_OverrideMainMusicianType)
                        {
                            dataList = ConsoleData.Instance.m_GlobalAuxData.GetList((AuxEnum)musicianRequestRef.m_OverrideMainMusicianType);
                        }
                        if (null != dataList)
                        {
                            if (null != item.MusicianType)
                            {
                                var data = dataList.FindLast(e => e.m_AuxEnum == (AuxEnum)item.MusicianType);
                                if (null != data)
                                {
                                    item.StartingDecibels = data.GetDBLevel();
                                    item.StartingGain = data.m_Gain;
                                    item.StartingTrim = data.m_Trim;
                                    Debug.Log($"Aux, StartingDecibels : {item.StartingDecibels}, starting Percentage {data.GetPercentageLevel()}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool HasIssueBeenSolved(MusicianRequestData musicianRequestRef)
        {
            if (musicianRequestRef.m_CanAutoRemove == true)
            {
                if (musicianRequestRef.m_CurrentTime > musicianRequestRef.m_LifespanSeconds)
                {
                    return true;
                }
            }

            if (musicianRequestRef.DEBUG_Compleated == true)
            {
                return true;
            }
            var mainPerson = musicianRequestRef.m_MainMusicianType;
            if (null != musicianRequestRef.m_OverrideMainMusicianType)
            {
                mainPerson = (MusicianTypeEnum)musicianRequestRef.m_OverrideMainMusicianType;
            }

            List<TrackDataMusician> auxData = ConsoleData.Instance.GetAuxDataChannel((AuxEnum)mainPerson);
            List<TrackDataMusician> inputData = ConsoleData.Instance.GetInputData();
            foreach (MusicianRequestBase item in musicianRequestRef.m_MusicianRequestPersonList)
            {
                var debugItem = item;
                AuxEnum auxEnum = (AuxEnum)item.MusicianType;
                TrackDataMusician trackData = auxData.Find(e => e.m_AuxEnum == auxEnum);
                if(item.InputAuxType == InputAuxTypeEnum.Input)
                {
                    trackData = inputData.Find(e => e.m_AuxEnum == auxEnum);
                }

                if (null == trackData)
                {
                    Debug.LogError($"THis should not be hit {(AuxEnum)mainPerson} does not have a slider for {auxEnum}\n {musicianRequestRef.m_Description}");
                }
                else
                {
                    var stateDebug = item.SliderState;
                    var stateStartDecDebug = item.StartingDecibels;
                    var trackedDBDebug = trackData.GetDBLevel();
                    if ((null != item.SliderState) && (null != item.StartingDecibels))
                    {
                        float currentLevel = trackData.GetDBLevel();
                        float startLevel = (float)item.StartingDecibels;
                        if (item.SliderState == SliderStateEnum.Up)
                        {
                            if (false == Mathf.Approximately(startLevel, DECIBEL_MAX))
                            {
                                if (true == Mathf.Approximately(currentLevel, startLevel))
                                {
                                    return false;
                                }
                                if(currentLevel <= startLevel)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (false == Mathf.Approximately(startLevel, DECIBEL_MIN))
                            {
                                if (true == Mathf.Approximately(currentLevel, startLevel))
                                {
                                    return false;
                                }
                                if (currentLevel >= startLevel)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if ((null != item.GainState) && (null != item.StartingGain))
                    {
                        float currentLevel = trackData.m_Gain;
                        float startLevel = (float)item.StartingGain;
                        if (item.GainState == SliderStateEnum.Up)
                        {                           
                            if (false == Mathf.Approximately(startLevel, GAIN_MAX))
                            {
                                if (true == Mathf.Approximately(currentLevel, startLevel))
                                {
                                    return false;
                                }
                                if (currentLevel <= startLevel)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (false == Mathf.Approximately(startLevel, GAIN_MIN))
                            {
                                if (true == Mathf.Approximately(currentLevel, startLevel))
                                {
                                    return false;
                                }
                                if (currentLevel >= startLevel)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    if ((null != item.TrimState) && (null != item.StartingTrim))
                    {
                        float currentLevel = trackData.m_Trim;
                        float startLevel = (float)item.StartingTrim;
                        if (item.TrimState == SliderStateEnum.Up)
                        {
                            if (false == Mathf.Approximately(startLevel, GAIN_MAX))
                            {
                                if (true == Mathf.Approximately(currentLevel, startLevel))
                                {
                                    return false;
                                }
                                if (currentLevel <= startLevel)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (false == Mathf.Approximately(startLevel, GAIN_MIN))
                            {
                                if (true == Mathf.Approximately(currentLevel, startLevel))
                                {
                                    return false;
                                }
                                if (currentLevel >= startLevel)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if ((null != item.Mute) && (trackData.m_Mute != item.Mute))
                    {
                        return false;
                    }

                    if ((null != item.Solo) && (trackData.m_Solo != item.Solo))
                    {
                        return false;
                    }

                    if ((null != item.PhantomPower) && (trackData.m_PhantomPower40V != item.PhantomPower))
                    {
                        return false;
                    }

                    if ((null != item.Phase) && (trackData.m_Phase != item.Phase))
                    {
                        return false;
                    }

                    if ((null != item.TimeDelay) && (musicianRequestRef.m_CurrentTime < item.TimeDelay))
                    {
                        return false;
                    }
                }                
            }

            // test button states 
            foreach (var consoleItem in musicianRequestRef.m_ConsoleButtonStateList)
            {
                if(PhysicalConsole.Instance.m_GroupButtonDictonary[consoleItem.m_ConsoleButtonEnum].ButtonState != consoleItem.m_ButtonStateEnum)
                {
                    return false;
                }
            }

            if(null != musicianRequestRef.m_CompleteIfTrue)
            {
                if(false == musicianRequestRef.m_CompleteIfTrue())
                {
                    return false;
                }
            }

            // test slider states 
            foreach (var consoleItem in musicianRequestRef.m_ConsoleSliderStateList)
            {
                if (consoleItem.m_SliderStateEnum == SliderStateEnum.Up)
                {
                    if (Mathf.Approximately(PhysicalConsole.Instance.m_GroupSliderDictonary[consoleItem.m_ConsoleSliderGroupEnum].SliderPercentageValue, 0))
                    {
                        return false;
                    }
                }
                else
                {
                    if (Mathf.Approximately(PhysicalConsole.Instance.m_GroupSliderDictonary[consoleItem.m_ConsoleSliderGroupEnum].SliderPercentageValue, 1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        private void TaskCompleted(MusicianRequestData musicianRequestRef, string message)
        {
            TaskBarManager.Instance.TaskCompleted(musicianRequestRef);

            // debug info for debugging
            if(musicianRequestRef.m_CurrentTime < 3)
            {
                Debug.LogError($"TOO QUICK FINISH :{message} ");
            }
            else
            {
                Debug.Log($"COMPLETED, COMPLETED :{message} ");
            }
            
            musicianRequestRef.m_OnComplete?.Invoke(musicianRequestRef);
            m_MusicianRequestsCompleted.Add(musicianRequestRef);
            OnCompetedTaskItem?.Invoke(m_MusicianRequestsCompleted, musicianRequestRef);
            m_MusicianRequestsCurrent.Remove(musicianRequestRef);
            SpecialTasksHitBox.Instance.DeleteHitBox(musicianRequestRef);
            foreach (var objects in musicianRequestRef.m_CreatedObjects)
            {
                UnityEngine.Object.Destroy(objects.Value);
            }
            musicianRequestRef.m_CreatedObjects.Clear();
        }

    }
}
