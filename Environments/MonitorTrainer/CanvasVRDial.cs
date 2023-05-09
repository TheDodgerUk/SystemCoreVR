using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class CanvasVRDial : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image m_Output;
        private Image m_Outline;
        private ControllerData m_Controller;
        private float m_FinalInput;
        public Action<float> m_GetValue;

        public void Initialise(Action<float> getValue)
        {
            m_Output = transform.GetChild(2).GetComponent<Image>();
            m_Outline = transform.GetChild(4).GetComponent<Image>();
            m_Outline.color = new Color(1f, 1f, 1f, 1f);
            transition = Transition.ColorTint;
            
            ColorBlock transitionColours = new ColorBlock();
            transitionColours.normalColor = new Color(1f, 1f, 1f, 0f);
            transitionColours.highlightedColor = new Color(1f, 1f, 1f, 1f);
            transitionColours.pressedColor = new Color(1f, 1f, 1f, 0f);
            transitionColours.selectedColor = new Color(1f, 1f, 1f, 0f);
            transitionColours.disabledColor = new Color(1f, 1f, 1f, 0f);
            transitionColours.colorMultiplier = 1f;
            colors = transitionColours;
            targetGraphic = m_Outline;
            m_GetValue = getValue;

            //Clamp at 0.2f because Adam wants it like that
            m_Output.fillAmount = MonitorTrainerConsts.GAIN_MIN;
            m_FinalInput = MonitorTrainerConsts.GAIN_MIN;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_Controller == null)
            {
                List<ControllerData> controllers = InputManagerVR.Instance.GetControllers();

                for (int i = 0; i < controllers.Count; i++)
                {
                    ControllerData controller = controllers[i];
                    if (controller.Hand == Handedness.Right)
                    {
                        m_Controller = controller;   
                    }
                }
            }
            InternalSetOutput(VRInputModule.Instance.GetPointerState(VRInputModule.Instance.Pointers[0]).ScrollValue);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_Controller != null)
            {
                float amount = VRInputModule.Instance.GetPointerState(VRInputModule.Instance.Pointers[0]).ScrollValue;

                InternalSetOutput(amount);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_FinalInput = m_Output.fillAmount;
        }


        public void SetOutput(float value)
        {
            m_FinalInput = value;
            m_Output.fillAmount = value;
        }

        private void InternalSetOutput(float value)
        {
            //Clamp at 0.2f because Adam wants it like that
            float fill = Mathf.Clamp(m_FinalInput + value, MonitorTrainerConsts.MIN_MAX_RANDOM_PERCENTAGE_START.x, MonitorTrainerConsts.GAIN_MAX);
            m_Output.fillAmount = fill;
            m_GetValue?.Invoke(fill);
        }
    }
}