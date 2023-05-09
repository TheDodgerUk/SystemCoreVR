
using MonitorTrainer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MonitorTrainer.MonitorTrainerConsts;
namespace MonitorTrainer
{

    public class MenuManagerDebug : MonoBehaviour
    {
        public const string HEADER = "HEADER";

        public class stringItem
        {
            public string Item;
            public bool Enable;
        }

        public enum DebugInt
        {
            OppositeSide,

            PhysicalConsole,
            MonitorTrainerRoot,
            LightSpotCharacters,

            HEADER_BandStuff,
            BandManager,
            Stage_Lighting,
            Stage_Lighting_Realtime,
            Stage_Particles,
            Stage_Spotlights,
            Stage_Static,

            HEADER_Basic,
 
            Static,
            Interactive,

            HEADER_UIGroup,

            CanvasGroup,
            CanvasMesh,
            Maskable,
            RaycastTarget,

            HEADER_Personal,

            Decals1,
            Decals2,
            Static1,
            Static2,
            Interactive1,
            Interactive2,

            Console1,
            Console2,

            HEADER_VRStuff, 
 
            ContentPickUp,
            ContentParticleSystem,
            ContentDrink,
            ContentFood,
            ContentPickUpSocket,
            ContentPickUpCable,
            ContentHinge,
            ContentHasRigidBody,

            HEADER_Spot,

            SpotLightAdd,
            SpotLightSoft,
            SpotLightTrans,
            TrackChanges, 

            HEADER_Speed,

            SpaceWarp,
            InputManagerVR,

            StringItem,
        }


