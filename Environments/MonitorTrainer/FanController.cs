using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace MonitorTrainer
{

    public class FanController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector m_Timeline;
        [SerializeField] private float m_FanSpeed = 1f;
        [SerializeField] private GameObject m_Blades = null;
        [SerializeField] private const int BLADES_SPEED_MULTIPLIER = 900;
        private bool m_IsFanActive = true;
        public void Initialise(PlayableDirector timeline)
        {
            this.m_Timeline = timeline;
            m_Blades = this.transform.Search("Misc_OfficeFan-Blades_LOD0").gameObject;
        }

        [InspectorButton]
        public void FanColliderPressed()
        {
            this.ActivateFan();
        }

        private void ActivateFan()
        {
            if (this.m_IsFanActive == true)
            {
                this.Create<ValueTween>(0.8f, EaseType.Linear, () =>
                {
                    this.m_IsFanActive = false;
                    this.m_Timeline.Pause();
                }).Initialise(1.0f, 0f, (e) =>
                 {
                     this.m_FanSpeed = e;
                     this.m_Timeline.playableGraph.GetRootPlayable(0).SetSpeed(m_FanSpeed);
                 });
            }
            else
            {
                this.m_Timeline.Play();
                this.Create<ValueTween>(0.8f, EaseType.Linear, () =>
                {
                    this.m_IsFanActive = true;
                }).Initialise(0f, 1.0f, (e) =>
                 {
                     this.m_FanSpeed = e;
                     this.m_Timeline.playableGraph.GetRootPlayable(0).SetSpeed(m_FanSpeed);
                 });
            }
        }

        private void Update()
        {
            m_Blades.transform.Rotate(Vector3.forward, m_FanSpeed * BLADES_SPEED_MULTIPLIER * Time.deltaTime);
        }
    }
}


