using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MonitorTrainer.AuxEnumExtension;
using static MonitorTrainer.PlayersEnumExtension;
using static MonitorTrainer.MonitorTrainerConsts;
using static VrInteractionBaseButton;

namespace MonitorTrainer
{
    public static class AuxEnumExtension
    {
        public class AuxEnumValidAttribute : System.Attribute
        {
            public AuxEnumValidAttribute(bool musician, bool isBlank)
            {
                IsMusician = musician;
                IsBlank = isBlank;
            }

            public bool IsMusician { get; private set; }
            public bool IsBlank { get; private set; }
        }

        public static bool IsMusician(this AuxEnum material)
        {
            var attribute = material.GetType().GetMember(material.ToString()).First().GetCustomAttributes(typeof(AuxEnumValidAttribute), false).FirstOrDefault();
            return attribute != null && ((AuxEnumValidAttribute)attribute).IsMusician;
        }
        public static bool IsBlank(this AuxEnum material)
        {
            var attribute = material.GetType().GetMember(material.ToString()).First().GetCustomAttributes(typeof(AuxEnumValidAttribute), false).FirstOrDefault();
            return attribute != null && ((AuxEnumValidAttribute)attribute).IsBlank;
        }
    }


    public static class PlayersEnumExtension
    {
        public class PlayersEnumValidAttribute : System.Attribute
        {
            public PlayersEnumValidAttribute(bool mainSide)
            {
                IsMainSide = mainSide;
            }
            public bool IsMainSide { get; private set; }
        }

        public static bool IsMainSide(this PlayersEnum player)
        {
            var attribute = player.GetType().GetMember(player.ToString()).First().GetCustomAttributes(typeof(PlayersEnumValidAttribute), false).FirstOrDefault();
            return attribute != null && ((PlayersEnumValidAttribute)attribute).IsMainSide;
        }
    }


    public class MonitorTrainerConsts
    {
        // search for this  string
        // MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Count
        // InitialiseSong
        public const string TAG = "[MonitorTrainer]";
        public const float DROP_VOLUME = 1f;// 50%;
        public const int MAX_PLAYERS = 4;
        public const int MOBILE_PHONE_MESSAGE_DURATION = 3;
        public const int ACHIEVEMENT_MIN_TEST_TIME = 1;
        public const int ACHIEVEMENT_MIN_COMPLETED_TIME = 1;
        public const int TASKBAR_MAX_SLOTS = 7;
        public const int TASK_ICON_WIDTH = 512;
        public const float TASK_ENTRY_TIME = 1.2f;
        public const float TASK_RESHUFFLE = 0.9f;
        public static readonly Vector3 TASK_SPAWN_POS = new Vector3(4130f, 256f, 0f);
        public static readonly Color TASK_TINT = new Color(0f, 0f, 0f, 0.66f);
        public static readonly Color TASK_SLOT_HIGHLIGHT = new Color(0.282353f, 0.572549f, 0.7019608f, 1f);
        public static readonly Color TASK_SLOT_NORMAL = new Color(0.1058824f, 0.2745098f, 0.3490196f, 1f);
        public static readonly float LERP_PERCENTAGE_TIMMING = 1f;
        public const string MASTER_CHANNEL = "Master";
        public const string MASTER_VOX = "MuteVox";
        public const string MASTER_GUITAR = "MuteGtr";
        public const string MASTER_DRUM = "MuteDrum";
        public const string MASTER_CONTROL = "Control";
        public const float SPEAKER_DECIBEL_LEVEL = -20;
        public const int CHANNEL_GROUP_SIZE = 4;
        public static readonly int TOTAL_CHANNELS = CHANNEL_GROUP_SIZE * System.Enum.GetNames(typeof(ChannelGroupEnum)).Length;
        public static readonly int TOTAL_PHYSICAL_SLIDERS = System.Enum.GetNames(typeof(ConsoleSliderGroupEnum)).Length;
        public static readonly int PHYSICAL_PERSON_SLIDERS = TOTAL_PHYSICAL_SLIDERS - 2;
        public const float DECIBEL_MAX = 10;
        public const float DECIBEL_MIN = -80;
        public const float INITIALISE_COROUTINE = 0.1f;
        public const string TXT_EXTENSION = "_Txt";
        public const float MIN_TIME_ADD_ACHIEVEMENT_ADDED = 2f;
        public const float SCENARIO_PRELUDE_TIMING = 5f;
        public const float BLACKOUT_TIMING = 1f;
        public const float DELAY_TO_STOP_SONG_SYNC_ISSUES = 0.5f;

