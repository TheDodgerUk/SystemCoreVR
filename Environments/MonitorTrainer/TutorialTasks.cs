using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;
using static VrInteractionBaseButton;

namespace MonitorTrainer
{
    public class TutorialTasks
    {
        public List<MusicianRequestData> CollectTutorialTasks()
        {
            List<MusicianRequestData> collected = new List<MusicianRequestData>();
            collected.Add(ScenarioTutorial_1_5PowersMicAndStage());
            collected.Add(ScenarioTutorial_2MoveFaders());
            collected.Add(ScenarioTutorial_6VoxChannel());

            collected.Add(ScenarioTutorial_7AuxTab());
            collected.Add(Scenario1_8StageHandShoutDIbox());
            collected.Add(Scenario1_9StageHandShoutDIboxReverse());

            collected.Add(Scenario1_10MuteAll());
            collected.Add(Scenario1_11GuitarUpAux());
            collected.Add(Scenario1_13DCA());

            return collected;
        }

        private MusicianRequestData ScenarioTutorial_1_5PowersMicAndStage()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Right, let’s get setup. CLICK the POWER button on the STAGE BOX to turn it on. The MIC SPLITTER is below, CLICK the POWER on that as well.";
            item.m_DescriptionForReport = "ScenarioTutorial_1_5PowersMicAndStage";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_CompleteIfTrue = PowerOn15;
            return item;

            bool PowerOn15()
            {
                return (PhysicalAmp.Instance.PowerAmp.ButtonState == ButtonStateEnum.Down && PhysicalAmp.Instance.PowerMic.ButtonState == ButtonStateEnum.Down);
            }
        }

        private MusicianRequestData ScenarioTutorial_2MoveFaders()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Nice, now let’s get you familiar with the console. CLICK the POWER button to" +
                " turn the console on, then check the controls. Drag the FIRST 4 FADERS UP and MUTE the FIRST 4 CHANNELS.";
            item.m_DescriptionForReport = "ScenarioTutorial_2MoveFaders";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;

            item.m_ConsoleButtonStateList.Add(new ConsoleButtonState(ConsoleButtonEnum.Power, ButtonStateEnum.Down));

            item.m_ConsoleSliderStateList.Add(new ConsoleSliderState(ConsoleSliderGroupEnum.Slider1, SliderStateEnum.Up));
            item.m_ConsoleSliderStateList.Add(new ConsoleSliderState(ConsoleSliderGroupEnum.Slider2, SliderStateEnum.Up));
            item.m_ConsoleSliderStateList.Add(new ConsoleSliderState(ConsoleSliderGroupEnum.Slider3, SliderStateEnum.Up));
            item.m_ConsoleSliderStateList.Add(new ConsoleSliderState(ConsoleSliderGroupEnum.Slider4, SliderStateEnum.Up));
            item.m_ConsoleButtonStateList.Add(new ConsoleButtonState(ConsoleButtonEnum.Mute1, ButtonStateEnum.Down));
            item.m_ConsoleButtonStateList.Add(new ConsoleButtonState(ConsoleButtonEnum.Mute2, ButtonStateEnum.Down));
            item.m_ConsoleButtonStateList.Add(new ConsoleButtonState(ConsoleButtonEnum.Mute3, ButtonStateEnum.Down));
            item.m_ConsoleButtonStateList.Add(new ConsoleButtonState(ConsoleButtonEnum.Mute4, ButtonStateEnum.Down));

