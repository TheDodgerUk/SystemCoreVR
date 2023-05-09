using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAutoHandFingers: MonoBehaviour
{
    public Finger[] m_fingers;

    private Autohand.Hand m_Hand;
    private Photon.Pun.PhotonView m_View;
    public bool m_IsLeftHand = false;

#if UNITY_EDITOR
    private int m_DEBUG_COUNT = 10;
#endif
    private float m_AvatarSafetySync = GlobalConsts.AVATAR_SAFETY_SYNC_TIMER;

    void Awake()
    {
        m_View = this.GetComponentInParent<Photon.Pun.PhotonView>();

        if(m_IsLeftHand == true)
        {
            m_Hand = CameraControllerVR.Instance.HandLeftRef;
        }
        else
        {
            m_Hand = CameraControllerVR.Instance.HandRightRef;
        }

        if (m_fingers.Length != 5)
        {
            Debug.LogError("m_fingers nned to be assign from model", this.gameObject);
        }



        foreach (var item in m_fingers)
        {
            item.enabled = false;
        }
        this.WaitFor(1f, () =>
        {
            if (m_IsLeftHand == false)
            {
                this.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
            }
            else
            {
                this.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
        });

    }

    void Update()
    {
        if(m_View == null)
        {
            return;
        }
        // m_SetHandForPositions this forces the shadow avatars to get correct positions
        // the positions of the fingers should never change, the rotations change really
        // without this some of the shadow hands fingers are in whole positions 
        // its like it gets weird data, this forces it to be fixed
        // so the owner shadows AND other multiplyer shadows are fixed as well 

        if (m_View.AmOwner == true || m_AvatarSafetySync > 0) 
        {
            if (m_Hand == null)
            {
                Debug.LogError("Hand not Set");
                return;
            }

            if (m_Hand.fingers.Length != m_fingers.Length)
            {
                Debug.LogError("Wrong Length");
                return;
            }

            // this sets the positions to make sure the positions
            // are correct as they were bad 
            m_AvatarSafetySync -= Time.deltaTime;
            m_AvatarSafetySync = Mathf.Max(m_AvatarSafetySync, 0);

            // root
            var thisItem = this.gameObject;
            var fingerRoot =    m_fingers[0].transform.parent;
            var handRoot = m_Hand.fingers[0].transform.parent;

            fingerRoot.localPosition = handRoot.localPosition;
            fingerRoot.localRotation = handRoot.localRotation;
            fingerRoot.localScale = handRoot.localScale;

            for (int i = 0; i < m_Hand.fingers.Length; i++)
            {
                if (m_Hand.fingers[i].fingerJoints.Length != m_fingers[i].fingerJoints.Length)
                {
                    Debug.LogError("m_Hand.fingers[i].fingerJoints.Length != m_fingers[i].fingerJoints.Length");
                }
                else
                {
                    // this is the ROOT of the finger which never rotates, but needs sysncing 
                    m_fingers[i].transform.parent.localPosition = m_Hand.fingers[i].transform.parent.localPosition;
                    m_fingers[i].transform.parent.localRotation = m_Hand.fingers[i].transform.parent.localRotation;
                    m_fingers[i].transform.parent.localScale = m_Hand.fingers[i].transform.parent.localScale;


                    for (int joint = 0; joint < m_Hand.fingers[i].fingerJoints.Length; joint++)
                    {
                        var finger =          m_fingers[i].fingerJoints[joint].transform;
                        var handFinger = m_Hand.fingers[i].fingerJoints[joint].transform;

                        finger.localPosition = handFinger.localPosition;
                        finger.localRotation = handFinger.localRotation;
                        finger.localScale =    handFinger.localScale;

#if UNITY_EDITOR
                        // this is very costly and only need to find it in editor

                        if (m_DEBUG_COUNT > 0)
                        {
                            if (finger.name != handFinger.name)
                            {
                                Debug.LogError($"this.gameObject {this.gameObject.name} fingerName: {finger}, handFingerName : {handFinger}", this.gameObject);
                            }
                            m_DEBUG_COUNT--;
                            m_DEBUG_COUNT = Mathf.Max(m_DEBUG_COUNT, 0);
                        }
#endif

                    }
                }
            }
        }
    }
}
