using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHulk
{
    public class SpaceHulkRoot : MonoBehaviour
    {
        public void Initialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        }

        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;

#if VR_INTERACTION
            CameraControllerVR.Instance.TeleportAvatar(this.gameObject.scene, new Vector3(0.7f, 0, 33f), null);
#endif

        }
    }
}
