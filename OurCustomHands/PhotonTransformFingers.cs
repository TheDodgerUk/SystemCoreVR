using System.Collections.Generic;
namespace Photon.Pun
{
    using UnityEngine;

    public class PhotonTransformFingers : MonoBehaviourPun, IPunObservable
    {
        [System.Serializable]
        public class Data
        {
            [SerializeField]
            public Transform TransformRef;
            [SerializeField]
            public Quaternion NetworkRotation;
            [SerializeField]
            public float Angle;
        }
        [SerializeField]
        public List<Data> m_LocalFingerBendData = new List<Data>();




        private NetworkAutoHandFingers m_NetworkFingers;


        private PhotonView m_PhotonView;

        bool m_firstTake = true;



        private void Awake()
        {
            m_PhotonView = this.GetComponentInParent<PhotonView>();
            m_NetworkFingers = this.GetComponent<NetworkAutoHandFingers>();
            CollectFingerBend();
        }

        [InspectorButton]
        public void CollectFingerBend()
        {
            if(m_NetworkFingers.m_fingers.Length != 5)
            {
                Debug.LogError("m_fingers nned to be assign from model", this.gameObject);
            }
            foreach (var finger in m_NetworkFingers.m_fingers)
            {
                foreach (var joint in finger.fingerJoints)
                {
                    Data localFinger = new Data();
                    localFinger.TransformRef = joint;
                    m_LocalFingerBendData.Add(localFinger);
                }
            }
        }



        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // Write
            if (stream.IsWriting)
            {
                foreach (var finger in m_NetworkFingers.m_fingers)
                {
                    foreach (var joint in finger.fingerJoints)
                    {
                        stream.SendNext(joint.transform.localRotation);
                    }
                }
            }
            else
            {
                if (m_firstTake == true)
                {
                    m_firstTake = false;
                    foreach (var finger in m_NetworkFingers.m_fingers)
                    {
                        foreach (var joint in finger.fingerJoints)
                        {
                            finger.transform.localRotation = (Quaternion)stream.ReceiveNext();
                        }                       
                    }
                }
                else
                {
                    foreach (var item in m_LocalFingerBendData)
                    {
                        item.NetworkRotation = (Quaternion)stream.ReceiveNext();
                        item.Angle = Quaternion.Angle(item.TransformRef.localRotation, item.NetworkRotation);
                    }
                }
            }
        }

        private void Update()
        {
            if(m_PhotonView == null)
            {
                return;
            }
            if (m_PhotonView.IsMine == false)
            {
                foreach (var finger in m_LocalFingerBendData)
                {
                    finger.TransformRef.localRotation = Quaternion.RotateTowards(finger.TransformRef.localRotation, finger.NetworkRotation, finger.Angle * (1.0f / PhotonNetwork.SerializationRate));
                }
            }

        }
    }
}

