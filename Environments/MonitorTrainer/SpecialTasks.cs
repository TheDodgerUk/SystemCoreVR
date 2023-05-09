using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static MonitorTrainer.MonitorTrainerConsts;
using static MonitorTrainer.MusicianRequestData;
using static VrInteractionBaseButton;

namespace MonitorTrainer
{
    public class SpecialTasks
    {

        private const string BEACH_BALL_PHOTON = "BeachBall";

        private const float TORQUE_BEACHBALL_MULTIPLYER = 10f;
        private const float TORQUE_BOTTLE_MULTIPLYER = 10f;
        private static readonly string BEACHBALL_CATALOGUE_GUID = "6248cb88-c266-4d82-b75d-47d879c1ef0c";

        private static readonly string BASKET_HOOP_CATALOGUE_GUID = "324afa20-ce12-4da5-8c21-c4270227b0b8";
        private static readonly string BASKET_BALL_CATALOGUE_GUID = "5cc95692-2f32-4cc7-bfd9-1fa371547ee0";

        private static readonly string MICROPHONE_CATALOGUE_GUID = "16052b3b-a746-4a90-9e13-7a2098c03a97";
        private static readonly string LIGHT_POWER_SWITCH_CATALOGUE_GUID = "d1d70ffc-fa2d-46ae-b808-96f08103e44b";
        private static readonly string BIN_CATALOGUE_GUID = "6c0fa611-5191-4b3c-b3d6-17ea30d847e3";

        private readonly Vector3 BottleMin = new Vector3(0.333f, 0, 0.2f);
        private readonly Vector3 BottleMax = new Vector3(-0.3f, 0, -0.08f);


        private static readonly List<string> DRINK_CATALOGUE_GUID = new List<string>()
        {
            {"3f263ca1-e5c2-4938-9aa3-a8060338b605"}, // Quest FizzyDrink
            {"8715d030-ab4a-4bf1-8ca6-d0c1b9b7323e"}, // Quest SparklingDrink   
        };


        private static readonly List<string> GUITAR_CATALOGUE_GUID = new List<string>()
        {
            {"52acc923-15b6-4bd3-9f23-91dd4ce5da29"}, // Quest AccousticGuitar
            {"d2549d65-ecec-4ade-a64d-09f60421abab"}, // Quest ElectricGuitar Galaxy
            {"7d466267-5c65-44c1-837c-b9b9c3b802de"}, // Quest ElectricGuitar Standard
            {"86b67890-acf2-457a-9153-f0ec7a566fa3"}, // banjo
        };

        private static readonly List<string> DRUM_STICK_CATALOGUE_GUID = new List<string>()
        {
            {"e516d3b8-97e1-4033-92bb-ec2feaf78e74"}, // Quest DrunmStick           
        };


        private List<GameObject> m_TaskCreatedItems = new List<GameObject>();

        public SpecialTasks()
        {
            Core.PhotonGenericRef.CollectVrCreation((vrName, vrInteraction) =>
            {
                m_TaskCreatedItems.Add(vrInteraction.gameObject);
                Debug.LogError($"vr Message {vrName}");
                if (vrName == BEACH_BALL_PHOTON)
                {

                }
            });
        }

        public void DeleteTaskCreatedItems()
        {
            for (int i = 0; i < m_TaskCreatedItems.Count; i++)
            {
                if (m_TaskCreatedItems[i] != null)
                {
                    UnityEngine.Object.Destroy(m_TaskCreatedItems[i]);
                }
            }
            m_TaskCreatedItems.Clear();
        }

        public List<MusicianRequestData> CollectRunTime()
        {
            List<MusicianRequestData> collected = new List<MusicianRequestData>();
            collected.Add(ScenarioTutorial2_RuntimeErrorPowerTurnedOff1());
            collected.Add(ScenarioTutorial2_RuntimeErrorPowerTurnedOff2());
            collected.Add(ScenarioTutorial2_RuntimeErrorPowerTurnedOff3());

            collected.Add(ScenarioTutorial2_RuntimeErrorMuteAllOff1());
            collected.Add(ScenarioTutorial2_RuntimeErrorMuteAllOff2());
            collected.Add(ScenarioTutorial2_RuntimeErrorMuteAllOff3());
            return collected;
        }