        public const string SONG_DATA_JSON = "SongData_";

        public static readonly Vector2 MIN_MAX_RANDOM_PERCENTAGE_START = new Vector2(0.4f, 0.6f);
        public static readonly Vector2 MIN_MAX_RANDOM_DB_START = new Vector2(-10f, 2f);

        public static readonly float GAIN_MIN = 0.2f;
        public static readonly float GAIN_MAX =1f;

        public enum PlayerPos
        {
            Menu, 
            PlayArea,
        }

        public class MuliplayerData
        {
            public string RootName = "InteractiveHolder_Player1And2";
            public string ConsoleName;
            public string PhoneName;
            public string AmpName;
            public string USbName;
            public string TeleportName;
            public Vector3 StartPosition;
            public Vector3 GUIStartPosition = new Vector3(-23.39283f, 1.178105f, 1.882788f);
            public Quaternion GUIStartRotation = Quaternion.Euler(0f, 90.16979f, 0f);
            public Vector3 RootGameobjectPosition;
            public Quaternion RootGameobjectRotation;
            public List<UnityEngine.Rendering.Universal.DecalProjector> DecalList;

            public readonly string GuiLaptopHolderName = "LaptopGUIHolder";
            public  Vector3 GUILapTopPosition = new Vector3(-23.39283f, 1.178105f, 1.882788f);
            public  Quaternion GUILapTopRotation = Quaternion.Euler(0f, 90.16979f, 0f);

            public MuliplayerData(PlayersEnum playerType, Vector3 pos, bool opposite)
            {
                string extra = "_ONE";
                if (playerType == PlayersEnum.Player2 || playerType == PlayersEnum.Player4)
                {
                    extra = "_TWO";
                }
                ConsoleName = "Quest_MonitorTrainer_Console_Prefab_Prefab_Player" + extra;

                ///////////////////////////////////////////////////////////////////


                extra = "";
                if (playerType == PlayersEnum.Player1 || playerType == PlayersEnum.Player3)
                {
                    GUILapTopPosition = new Vector3(1.982f, -1.14f, 22.321f);
                    GUILapTopRotation = Quaternion.Euler(0f, 120.1758f, 0f);
                }
                else
                {
                    extra = " (1)";
                    GUILapTopPosition = new Vector3(1.993f, -1.135f, 22.405f);
                    GUILapTopRotation = Quaternion.Euler(0f, 120.1758f, 0f);
                }

                PhoneName = "Quest_Phone_Prefab" + extra;
                AmpName = "Quest_AmpPowerUnit_Prefab" + extra;
                USbName = "Quest_USB_Prefab" + extra;
                TeleportName = "Teleport" + extra;
                StartPosition = pos;

                RootGameobjectPosition = Vector3.zero;
                RootGameobjectRotation = Quaternion.identity;
                RootName = "InteractiveHolder_Player1And2";
                if(opposite == true)
                {
                    RootName = "InteractiveHolder_Player3And4";
                }

            }
        }

        public static readonly Dictionary<PlayersEnum, MuliplayerData> MULIPLYER_DATA = new Dictionary<PlayersEnum, MuliplayerData>()
        {
            {PlayersEnum.Player1,new MuliplayerData(PlayersEnum.Player1, new Vector3(-9.452f, 1f, -2.46f), false)},
            {PlayersEnum.Player2,new MuliplayerData(PlayersEnum.Player2, new Vector3(-9.452f, 1f, -5.915f), false)},
            {PlayersEnum.Player3,new MuliplayerData(PlayersEnum.Player3, new Vector3(9.452f, 1f, -5.915f), true)},
            {PlayersEnum.Player4,new MuliplayerData(PlayersEnum.Player4, new Vector3(9.452f, 1f, -2.46f), true)},
        };

