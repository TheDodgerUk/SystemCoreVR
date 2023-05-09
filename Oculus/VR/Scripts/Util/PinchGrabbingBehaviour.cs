using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRHand;

public class PinchGrabbingBehaviour : OVRGrabber
{
    private OVRHand m_hand;
    private float pinchThreshold = 0.7f;
    private HandFinger m_ThisHandFinger = HandFinger.Thumb;
     
    protected override void Start()
    {
        base.Start();
        m_hand = GetComponent<OVRHand>();
        if(m_hand == null)
        {
            m_hand = GetComponentInParent<OVRHand>();
        }

        if(this.name.Contains(HandFinger.Thumb.ToString()) == true)
        {
            m_ThisHandFinger = HandFinger.Thumb;
        }
        else if (this.name.Contains(HandFinger.Index.ToString()) == true)
        {
            m_ThisHandFinger = HandFinger.Index;
        }
        else if (this.name.Contains(HandFinger.Middle.ToString()) == true)
        {
            m_ThisHandFinger = HandFinger.Middle;
        }
        else if (this.name.Contains(HandFinger.Ring.ToString()) == true)
        {
            m_ThisHandFinger = HandFinger.Ring;
        }
        else if (this.name.Contains(HandFinger.Pinky.ToString()) == true)
        {
            m_ThisHandFinger = HandFinger.Pinky;
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
        if(m_ThisHandFinger != HandFinger.Index)            // only doing index 
        {
            return;
        }

        bool pinch = m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index);// only doing index 
        if (!m_grabbedObj && pinch == true && m_grabCandidates.Count > 0)
        {
            Debug.LogError("GrabBegin");
            GrabBegin();
        }
        else if (m_grabbedObj && pinch == false)
        {
            Debug.LogError("GrabEnd");
            GrabEnd();
        }

    }
}
