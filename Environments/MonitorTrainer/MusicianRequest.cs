using System;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class MusicianRequestData : ICloneable
    {

        public enum RequestTypeEnum
        {
            Generated,
            Special,
            SpecialForAll
        }
        public enum RuntimeTypeEnum
        {
            None,
            Power,
            MuteAll,
        }

        public string m_Description = "And the lord said unto John, “come forth and receive eternal life.” But John came fifth and won a toaster.";
        public string m_DescriptionForReport = "";
        public List<string> m_HurryupDescription = new List<string>();
        public MusicianTypeEnum m_MainMusicianType;
        public MusicianTypeEnum? m_OverrideMainMusicianType = null;
        public MonitorTrainerConsts.DifficultyModeEnum m_Difficulty = DifficultyModeEnum.Easy;
        public RequestTypeEnum m_RequestType = RequestTypeEnum.Generated;
        public bool m_CanAutoRemove = false;
        public string m_SpecialForAllNickName = "";
        //public bool m_Repeatable = false; //we won't have repeatable tasks, but will instead trigger a new instance
        public float m_LifespanSeconds = 0f;
        public float m_CompletedDelayTime = 0f;
        public float m_DelayStartTime = 0f;

        public float m_CurrentTime = 0f;
        public Action<MusicianRequestData> m_OnComplete;
        public Action m_OnFailed;
        public Action<MusicianRequestData> m_OnStart;
        public Action<MusicianRequestData> m_OnDelayStart;
        public Action<MusicianRequestData> m_OnSpecialTasksHitBox;
        public Func<bool> m_CompleteIfTrue;
        public bool m_IsCompleted = false;
        public Dictionary<string, GameObject> m_CreatedObjects = new Dictionary<string, GameObject>();

        public List<ConsoleSliderState> m_ConsoleSliderStateList = new List<ConsoleSliderState>();
        public List<ConsoleButtonState> m_ConsoleButtonStateList = new List<ConsoleButtonState>();
        public List<MusicianRequestBase> m_MusicianRequestPersonList = new List<MusicianRequestBase>();
        public DificultySettings m_CurrentDificultySettings;
        public VrInteraction m_VrInteraction;

        public bool DEBUG_Compleated = false;

        public bool HasHitBox => (m_HitBox != null);
        private HitBox m_HitBox;

        public HitBox HitBox => GetHitBox();


        private HitBox GetHitBox()
        {
            if (m_HitBox == null)
            {
                m_HitBox = SpecialTasksHitBox.Instance.CreateTasksHitBox();
            }
            return m_HitBox;
        }


        public RuntimeTypeEnum m_RuntimeTypeEnum = RuntimeTypeEnum.None;
        public MusicianRequestData Clone() { return (MusicianRequestData)this.MemberwiseClone(); }
        object ICloneable.Clone() { return Clone(); }
    }
}