        private static ButtonStateEnum MuteSoloToEnum(bool value) => (value == true) ? ButtonStateEnum.Down : ButtonStateEnum.Up;
        private static bool MuteSoloEnumToBool(ButtonStateEnum value) => (value == ButtonStateEnum.Down) ? true : false;

        public class GroupData
        {
            public float Slider { get; private set; }
            public ButtonStateEnum MuteEnum { get; private set; }
            public ButtonStateEnum SoloEnum { get; private set; }
            public bool Mute => MuteSoloEnumToBool(MuteEnum);
            public bool Solo => MuteSoloEnumToBool(SoloEnum);

            public GroupData(float slider, ButtonStateEnum mute, ButtonStateEnum solo)
            {
                Slider = slider;
                MuteEnum = mute;
                SoloEnum = solo;
            }

            public GroupData(float slider, bool mute, bool solo)
            {
                Slider = slider;
                MuteEnum = MuteSoloToEnum(mute);
                SoloEnum = MuteSoloToEnum(solo);
            }
        }

        public enum RatingEnum
        {
            Amazing,
            Happy,
            Neutral,
            Sad,
            Angry,
        }

        public enum DifficultyModeEnum
        {
            Easy = 50,
            Medium = 75,
            Hard = 100,
            Global = 150, 
        }



        public enum PlayStyleEnum
        {
            Solo,
            MultiplayerHost,
            MultiplayerGuest,
        }

        public enum LeaderboardEnum
        {
            Global,
            Friend,
        }

        public enum InputAuxTypeEnum
        {
            Input,
            Aux,
        }

        public enum TeamEnum
        {
            Team1,
            Team2,
            None,
        }

        public enum PlayersEnum
        {
            [PlayersEnumValid(true)] Player1,
            [PlayersEnumValid(true)] Player2,
            [PlayersEnumValid(false)] Player3,
            [PlayersEnumValid(false)] Player4,
        }

        public enum ScenarioEnum
        {
            Blank,
            Menu,
            TutorialPart1,
            TutorialPart2,
            Stackable,
            SongFinishedCompleted,
        }


        public enum MusicianTypeEnum
        {
            // these up to Bass , cannot be moved or changed, other wise linakges to the amp will break
            Bass,
            LeadGuitar,
            Vocals,
            Synth1,
            Synth2,
            RhythmGuitar,
            Drums,

            StageHand,
            StageManager,
            FrontOfHouse,
        }
        
        public enum SliderStateEnum
        {
            Up,
            Down,
        }


        public enum AuxEnum
        {
            [AuxEnumValid(true, false)] Bass = MusicianTypeEnum.Bass,                  // MonitorTrainerAmp: Control1
            [AuxEnumValid(true, false)] LeadGuitar = MusicianTypeEnum.LeadGuitar,      // MonitorTrainerAmp: Control2
            [AuxEnumValid(true, false)] Vocals = MusicianTypeEnum.Vocals,              // MonitorTrainerAmp: Control3
            [AuxEnumValid(true, false)] Synth1 = MusicianTypeEnum.Synth1,              // MonitorTrainerAmp: Control4
            [AuxEnumValid(true, false)] Synth2 = MusicianTypeEnum.Synth2,              // MonitorTrainerAmp: Control5
            [AuxEnumValid(true, false)] RhythmGuitar = MusicianTypeEnum.RhythmGuitar,  // MonitorTrainerAmp: Control6
            [AuxEnumValid(true, false)] Drums = MusicianTypeEnum.Drums,                // MonitorTrainerAmp: Control7

            [AuxEnumValid(false, false)] StageHand = MusicianTypeEnum.StageHand,
            [AuxEnumValid(false, false)] StageManager = MusicianTypeEnum.StageManager,
            [AuxEnumValid(false, false)] FrontOfHouse = MusicianTypeEnum.FrontOfHouse,
            [AuxEnumValid(false, true)] Blank1,
            [AuxEnumValid(false, true)] Blank2,
            [AuxEnumValid(false, true)] Blank3,
        }

