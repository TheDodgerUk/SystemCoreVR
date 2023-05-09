using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(VRUIPointer))]
public class FingerPokeUICanvas : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private VRUIPointer m_VRUIPointer;


    [SerializeField]
    private UnityEngine.SpatialTracking.TrackedPoseDriver m_TrackedPoseDrivers;

    private CapsuleCollider m_IndexTipCollider;
    private PhysicMaterial m_IndexTipOriginalPhysicMaterial;
    private void Awake()
    {
        SanityCheck();
        if (CameraControllerVR.Instance != null)
        {
            var trackedPoseDrivers = CameraControllerVR.Instance.GetComponentsInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>().ToList();
            if (this.name.CaseInsensitiveContains("Rtouch") == true)
            {
                m_TrackedPoseDrivers = trackedPoseDrivers.Find(e => e.poseSource == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.RightPose);
            }
            else
            {
                m_TrackedPoseDrivers = trackedPoseDrivers.Find(e => e.poseSource == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.LeftPose);
            }
            if (m_TrackedPoseDrivers == null)
            {
                Debug.LogError("Cannot find TrackedPoseDriver", this.gameObject);
            }

            m_VRUIPointer = GetComponentInParent<VRUIPointer>();

        }
    }

    public void SetIndexPoint(Transform indexPoint)
    {
        m_IndexTipCollider = indexPoint.GetComponent<CapsuleCollider>();
        m_IndexTipOriginalPhysicMaterial = m_IndexTipCollider.material;
        m_IndexTipCollider.material = PhysicMaterials.ButtonSliderPhysicMaterial;
    }
    private void SanityCheck()
    {
        var colliders = this.GetComponentsInChildren<Collider>().ToList();
        if(colliders.Count != 1)
        {
            Debug.LogError("Should only have one colider");
        }
        else
        {
            bool isLeft = GetComponentInParent<Autohand.Hand>().left;
            var item = (CapsuleCollider)colliders[0];


            item.isTrigger = true;           
            item.center = new Vector3(0f, 0f, 0.02f);
            item.radius = 0.005f;
            item.height = 0.02f;
            item.direction = 2; // 2 is Z
        }
    }

    private void OnTriggerEnter(Collider other) => Enter(other.gameObject);
    private void OnTriggerStay(Collider other) => Stay(other.gameObject);
    private void OnTriggerExit(Collider other) => End(other.gameObject);

    [SerializeField]
    private GameObject m_LastPressed = null;

    private int m_LastStayCount = 0;

    private void Update()
    {
        if (m_LastStayCount == 0)
        { 
            if(m_LastPressed != null)
            {
                ForceEnd(m_LastPressed);
                m_LastPressed = null;
            }
        }
        else
        {
            m_LastStayCount--;
            m_LastStayCount = Mathf.Max(0, m_LastStayCount);
        }
    }

    private void Enter(GameObject obj)
    {
        if (m_LastPressed == null)
        {
            if (CameraControllerVR.Instance != null)
            {
                var vr = obj.GetComponent<VrInteractionCanvas>();
                if (vr != null)
                {
                    m_LastStayCount = GlobalConsts.FINGER_POKE_FRAMES;
                    if (m_IndexTipCollider != null)
                    {
                        m_IndexTipCollider.material = m_IndexTipOriginalPhysicMaterial;
                    }
                    m_VRUIPointer.FingerPress(true, obj);
                    m_LastPressed = obj;
                }
            }
        }
    }

    private void Stay(GameObject obj)
    {
        if (m_LastPressed == obj)
        {
            if (CameraControllerVR.Instance != null)
            {
                var vr = obj.GetComponent<VrInteractionCanvas>();
                if (vr != null)
                {
                    m_LastStayCount = GlobalConsts.FINGER_POKE_FRAMES;
                    m_VRUIPointer.FingerPress(true, obj);
                }
            }
        }
    }

    private void End(GameObject obj)
    {
        if (m_LastPressed == obj)
        {
            if (CameraControllerVR.Instance != null)
            {
                var vr = obj.GetComponent<VrInteractionCanvas>();
                if (vr != null)
                {
                    ForceEnd(obj);
                    // this weird delay is stop:
                    // if you       SetActive(false) and SetActive(true) on another page, then the delay will stop OnEnter getting hit 
                    this.WaitForFrames(GlobalConsts.FINGER_POKE_FRAMES, () =>
                    {
                        m_LastPressed = null;
                    });
                }
            }
        }
    }

    private void ForceEnd(GameObject obj)
    {
        if (m_IndexTipCollider != null)
        {
            m_IndexTipCollider.material = PhysicMaterials.ButtonSliderPhysicMaterial;
        }
        m_VRUIPointer.FingerPress(false, obj);

    }
}
