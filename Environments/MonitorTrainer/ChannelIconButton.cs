using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class ChannelIconButton : MonoBehaviour
    {
        [SerializeField] private Image m_Background;
        [SerializeField] private Image m_Head;
        [SerializeField] private TextMeshProUGUI m_ChannelLabel;

        [SerializeField] private Animator m_Animator;
        private Toggle m_Toggle;
        internal void Init()
        {
            m_Background = transform.Search("Background").GetComponent<Image>();
            m_Head = transform.Search("Head").GetComponent<Image>();
            m_ChannelLabel = transform.Search("Label").GetComponent<TextMeshProUGUI>();

            m_Animator = GetComponent<Animator>();
            m_Toggle = GetComponent<Toggle>();
        }

        internal void InitCallback(AuxEnum auxEnum, Action<bool, AuxEnum> SetData)
        {
            m_Toggle.onValueChanged.RemoveAllListeners();

            if (auxEnum.IsMusician() == true)
            {
                m_Toggle.onValueChanged.AddListener((value) => SetData(value, auxEnum));
            }
        }
        [EditorButton]
        private void Debug_Press() => m_Toggle.onValueChanged.Invoke(true);

        public void SetIconBtnData(CharacterDataClass characterData, Sprite sprite, int channelNumber)
        {
            m_Animator.SetInteger("RefreshStep", 1);

            Core.Mono.WaitForFrames(2, () =>
            {
                m_Background.color = characterData.Colour;
                m_ChannelLabel.SetText("CH" + (channelNumber + 1).ToString("00"));
                m_Head.sprite = sprite;
                m_Animator.SetInteger("RefreshStep", 2);
            });
        }

        public void ToggleVisual(bool state)
        {
            m_Animator.SetBool("Selected", state);
        }
    }
}