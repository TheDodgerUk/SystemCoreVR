using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class TaskDisplay : MonoBehaviour
    {
        [SerializeField] private Image m_Background;
        [SerializeField] private Image m_Progress;
        [SerializeField] private Image m_Border;
        [SerializeField] private Image m_HeadIcon;
        [SerializeField] private Image m_Warning;
        [SerializeField] private Animator m_Animator;
        public MusicianRequestData m_Data;
        public bool m_HurryPromptVisible = false;
        private float m_Lifespan = 0f;
        public int m_CurrentSlot = -1;

        [EditorButton]
        private void Debug_CompleteTask() => m_Data.DEBUG_Compleated = true;

        public void Initialise(MusicianRequestData musicianRequest)
        {
            m_Data = musicianRequest;

            m_Background = transform.SearchComponent<Image>("Background");
            m_Progress = transform.SearchComponent<Image>("Progress");
            m_Border = transform.SearchComponent<Image>("Border");
            m_HeadIcon = transform.SearchComponent<Image>("Head");
            m_Warning = transform.SearchComponent<Image>("Warning");
            m_Animator = transform.SearchComponent<Animator>("TaskItem");
            Button button = transform.SearchComponent<Button>("TaskItem");
            button.onClick.AddListener(OnTaskSelected);

            var characterData = MonitorTrainerConsts.GetCharacterData(m_Data.m_MainMusicianType);
            m_Background.color = characterData.Colour;

            // scaled down as you can see the colour peeking out of the border
            m_Background.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            m_Progress.color = MonitorTrainerConsts.TASK_TINT;

            Sprite sprite = TaskDescManager.Instance.m_HeadsIcons.FindLast(e => e.name.ToLower() == characterData.CharacterName.ToLower());
            if (null == sprite)
            {
                Debug.LogError($"Cannot find sprite for {characterData.CharacterName.ToString()}");

                //backup Radio image
                sprite = TaskDescManager.Instance.m_HeadsIcons.FindLast(e => e.name.ToLower() == "Daniel James".ToLower());
            }

            m_HeadIcon.sprite = sprite;
          
            m_Lifespan = m_Data.m_LifespanSeconds;
            m_Warning.gameObject.SetActive(false); //this wont be needed, will remove towards the end
            m_Progress.fillAmount = 0f;
            m_Border.fillAmount = 0f;
        }

        public IEnumerator ShowTask(int targetSlot, Action callback)
        {
            m_CurrentSlot = targetSlot;

            m_Animator.SetTrigger("Show");

            yield return new WaitForSeconds(m_Animator.GetCurrentAnimatorStateInfo(0).length + m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            if (m_CurrentSlot == 0)
            {
                OnTaskSelected();
            }
            BeginTimer();
            callback?.Invoke();
        }

        public IEnumerator ShuffleTask(float newPos)
        {
            m_Animator.SetTrigger("Shuffle");
            yield return null;
            //WIP
            //yield return new WaitForSeconds(m_Animator.GetCurrentAnimatorStateInfo(0).length + m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            transform.localPosition = new Vector3(newPos, transform.localPosition.y, transform.localPosition.z);

            if (m_CurrentSlot == 0 && TaskDescManager.Instance.m_IsVisible == false)
            {
                OnTaskSelected();
            }
        }

        private void BeginTimer()
        {
            if (m_Lifespan == 0)
            {
                m_Border.fillAmount = 1f;
                m_Border.color = Color.white;
                return;
            }

            m_Border.Create<ColourTween>(m_Lifespan / 2, EaseType.Linear, (a) =>
             {
                 m_Progress.fillAmount = a / 2;
                 m_Border.fillAmount = a / 2;
             }, () =>
             {
                 m_Animator.SetTrigger("Grow");

                 m_Border.Create<ColourTween>(m_Lifespan / 2, EaseType.Linear, (b) =>
                 {
                     m_Progress.fillAmount = 0.5f + (b / 2);
                     m_Border.fillAmount = 0.5f + (b / 2);
                 }, () =>
                 {
                     m_HurryPromptVisible = true;
                     TaskDescManager.Instance.TogglePrompt(this, true);
                 }).Initialise(Color.red, Color.yellow);
             }).Initialise(Color.yellow, Color.green);
        }

        [EditorButton]
        private void DEBUG_OnTaskSelected() => OnTaskSelected();
        public void OnTaskSelected()
        {
            TaskDescManager.Instance.Toggle(this);
            m_Data.m_OnSpecialTasksHitBox?.Invoke(m_Data);
            if(m_Data.HasHitBox == true)
            {
                SpecialTasksHitBox.Instance.VFX.SetActive(m_Data.HitBox.m_VFXData.m_Show);
                SpecialTasksHitBox.Instance.VFX.gameObject.transform.localScale = m_Data.HitBox.m_VFXData.m_Scale;
                SpecialTasksHitBox.Instance.VFX.transform.position = m_Data.HitBox.transform.position;
            }
            else
            {
                SpecialTasksHitBox.Instance.VFX.SetActive(false);
            }
            
        }

        public void HideTask()
        {
            m_Animator.SetTrigger("Hide");

            TaskDescManager.Instance.Close(this);

            this.WaitForFrames(4, () => { this.DestroyGameObject(); });
        }
    }
}