        public List<MusicianRequestData> CollectSpecial()
        {
            float ff = TaskManager.SPECIALTASK_PROBABILITY;//20%// this is for refernce
            List<MusicianRequestData> collected = new();
            collected.Add(MonitorAmpTurnOff());       // pass
            collected.Add(CableFallOut());            // pass
            collected.Add(ConsoleGlitch());           // pass
            collected.Add(BottleOnConsole());         // pass
            collected.Add(BeachBall());               // pass
            collected.Add(DunkBasketball());          // FAIL

            return collected;
        }

        public List<MusicianRequestData> CollectSpecialForAll()
        {
            List<MusicianRequestData> collected = new();
            collected.Add(GuitarStringBroken());    // pass
            collected.Add(LostDrumStick());         // pass
            collected.Add(LightingPowerCut());      // pass, can pull to far though
            collected.Add(FoodWanted());            // pass
            collected.Add(DrinkWantedRhythmGuitar());           // pass
            collected.Add(BinFire());               // Pass
            collected.Add(MicrophoneWanted());      // not tested
            return collected;
        }

        public List<TaskSpecialForAllTimings> CreateTimingsTaskSpecialForAll(List<MusicianRequestData> specailsForAll)
        {
            var songLength = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongLength;
            float half = songLength / 2;
            float quarter = songLength / 4;
            float randomTiming = UnityEngine.Random.Range(0, half);
            randomTiming += quarter;

            List<TaskSpecialForAllTimings> newList = new List<TaskSpecialForAllTimings>();
            TaskSpecialForAllTimings newItem = new TaskSpecialForAllTimings();
            newItem.Time = randomTiming;


            int itemIndex = UnityEngine.Random.Range(0, specailsForAll.Count);
            newItem.SpecialTaskIndex = itemIndex;

            Debug.LogError($"time {newItem.Time},  CollectSpecialForAll() {specailsForAll[itemIndex].m_Description}");
            newList.Add(newItem);
            return newList;
        }


        #region RUNTIME
        public MusicianRequestData ScenarioTutorial2_RuntimeErrorPowerTurnedOff1(System.Action callback = null)
        {
            var item = new MusicianRequestData();
            item.m_Description = "The console is off!! Why is it off?!? Turn it back on ASAP, the band can’t hear anything!";
            item.m_DescriptionForReport = "ScenarioTutorial2_RuntimeErrorPowerTurnedOff1";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_LifespanSeconds = 0;
            item.m_RuntimeTypeEnum = RuntimeTypeEnum.Power;
            item.m_CompleteIfTrue = PowerOnScenario2;
            return item;
        }

        public MusicianRequestData ScenarioTutorial2_RuntimeErrorPowerTurnedOff2()
        {
            var item = ScenarioTutorial2_RuntimeErrorPowerTurnedOff1(null);
            item.m_Description = "I think you’ve turned the console off… You should probably turn that on before anyone notices!";
            item.m_DescriptionForReport = "ScenarioTutorial2_RuntimeErrorPowerTurnedOff2";
            item.m_MainMusicianType = MusicianTypeEnum.StageHand;
            item.m_RuntimeTypeEnum = RuntimeTypeEnum.Power;
            item.m_CompleteIfTrue = PowerOnScenario2;
            return item;
        }

        public MusicianRequestData ScenarioTutorial2_RuntimeErrorPowerTurnedOff3()
        {
            var item = ScenarioTutorial2_RuntimeErrorPowerTurnedOff1(null);
            item.m_Description = "Getting nothing through from your end over here? Is everything okay? Is the console on?";
            item.m_DescriptionForReport = "ScenarioTutorial2_RuntimeErrorPowerTurnedOff3";
            item.m_MainMusicianType = MusicianTypeEnum.FrontOfHouse;
            item.m_RuntimeTypeEnum = RuntimeTypeEnum.Power;
            item.m_CompleteIfTrue = PowerOnScenario2;
            return item;
        }

        private bool PowerOnScenario2()
        {
            return (PhysicalConsole.Instance.Power.ButtonState == ButtonStateEnum.Down);
        }

