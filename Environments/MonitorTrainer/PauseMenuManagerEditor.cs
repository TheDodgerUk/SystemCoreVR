#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorTrainer;
using static MonitorTrainer.MonitorTrainerConsts;

[CustomEditor(typeof(MonitorTrainer.PauseMenuManager))]
public class PauseManagerEditor : MenuEditorBase
{

    public override void OnInspectorGUI()
    {
        MonitorTrainer.PauseMenuManager menu = (MonitorTrainer.PauseMenuManager)target;

        if (GUILayout.Button("ToggleShow"))
        {
            menu.ToggleShow();
        }

        Display(menu.PauseDataRef.m_Button_Return);
        Display(menu.PauseDataRef.m_Button_Settings);
        Display(menu.PauseDataRef.m_Button_LeaveGame);
    }
}
#endif