        public enum ChannelGroupEnum
        {
            Channel1_4,
            Channel5_8,
            Channel9_12,
            Channel13_16,
            Channel17_20,
            Channel21_24,
        }

        public enum ConsoleSliderGroupEnum
        {
            Slider1,
            Slider2,
            Slider3,
            Slider4,
            SliderVox,
            SliderGTR,
        }

        public enum ConsoleButtonEnum
        {
            Power,

            Solo1,
            Solo2,
            Solo3,
            Solo4,
            Solo5,
            Solo6,

            Mute1,
            Mute2,
            Mute3,
            Mute4,
            Mute5,
            Mute6,

            MuteGroupAll,
            MuteGroupVox,
            MuteGroupGuitar,
            MuteGroupDrum,
        }

        public class GuestSwap
        {
            [SerializeField]
            public int ActorNumber = 0;
        }

        public class TaskSpecialForAllTimings
        {
            public float Time;
            public int SpecialTaskIndex;
        }

        public class TaskGenericTimings
        {
            public float Time;
            public DifficultyModeEnum Difficulty = DifficultyModeEnum.Easy;
        }

        public class DificultySettings
        {
            public Dictionary<DifficultyModeEnum, float> TasksPerMin = new Dictionary<DifficultyModeEnum, float>();
            public Dictionary<DifficultyModeEnum, float> MinTimeBetweenTasks = new Dictionary<DifficultyModeEnum, float>();
            public Dictionary<DifficultyModeEnum, float> MaxTimeBetweenTasks = new Dictionary<DifficultyModeEnum, float>();
            public Dictionary<DifficultyModeEnum, float> TimeAtMaxScore = new Dictionary<DifficultyModeEnum, float>();
            public Dictionary<DifficultyModeEnum, float> TimeAtMinScore = new Dictionary<DifficultyModeEnum, float>();
            public Dictionary<DifficultyModeEnum, float> MaxScore = new Dictionary<DifficultyModeEnum, float>();
        }

        public class SongData
        {
            public string SongName;
            public string ArtistName;
            public string BandInfo ="";
            public float SongLength;
            public float NoTasksForStart;
            public float NoTasksForLast;
            public string AmazonLink;
            public string YouTubeLink;
            public string SpotifyLink;
            public string AppleLink;
            public List<CharacterDataClass> BandMembers = new List<CharacterDataClass>();
        };

        [SerializeField]
        public class PlayerChoiceData
        {
            public string CurrentSongChoiceName  = "Better Of Alone";
            public DifficultyModeEnum CurrentDifficulty = DifficultyModeEnum.Medium;
            public PlayStyleEnum PlayStyle  = PlayStyleEnum.Solo;
            public string LobbyCode;
            public bool PrivateLobby = false;

            public PlayersEnum CurrentPlayersEnum = PlayersEnum.Player1;

            [System.NonSerialized]
            public SongData SongDataRef = null;
        }


        [SerializeField]
        public class LocalPlayerData
        {
            public const string PLAYER_PREFS_DATA = "MonitorTrainer_PLAYER_PREFS_DATA";
            public const string MONITOR_LOOKUP = "Console";
            public const string FLIGHTCASE_LOOKUP = "Case";
            public const string LAPTOP_LOOKUP = "LapTop";
            public const string LAMP_LOOKUP = "Lamp";

            public class SaveDataClass
            {

                [SerializeField]
                public float MasterVolume = 1f;
                [SerializeField]
                public float ChatVolume = 1f;
                [SerializeField]
                public float MusicVolume = 1f;

                public SendData SendDataRef = new SendData();
                public class SendData
                {
                    [SerializeField]
                    public Color PrimaryColor = Color.white;
                    [SerializeField]
                    public Color SecondaryColor = Color.white;
                    [SerializeField]
                    public string Monitor = "";
                    [SerializeField]
                    public string FlightCase = "";
                    [SerializeField]
                    public string LapTop = "";
                    [SerializeField]
                    public string Lamp = "";

                    [SerializeField]
                    public string LobbyCode = "";

                    [SerializeField]
                    public int ActorNumber;