        public MusicianRequestData ScenarioTutorial2_RuntimeErrorMuteAllOff1()
        {
            var item = new MusicianRequestData();
            item.m_Description = "All the inputs are muted! Make sure MUTE ALL is OFF!";
            item.m_DescriptionForReport = "ScenarioTutorial2_RuntimeErrorMuteAllOff1";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_LifespanSeconds = 0;
            item.m_RuntimeTypeEnum = RuntimeTypeEnum.MuteAll;
            item.m_CompleteIfTrue = RuntimeMuteAllScenario2;
            return item;
        }

        public MusicianRequestData ScenarioTutorial2_RuntimeErrorMuteAllOff2()
        {
            var item = ScenarioTutorial2_RuntimeErrorMuteAllOff1();
            item.m_Description = "Why is MUTE ALL on? We’re in the middle of a show!! Turn it OFF.";
            item.m_DescriptionForReport = "ScenarioTutorial2_RuntimeErrorMuteAllOff2";
            item.m_MainMusicianType = MusicianTypeEnum.StageHand;
            item.m_RuntimeTypeEnum = RuntimeTypeEnum.MuteAll;
            item.m_CompleteIfTrue = RuntimeMuteAllScenario2;
            return item;
        }

        public MusicianRequestData ScenarioTutorial2_RuntimeErrorMuteAllOff3(System.Action callback = null)
        {
            var item = ScenarioTutorial2_RuntimeErrorMuteAllOff1();
            item.m_Description = "All the channels are apparently muted, they shouldn’t be! Make sure MUTE ALL is OFF!";
            item.m_DescriptionForReport = "ScenarioTutorial2_RuntimeErrorMuteAllOff3";
            item.m_MainMusicianType = MusicianTypeEnum.FrontOfHouse;
            item.m_RuntimeTypeEnum = RuntimeTypeEnum.MuteAll;
            item.m_CompleteIfTrue = RuntimeMuteAllScenario2;
            return item;
        }

        private bool RuntimeMuteAllScenario2()
        {
            return (PhysicalConsole.Instance.MuteGroupAll.ButtonState == ButtonStateEnum.Up);
        }

        #endregion RUNTIME

        private MusicianRequestData MonitorAmpTurnOff()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.FrontOfHouse;
            item.m_Description = "We've lost all audio, what a joke. Is your Amp even on?!?";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.Special;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_CompleteIfTrue = MonitorAmpCompleate;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 2f;
            item.m_OnDelayStart = OnStart;
            return item;

            void OnStart(MusicianRequestData data) => PhysicalAmp.Instance.Button2.ButtonState = ButtonStateEnum.Up;
            bool MonitorAmpCompleate() => (PhysicalAmp.Instance.Button2.ButtonState == ButtonStateEnum.Down);
        }



        private MusicianRequestData CableFallOut()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;

            CharacterDataClass bandMember = null;
            do
            {
                bandMember = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.GetRandom();
            } while (bandMember.AuxEnum.IsMusician() == false);

            item.m_Description = $"We're not hearing {bandMember.AuxEnum} .Is everything plugged in correctly at your end?";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.Special;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            // moved to m_OnDelayStart item.m_OnStart = PullCableOut;
            item.m_CompleteIfTrue = PullCableOutFixed;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 2f;
            item.m_OnDelayStart = PullCableOut;
            return item;

            void PullCableOut(MusicianRequestData data)
            {
                PhysicalAmp.Instance.LooseCable.ForceDisonnect();
                PhysicalAmp.Instance.LooseCable.SetState(VrInteraction.StateEnum.On);
            }

