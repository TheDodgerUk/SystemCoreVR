using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonitorTrainer
{
    public class SpecialTasksHitBox : MonoSingleton<SpecialTasksHitBox>
    {
        private List<HitBox> m_CreatedHitBoxs = new List<HitBox>();
        public GameObject VFX { get; set; }


        private void Start()
        {
            Core.AssetsLocalRef.GameObjectLocalRef.GetItemInstantiated("TargetZonePrefab", (item) =>
            {
                VFX = item;
                VFX.transform.SetParent(this.transform);
                VFX.ClearLocals();
                VFX.SetActive(false);
            });
        }
        public HitBox CreateTasksHitBox()
        {
            if (MonitorTrainerRoot.Instance.CurrentScenario == MonitorTrainerConsts.ScenarioEnum.Stackable)
            {
                GameObject hitBoxRoot = new GameObject("hitBox");
                hitBoxRoot.transform.parent = this.transform;
                var hitbox = hitBoxRoot.transform.AddComponent<HitBox>();
                m_CreatedHitBoxs.Add(hitbox);
                return hitbox;
            }
            return null;
        }


        public void DeleteAllHitBoxs()
        {
            foreach (var item in m_CreatedHitBoxs)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
            m_CreatedHitBoxs.Clear();
        }
        public void DeleteHitBox(MusicianRequestData item) => DeleteHitBox(item.HitBox);    
        public void DeleteHitBox(HitBox hitBox)
        {
            m_CreatedHitBoxs.Remove(hitBox);
            UnityEngine.Object.Destroy(hitBox.gameObject);
        }   

        public HitBox GetRayCastHit(Type type)
        {
            foreach (var item in m_CreatedHitBoxs)
            {
                if (item.m_DetectionType == HitBox.DetectionType.RayCast)
                {
                    if (item.m_RayCast.m_RayCastMonoType == type)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

    }




    public class HitBox : MonoBehaviour
    {
        private readonly Vector3 TARGETZONE_SCALE = new Vector3(0.25f, 1, 0.25f);

        public class VFXData
        {
            public bool m_Show = false;
            public Vector3 m_Scale = Vector3.one;
        }

        public class RayCast
        {
            public float m_RayCastHitMaxTimer = 0;
            public float m_RayCastHitTimer = 0;
            public Type m_RayCastMonoType;
            public bool m_LastHit = false;
            public string m_ActorNickNameTouched;
        }

        public class OnTrigger
        {
            public int MAX_FRAMES = 100;
            public int ALLOW_ON_ENTER = 20;
            public int m_AllowEnterFrames = 0;
            public float m_Distance = 0;
            public List<VrInteraction> m_ValidHitItems = new List<VrInteraction>();
        }


        public enum DetectionType
        {
            OnTriggerEnter,
            OnTriggerExit,
            RayCast,
        }

        public enum TriggerType
        {
            OnTriggerEnter = DetectionType.OnTriggerEnter,
            OnTriggerExit = DetectionType.OnTriggerExit,
        }

        private CapsuleCollider m_CapsuleCollider;
        private BoxCollider m_BoxCollider;

        private MusicianRequestData m_MusicianRequestData;
        public DetectionType m_DetectionType = DetectionType.OnTriggerEnter;
        private OnTrigger m_OnTrigger = new OnTrigger();
        public RayCast m_RayCast = new RayCast();
        public VFXData m_VFXData { get; private set; } = new VFXData();

        protected void Awake()
        { 
            m_CapsuleCollider = this.AddComponent<CapsuleCollider>();
            m_CapsuleCollider.isTrigger = true;
            m_CapsuleCollider.height = 100;

            m_BoxCollider = this.AddComponent<BoxCollider>();
            m_BoxCollider.isTrigger = true;

        }

        public void SetOnTriggerFor(List<VrInteraction> objs, MusicianRequestData task, bool showVFX)
        {
            m_VFXData.m_Show = showVFX;
            m_DetectionType = DetectionType.OnTriggerEnter;
            m_OnTrigger.m_ValidHitItems = objs;
            m_MusicianRequestData = task;
            m_OnTrigger.m_AllowEnterFrames = 0;
        }

        public void SetRadius( float radius, float height = 100)
        {
            m_CapsuleCollider.enabled = true;
            m_BoxCollider.enabled = false;


            m_CapsuleCollider.radius = radius;
            m_CapsuleCollider.height = height;
            m_VFXData.m_Scale = TARGETZONE_SCALE * radius;
            m_OnTrigger.m_AllowEnterFrames = 0;
        }


        public void SetBox(TriggerType enter, Vector3 size, float distance = 0)
        {
            m_DetectionType = (DetectionType)enter;
            m_CapsuleCollider.enabled = false;
            m_BoxCollider.enabled = true;
            m_BoxCollider.size = size;
            m_OnTrigger.m_AllowEnterFrames = 0;
            m_OnTrigger.m_Distance = distance;
        }

        public void UseRayCastHitTimer(Type mono, float timer, MusicianRequestData musicianRequestData)
        {
            this.transform.SetActive(true);
            m_DetectionType = DetectionType.RayCast;
            m_RayCast.m_RayCastHitMaxTimer = timer;
            m_RayCast.m_RayCastHitTimer = 0;
            m_RayCast.m_RayCastMonoType = mono;
            m_MusicianRequestData = musicianRequestData;
        }

        public void RayCastHit(Type item, string nickName)
        {
            if (item == m_RayCast.m_RayCastMonoType)
            {
                m_RayCast.m_LastHit = true;
                m_RayCast.m_ActorNickNameTouched = nickName;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_DetectionType == DetectionType.OnTriggerEnter)
            {
                Trigger(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_DetectionType == DetectionType.OnTriggerExit)
            {
                Trigger(other);
            }
        }


        private void Trigger(Collider other)
        {
            if (m_OnTrigger.m_ValidHitItems == null || m_OnTrigger.m_ValidHitItems.Count == 0)
            {
                return;
            }
            if (m_OnTrigger.m_AllowEnterFrames < m_OnTrigger.ALLOW_ON_ENTER)
            {
                return;
            }

            var baseVr = other.GetComponentInParent<VrInteraction>();
            var foundItem = m_OnTrigger.m_ValidHitItems.Find(e => e == baseVr);
            if (foundItem == null)
            {
                return;
            }

            Debug.LogError($"foundItem {foundItem.OriginalFullPath}");
            if (m_OnTrigger.m_Distance != 0)
            {
                var distance = Vector3.Distance(baseVr.ActorNickNamePosition, other.transform.position);
                if (distance > m_OnTrigger.m_Distance)
                {
                    ItemHit(foundItem);
                }
            }
            else
            {
                ItemHit(foundItem);
            }

            Debug.LogError($"TriggerEnter m_ValidHitItems {m_OnTrigger.m_ValidHitItems.Count}, {other},   {baseVr.OriginalFullPath}");
        }


        private void Update()
        {
            OnTriggerUpdate();
            UpdateForRayCast();
        }

        private void OnTriggerUpdate()
        {
            if (m_DetectionType == DetectionType.OnTriggerEnter)
            {
                m_OnTrigger.m_AllowEnterFrames++;
                if (m_OnTrigger.m_AllowEnterFrames > m_OnTrigger.MAX_FRAMES)
                {
                    m_OnTrigger.m_AllowEnterFrames = m_OnTrigger.MAX_FRAMES;
                }
            }
        }

        private void UpdateForRayCast()
        {
            if (m_DetectionType == DetectionType.RayCast)
            {
                if (m_RayCast.m_LastHit == true)
                {
                    m_RayCast.m_RayCastHitTimer += Time.deltaTime;
                }
                else
                {
                    m_RayCast.m_RayCastHitTimer -= Time.deltaTime;
                }
                m_RayCast.m_RayCastHitTimer = MathF.Max(0, m_RayCast.m_RayCastHitTimer);
                m_RayCast.m_LastHit = false;
                if (m_RayCast.m_RayCastHitTimer > m_RayCast.m_RayCastHitMaxTimer)
                {
                    m_MusicianRequestData.m_IsCompleted = true;
                    m_MusicianRequestData.m_VrInteraction.ActorNickNameTouched = m_RayCast.m_ActorNickNameTouched;
                    if (m_MusicianRequestData.m_RequestType == MusicianRequestData.RequestTypeEnum.SpecialForAll)
                    {
                        NetworkMessagesManager.Instance.SendTaskSpecialForAllHitCompleted(m_RayCast.m_ActorNickNameTouched);
                    } 
                }
            }
        }

        private void ItemHit(VrInteraction vrInteraction)
        {
            Debug.LogError($"ItemHit {vrInteraction.OriginalName}");
            m_MusicianRequestData.m_IsCompleted = true;
            m_MusicianRequestData.m_VrInteraction = vrInteraction;
            if (m_MusicianRequestData.m_RequestType == MusicianRequestData.RequestTypeEnum.SpecialForAll)
            {
                NetworkMessagesManager.Instance.SendTaskSpecialForAllHitCompleted(vrInteraction.ActorNickNameTouched);
            }

        }

        
    }
}
