#if VR_INTERACTION
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demeo
{
    public class DemeoRoot : MonoBehaviour
    {

        private class Hands
        {
            public Vector3 m_Position;
        }

        private float m_CurrentScale = 1f;
        private Dictionary<Handedness, Vector3> m_StartPositions = new Dictionary<Handedness, Vector3>();
        private VrInteraction m_Table;

        public void Initialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        }

        private void InternalInitialise()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= InternalInitialise;
            Debug.LogError("DemeoRoot");
            CameraControllerVR.Instance.TeleportAvatar(this.gameObject.scene, new Vector3(0.0f, 1, -1f), null);

            InputManagerVR.Instance.AnySubscription.Grab.Begin += OnControllerBegin;
            InputManagerVR.Instance.AnySubscription.Grab.End += OnControllerEnd;

            CameraControllerVR.Instance.AutoHandPlayerRef.useMovement = false;
            CameraControllerVR.Instance.AutoHandPlayerRef.snapTurning = false;
            List<VrInteraction> allTables = Core.Scene.GetSpawnedVrInteractionGUID("71b28723-5f90-42b4-858a-8d12c9099772"); // table
            m_Table = allTables[0];

            this.AddComponent<MeshGeneratorV2>();
        }


        private void OnControllerEnd(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
#if VR_INTERACTION
            if (m_StartPositions.ContainsKey(interaction.Main.Hand) == false)
            {
                if(interaction.Main.Hand == Handedness.Left)
                {
                    m_StartPositions.Add(interaction.Main.Hand, CameraControllerVR.Instance.HandLeftRef.transform.localPosition);
                }
                else
                {
                    m_StartPositions.Add(interaction.Main.Hand, CameraControllerVR.Instance.HandRightRef.transform.localPosition);

                }
            }   
#endif
        }

        private void OnControllerBegin(ControllerStateInteraction interaction, bool sendPhotonMessage)
        {
            if (m_StartPositions.Count == 2)
            {
                m_StartPositions.Remove(interaction.Main.Hand);
                if(m_StartPositions.Count == 1)
                {
                    // save new scale
                    m_CurrentScale = CameraControllerVR.Instance.transform.localScale.x;
                }
            }
        }

        private void Update()
        {
            if(m_StartPositions.Count == 1)
            {
                MoveTable();
            }
            else if(m_StartPositions.Count == 2)
            {
                ScalePerson();
            }
        }

        private void MoveTable()
        {
            Vector3 oldPosition = Vector3.zero;
            Autohand.Hand hand = null;
            if (m_StartPositions.ContainsKey(Handedness.Left) == true)
            {
                oldPosition = m_StartPositions[Handedness.Left];
                hand = CameraControllerVR.Instance.HandLeftRef;
            }
            else
            {
                oldPosition = m_StartPositions[Handedness.Right];
                hand = CameraControllerVR.Instance.HandRightRef;
            }

            Vector3 newPosition = oldPosition - hand.transform.localPosition;
            CameraControllerVR.Instance.AutoHandPlayerRef.SetPosition(newPosition);
        }


        private void ScalePerson()
        {
            float leftOld = m_StartPositions[Handedness.Left].x;
            float rightOld = m_StartPositions[Handedness.Right].y;
            float oldDistance = Mathf.Abs(rightOld - leftOld);

            float leftNew = CameraControllerVR.Instance.HandLeftRef.transform.localPosition.x;
            float rightNew = CameraControllerVR.Instance.HandRightRef.transform.localPosition.x;
            float newDistance = Mathf.Abs(rightNew - leftNew);

            float dif = (oldDistance - newDistance) + m_CurrentScale;

            CameraControllerVR.Instance.transform.localScale = new Vector3(dif, dif, dif);
            // move hands wide for scale 
            // scale
        }
    }
}
#endif