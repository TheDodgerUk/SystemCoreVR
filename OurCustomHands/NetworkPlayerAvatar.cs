#if VR_INTERACTION
using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
#endif



/// this is  stright copy of SampleInputManager aprt from parts below which i will mark

using Node = UnityEngine.XR.XRNode;

/* This is an example class for how to send input and IK transforms to the sdk from any source
 * InputTrackingDelegate and InputControlDelegate are set on BodyTracking.
 */
public class NetworkPlayerAvatar : OvrAvatarInputManager
{
    private const string logScope = "sampleInput";

    [field: SerializeField, ReadOnly]
    private bool m_UpdateOwnerData = true;

    private SampleAvatarEntity m_SampleAvatarEntity;

    private RemoteOurOvrInputTrackingDelegate m_RemoteOurOvrInputTrackingDelegate;


    public PhotonTransformSampleAvatarEntityView m_AvatarRoot = null;
    public PhotonTransformView m_AvatarHead = null;
    public PhotonTransformView m_AvatarLeftHand = null;
    public PhotonTransformView m_AvatarRightHand = null;

    private List<Transform> m_Lods = new List<Transform>();

    private AutoHandAvatarInputManager m_AutoHandAvatarInputManager;

    private List<Collider> m_Colliders = new List<Collider>();

#if UNITY_EDITOR
    [SerializeField]
    private bool m_DEBUG_ForceShow = false;
#endif

    // Only used in editor, produces warnings when packaging
#pragma warning disable CS0414 // is assigned but its value is never used
    [SerializeField] private bool _debugDrawTrackingLocations = false;
#pragma warning restore CS0414 // is assigned but its value is never used

    protected void Awake()
    {
        // Debug Drawing
#if UNITY_EDITOR
#if UNITY_2019_3_OR_NEWER
        SceneView.duringSceneGui += OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
#endif
    }


    private void Start()
    {
        Debug.LogError("This syncs up the Avatar and the \"shadow\"  avatar");
        m_AutoHandAvatarInputManager = CameraControllerVR.Instance.GetComponentInChildren<AutoHandAvatarInputManager>(true);
        if (BodyTracking != null)
        {
            m_RemoteOurOvrInputTrackingDelegate = new RemoteOurOvrInputTrackingDelegate(null, this); //this has been added
            BodyTracking.InputTrackingDelegate = m_RemoteOurOvrInputTrackingDelegate;
            BodyTracking.InputControlDelegate = new RemoteOurOvrInputControlDelegate();
        }
        m_SampleAvatarEntity = this.GetComponent<SampleAvatarEntity>();

        if (m_SampleAvatarEntity.transform.gameObject.layer != Layers.LoadingLayer)
        {
            DebugBeep.LogError("it should be Layers.LoadingLayer, so can turn it back back to normal", DebugBeep.MessageLevel.High);
        }

        Core.Mono.WaitUntil(5, () => Succuss(), () =>
        {
            Debug.LogError("WaitUntil LoadState");
            m_SampleAvatarEntity.transform.gameObject.SetLayerRecursively(Layers.DefaultLayer);
        });

        bool Succuss()
        {
            m_SampleAvatarEntity.transform.gameObject.SetLayerRecursively(Layers.LoadingLayer);
            return m_SampleAvatarEntity.LoadState == OvrAvatarEntity.LoadingState.Success;
        }

        //needs a delay, other wise the scale will get overwritten 
        this.WaitFor(1f, () =>
        {
            m_AvatarLeftHand.transform.GetChild(0).GetChild(0).localPosition = CameraControllerVR.Instance.HandLeftRef.transform.GetChild(0).localPosition;
            m_AvatarLeftHand.transform.GetChild(0).GetChild(0).localRotation = CameraControllerVR.Instance.HandLeftRef.transform.GetChild(0).localRotation;
            m_AvatarLeftHand.transform.GetChild(0).GetChild(0).localScale = CameraControllerVR.Instance.HandLeftRef.transform.GetChild(0).localScale;

            m_AvatarRightHand.transform.GetChild(0).GetChild(0).localPosition = CameraControllerVR.Instance.HandRightRef.transform.GetChild(0).localPosition;
            m_AvatarRightHand.transform.GetChild(0).GetChild(0).localRotation = CameraControllerVR.Instance.HandRightRef.transform.GetChild(0).localRotation;
            m_AvatarRightHand.transform.GetChild(0).GetChild(0).localScale = CameraControllerVR.Instance.HandRightRef.transform.GetChild(0).localScale;


            Vector3 ScaleFix = new Vector3(1f, 1f, -1f);  
            m_AvatarLeftHand.transform.GetChild(0).localScale = ScaleFix;
            m_AvatarRightHand.transform.GetChild(0).localScale = ScaleFix;

            var hands = this.GetComponentsInChildren<Autohand.Hand>(true);
            foreach (var hand in hands)
            {
                RemoveItems(hand);
            }

        });
    }


