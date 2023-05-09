using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class TaskBarManager : MonoBehaviour
    {
        public static TaskBarManager Instance;
        [SerializeField] private Animator m_NoOrders;
        [SerializeField] private Image[] m_TaskSlots = new Image[MonitorTrainerConsts.TASKBAR_MAX_SLOTS];
        [SerializeField] private Animation m_Fade;
        [SerializeField] private GameObject m_TaskPrefab;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        private List<TaskDisplay> m_VisibleTasks = new List<TaskDisplay>();
        private AudioClip[] m_TaskAudioClips = new AudioClip[2];
        private AudioSource m_AudioSource;
        private int m_OccupiedSlots = 0;
        private int m_TasksCount = 0;

        private readonly int COLOR = Shader.PropertyToID("_Color");
        private PoolManagerLocalComponent<Transform> m_PoolManager;

        public void Initialise(Dictionary<string, GameObject> content)
        {
            Instance = this;
            m_CanvasGroup = GetComponentInParent<CanvasGroup>(); // parent is TasksRoot
            if (m_CanvasGroup == null)
            {
                DebugBeep.LogError($"m_CanvasGroup should not be null", DebugBeep.MessageLevel.High);
            }


            var taskSlots = transform.Search("TaskSlots");
            m_TaskSlots = taskSlots.GetComponentsInChildren<Image>();
            m_NoOrders = transform.Search("Background/NoOrders").gameObject.GetComponent<Animator>();

            m_Fade = transform.Search("Fade").GetComponent<Animation>();
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.volume = 0.5f;

            m_TaskPrefab = content["Task"];
            m_PoolManager = new PoolManagerLocalComponent<Transform>(m_TaskPrefab, this.transform);


            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "Task_Show", (soundclip) =>
            {
                m_TaskAudioClips[0] = soundclip;
            });

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "Task_Complete", (soundclip) =>
            {
                m_TaskAudioClips[1] = soundclip;
            });

            m_NoOrders.SetBool("ShowDots", (m_OccupiedSlots == 0));
            foreach (Image item in m_TaskSlots)
            {
                item.material.SetAlpha(0f);
            }
        }

        public void SetVisible(bool state)
        {
            if(m_CanvasGroup != null)
            {
                m_CanvasGroup.VisibleAndInteractive(state);
            }
        }

        public void ClearAllTasks()
        {
            m_PoolManager.DeSpawnAll();
            m_VisibleTasks.Clear();
            m_OccupiedSlots = 0;
            m_TasksCount = 0;
        }

        public void GenerateAndAddTask(MusicianRequestData musicianRequest)
        {

            GameObject item = m_PoolManager.SpawnObject().gameObject;
            item.transform.SetParent(this.transform, false);
            item.SetActive(true);
            item.transform.Rotate(Vector3.zero);
            item.transform.localPosition = MonitorTrainerConsts.TASK_SPAWN_POS;
            item.transform.SetSiblingIndex(2); // Must be under Background and TaskSlots in hierarchy

            TaskDisplay task = item.ForceComponent<TaskDisplay>();
            task.Initialise(musicianRequest);
            ShowTask(task, () =>
            {
                if (musicianRequest.m_RequestType == MusicianRequestData.RequestTypeEnum.SpecialForAll)
                {
                    Debug.LogError("Force SpecialForAll to hightlight for all, need sound for this");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.Beep();
#endif
                    TaskDescManager.Instance.Toggle(task, forceShow:true);
                }
            });


            TriggerCrowdSound();
        }

        private void TriggerCrowdSound()
        {
            if (MonitorTrainerRoot.Instance.CurrentScenario != MonitorTrainerConsts.ScenarioEnum.Stackable)
            {
                return;
            }

            if (m_TasksCount == 0)
            {
                CrowdAndGenericRockSoundManager.Instance.SetRating(MonitorTrainerConsts.RatingEnum.Amazing);
            }
            else if (m_TasksCount == 1)
            {
                CrowdAndGenericRockSoundManager.Instance.SetRating(MonitorTrainerConsts.RatingEnum.Happy);
            }
            else if (m_TasksCount == 2)
            {
                CrowdAndGenericRockSoundManager.Instance.SetRating(MonitorTrainerConsts.RatingEnum.Neutral);
            }
            else if (m_TasksCount == 3)
            {
                CrowdAndGenericRockSoundManager.Instance.SetRating(MonitorTrainerConsts.RatingEnum.Sad);
            }
            else if (m_TasksCount >= 4)
            {
                CrowdAndGenericRockSoundManager.Instance.SetRating(MonitorTrainerConsts.RatingEnum.Angry);
            }
        }

        private void ShowTask(TaskDisplay task, Action callback)
        {
            if (m_VisibleTasks.Count == MonitorTrainerConsts.TASKBAR_MAX_SLOTS)
            {
                return;
            }

            int targetSlot = m_VisibleTasks.Count;
            int targetPos = MonitorTrainerConsts.TASK_ICON_WIDTH * (targetSlot + 1);

            task.transform.localPosition = new Vector3(3618f + targetPos, 256, 0);

            StartCoroutine(task.ShowTask(targetSlot, callback));

            m_TasksCount++;
            RefreshSlots();

            m_VisibleTasks.Add(task);

            this.WaitForFrames(4 + targetSlot, () =>
            {
                m_AudioSource.clip = m_TaskAudioClips[0];
                m_AudioSource.volume = 0.8f;
                m_AudioSource.Play();
                m_Fade.Play();
            });
        }

        public void TaskCompleted(MusicianRequestData musicianRequest)
        {
            TaskDisplay task = m_VisibleTasks.FindLast(e => e.m_Data == musicianRequest);

            if (task != null)
            {
                if (m_VisibleTasks.Count >= task.m_CurrentSlot)
                {
                    RemoveTask(task);
                    TriggerCrowdSound();
                }
                else
                {
                    Debug.LogError("Failed to complete task. Visible tasks count is: "
                        + m_VisibleTasks.Count + " and task to be removed is at slot " + task.m_CurrentSlot);
                }
            }
            else
            {
                Debug.LogError($"Cannot find task, MAJOR ERROR, m_VisibleTasks Count: {m_VisibleTasks.Count}");
            }
        }

        #region testing

        [InspectorButton]
        public void CompleteTaskOne()
        {
            RemoveTask(m_VisibleTasks[0]);
        }

        [InspectorButton]
        public void CompleteTaskTwo()
        {
            RemoveTask(m_VisibleTasks[1]);
        }

        [InspectorButton]
        public void CompleteTaskThree()
        {
            RemoveTask(m_VisibleTasks[2]);
        }

        [InspectorButton]
        public void CompleteTaskFour()
        {
            RemoveTask(m_VisibleTasks[3]);
        }

        [InspectorButton]
        public void CompleteTaskFive()
        {
            RemoveTask(m_VisibleTasks[4]);
        }

        [InspectorButton]
        public void CompleteTaskSix()
        {
            RemoveTask(m_VisibleTasks[5]);
        }

        [InspectorButton]
        public void CompleteTaskSeven()
        {
            RemoveTask(m_VisibleTasks[6]);
        }

        #endregion testing

        private void RemoveTask(TaskDisplay task)
        {
            m_AudioSource.clip = m_TaskAudioClips[1];
            m_AudioSource.volume = 0.5f;
            m_AudioSource.Play();
            task.HideTask();
            int index = m_VisibleTasks.IndexOf(task);
            if (index != -1)
            {
                m_VisibleTasks.PopAt(index);
                m_TasksCount--;
                StartCoroutine(RefreshTasksPositions(index));
            }
            else
            {
                if (null == task)
                {
                    Debug.LogError($"task is null");
                }
                else
                {
                    Debug.LogError($"Cannot find task {task.m_Data.m_Description}");
                }
            }
        }

        private IEnumerator RefreshTasksPositions(int startingIndex)
        {
            for (int i = startingIndex; i < m_VisibleTasks.Count; i++)
            {
                int targetSlot = Mathf.Clamp(m_VisibleTasks[i].m_CurrentSlot - 1, 0, m_VisibleTasks.Count);

                m_VisibleTasks[i].m_CurrentSlot = targetSlot;

                for (int k = 0; k < targetSlot + 1; k++)
                {
                    yield return null;
                    // ity craps out on this part yield return new WaitForEndOfFrame();
                }

                float newPos = (m_VisibleTasks[i].m_CurrentSlot * MonitorTrainerConsts.TASK_ICON_WIDTH)
                                                                + (MonitorTrainerConsts.TASK_ICON_WIDTH * 2);
                yield return StartCoroutine(m_VisibleTasks[i].ShuffleTask(newPos));
            }

            //WIP
            //if (TaskDescManager.Instance.m_IsVisible == false && m_VisibleTasks.Count >= 1)
            //{
            //    m_VisibleTasks[0].OnTaskSelected();
            //}

            yield return null;
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (m_TasksCount > m_OccupiedSlots)
            {
                AddSlot();
                RefreshSlots();
            }
            else if (m_TasksCount < m_OccupiedSlots)
            {
                HideSlot();
                RefreshSlots();
            }

            m_NoOrders.SetBool("ShowDots", (m_OccupiedSlots == 0));
        }

        private void AddSlot()
        {
            m_OccupiedSlots++;

            Image slot = m_TaskSlots[m_OccupiedSlots - 1];
            this.Create<ValueTween>(0.3f, EaseType.QuadIn).Initialise(0f, 1f, (e) =>
            {
                slot.material.SetAlpha(e);
            });
        }

        private void HideSlot()
        {
            if (m_OccupiedSlots == 0)
            {
                Debug.LogError("Attempting to hide a slot when none are on.");
                return;
            }

            Image slot = m_TaskSlots[m_OccupiedSlots - 1];
            this.Create<ValueTween>(0.3f, EaseType.QuadIn).Initialise(1f, 0f, (e) =>
            {
                slot.material.SetAlpha(e);
            });

            m_OccupiedSlots--;
        }

        public void HighlightSlot(int slotIndex, bool state)
        {
            foreach (Image item in m_TaskSlots)
            {
                item.material.SetColor(COLOR, MonitorTrainerConsts.TASK_SLOT_NORMAL);
            }
            if (state)
            {
                m_TaskSlots[slotIndex].material.SetColor(COLOR, MonitorTrainerConsts.TASK_SLOT_HIGHLIGHT);
            }
        }
    }
}