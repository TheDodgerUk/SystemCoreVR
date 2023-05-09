#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorTrainer;
using static MonitorTrainer.MonitorTrainerConsts;

[CustomEditor(typeof(MonitorTrainer.LaptopMenuManager))]
public class LaptopMenuManagerEditor : MenuEditorBase
{

    public override void OnInspectorGUI()
    {
        MonitorTrainer.LaptopMenuManager menu = (MonitorTrainer.LaptopMenuManager)target;
        foreach (var item in menu.m_BandPages)
        {
            Display(item.m_Apple);
            Display(item.m_Spotify);
            Display(item.m_Amazon);
            Display(item.m_YouTube);
        }

        if (GUILayout.Button("UpdateRunTimeData"))
        {
            menu.UpdateRunTimeData();
        }
    }
}
#endif