                    [SerializeField]
                    public PlayersEnum CurrentPlayersEnum = PlayersEnum.Player1;

                    [SerializeField]
                    public PlayersEnum? CurrentPlayersMenuPlaceEnum = null;

                    [SerializeField]
                    public TeamEnum CurrentTeamEnum = TeamEnum.None;

                    [SerializeField]
                    public string Nickname;

                    [SerializeField]
                    public int XP;

                    [SerializeField]
                    public int TasksCompleted;
                }
            }

            public SaveDataClass SaveDataRef = new SaveDataClass();
            public void ReadData()
            {
                var data = Json.JsonNet.WriteToText<SaveDataClass>(SaveDataRef, true);
                string loadedData = PlayerPrefs.GetString(PLAYER_PREFS_DATA, data);
                try
                {
                    SaveDataRef = Json.JsonNet.ReadFromText<SaveDataClass>(loadedData);
                }
                catch (System.Exception)
                {
                    // this should only happen if change a varaibel assignment , float to int for example
                    PlayerPrefs.DeleteKey(PLAYER_PREFS_DATA);
                    SaveDataRef = new SaveDataClass();
                }

            }

            public void SaveData()
            {
                var data = Json.JsonNet.WriteToText<SaveDataClass>(SaveDataRef, true);
                PlayerPrefs.SetString(PLAYER_PREFS_DATA, data);
            } 
        }

        public class ConsoleButtonState
        {
            public ConsoleButtonEnum m_ConsoleButtonEnum;
            public ButtonStateEnum m_ButtonStateEnum;

            public ConsoleButtonState(ConsoleButtonEnum consoleButtonEnum, ButtonStateEnum buttonStateEnum)
            {
                m_ConsoleButtonEnum = consoleButtonEnum;
                m_ButtonStateEnum = buttonStateEnum;
            }
        }

        public class ConsoleSliderState
        {
            public ConsoleSliderGroupEnum m_ConsoleSliderGroupEnum;
            public SliderStateEnum m_SliderStateEnum;

            public ConsoleSliderState(ConsoleSliderGroupEnum sliderEnum, SliderStateEnum sliderState)
            {
                m_ConsoleSliderGroupEnum = sliderEnum;
                m_SliderStateEnum = sliderState;
            }
        }


        abstract public class MusicianRequestBase
        {           
            public InputAuxTypeEnum InputAuxType { get; private set; }

            public float? StartingDecibels { get; set; }
            public float? StartingGain { get; set; }
            public float? StartingTrim { get; set; }
            public MusicianTypeEnum? MusicianType { get; protected set; }
            public SliderStateEnum? SliderState { get; protected set; }
            public SliderStateEnum? GainState { get; protected set; }
            public SliderStateEnum? TrimState { get; protected set; }
            public bool? Trim { get; protected set; }
            public bool? Gain { get; protected set; }
            public bool? Mute { get; protected set; }
            public bool? Solo { get; protected set; }
            public bool? PhantomPower { get; protected set; }
            public bool? Phase { get; protected set; }
            public float? TimeDelay { get; protected set; }
            public MusicianRequestBase(InputAuxTypeEnum inputAuxType)
            {
                InputAuxType = inputAuxType;
                StartingDecibels = null;
                StartingGain = null;
                MusicianType = null;
                SliderState = null;
                GainState = null;
                TrimState = null;
                Trim = null;
                Gain = null;
                Mute = null;
                Solo = null;
                PhantomPower = null;
                Phase = null;
                TimeDelay = null;
            }
        }

        public class MusicianRequestTimeDelay : MusicianRequestBase
        {
            public MusicianRequestTimeDelay(MusicianTypeEnum musicianType, float timeDelay) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                TimeDelay = timeDelay;
            }
        }

