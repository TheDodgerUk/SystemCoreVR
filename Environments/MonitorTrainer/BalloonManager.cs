using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;
using PathCreation.Examples;
using PathCreation;

namespace MonitorTrainer
{
    public class BalloonManager : MonoBehaviour
    {
        public static BalloonManager Instance;
        private GameObject m_BalloonPrefab;
        private float m_Timer = 10f;
        private int m_Index = 0;
        public PathCreator PathCreatorComponent => m_PathCreatorComponent;
        private PathCreator m_PathCreatorComponent;
        public float PathOriginPosition;


        private List<Material> m_MaterialList = new List<Material>();

        private List<ParticleSystem> m_BackgroundParticleSystems = new List<ParticleSystem>();

        public void Initialise()
        {
            Instance = this;
            m_BalloonPrefab = transform.Find("Balloon").gameObject;
            m_BalloonPrefab.ForceComponent<Balloon>();
            m_BalloonPrefab.transform.SetActive(false);
            SetupBackgroundParticleEffects();
            LoadBalloonMaterial();
            SetTheBalloonsPath();
            //Debug.LogError("This whole thing needs redoing, needs a pool manager");

            // disable the basic balloons
            var root = this.gameObject.scene.GetRootGameObjects();
            foreach (var item in root)
            {
                var found = item.transform.SearchComponent<Transform>("BackgroundBalloonParticleEffect", false);
                if(found != null)
                {
                    found.SetActive(false);
                    break;
                }
            }
        }

        private void SetTheBalloonsPath()
        {
            m_PathCreatorComponent = this.GetComponent<PathCreator>();
            m_PathCreatorComponent.bezierPath.GlobalNormalsAngle = 90;
            PathOriginPosition = this.transform.position.x;
        }

        private void StartGeneratingBalloon()
        {
            DebugBeep.Log($"Disable ballons heading towards you", DebugBeep.MessageLevel.Medium);
            //// StartCoroutine(StartGeneratingBalloonCorutine());
        }

        private IEnumerator StartGeneratingBalloonCorutine()
        {
            while (true)
            {
                m_Timer = UnityEngine.Random.Range(120, 180);
                yield return new WaitForSeconds(m_Timer);
                GenerateBalloon();
            }
        }

        [InspectorButton]
        public void GenerateBalloon()
        {
            GameObject ballInstance = Instantiate(m_BalloonPrefab);
            ballInstance.name = $"BalloonInstance ({m_Index})";
            m_Index++;
            ballInstance.transform.SetActive(true);
            if (m_MaterialList.Count > 0)
            {
                int randomMat = UnityEngine.Random.Range(0, m_MaterialList.Count);

                Renderer renderer = ballInstance.GetComponent<Renderer>();
                renderer.material = m_MaterialList[randomMat];
                renderer.material.shader = Shader.Find(renderer.material.shader.name);
            }
            ballInstance.GetComponent<Balloon>().Initialise();
        }


        public void ChangeToScenario(ScenarioEnum Scenario)
        {
            switch (Scenario)
            {
                case ScenarioEnum.Blank:
                    Nothing();
                    break;

                case ScenarioEnum.TutorialPart1:
                    Nothing();
                    break;

                case ScenarioEnum.Stackable:
                    PlayBackgroundEffects();
                    StartGeneratingBalloon();
                    break;

                default:
                    break;
            }
        }

        private void LoadBalloonMaterial()
        {
            Core.AssetBundlesRef.MaterialAssetBundleRef.GetItemList(this, "InteractiveBalloon_", (materialList) =>
            {
                foreach (var mat in materialList)
                {
                    Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(this, mat, (material) => 
                    {
                        m_MaterialList.Add(material);
                    });
                }
            });
        }

        private void PlayBackgroundEffects()
        {
            m_BackgroundParticleSystems.ForEach(e => e.Play());
        }

        private void SetupBackgroundParticleEffects()
        {
            var flashRoot = GameObject.Find("FlashEffect").transform;
            var balloonRoot = GameObject.Find("BackgroundBalloonParticleEffect").transform;

            for (int i = 0; i < flashRoot.childCount; ++i)
            {
                ParticleSystem particleeffect = flashRoot.GetChild(i).GetComponent<ParticleSystem>();
                particleeffect.Stop();
                m_BackgroundParticleSystems.Add(particleeffect);
            }

            for (int i = 0; i < balloonRoot.childCount; ++i)
            {
                ParticleSystem particleEffect = balloonRoot.GetChild(i).GetComponent<ParticleSystem>();
                particleEffect.Stop();
                m_BackgroundParticleSystems.Add(particleEffect);
            }
        }

        private void Nothing()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
