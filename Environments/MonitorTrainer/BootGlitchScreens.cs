using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonitorTrainer
{
    public class BootGlitchScreens : MonoBehaviour
    {
        private const float BOOT_TIME = 3.5f;
        public class Boot
        {
            public GameObject m_Root;
            public Animator m_Animator;
        }
        public class Glitch
        {
            public Image m_Image;
        }


        private GameObject m_BootRoot;
        private List<Boot> m_BootList = new List<Boot>();

        private GameObject m_GlitchRoot;
        private Animator m_GlitchAnimator;
        private List<Glitch> m_GlitchList = new List<Glitch>();

        public void Init(GameObject boot, GameObject glitch)
        {
            m_BootRoot = boot;
            m_GlitchRoot = glitch;
#if UNITY_EDITOR
            // test in build 
            var index = m_BootRoot.transform.GetSiblingIndex();
            var count = m_BootRoot.transform.SiblingCount();
            if(index < count-2)
            {
                m_BootRoot.transform.SetSiblingIndex(count - 2);
                Debug.Log("m_BootRoot needs to be at the bottom, to cover objects");
            }
#endif
            var anim = boot.GetComponentsInChildren<Animator>().ToList();
            foreach (var item in anim)
            {
                Boot newBoot = new Boot();
                newBoot.m_Animator = item;
                newBoot.m_Root = newBoot.m_Animator.gameObject;
                m_BootList.Add(newBoot);
            }

            var images = boot.GetComponentsInChildren<Image>().ToList();
            foreach (var item in images)
            {
                Glitch newGlitch = new Glitch();
                newGlitch.m_Image = item;
                m_GlitchList.Add(newGlitch);
            }

            m_GlitchAnimator = boot.GetComponentInChildren<Animator>();

            m_BootRoot.SetActive(false);
            m_GlitchRoot.SetActive(false);

        }

        public void StopBoot() => m_BootRoot.SetActive(false);
        public void PlayBoot()
        {
            m_BootRoot.SetActive(true);
            var boot = m_BootList.GetRandom();
            boot.m_Animator.Play("BootUpSequence_1");
            Core.Mono.WaitFor(BOOT_TIME, () =>
            {
                m_BootRoot.SetActive(false);
            });
        }

        public void StopGlicth() => m_GlitchRoot.SetActive(false);
        public void PlayGlitch()
        {
            m_GlitchRoot.SetActive(true);
            m_GlitchList.ForEach(e => e.m_Image.gameObject.SetActive(false));
            var glitch = m_GlitchList.GetRandom();
            glitch.m_Image.SetActive(true);
            m_GlitchAnimator.Play("GlitchAnim");
        }
    }
}