    private void RemoveItems(Autohand.Hand hand)
    {
#if VR_INTERACTION
        var link = hand.GetComponent<Autohand.Demo.OVRHandControllerLink>();
        if (link != null)
        {
            Destroy(link);
        }

        var handitem = hand.GetComponent<Autohand.Hand>();
        if (handitem != null)
        {
            Destroy(handitem);
        }

        var fingerItems = hand.GetComponentsInChildren<Autohand.Finger>(true);
        foreach (var finger in fingerItems)
        {
            finger.enabled = false;
        }
#endif
    }


    public void SetUpdateOwnerData(bool enable)
    {
        Debug.LogError($"SetUpdateOwnerData   {enable}");
        m_UpdateOwnerData = enable;
    }

    public void Update()
    {
        if (m_Lods.Count == 0)
        {
            m_Lods.Add(this.transform.SearchComponent<Transform>("LOD0", false));
            m_Lods.Add(this.transform.SearchComponent<Transform>("LOD1", false));
            m_Lods.Add(this.transform.SearchComponent<Transform>("LOD2", false));
            m_Lods.Add(this.transform.SearchComponent<Transform>("LOD3", false));
            m_Lods.Add(this.transform.SearchComponent<Transform>("LOD4", false));
            m_Lods.RemoveAll(e => e == null);
        }

        if (m_Colliders.Count == 0)
        {
            m_Colliders = this.transform.GetComponentsInChildren<Collider>(true).ToList();
        }


        if (m_AvatarHead.photonView.IsMine == true)
        {
            m_Colliders.ForEach(e => e.enabled = false);

            // disable shadow
            m_Lods.ForEach(e => e.SetActive(false));

#if UNITY_EDITOR
            if(m_DEBUG_ForceShow == true)
            {
                m_Lods.ForEach(e => e.SetActive(true));
            }
#endif

            m_AvatarRoot.m_RenderAndColliderObjectActive = this.m_UpdateOwnerData;

            m_AvatarRoot.transform.position = CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer.position;
            m_AvatarRoot.transform.rotation = CameraControllerVR.Instance.AutoHandPlayerRef.trackingContainer.rotation;
            m_AvatarHead.transform.position = CameraControllerVR.Instance.MainCamera.transform.position;
            m_AvatarHead.transform.rotation = CameraControllerVR.Instance.MainCamera.transform.rotation;

            m_AvatarLeftHand.transform.position = CameraControllerVR.Instance.HandLeftRef.transform.position;
            m_AvatarLeftHand.transform.rotation = CameraControllerVR.Instance.HandLeftRef.transform.rotation;

            m_AvatarRightHand.transform.position = CameraControllerVR.Instance.HandRightRef.transform.position;
            m_AvatarRightHand.transform.rotation = CameraControllerVR.Instance.HandRightRef.transform.rotation;
        }

    }



    protected override void OnDestroyCalled()
    {
#if UNITY_EDITOR
#if UNITY_2019_3_OR_NEWER
        SceneView.duringSceneGui -= OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
#endif

        base.OnDestroyCalled();
    }

#if UNITY_EDITOR
#region Debug Drawing

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_debugDrawTrackingLocations)
        {
            DrawTrackingLocations();
        }
    }

    private void DrawTrackingLocations()
    {
        var inputTrackingState = BodyTracking.InputTrackingState;

        float radius = 0.2f;
        Quaternion orientation;
        float outerRadius() => radius + 0.25f;
        Vector3 forward() => orientation * Vector3.forward;

        Handles.color = Color.blue;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.headset.position, radius);

        orientation = inputTrackingState.headset.orientation;
        Handles.DrawLine((Vector3)inputTrackingState.headset.position + forward() * radius,
            (Vector3)inputTrackingState.headset.position + forward() * outerRadius());

        radius = 0.1f;
        Handles.color = Color.yellow;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.leftController.position, radius);

        orientation = inputTrackingState.leftController.orientation;
        Handles.DrawLine((Vector3)inputTrackingState.leftController.position + forward() * radius,
            (Vector3)inputTrackingState.leftController.position + forward() * outerRadius());

        Handles.color = Color.yellow;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.rightController.position, radius);

        orientation = inputTrackingState.rightController.orientation;
        Handles.DrawLine((Vector3)inputTrackingState.rightController.position + forward() * radius,
            (Vector3)inputTrackingState.rightController.position + forward() * outerRadius());
    }

#endregion
#endif // UNITY_EDITOR
}
#endif