        private void Awake()
        {
            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.OppositeSide, (enable) =>
            {
                OppositeSide.Instance.transform.SetActive(enable);
            }) ;
            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.BandManager, (enable) =>
            {
                BandManager.Instance.transform.SetActive(Convert.ToBoolean(enable));
            });
            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.PhysicalConsole, (enable) =>
            {
                PhysicalConsole.Instance.transform.SetActive(Convert.ToBoolean(enable));
            });
            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.MonitorTrainerRoot, (enable) =>
            {
                MonitorTrainerRoot.Instance.transform.SetActive(Convert.ToBoolean(enable));
            });


            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Decals1, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Decals_Player1").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Decals2, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Decals_Player2").SetActive(Convert.ToBoolean(enable));
            });


            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Static1, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Static_Player1 ").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Static2, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Static_Player2").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Interactive1, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Interactive_Player1").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Interactive2, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Interactive_Player2").SetActive(Convert.ToBoolean(enable));
            });


            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Static, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Static").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Interactive, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Interactive").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.CanvasGroup, (enable) =>
            {
                var all = FindObjectsOfType<CanvasGroup>(true).ToList();
                all.ForEach(e => e.enabled = enable);
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.CanvasMesh, (enable) =>
            {
                var all = FindObjectsOfType<CanvasRenderer>(true).ToList();
                all.ForEach(e => e.cullTransparentMesh = !enable);
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Maskable, (enable) =>
            {
                var all = FindObjectsOfType<UnityEngine.UI.Image>(true).ToList();
                all.ForEach(e => e.maskable = !enable);
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.RaycastTarget, (enable) =>
            {
                var all = FindObjectsOfType<UnityEngine.UI.Image>(true).ToList();
                all.ForEach(e => e.raycastTarget = !enable);
            });


            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Console1, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Quest_MonitorTrainer_Console_Prefab_Prefab_Player_ONE").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.Console2, (enable) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>("Quest_MonitorTrainer_Console_Prefab_Prefab_player_TWO").SetActive(Convert.ToBoolean(enable));
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.SpaceWarp, (enable) =>
            {
                OVRManager.SetSpaceWarp(enable);
                Debug.LogError($"SetSpaceWarp {enable}");
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.InputManagerVR, (enable) =>
            {
                InputManagerVR.Instance.SetActive(enable);
                Debug.LogError($"InputManagerVR {enable}");
            });
            

            Core.PhotonGenericRef.CollectDebugIntDataMessage<stringItem>((int)DebugInt.StringItem, (ItemData) =>
            {
                PhysicalConsole.Instance.transform.SearchComponent<Transform>(ItemData.Item).SetActive(Convert.ToBoolean(ItemData.Enable));
            });


            Core.PhotonGenericRef.CollectDebugIntDataMessage<stringItem>((int)DebugInt.SpotLightAdd, (ItemData) =>
            {
                var all = GameObject.FindObjectsOfType<VLB.VolumetricLightBeam>();
                foreach (var item in all)
                {
                    item.blendingMode = VLB.BlendingMode.Additive;
                }
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<stringItem>((int)DebugInt.SpotLightSoft, (ItemData) =>
            {
                var all = GameObject.FindObjectsOfType<VLB.VolumetricLightBeam>();
                foreach (var item in all)
                {
                    item.blendingMode = VLB.BlendingMode.SoftAdditive;
                }
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<stringItem>((int)DebugInt.SpotLightTrans, (ItemData) =>
            {
                var all = GameObject.FindObjectsOfType<VLB.VolumetricLightBeam>();
                foreach (var item in all)
                {
                    item.blendingMode = VLB.BlendingMode.TraditionalTransparency;
                }
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.TrackChanges, (enable) =>
            {
                var all = GameObject.FindObjectsOfType<VLB.VolumetricLightBeam>();
                foreach (var item in all)
                {
                    item.trackChangesDuringPlaytime = enable;
                }
            });

            Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)DebugInt.LightSpotCharacters, (enable) =>
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene s = SceneManager.GetSceneAt(i);
                    if (s.name.EndsWith("Stage"))
                    {
                        var gameObjects = s.GetRootGameObjects().ToList();
                        var rootObj = gameObjects.Find(e => e.name == "Spotlights");
                        if (rootObj != null)
                        {
                            var item = rootObj.transform.SearchComponent<Transform>("Light_Spot_Characters");
                            if (item != null)
                            {
                                item.SetActive(Convert.ToBoolean(enable));
                            }
                        }
                    }
                }
            });


            foreach (DebugInt debugInt in Enum.GetValues(typeof(DebugInt)))
            {
                foreach (MetaDataType meta in Enum.GetValues(typeof(MetaDataType)))
                {
                    if(debugInt.ToString() == meta.ToString())
                    {
                        Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)debugInt, (enable) =>
                        {
                            var itemFound = Core.Scene.GetAllSpawnedVrInteraction();
                            foreach (var item in itemFound)
                            {
                                var keys = item.BaseVrMetaDatas.GetKeys();
                                if (keys.Contains(meta))
                                {
                                    item.gameObject.SetActive(Convert.ToBoolean(enable));
                                }
                            }
                        });
                    }
                }
            }


            // split enums
            foreach (DebugInt foo in Enum.GetValues(typeof(DebugInt)))
            {
                var realStrings = foo.ToString().Split("_").ToList();
                if((realStrings.Count > 1) && (realStrings[0] != HEADER))
                {
                    Core.PhotonGenericRef.CollectDebugIntDataMessage<bool>((int)foo, (enable) => FindItem(foo, enable));
                }
            }
         }

        private void FindItem(DebugInt debugInt, bool enable)
        {
            var realStrings = debugInt.ToString().Split("_").ToList();
            if(realStrings.Count == 2)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene s = SceneManager.GetSceneAt(i);
                    if (s.name.EndsWith(realStrings[0]))
                    {
                        var gameObjects = s.GetRootGameObjects().ToList();
                        var item = gameObjects.Find(e => e.name == realStrings[1]);
                        if (item != null)
                        {
                            item.SetActive(Convert.ToBoolean(enable));
                        }
                    }
                }
            }
            if (realStrings.Count == 3)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene s = SceneManager.GetSceneAt(i);
                    if (s.name.EndsWith(realStrings[0]))
                    {
                        var gameObjects = s.GetRootGameObjects().ToList();
                        var item = gameObjects.Find(e => e.name == realStrings[1]);
                        if (item != null)
                        {
                            var sub = item.SearchComponent<Transform>(realStrings[2]);
                            if (sub != null)
                            {
                                sub.SetActive(Convert.ToBoolean(enable));
                            }
                        }
                    }
                }
            }
        }
    }

}