            return item;
        }


        private MusicianRequestData ScenarioTutorial_6VoxChannel()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Okay now let’s take a look at the inputs. CLICK the INPUTS TAB and find VOX CHANNEL, it may be in a different bank. CLICK VOX CHANNEL to open it and turn PHASE ON.";
            item.m_DescriptionForReport = "ScenarioTutorial_6VoxChannel";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_OverrideMainMusicianType = MusicianTypeEnum.RhythmGuitar;

            item.m_MusicianRequestPersonList.Add(new MusicianRequestPhaseInput(MusicianTypeEnum.Vocals, false, true));

            return item;
        }

        private MusicianRequestData ScenarioTutorial_7AuxTab()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Next, go to the AUX TAB and CLICK BASS AUX. Don't forget to make sure SENDS ON is ON. Find DRUM and turn it UP." +
                " Remember to check other channel pages if you can’t find it!";
            item.m_DescriptionForReport = "ScenarioTutorial_7AuxTab";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_OverrideMainMusicianType = MusicianTypeEnum.Bass;

            item.m_MusicianRequestPersonList.Add(new MusicianRequestAuxSlider(MusicianTypeEnum.Drums, SliderStateEnum.Up));
            return item;
        }

        private MusicianRequestData Scenario1_8StageHandShoutDIbox()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Hey, Daniel here! I need to move the DI Box, could you MUTE KEY1 for me and switch 48V, cheers!";
            item.m_DescriptionForReport = "Scenario1_8StageHandShoutDIbox";
            item.m_MainMusicianType = MusicianTypeEnum.StageHand;
            item.m_MusicianRequestPersonList.Add(new MonitorTrainerConsts.MusicianRequestPhantomPowerInput(MusicianTypeEnum.Synth1, true, true));
            return item;
        }

        private MusicianRequestData Scenario1_9StageHandShoutDIboxReverse()
        {
            var item = new MusicianRequestData();
            item.m_Description = "That’s all sorted now, nice one. Don’t forget to UNMUTE KEY1, cheers!";
            item.m_DescriptionForReport = "Scenario1_9StageHandShoutDIboxReverse";
            item.m_MainMusicianType = MusicianTypeEnum.StageHand;

            item.m_MusicianRequestPersonList.Add(new MonitorTrainerConsts.MusicianRequestPhantomPowerInput(MusicianTypeEnum.Synth1, false, true));
            return item;
        }

        private MusicianRequestData Scenario1_10MuteAll()
        {
            var item = new MusicianRequestData();
            item.m_Description = "URGENT! We’re moving a bunch of gear around on stage and it’s going to sound awful. MUTE ALL CHANNELS before we start, do it QUICKLY!!";
            item.m_DescriptionForReport = "Scenario1_10MuteAll";
            item.m_MainMusicianType = MusicianTypeEnum.StageHand;

            item.m_CompleteIfTrue = MuteAllTask;
            return item;
        }

        private bool MuteAllTask()
        {
            if (PhysicalConsole.Instance.m_GroupButtonDictonary[ConsoleButtonEnum.MuteGroupAll].ButtonState == ButtonStateEnum.Down)
            {
                return true;
            }
            if ((PhysicalConsole.Instance.m_GroupButtonDictonary[ConsoleButtonEnum.MuteGroupDrum].ButtonState == ButtonStateEnum.Down) &&
                (PhysicalConsole.Instance.m_GroupButtonDictonary[ConsoleButtonEnum.MuteGroupGuitar].ButtonState == ButtonStateEnum.Down) &&
                (PhysicalConsole.Instance.m_GroupButtonDictonary[ConsoleButtonEnum.MuteGroupVox].ButtonState == ButtonStateEnum.Down))
            {
                return true;
            }

            return false;
        }

        private MusicianRequestData Scenario1_11GuitarUpAux()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Hey! I need you to go into MY AUX and turn MY CHANNEL UP! Need that doing ASAP, nice one!";
            item.m_DescriptionForReport = "Scenario1_11GuitarUpAux";
            item.m_MainMusicianType = MusicianTypeEnum.LeadGuitar;

            item.m_MusicianRequestPersonList.Add(new MusicianRequestAuxSlider(MusicianTypeEnum.LeadGuitar, SliderStateEnum.Up));
            return item;
        }

        private MusicianRequestData Scenario1_12PowerFailure()
        {
            //  The power has just gone, wait 10seconds for the backup generator to kick in. 
            // After the power comes on everything becomes muted. 
            // -Check and unmute the channels being used in that current song (this is not all of the channels on the desk)    
            var item = new MusicianRequestData();
            item.m_Description = "Sorry about that! We just had a power cut. Everything should be fine now, CLICK the POWER to turn the console back on and don’t forget to UNMUTE ALL CHANNELS.";
            item.m_DescriptionForReport = "Scenario1_12PowerFailure";
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;

            item.m_OnStart = Start;// reset power
            item.m_CompleteIfTrue = UnMuteAllTask;
            return item;

            void Start(MusicianRequestData data) => PhysicalConsole.Instance.Power.ButtonState = ButtonStateEnum.Up;
            bool UnMuteAllTask()
            {
                return !MuteAllTask(); // reverses it all 
            }
        }


        private MusicianRequestData Scenario1_13DCA()
        {
            var item = new MusicianRequestData();
            item.m_Description = "Could you check if VCA1 is muted please? If it's muted by mistake, UNMUTE it please, thanks!";
            item.m_DescriptionForReport = "Scenario1_13DCA";
            item.m_MainMusicianType = MusicianTypeEnum.LeadGuitar;

            item.m_ConsoleButtonStateList.Add(new ConsoleButtonState(ConsoleButtonEnum.Mute5, ButtonStateEnum.Up));
            return item;
        }

        private MusicianRequestData Scenario2_14CalmMessage()
        {
            float messageLengthTime = 10; // fixed time
            var item = new MusicianRequestData();
            item.m_MainMusicianType = MusicianTypeEnum.StageManager;
            item.m_Description = $"Right! All the checks are done, nice work! Now it’s time for the show, stay calm and try not to let the tasks get on top of you! Good luck!";
            item.m_DescriptionForReport = "Scenario2_14CalmMessage";
            item.m_LifespanSeconds = messageLengthTime;

            item.m_MusicianRequestPersonList.Add(new MusicianRequestTimeDelay(MusicianTypeEnum.LeadGuitar, messageLengthTime));
            return item;
        }
    }
}
