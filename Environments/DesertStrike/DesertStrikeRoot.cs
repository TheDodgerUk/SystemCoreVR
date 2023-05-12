using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesertStrike
{
    public class DesertStrikeRoot : MonoBehaviour
    {

        private VrInteraction m_Helicopter;
        public void Initialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        }

        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;
            m_Helicopter = Core.Scene.GetSpawnedVrInteractionGUID("857fb936-33a1-423e-bb46-5c70fb7b8be9")[0];
        }
    }
}