            bool PullCableOutFixed() => (PhysicalAmp.Instance.LooseCable.IsSocketed == true);

        }

        private MusicianRequestData ConsoleGlitch()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.FrontOfHouse;
            item.m_Description = "I've lost track of how many times I've told you to keep your coffee away from the console! Try a power cycle to fix it!";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.Special;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_CompleteIfTrue = PowerOff;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 2f;
            item.m_OnDelayStart = GlitchScreen;
            return item;

            void GlitchScreen(MusicianRequestData data)
            {
                ConsoleScreenManager.Instance.BootGlitchScreensRef.PlayGlitch();
            }

            bool PowerOff()
            {
                return (PhysicalConsole.Instance.Power.ButtonState == ButtonStateEnum.Up);
            }
        }


        private MusicianRequestData DunkBasketball()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_Description = $"DunkBasketball";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.Special;
            item.m_CanAutoRemove = true;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_CompleteIfTrue = BeachBallOfPlayArea;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 2f;
            item.m_OnSpecialTasksHitBox = SpecialTasksHitBoxStart;
            return item;


            void SpecialTasksHitBoxStart(MusicianRequestData musicianRequestData)
            {
                var hoops = Core.Scene.GetSpawnedVrInteractionGUID(BASKET_HOOP_CATALOGUE_GUID);
                var hoop = hoops.Find(e => e.isActiveAndEnabled == true);
                var col = hoop.GetVrInteractionFromRoot(MetaDataType.ContentCollision)[0];
                musicianRequestData.HitBox.transform.position = col.transform.position;
                musicianRequestData.HitBox.SetBox(HitBox.TriggerType.OnTriggerEnter, new Vector3(0.01f, 0.01f, 0.01f), 2f);

                SetTriggerFor(BASKET_BALL_CATALOGUE_GUID, musicianRequestData, false);
            }

            bool BeachBallOfPlayArea() => item.m_IsCompleted && item.m_VrInteraction.ActorNickNameTouched == Core.PhotonMultiplayerRef.MySelf.NickName;
        }



        private MusicianRequestData BottleOnConsole()
        {
            string SPARKS = "Sparks";
            string BOTTLE_SMASH = "BOTTLE_SMASH";

            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_Description = "You treat your console like a dumpster! Clean it up!!!";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.Special;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_CompleteIfTrue = PowerOff;
            item.m_OnComplete = RemoveSparks;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 2f;
            item.m_OnDelayStart = GlitchScreen;
            return item;

            void GlitchScreen(MusicianRequestData item)
            {
                Core.Scene.SpawnObject(DRINK_CATALOGUE_GUID.GetRandom(), (beer) =>
                {
                    m_TaskCreatedItems.Add(beer.gameObject);

                    MuliplayerData currentPlayerData = MonitorTrainerConsts.MULIPLYER_DATA[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum];
                    var console = Core.Scene.GetSpawnedVrInteraction(currentPlayerData.ConsoleName, currentPlayerData.RootName);

                    beer.transform.position = console.transform.position + new Vector3(0f, 5f, 0f);
                    beer.transform.position += BottleMin.Random(BottleMax);
                    beer.GetComponent<Rigidbody>().AddRelativeTorque(Random.insideUnitSphere * TORQUE_BOTTLE_MULTIPLYER, ForceMode.Impulse);

                    BottleSmash(beer.gameObject, () =>
                    {
                        Core.AssetsLocalRef.VisualEffectLocalRef.GetItemInstantiated("VFX_Electric Sparks", (createdVfx) =>
                        {
                            item.m_CreatedObjects[SPARKS] = createdVfx;
                            m_TaskCreatedItems.Add(createdVfx);
                            createdVfx.transform.position = console.transform.position;
                        });

                        Core.Mono.WaitFor(1, () =>
                        {
                            ConsoleScreenManager.Instance.BootGlitchScreensRef.PlayGlitch();
                        });
                    });

                });

            }

            void BottleSmash(GameObject beer, System.Action callback)
            {
                VelocityImpact impact = beer.AddComponent<VelocityImpact>();
                impact.CallbackMagnitude((mag) =>
                {
                    impact.ClearCallbackMagnitude();
                    Core.AssetsLocalRef.VisualEffectLocalRef.GetItemInstantiated("VFX_ShatteredBottle", (createdVfx) =>
                    {
                        createdVfx.transform.position = impact.transform.position;
                        UnityEngine.Object.Destroy(beer.gameObject);

                        Core.Mono.WaitFor(0.4f, () =>
                        {
                            callback?.Invoke();
                        });



                        Core.Mono.WaitFor(2f, () =>
                        {
                            UnityEngine.Object.Destroy(createdVfx.gameObject);
                        });
                    });
                });
            }

            bool PowerOff() => (PhysicalConsole.Instance.Power.ButtonState == ButtonStateEnum.Up);
            void RemoveSparks(MusicianRequestData item)
            {
                if (item.m_CreatedObjects.ContainsKey(SPARKS) == true)
                {
                    UnityEngine.Object.Destroy(item.m_CreatedObjects[SPARKS]);
                }
            }
        }

        private MusicianRequestData BeachBall()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.StageHand;
            item.m_Description = "Sorry about the ball.... not sure where it's come from. Just throw it out the back when you get chance!";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.Special;
            item.m_CanAutoRemove = true;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_CompleteIfTrue = BeachBallOfPlayArea;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 2f;
            item.m_OnDelayStart = DropBeachBall;
            item.m_OnStart = SpecialTasksHitBoxStart;
            item.m_OnSpecialTasksHitBox = SpecialTasksHitBoxStart;
            return item;

            void DropBeachBall(MusicianRequestData musicianRequestData)
            {
                MuliplayerData currentPlayerData = MonitorTrainerConsts.MULIPLYER_DATA[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum];

                Vector3 pos  = currentPlayerData.StartPosition + new Vector3(0f, 2f, 0f);
                pos += BottleMin.Random(BottleMax);
                Core.Scene.SpawnObjectNetwork(BEACHBALL_CATALOGUE_GUID, BEACH_BALL_PHOTON, 2f, pos, Quaternion.identity, (ball) =>
                {
                    m_TaskCreatedItems.Add(ball.gameObject);
                    MuliplayerData currentPlayerData = MonitorTrainerConsts.MULIPLYER_DATA[MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum];
                    ball.transform.position = pos;
                    Rigidbody body = ball.GetComponent<Rigidbody>();
                    body.AddRelativeTorque(Random.insideUnitSphere * TORQUE_BEACHBALL_MULTIPLYER, ForceMode.Impulse);

                    // first bounce lesson it 
                    SphereCollider collider = body.GetComponentInChildren<SphereCollider>();
                    PhysicMaterial bouncy = collider.material;
                    collider.material = PhysicMaterials.JengaPhysicMaterial;
                    VelocityImpact velImpact = ball.AddComponent<VelocityImpact>();
                    velImpact.CallbackMagnitude((amount) =>
                    {
                        // put realbounce back
                        collider.material = bouncy;
                        UnityEngine.Object.Destroy(velImpact);
                    });

                });
            }

            void SpecialTasksHitBoxStart(MusicianRequestData musicianRequestData)
            {
                if (MonitorTrainerRoot.Instance.PlayerChoiceDataRef.CurrentPlayersEnum.IsMainSide() == true)
                {
                    musicianRequestData.HitBox.transform.position = new Vector3(-13, 0, -5);
                }
                else
                {
                    musicianRequestData.HitBox.transform.position = new Vector3(13, 0, -5);
                }

                musicianRequestData.HitBox.SetBox(HitBox.TriggerType.OnTriggerEnter, new Vector3(2, 10, 20));

                SetTriggerFor(BEACHBALL_CATALOGUE_GUID, musicianRequestData, false);
            }

            bool BeachBallOfPlayArea() => item.m_IsCompleted;
        }


        private MusicianRequestData GuitarStringBroken()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.RhythmGuitar;
            item.m_Description = "The Guitarist Has lost a string. Get a new guitar out to him... Ummmm Just toss it!";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_CanAutoRemove = true;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_OnStart = GuitarHitBoxEnable;
            item.m_OnComplete = GuitarToCharacter;
            item.m_OnSpecialTasksHitBox = GuitarHitBoxEnable;
            item.m_CompleteIfTrue = GuitarLandOnStage;
            item.m_LifespanSeconds = 30;
            return item;

            bool GuitarLandOnStage() => item.m_IsCompleted;

            void GuitarHitBoxEnable(MusicianRequestData musicianRequestData)
            {
                ConsoleData.Instance.SetForcedMuteTasks(MusicianTypeEnum.RhythmGuitar, true);
                item.HitBox.transform.position = new Vector3(0, 0, -4);
                item.HitBox.SetRadius(10f);

                SetTriggerFor(GUITAR_CATALOGUE_GUID, musicianRequestData, true);
                Debug.LogError($"character throws guitar away");
            }

            void GuitarToCharacter(MusicianRequestData musicianRequestData)
            {
                Debug.LogError($"lerp guitar to correct charater");
                ConsoleData.Instance.SetForcedMuteTasks(MusicianTypeEnum.RhythmGuitar, false);
            }
        }



        private MusicianRequestData FoodWanted()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.RhythmGuitar;
            item.m_Description = $"FoodWanted";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_CanAutoRemove = true;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_OnSpecialTasksHitBox = FoodHitBoxEnable;
            item.m_CompleteIfTrue = FoodLandOnStage;
            item.m_LifespanSeconds = 30;
            return item;

            bool FoodLandOnStage() => item.m_IsCompleted;

            void FoodHitBoxEnable(MusicianRequestData musicianRequestData)
            {
                item.HitBox.transform.position = new Vector3(0, 0, -4);
                item.HitBox.SetRadius(10f);

                SetTriggerFor(MetaDataType.ContentFood, musicianRequestData, true);
            }
        }


        private MusicianRequestData MicrophoneWanted()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.Vocals;
            item.m_Description = $"MicrophoneWanted";
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_CanAutoRemove = true;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_OnStart = MicrophoneBoxEnable;
            item.m_OnComplete = MicrophoneToCharacter;
            item.m_CompleteIfTrue = MicrophoneOnStage;
            item.m_LifespanSeconds = 30;
            return item;

            bool MicrophoneOnStage() => item.m_IsCompleted;

            void MicrophoneBoxEnable(MusicianRequestData musicianRequestData)
            {
                ConsoleData.Instance.SetForcedMuteTasks(MusicianTypeEnum.Vocals, true);

                item.HitBox.transform.position = new Vector3(0, 0, -4);
                item.HitBox.SetRadius(10f);

                SetTriggerFor(MICROPHONE_CATALOGUE_GUID, musicianRequestData, true);
            }

            void MicrophoneToCharacter(MusicianRequestData musicianRequestData)
            {
                Debug.LogError($"lerp guitar to correct charater");
                ConsoleData.Instance.SetForcedMuteTasks(MusicianTypeEnum.Vocals, false);
            }
        }



        private MusicianRequestData DrinkWantedRhythmGuitar()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.RhythmGuitar;

            var drums = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == AuxEnum.RhythmGuitar);
            int index = UnityEngine.Random.Range(0, 2);
            switch (index)
            {
                case 0:
                    item.m_Description = "(Bandmember) is gasping for a drink... Hurl something their way will ya!";
                    break;
                case 1:
                    item.m_Description = "The drummer dropped one of his drumsticks. Throw him one back will ya!";
                    break;
            }
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_OnSpecialTasksHitBox = DrinkHitBoxEnable;
            item.m_CompleteIfTrue = DrinkLandOnStage;
            item.m_LifespanSeconds = 30;
            return item;

            bool DrinkLandOnStage() => item.m_IsCompleted;

            void DrinkHitBoxEnable(MusicianRequestData musicianRequestData)
            {
                var rhythmGuitar = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == AuxEnum.RhythmGuitar);

                item.HitBox.transform.position = rhythmGuitar.Position;
                item.HitBox.SetRadius(10f);
                SetTriggerFor(MetaDataType.ContentDrink, musicianRequestData, true);
            }
        }

        private MusicianRequestData LostDrumStick()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.Drums;
            int index = UnityEngine.Random.Range(0, 2);
            switch (index)
            {
                case 0:
                    item.m_Description = "The drummer got a little carried away during his solo and lost both his drumsticks. Throw him a new pair will you!";
                    break;
                case 1:
                    item.m_Description = "The drummer dropped one of his drumsticks. Throw him one back will ya!";
                    break;
            }

            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_OnSpecialTasksHitBox = StickHitBoxEnable;
            item.m_OnComplete = DrumStickToCharacter;
            item.m_CompleteIfTrue = StickLandOnStage;
            item.m_LifespanSeconds = 30;
            return item;

            bool StickLandOnStage() => item.m_IsCompleted;

            void StickHitBoxEnable(MusicianRequestData musicianRequestData)
            {
                ConsoleData.Instance.SetForcedMuteTasks(MusicianTypeEnum.Drums, true);
                var drums = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == AuxEnum.Drums);

                item.HitBox.transform.position = drums.Position;
                item.HitBox.SetRadius(5f);

                SetTriggerFor(DRUM_STICK_CATALOGUE_GUID, musicianRequestData, true);
            }

            void DrumStickToCharacter(MusicianRequestData musicianRequestData)
            {
                Debug.LogError($"lerp guitar to correct charater");
                ConsoleData.Instance.SetForcedMuteTasks(MusicianTypeEnum.Drums, false);
            }
        }


        private MusicianRequestData LightingPowerCut()
        {
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;

            int index = UnityEngine.Random.Range(0, 3);
            switch (index)
            {
                case 0:
                    item.m_Description = "And now the powers gone... this show is a disaster... Just... Just get the power back on if you can!";
                    break;
                case 1:
                    item.m_Description = "Where have the lights gone! I give up...  Flip the power back on, Should be over your shoulder";
                    break;
                case 2:
                    item.m_Description = "Quick get the lights back on! The power should be located just behind you!";
                    break;
            }


            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_Difficulty = DifficultyModeEnum.Easy;
            item.m_OnStart = AssignLever;
            item.m_OnComplete = LeverPulled;
            item.m_CompleteIfTrue = PowerLeverPulled;
            item.m_LifespanSeconds = 30;
            return item;

            bool PowerLeverPulled() => item.m_IsCompleted;

            void AssignLever(MusicianRequestData data)
            {
                LightsEnabled(false);
                var levers = Core.Scene.GetSpawnedVrInteractionGUID(LIGHT_POWER_SWITCH_CATALOGUE_GUID);
                foreach (var item in levers)
                {
                    var slider = item.GetVrInteractionFromRoot(MetaDataType.ContentHinge)[0];
                    slider.ClearAddCallback((amount) =>
                    {
                        if (amount > 0.75f)
                        {
                            data.m_IsCompleted = true;
                        }
                    });
                }
            }

            void LeverPulled(MusicianRequestData data) => LightsEnabled(true);

            void LightsEnabled(bool enable)
            {
                var scene = GetSceneEndedWith("_Stage");
                var rootObjects = scene.GetRootGameObjects().ToList();
                var spotlights = rootObjects.Find(e => e.name == "Spotlights");
                if (spotlights != null)
                {
                    spotlights.SetActive(enable);
                }

                var lighting = rootObjects.Find(e => e.name == "Lighting");
                if (lighting != null)
                {
                    lighting.SetActive(enable);
                }
            }
        }

        private MusicianRequestData BinFire()
        {
            string FIRE = "FIRE";
            var item = new MusicianRequestData();
            item.m_CreatedObjects.Add(FIRE, null);
            item.m_MainMusicianType = MusicianTypeEnum.FrontOfHouse;
            int index = UnityEngine.Random.Range(0, 2);
            switch (index)
            {
                case 0:
                    item.m_Description = "You're on fire!!!! Get it out! Get it out!";
                    break;
                case 1:
                    item.m_Description = "Just to let you know... your booth is on fire. Grab an extinguisher!";
                    break;
            }
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_RequestType = RequestTypeEnum.SpecialForAll;
            item.m_Difficulty = DifficultyModeEnum.Easy;

            item.m_OnSpecialTasksHitBox = TaskHitBox;
            item.m_OnComplete = StopBinFire;
            item.m_CompleteIfTrue = BinFireOut;
            item.m_LifespanSeconds = 30;
            item.m_DelayStartTime = 3f;
            item.m_OnDelayStart = SparksAndFire;
            return item;

            bool BinFireOut() => item.m_IsCompleted && item.m_VrInteraction.ActorNickNameTouched == Core.PhotonMultiplayerRef.MySelf.NickName;

            void SparksAndFire(MusicianRequestData musicianRequestData)
            {
                Core.AssetsLocalRef.VisualEffectLocalRef.GetItemInstantiated("VFX_Electric Sparks", (createdVfx) =>
                {
                    m_TaskCreatedItems.Add(createdVfx);
                    var levers = Core.Scene.GetSpawnedVrInteractionGUID(LIGHT_POWER_SWITCH_CATALOGUE_GUID);
                    var activeLever = levers.Find(e => e.enabled == true);

                    var pivotPoint = activeLever.GetVrInteractionFromRoot(MetaDataType.ContentHinge)[0];

                    createdVfx.transform.position = pivotPoint.transform.position;
                    var sparks = createdVfx.GetComponent<VisualEffect>();
                    sparks.Reinit();

                    Core.Mono.WaitFor(1f, () => StartBinFire(musicianRequestData));

                    Core.Mono.WaitFor(3f, () => UnityEngine.Object.Destroy(createdVfx));
                });
            }

            void StartBinFire(MusicianRequestData musicianRequestData)
            {
                var bins = Core.Scene.GetAllSpawnedVrInteraction().FindAll(e => e.CatalogueEntryRef.Guid == BIN_CATALOGUE_GUID);
                var activeBin = bins.Find(e => e.enabled == true);
                musicianRequestData.HitBox.transform.position = activeBin.transform.position;
                musicianRequestData.HitBox.SetRadius(0.5f, 1.5f);
                musicianRequestData.HitBox.UseRayCastHitTimer(typeof(FireExtinguisher), timer: 3f, musicianRequestData);

                Core.AssetsLocalRef.VisualEffectLocalRef.GetItemInstantiated("VFX_Console Fire", (createdItem) =>
                {
                    m_TaskCreatedItems.Add(createdItem);
                    musicianRequestData.m_CreatedObjects[FIRE] = createdItem;
                    var vfx = createdItem.GetComponent<VisualEffect>();
                    vfx.Reinit();
                    vfx.transform.position = musicianRequestData.HitBox.transform.position;
                    vfx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                });
            }


            void TaskHitBox(MusicianRequestData musicianRequestData)
            {
                var bins = Core.Scene.GetAllSpawnedVrInteraction().FindAll(e => e.CatalogueEntryRef.Guid == BIN_CATALOGUE_GUID);
                var activeBin = bins.Find(e => e.enabled == true);
                musicianRequestData.HitBox.transform.position = activeBin.transform.position;
                musicianRequestData.HitBox.SetRadius(0.5f, 1.5f);
                musicianRequestData.HitBox.UseRayCastHitTimer(typeof(FireExtinguisher), timer: 3f, musicianRequestData);
            }

            void StopBinFire(MusicianRequestData musicianRequestData)
            {
                VisualEffect vfx = musicianRequestData.m_CreatedObjects[FIRE].GetComponent<VisualEffect>();
                if (vfx != null)
                {
                    vfx.Stop();
                    Core.Mono.WaitFor(3f, () =>
                    {
                        if (vfx != null && vfx.gameObject != null)
                        {
                            UnityEngine.Object.Destroy(vfx.gameObject);
                        }
                    });
                }
            }
        }


        #region Helpers

        private UnityEngine.SceneManagement.Scene GetSceneEndedWith(string end)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name.EndsWith(end))
                {
                    return scene;
                }
            }
            return UnityEngine.SceneManagement.SceneManager.GetSceneAt(0);
        }

        private void SetTriggerFor(string guid, MusicianRequestData data, bool useHightlight)
        {
            List<string> allItems = new List<string>();
            allItems.Add(guid);
            SetTriggerFor(allItems, data, useHightlight);
        }

        private void SetTriggerFor(List<string> guidList, MusicianRequestData data, bool useHightlight)
        {
            var allSpawned = Core.Scene.GetAllSpawnedVrInteraction();
            List<VrInteraction> allItems = new List<VrInteraction>();
            foreach (var item in guidList)
            {
                var partItems = allSpawned.FindAll(e => e.CatalogueEntryRef.Guid == item);
                allItems.AddRange(partItems);
                if (partItems.Count == 0)
                {
                    Debug.LogError($"Cannot find any guid {item}");
                }
            }
            data.HitBox.SetOnTriggerFor(allItems, data, useHightlight);
        }

        private void SetTriggerFor(MetaDataType metaDataType, MusicianRequestData data, bool useHightlight)
        {
            List<VrInteraction> allItems = new List<VrInteraction>();

            var allVR = Core.Scene.GetAllSpawnedVrInteraction();
            foreach (var item in allVR)
            {
                var meta = item.GetMetaDataTypeFromRoot(metaDataType);
                if (meta.Count > 0)
                {
                    allItems.Add(item);
                }
            }

            data.HitBox.SetOnTriggerFor(allItems, data, useHightlight);
        }

        #endregion Helpers
    }
}
