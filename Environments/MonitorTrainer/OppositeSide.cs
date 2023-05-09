using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;
using static VrInteractionBaseButton;

namespace MonitorTrainer
{
    public class OppositeSide : MonoBehaviour
    {
        public static OppositeSide Instance;

        public Transform m_StaticHolder1and2;
        public Transform m_StaticHolder3and4;
        public void Initialise()
        {
            Instance = this;
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }


        private void OnEnvironmentLoadingComplete()
        {
            m_StaticHolder1and2 = this.transform.SearchComponent<Transform>("OppositeStatic1and2");
            m_StaticHolder3and4 = this.transform.SearchComponent<Transform>("OppositeStatic3and4");
        }
    }
}
