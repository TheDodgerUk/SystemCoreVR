using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrabbingBehaviour : OVRGrabber
{
    private OVRHand m_hand;
    private float pinchThreshold = 0.7f;

    protected override void Start()
    {
        base.Start();
        m_hand = GetComponent<OVRHand>();
        if(m_hand == null)
        {
            m_hand = GetComponentInParent<OVRHand>();
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        CheckIndexPinch();
    }

    void CheckIndexPinch()
    {
        float pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);

        if (!m_grabbedObj && pinchStrength > pinchThreshold && m_grabCandidates.Count > 0)
        {
            GrabBegin();
        }
        else if (m_grabbedObj && !(pinchStrength > pinchThreshold))
        {
            GrabEnd();
        }
    }
}
