using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesertStrike
{
    public class DesertStrikeRoot : MonoBehaviour
    {

        public static DesertStrikeRoot Instance;

        private VrInteraction m_Helicopter;
        public void Initialise()
        {
            Debug.LogError("DesertStrikeRoot Initialise");
            Instance = this;
            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        }

        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;
            m_Helicopter = Core.Scene.GetSpawnedVrInteractionGUID("857fb936-33a1-423e-bb46-5c70fb7b8be9")[0];

            m_Helicopter.AddComponent<HelicopterPlayer>();
            CameraControllerVR.Instance.transform.SetParent(m_Helicopter.transform);
            CameraControllerVR.Instance.transform.ClearLocals();
        }
    }
}