        public class MusicianRequestDBInput : MusicianRequestBase
        {
            public MusicianRequestDBInput(MusicianTypeEnum musicianType, SliderStateEnum sliderState, bool mute, bool solo, bool phantomPower, bool phase) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                SliderState = sliderState;
                Mute = mute;
                Solo = solo;
                PhantomPower = phantomPower;
                Phase = phase;
            }
        }

        public class MusicianRequestInput : MusicianRequestBase
        {
            public MusicianRequestInput(MusicianTypeEnum musicianType, bool mute, bool solo, bool phantomPower, bool phase) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                Mute = mute;
                Solo = solo;
                PhantomPower = phantomPower;
                Phase = phase;
            }
        }

        public enum MusicianRequestBaseType
        {
            Mute, 
            Slider, 
            Phontom, 
            Phase,
            Gain, 
            AuxSlider, 
            Aux,
        }
        public class MusicianRequestInputMute : MusicianRequestBase
        {
            public MusicianRequestInputMute(MusicianTypeEnum musicianType, bool mute) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                Mute = mute;
            }
        }

        public class MusicianRequestInputSlider : MusicianRequestBase
        {
            public MusicianRequestInputSlider(MusicianTypeEnum musicianType, SliderStateEnum sliderState) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                SliderState = sliderState;              
            }
        }

        public class MusicianRequestPhantomPowerInput : MusicianRequestBase
        {
            public MusicianRequestPhantomPowerInput(MusicianTypeEnum musicianType, bool mute, bool phantomPower) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                Mute = mute;
                PhantomPower = phantomPower;
            }
        }

        public class MusicianRequestPhaseInput : MusicianRequestBase
        {
            public MusicianRequestPhaseInput(MusicianTypeEnum musicianType, bool mute, bool phase) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                Mute = mute;
                Phase = phase;
            }
        }

        public class MusicianRequestGainInput : MusicianRequestBase
        {
            public MusicianRequestGainInput(MusicianTypeEnum musicianType, SliderStateEnum gainState) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                GainState = gainState;
            }
        }

        public class MusicianRequestTrimInput : MusicianRequestBase
        {
            public MusicianRequestTrimInput(MusicianTypeEnum musicianType, SliderStateEnum trimState) : base(InputAuxTypeEnum.Input)
            {
                MusicianType = musicianType;
                TrimState = trimState;
            }
        }

        public class MusicianRequestAuxSlider : MusicianRequestBase
        {
            public MusicianRequestAuxSlider(MusicianTypeEnum musicianType, SliderStateEnum sliderState) : base(InputAuxTypeEnum.Aux)
            {
                MusicianType = musicianType;
                SliderState = sliderState;
            }
        }

        public class MusicianRequestAux : MusicianRequestBase
        {
            public MusicianRequestAux(MusicianTypeEnum musicianType, SliderStateEnum sliderState, bool mute, bool solo) : base(InputAuxTypeEnum.Aux)
            {
                MusicianType = musicianType;
                SliderState = sliderState;
                Mute = mute;
                Solo = solo;
            }
        }


        public class TrackDataMusician
        {
            public bool m_PhantomPower40V = false;
            public bool m_Phase = false;
            public float m_Trim = GAIN_MIN;
            public float m_Gain = GAIN_MIN;
            public bool m_Mute = false;
            public bool m_Solo = false;
            public ButtonStateEnum m_MuteState => MuteSoloToEnum(m_Mute);
            public ButtonStateEnum m_SoloState => MuteSoloToEnum(m_Solo);
            private float m_SliderPercentageLevel = 0;
            public AuxEnum m_AuxEnum;

            public TrackDataMusician(AuxEnum aux)
            {
                m_AuxEnum = aux;
            }

            public float GetPercentageLevel() => m_SliderPercentageLevel;
            public void SetPercentageLevel(float percentage) => m_SliderPercentageLevel = percentage;
            public float GetDBLevel() => PercentageToDecibel(m_SliderPercentageLevel);

        }

        [SerializeField]
        public class CharacterDataClass
        {
            public  string CharacterName = "Joe Bloggs";
            public  string Abbreviation = "Joe";
            public Color Colour = new Color(22, 22, 22);
            public string ColourName = "Pink";
            public AuxEnum AuxEnum = AuxEnum.Bass;
            public int InputIndex = 1;

            public string MusicianModel = "ARNIE";
            public Vector3 Position;
            public Vector3 Rotation;


            public CharacterDataClass()
            { }
            public CharacterDataClass(AuxEnum auxEnum, string abbreviation, int inputIndex, string name, Color color, string colourName)
            {
                this.Abbreviation = abbreviation;
                this.CharacterName = name;
                this.Colour = color;
                this.ColourName = colourName;
                this.AuxEnum = auxEnum;
                this.InputIndex = inputIndex;
            }
        }


        public static CharacterDataClass GetCharacterData(AuxEnum aux) => MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.FindLast(e => e.AuxEnum == aux);
        public static CharacterDataClass GetCharacterData(MusicianTypeEnum musicianType) => MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.FindLast(e => e.AuxEnum == (AuxEnum)musicianType);


        /* Colours from Adam
        Blue - 30, 170, 235
        Mint - 30, 235, 170
        Green - 30, 235, 30
        Indigo - 100, 30, 235
        Purple - 160, 70, 235
        Pink - 255, 90, 235
        Red - 245, 60, 60
        Orange - 245, 125, 60
        Yellow - 245, 235, 30
        */


        // this is all done by looking at the slider markings 
        // mapping slider percentage to db level on that slider 
        // so on slider its marked -30, this is 0.1508333 percentage of the slider
        // tested by PhysicalConsole   Debug.LogError($"percentage:{percentage}, DB:{DB}, calcPer{per}");
        private static readonly List<KeyValuePair<float, float>> SLIDER_DATA = new List<KeyValuePair<float, float>>()
        {
            new KeyValuePair<float, float>(10, 1),
            new KeyValuePair<float, float>(-10, 0.3416666f),
            new KeyValuePair<float, float>(-30, 0.1508333f),
            new KeyValuePair<float, float>(-80, 0),
        };


        public static float DecibelToPercentage(float db)
        {
            if(db >= SLIDER_DATA[0].Key)
            {
                return SLIDER_DATA[0].Value;
            }
            else if (db >= SLIDER_DATA[1].Key)
            {
                float per = Mathf.InverseLerp(SLIDER_DATA[1].Key, SLIDER_DATA[0].Key, db);
                return Mathf.Lerp(SLIDER_DATA[1].Value, SLIDER_DATA[0].Value, per);
            }
            else if (db >= SLIDER_DATA[2].Key)
            {
                float per = Mathf.InverseLerp(SLIDER_DATA[2].Key, SLIDER_DATA[1].Key, db);
                return Mathf.Lerp(SLIDER_DATA[2].Value, SLIDER_DATA[1].Value, per);
            }
            else
            {
                float per = Mathf.InverseLerp(SLIDER_DATA[3].Key, SLIDER_DATA[2].Key, db);
                return Mathf.Lerp(SLIDER_DATA[3].Value, SLIDER_DATA[2].Value, per);
            }
        }

        public static float PercentageToDecibel(float percentage)
        {
            if (percentage >= SLIDER_DATA[0].Value)
            {
                return SLIDER_DATA[0].Key;
            }
            else if (percentage >= SLIDER_DATA[1].Value)
            {
                float db = Mathf.InverseLerp(SLIDER_DATA[1].Value, SLIDER_DATA[0].Value, percentage);
                return Mathf.Lerp(SLIDER_DATA[1].Key, SLIDER_DATA[0].Key, db);
            }
            else if (percentage >= SLIDER_DATA[2].Value)
            {
                float db = Mathf.InverseLerp(SLIDER_DATA[2].Value, SLIDER_DATA[1].Value, percentage);
                return Mathf.Lerp(SLIDER_DATA[2].Key, SLIDER_DATA[1].Key, db);
            }
            else
            {
                float db = Mathf.InverseLerp(SLIDER_DATA[3].Value, SLIDER_DATA[2].Value, percentage);
                return Mathf.Lerp(SLIDER_DATA[3].Key, SLIDER_DATA[2].Key, db);
            }
        }

        public static bool IsValidFlipOnState() => (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Aux && ConsoleScreenManager.Instance.FlipOn == true) || (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Input);
        public static bool IsInputAuxType_INPUT() => (ConsoleData.Instance.m_InputAuxType == InputAuxTypeEnum.Input);

    }

}