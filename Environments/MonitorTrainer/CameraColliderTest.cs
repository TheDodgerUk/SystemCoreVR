using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonitorTrainer
{
    public class CameraColliderTest : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.name == "BalloonCollider")
            {
                other.gameObject.GetComponentInParent<Balloon>().ExplodeTheBalloon();
            }
        }
    }
}
