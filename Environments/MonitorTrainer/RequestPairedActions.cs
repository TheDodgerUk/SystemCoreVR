using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class RequestPairedActions
    {
        public enum PairedActionsGroupEnum
        {
            USB,
            MicCable,
        }

        public enum PairedActionsItemEnum
        {
            USB,
            Dummy_USB,
            MonitorTrainer_MicBoxCables,
            MonitorTrainer_MicBox_Port,
        }
        public PairedActionsItemEnum FirstItem { get; protected set; }
        public PairedActionsItemEnum SecondItem { get; protected set; }
        public PairedActionsGroupEnum PairedActions { get; protected set; }
        public bool PairedCompleted { get; protected set; }
        public bool RotateToEndRotation { get; protected set; }

        public RequestPairedActions(PairedActionsGroupEnum pairedActions, PairedActionsItemEnum first, PairedActionsItemEnum second, bool rotateToEndRotation)
        {
            FirstItem = first;
            SecondItem = second;
            PairedActions = pairedActions;
            PairedCompleted = false;
            RotateToEndRotation = rotateToEndRotation;
        }

        public void SetPairedCompleted()
        {
            PairedCompleted = true;
        }
    }

    public class RequestScenarioChange
    {
        public ScenarioEnum Scenario { get; private set; }
        public RequestScenarioChange(ScenarioEnum scenario)
        {
            Scenario = scenario;
        }
    }
}
