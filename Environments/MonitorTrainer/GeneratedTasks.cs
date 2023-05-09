using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class GeneratedTasks
    {
        private const float MIN_PERCENTAGE_ERROR = 0.10f;
        private const float MAX_PERCENTAGE_ERROR = 0.90f;

        private const float MIN_PERCENTAGE = 0.15f;
        private const float MAX_PERCENTAGE = 0.85f;

        public List<TaskGenericTimings> m_TaskTimings = new List<TaskGenericTimings>();

        private CharacterDataClass m_LastCharacterDataClass = null;

        private List<MusicianTypeEnum> m_AllBandMembersForStage = new List<MusicianTypeEnum>();

        public class TrimGainData
        {
            public MusicianTypeEnum m_MusicianTypeEnum;
            public bool m_IsGain;
        }

        public void CreateTaskTimings()
        {
            float START_WAIT = 5;
            float END_WAIT = 5;

            m_AllBandMembersForStage.Clear();
            foreach (var bandMember in MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers)
            {
                if (bandMember.AuxEnum.IsMusician() == true)
                {
                    m_AllBandMembersForStage.Add((MusicianTypeEnum)bandMember.AuxEnum);
                }
            }


            m_TaskTimings.Clear();
            TaskSettings.Instance.SetCurrentData();
            var songLength = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.SongLength;
            songLength = songLength - END_WAIT; // so nothing comes right at end
            songLength = songLength - START_WAIT; // so nothing comes right at start
            DificultySettings holdData = new DificultySettings();
            holdData.TasksPerMin[DifficultyModeEnum.Easy] = songLength / 60 * TaskSettings.Instance.CurrentDificultySettings.TasksPerMin[DifficultyModeEnum.Easy];
            //holdData.TasksPerMin[DifficultyModeEnum.Medium] = songLength / 60 * hardCodedDiff.TasksPerMin[DifficultyModeEnum.Medium];
            holdData.TasksPerMin[DifficultyModeEnum.Hard] = songLength / 60 * TaskSettings.Instance.CurrentDificultySettings.TasksPerMin[DifficultyModeEnum.Hard];

            int easy = (int)holdData.TasksPerMin[DifficultyModeEnum.Easy];
            for (int i = 0; i < easy; i++)
            {
                var timing = new TaskGenericTimings();
                timing.Difficulty = DifficultyModeEnum.Easy;
                m_TaskTimings.Add(timing);
            }

            //////int medium = (int)holdData.TasksPerMin[DifficultyModeEnum.Medium];
            //////for (int i = 0; i < medium; i++)
            //////{
            //////    var timing = new TaskTimings();
            //////    timing.Difficulty = DifficultyModeEnum.Medium;
            //////    m_TaskTimings.Add(timing);
            //////}

            int hard = (int)holdData.TasksPerMin[DifficultyModeEnum.Hard];
            for (int i = 0; i < hard; i++)
            {
                var timing = new TaskGenericTimings();
                timing.Difficulty = DifficultyModeEnum.Hard;
                m_TaskTimings.Add(timing);
            }

            m_TaskTimings = m_TaskTimings.Randomise();
            float evenGap = songLength / m_TaskTimings.Count;
            for (int i = 0; i < m_TaskTimings.Count; i++)
            {
                float timeToAdd = START_WAIT + (evenGap * (i + 1));
                m_TaskTimings[i].Time = timeToAdd;
            }

            Debug.LogError($"songLength {songLength}, START_WAIT {START_WAIT}, Count:{m_TaskTimings.Count} , Gap {evenGap} ");
        }




        public MusicianRequestData CheckForBadPositionsOnAuxSliders()
        {
            foreach (var owner in MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers)
            {
                foreach (var bandMember in MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers)
                {
                    if (bandMember.AuxEnum.IsMusician() == true && bandMember != owner)
                    {
                        var direction = AuxSliderIsError(owner, (MusicianTypeEnum)bandMember.AuxEnum);
                        if (direction != null)
                        {
                            return MakeTaskForBadPositionsOnAuxSliders(owner, bandMember, (SliderStateEnum)direction);
                        }
                    }
                }
            }
            return null;
        }

        public MusicianRequestData MakeTaskForBadPositionsOnAuxSliders(CharacterDataClass owner, CharacterDataClass otherMember, SliderStateEnum direction)
        {
            var musicianRequestData = new MusicianRequestData();
            musicianRequestData.m_MainMusicianType = (MusicianTypeEnum)owner.AuxEnum;
            musicianRequestData.m_MusicianRequestPersonList.Add(new MusicianRequestAuxSlider((MusicianTypeEnum)otherMember.AuxEnum, direction));
            int index = UnityEngine.Random.Range(0, 4);
            switch (index)
            {
                case 0:
                    musicianRequestData.m_Description = $"What have you done, {otherMember.Abbreviation} {direction.ToString().ToUpper()} in MY AUX. The sooner the better, cheers1.";
                    break;
                case 1:
                    musicianRequestData.m_Description = $"What have you done, {otherMember.Abbreviation} {direction.ToString().ToUpper()} in MY AUX. The sooner the better, cheers2.";
                    break;
                case 2:
                    musicianRequestData.m_Description = $"What have you done, {otherMember.Abbreviation} {direction.ToString().ToUpper()} in MY AUX. The sooner the better, cheers3.";
                    break;
                case 3:
                    musicianRequestData.m_Description = $"What have you done, {otherMember.Abbreviation} {direction.ToString().ToUpper()} in MY AUX. The sooner the better, cheers4.";
                    break;
            }
            return musicianRequestData;
        }


        public MusicianRequestData MakeStackableTask(MonitorTrainerConsts.DifficultyModeEnum difficulty)
        {
            var item = new MusicianRequestData();
            item.m_Difficulty = difficulty;
            item.m_DescriptionForReport = System.Reflection.MethodBase.GetCurrentMethod().Name;
            item.m_LifespanSeconds = TaskSettings.Instance.CurrentDificultySettings.TimeAtMaxScore[difficulty] + TaskSettings.Instance.CurrentDificultySettings.TimeAtMinScore[difficulty];

            CharacterDataClass randomOwner = null;
            List<MusicianTypeEnum> otherBandMembers = null;
            bool isMusician = false;

            DoUntil until = new DoUntil(() => m_LastCharacterDataClass != randomOwner, () =>
            {
                CollectBaseInfo(item, ref randomOwner, ref otherBandMembers, ref isMusician);
            });

            m_LastCharacterDataClass = randomOwner;

            if (isMusician == true || m_AllBandMembersForStage.Count == 0)
            {
                if(isMusician == false)
                {
                    DoUntil untilSecond = new DoUntil(() => isMusician == true, () =>
                    {
                        CollectBaseInfo(item, ref randomOwner, ref otherBandMembers, ref isMusician);
                    });
                }
                MakeItemForBandMember(randomOwner, otherBandMembers, item);
            }
            else
            {
                
                MakeItemForStagePeople(item);
            }

            return item;
        }


        private void CollectBaseInfo(MusicianRequestData musicianRequestData, ref  CharacterDataClass owner, ref List<MusicianTypeEnum> otherMembers, ref bool isMusician)
        {
            owner = null;
            while (owner == null)
            {
                owner = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.GetRandom();
                if(owner.AuxEnum.IsBlank() == true)
                {
                    Debug.Log("Not not use");
                    owner = null;
                }
            }
            isMusician = owner.AuxEnum.IsMusician();

            otherMembers = new List<MusicianTypeEnum>();
            foreach (var band in MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers)
            {
                if (band.AuxEnum.IsMusician() == true)
                {
                    otherMembers.Add((MusicianTypeEnum)band.AuxEnum);
                }
            }
            otherMembers.Remove((MusicianTypeEnum)owner.AuxEnum);

            musicianRequestData.m_MainMusicianType = (MusicianTypeEnum)owner.AuxEnum;
        }

        private void MakeItemForBandMember(CharacterDataClass randomOwner, List<MusicianTypeEnum> otherMembers, MusicianRequestData musicianRequestData)
        {
            if(musicianRequestData.m_Difficulty == DifficultyModeEnum.Easy)
            {
                MakeOneAUXSlider(randomOwner, otherMembers, musicianRequestData);
            }
            else
            {
                MakeNotOneAUXSlider(randomOwner, otherMembers, musicianRequestData, UnityEngine.Random.Range(2, 5));
            }
        }

        private void MakeItemForStagePeople(MusicianRequestData musicianRequestData)
        {
            MusicianTypeEnum musicianTypeEnum = MusicianTypeEnum.Bass;
            int index = UnityEngine.Random.Range(0, 3);
            switch (index)
            {
                case 0:
                    musicianTypeEnum = MakeOneGainSlider(m_AllBandMembersForStage, musicianRequestData);
                    break;
                case 1:
                    musicianTypeEnum = MakeOneTrimSlider(m_AllBandMembersForStage, musicianRequestData);
                    break;
                case 2:
                    musicianTypeEnum = MakeOneVolumeSlider(m_AllBandMembersForStage, musicianRequestData);
                    break;    
            }
            m_AllBandMembersForStage.Remove(musicianTypeEnum);
        }


        private void MakeOneAUXSlider(CharacterDataClass owner, List<MusicianTypeEnum> otherMembers, MusicianRequestData musicianRequestData)
        {
            List<TrackDataMusician> auxData = ConsoleData.Instance.GetAuxDataChannel((AuxEnum)owner.AuxEnum);
            var other = otherMembers.GetRandom();
            TrackDataMusician trackData = null;

            // make sure there is a valid data set in here 
            DoUntil until = new DoUntil(() => trackData != null, () =>
             {
                 other = otherMembers.GetRandom();
                 trackData = auxData.Find(e => e.m_AuxEnum == (AuxEnum)other);
             });

            SliderStateEnum randomDirection = (SliderStateEnum)UnityEngine.Random.Range(0, 2);
            if (AuxSliderIsValid(owner, other, randomDirection) == false)
            {
                randomDirection = randomDirection.WrapIncrementEnum();
            }

            musicianRequestData.m_MusicianRequestPersonList.Add(new MusicianRequestAuxSlider(other, randomDirection));

            var otherClass = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == (AuxEnum)other);

            int index = UnityEngine.Random.Range(0, 4);
            if (randomDirection == SliderStateEnum.Up)
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Need1 you to turn {otherClass.Abbreviation} UP in MY AUX. The sooner the better, cheers.";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Need2 Need you to turn {otherClass.Abbreviation} UP in MY AUX. The sooner the better, cheers.";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Need3 you to turn {otherClass.Abbreviation} UP in MY AUX. The sooner the better, cheers.";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"Need4 you to turn {otherClass.Abbreviation} UP in MY AUX. The sooner the better, cheers.";
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Need5 you to turn {otherClass.Abbreviation} Down in MY AUX. The sooner the better, cheers.";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Need6 you to turn {otherClass.Abbreviation} Down in MY AUX. The sooner the better, cheers.";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Need7 you to turn {otherClass.Abbreviation} Down in MY AUX. The sooner the better, cheers.";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"Need8 you to turn {otherClass.Abbreviation} Down in MY AUX. The sooner the better, cheers.";
                        break;
                }
            }
        }

        private void MakeNotOneAUXSlider(CharacterDataClass owner, List<MusicianTypeEnum> otherMembers, MusicianRequestData musicianRequestData, int number)
        {
            List<TrackDataMusician> auxData = ConsoleData.Instance.GetAuxDataChannel((AuxEnum)owner.AuxEnum);
            List<TrackDataMusician> allMusician = auxData.FindAll(e => e.m_AuxEnum.IsMusician());


            int oldNumber = number;
            number = Mathf.Min(number, allMusician.Count);
            if(oldNumber != number)
            {
                Debug.LogError($"number changed froms {oldNumber} to {number}");
            }


            for (int i = 0; i < number; i++)
            {
                TrackDataMusician other = allMusician.GetRandom();

                allMusician.Remove(other);
                SliderStateEnum randomDirection = (SliderStateEnum)UnityEngine.Random.Range(0, 2);
                if (AuxSliderIsValid(owner, (MusicianTypeEnum)other.m_AuxEnum, randomDirection) == false)
                {
                    randomDirection = randomDirection.WrapIncrementEnum();
                }
                musicianRequestData.m_MusicianRequestPersonList.Add(new MusicianRequestAuxSlider((MusicianTypeEnum)other.m_AuxEnum, randomDirection));
            }
 
            string middleString = "";
            for (int i = 0; i < musicianRequestData.m_MusicianRequestPersonList.Count; i++)
            {
                var data =(MusicianRequestAuxSlider) musicianRequestData.m_MusicianRequestPersonList[i];
                var otherClass1 = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == (AuxEnum)data.MusicianType);

                if (i == 0)
                {
                    middleString += $"{ otherClass1.Abbreviation} { data.SliderState.ToString().ToUpper()}";
                }
                else
                {
                    middleString += $" && { otherClass1.Abbreviation} { data.SliderState.ToString().ToUpper()}";
                }
            }


            int index = UnityEngine.Random.Range(0, 4);

            switch (index)
            {
                case 0:
                    musicianRequestData.m_Description = $"Need10 you to turn {middleString} in MY AUX. The sooner the better, cheers.";
                    break;
                case 1:
                    musicianRequestData.m_Description = $"Need11 you to turn {middleString} in MY AUX. The sooner the better, cheers.";
                    break;
                case 2:
                    musicianRequestData.m_Description = $"Need12 you to turn {middleString} in MY AUX. The sooner the better, cheers."; 
                    break;
                case 3:
                    musicianRequestData.m_Description = $"Need13 you to turn {middleString} in MY AUX. The sooner the better, cheers."; 
                    break;
            }

        }

        private MusicianTypeEnum MakeOneGainSlider(List<MusicianTypeEnum> allBandMembers, MusicianRequestData musicianRequestData)
        {
            MusicianTypeEnum other = allBandMembers.GetRandom();
            SliderStateEnum randomDirection = (SliderStateEnum)UnityEngine.Random.Range(0, 2);
            if (GainIsValid(other, randomDirection) == false)
            {
                randomDirection = randomDirection.WrapIncrementEnum();
            }

            musicianRequestData.m_MusicianRequestPersonList.Add(new MusicianRequestGainInput(other, randomDirection));

            var otherClass = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == (AuxEnum)other);

            int index = UnityEngine.Random.Range(0, 4);
            if (randomDirection == SliderStateEnum.Up)
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Hey0, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the GAIN UP for me!";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Hey1, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the GAIN UP for me!";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Hey2, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the GAIN UP for me!";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"Hey3, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the GAIN UP for me!";
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Hey4, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the GAIN DOWN for me!";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Hey5, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the GAIN DOWN for me!";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Hey6, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the GAIN DOWN for me!";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"He7, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the GAIN DOWN for me!";
                        break;
                }
            }
            return other;
        }

        private MusicianTypeEnum MakeOneTrimSlider(List<MusicianTypeEnum> allBandMembers, MusicianRequestData musicianRequestData)
        {
            MusicianTypeEnum other = allBandMembers.GetRandom();
            SliderStateEnum randomDirection = (SliderStateEnum)UnityEngine.Random.Range(0, 2);
            if (TrimIsValid(other, randomDirection) == false)
            {
                randomDirection = randomDirection.WrapIncrementEnum();
            }

            musicianRequestData.m_MusicianRequestPersonList.Add(new MusicianRequestTrimInput(other, randomDirection));

            var otherClass = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == (AuxEnum)other);

            int index = UnityEngine.Random.Range(0, 4);
            if (randomDirection == SliderStateEnum.Up)
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Hey0, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the TRIM UP for me!";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Hey1, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the TRIM UP for me!";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Hey2, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the TRIM UP for me!";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"Hey3, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the TRIM UP for me!";
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Hey4, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the TRIM DOWN for me!";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Hey5, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the TRIM DOWN for me!";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Hey6, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the TRIM DOWN for me!";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"He7, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the TRIM DOWN for me!";
                        break;
                }
            }
            return other;
        }

        private MusicianTypeEnum MakeOneVolumeSlider(List<MusicianTypeEnum> allBandMembers, MusicianRequestData musicianRequestData)
        {
            MusicianTypeEnum other = allBandMembers.GetRandom();
            SliderStateEnum randomDirection = (SliderStateEnum)UnityEngine.Random.Range(0, 2);
            if (VolumeIsValid(other, randomDirection) == false)
            {
                randomDirection = randomDirection.WrapIncrementEnum();
            }

            musicianRequestData.m_MusicianRequestPersonList.Add(new MusicianRequestInputSlider(other, randomDirection));

            var otherClass = MonitorTrainerRoot.Instance.PlayerChoiceDataRef.SongDataRef.BandMembers.Find(e => e.AuxEnum == (AuxEnum)other);

            int index = UnityEngine.Random.Range(0, 4);
            if (randomDirection == SliderStateEnum.Up)
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Hey0, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the VOLUME UP for me!";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Hey1, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the VOLUME UP for me!";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Hey2, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the VOLUME UP for me!";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"Hey3, the {otherClass.Abbreviation} INPUT is coming in really low from your desk!! Need you to turn the VOLUME UP for me!";
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        musicianRequestData.m_Description = $"Hey4, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the VOLUME DOWN for me!";
                        break;
                    case 1:
                        musicianRequestData.m_Description = $"Hey5, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the VOLUME DOWN for me!";
                        break;
                    case 2:
                        musicianRequestData.m_Description = $"Hey6, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the VOLUME DOWN for me!";
                        break;
                    case 3:
                        musicianRequestData.m_Description = $"He7, the {otherClass.Abbreviation} INPUT is coming in really hot from your desk!! Need you to turn the VOLUME DOWN for me!";
                        break;
                }
            }
            return other;
        }

        private bool AuxSliderIsValid(CharacterDataClass owner, MusicianTypeEnum musicianTypeEnum, SliderStateEnum sliderState)
        {
            List<TrackDataMusician> auxData = ConsoleData.Instance.GetAuxDataChannel((AuxEnum)owner.AuxEnum);
            TrackDataMusician trackData = auxData.Find(e => e.m_AuxEnum == (AuxEnum)musicianTypeEnum);
            if (trackData != null)
            {
                float sliderLevel = trackData.GetPercentageLevel();
                if (sliderState == SliderStateEnum.Down)
                {
                    return (sliderLevel > MIN_PERCENTAGE);
                }
                else
                {
                    return (sliderLevel < MAX_PERCENTAGE);
                }
            }
            else
            {
                Debug.LogError($"AuxSliderIsValid  owner : {owner.AuxEnum}, error {musicianTypeEnum}");
            }
            return false;
        }

        private SliderStateEnum? AuxSliderIsError(CharacterDataClass owner, MusicianTypeEnum musicianTypeEnum)
        {
            List<TrackDataMusician> auxData = ConsoleData.Instance.GetAuxDataChannel((AuxEnum)owner.AuxEnum);
            TrackDataMusician trackData = auxData.Find(e => e.m_AuxEnum == (AuxEnum)musicianTypeEnum);
            float sliderLevel = trackData.GetPercentageLevel();
            if (sliderLevel > MIN_PERCENTAGE_ERROR)
            {
                return SliderStateEnum.Down;
            }
            if (sliderLevel < MAX_PERCENTAGE_ERROR)
            {
                return SliderStateEnum.Down;
            }

            return null;
        }

        private bool GainIsValid(MusicianTypeEnum musicianTypeEnum, SliderStateEnum sliderState)
        {
            List<TrackDataMusician> inputData = ConsoleData.Instance.GetInputData();
            var trackData = inputData.Find(e => e.m_AuxEnum == (AuxEnum)musicianTypeEnum);
            float gainLevel = trackData.m_Gain;
            if (sliderState == SliderStateEnum.Down)
            {
                return (gainLevel > MIN_PERCENTAGE);
            }
            else
            {
                return (gainLevel < MAX_PERCENTAGE);
            }
        }

        private bool TrimIsValid(MusicianTypeEnum musicianTypeEnum, SliderStateEnum sliderState)
        {
            List<TrackDataMusician> inputData = ConsoleData.Instance.GetInputData();
            var trackData = inputData.Find(e => e.m_AuxEnum == (AuxEnum)musicianTypeEnum);
            float trimLevel = trackData.m_Trim;
            if (sliderState == SliderStateEnum.Down)
            {
                return (trimLevel > MIN_PERCENTAGE);
            }
            else
            {
                return (trimLevel < MAX_PERCENTAGE);
            }
        }

        private bool VolumeIsValid(MusicianTypeEnum musicianTypeEnum, SliderStateEnum sliderState)
        {
            List<TrackDataMusician> inputData = ConsoleData.Instance.GetInputData();
            var trackData = inputData.Find(e => e.m_AuxEnum == (AuxEnum)musicianTypeEnum);
            float VolumeLevel = trackData.GetPercentageLevel();
            if (sliderState == SliderStateEnum.Down)
            {
                return (VolumeLevel > MIN_PERCENTAGE);
            }
            else
            {
                return (VolumeLevel < MAX_PERCENTAGE);
            }
        }

        private bool GainIsValid(MusicianTypeEnum musicianTypeEnum)
        {
            List<TrackDataMusician> inputData = ConsoleData.Instance.GetInputData();
            var trackData = inputData.Find(e => e.m_AuxEnum == (AuxEnum)musicianTypeEnum);
            float gainLevel = trackData.m_Gain;
            return (gainLevel > MIN_PERCENTAGE) && (gainLevel < MAX_PERCENTAGE);
        }
    }
}
