using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using PathCreation;
using static MonitorTrainer.MonitorTrainerConsts;
using PathCreation.Examples;

namespace MonitorTrainer
{
    public class Balloon : MonoBehaviour
    {
        private const float BALLON_SPEED = 0.5f;


        private MeshRenderer m_BalloonMeshRender;
        private ParticleSystem m_BalloonParticleSystem;
        private GameObject m_ExplosionEffect;
        private Vector3 m_RandomRotation;
        

        private AudioSource m_PoppingAudioSource;
        private AudioClip m_PoppingSound;
        private PhysicMaterial m_BalloonPhysicMaterial;

        private PathCreator m_BalloonPathCreatorComponent;
        private PathFollower m_BalloonPathFollowerComponent;
        private float m_pathOriginPosition;

        private VrInteractionClickCallBack m_ClickBack;

        public void Initialise()
        {
            m_ClickBack = this.AddComponent<VrInteractionClickCallBack>();
            m_ClickBack.AddCallback((amount) =>
            {
                ExplodeTheBalloon();
            });

            enabled = true;
            m_BalloonMeshRender = gameObject.GetComponent<MeshRenderer>();
            m_PoppingAudioSource = gameObject.GetComponent<AudioSource>();
            m_ExplosionEffect = this.transform.Find("ExplosionEffect").gameObject;
            m_BalloonParticleSystem = gameObject.GetComponent<ParticleSystem>();
            m_BalloonParticleSystem.Stop();
            LoadTheSound();
            SetRandomRotation();
            m_BalloonMeshRender.enabled = true;

            m_BalloonPathCreatorComponent = BalloonManager.Instance.PathCreatorComponent;
            m_BalloonPathFollowerComponent = gameObject.ForceComponent<PathFollower>();
            m_BalloonPathFollowerComponent.pathCreator = m_BalloonPathCreatorComponent;
            m_BalloonPathFollowerComponent.endOfPathInstruction = EndOfPathInstruction.Stop;
            m_BalloonPathFollowerComponent.speed = BALLON_SPEED;
            m_pathOriginPosition = BalloonManager.Instance.PathOriginPosition;
            StartCoroutine(StartMovingBalloon());
        }


        private void SetRandomRotation()
        {
            float x = UnityEngine.Random.Range(-10, 10);
            float y = UnityEngine.Random.Range(-10, 10);
            float z = UnityEngine.Random.Range(-10, 10);
            m_RandomRotation.Set(x, y, z);
            this.transform.rotation = Quaternion.Euler(m_RandomRotation);
        }

        private IEnumerator StartMovingBalloon()
        {
            bool hasTheBalloonreachedTheEndOfThePath = false;
            int numberOfSegment = m_BalloonPathCreatorComponent.bezierPath.NumSegments;
            Vector3[] lastControlPointInSegment = m_BalloonPathCreatorComponent.bezierPath.GetPointsInSegment(numberOfSegment);
            float minBalloonPosition = lastControlPointInSegment[2].x + m_pathOriginPosition;
            float maxBalloonPosition = lastControlPointInSegment[3].x + m_pathOriginPosition;
            while (hasTheBalloonreachedTheEndOfThePath == false)
            {
                if (this.transform.position.x <= minBalloonPosition && this.transform.position.x >= maxBalloonPosition)
                {
                    hasTheBalloonreachedTheEndOfThePath = true;
                    ExplodeTheBalloon();
                }
                yield return null;
            }
        }

        [InspectorButton]
        public void ExplodeTheBalloon()
        {
            m_ClickBack.ColliderList.ForEach(e => e.enabled = false);
            SetTheExplosionEffectConstraint();
            m_BalloonMeshRender.enabled = false;
            m_BalloonParticleSystem.SetActive(true);
            m_BalloonParticleSystem.Play();
            m_PoppingAudioSource.Play();
            StartCoroutine(DestroyTheClone());
        }

        private void SetTheExplosionEffectConstraint()
        {
            m_ExplosionEffect.transform.LookAt(Camera.main.transform);
        }

        public IEnumerator DestroyTheClone()
        {
            while (false == m_BalloonParticleSystem.isStopped)
            {
                yield return new WaitForEndOfFrame();
            }
            Destroy(this.gameObject);
        }

        private void LoadTheSound()
        {
            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(this, "BalloonPop", (soundclip) =>
            {
                m_PoppingSound = soundclip;
                m_PoppingAudioSource.clip = m_PoppingSound;
                m_PoppingAudioSource.volume = 0.3f;
            });
        }
    }
}
