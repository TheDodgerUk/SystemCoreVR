#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorTrainer;
using static MonitorTrainer.MonitorTrainerConsts;
using static MonitorTrainer.MenuManagerDebug;
using System;

namespace MonitorTrainer
{
    [CustomEditor(typeof(MonitorTrainer.MenuManagerDebug))]
    public class MenuManagerDebugEditor : MenuEditorBase
    {
        private string m_SearchFor = "";

        Dictionary<DebugInt, bool> m_State = new Dictionary<DebugInt, bool>();
        GUIStyle m_Red;
        GUIStyle m_Green;
        private void Awake()
        {
            foreach (DebugInt foo in Enum.GetValues(typeof(DebugInt)))
            {
                if (m_State.ContainsKey(foo) == false)
                {
                    m_State.Add(foo, true);
                }
            }
        }
        public override void OnInspectorGUI()
        {
            foreach (DebugInt foo in Enum.GetValues(typeof(DebugInt)))
            {
                DisplayEnum(foo);
            }
        }

        private void DisplayEnum(DebugInt val)
        {
            if (m_Red == null)
            {
                m_Red = new GUIStyle(GUI.skin.button);

                m_Red.normal.textColor = Color.red;

                m_Green = new GUIStyle(GUI.skin.button);

                m_Green.normal.textColor = Color.green;
            }

            string valString = val.ToString();
            if(valString.StartsWith(HEADER) == true)
            {
                var splits = valString.Split("_");
                GUILayout.Space(10);
                GUILayout.Label(splits[1]);
            }
            else if(val == DebugInt.StringItem)
            {

                GUILayout.Space(10);
                GUILayout.Space(10);

                m_SearchFor = GUILayout.TextField(m_SearchFor, 25);
                if (GUILayout.Button("ON"))
                {
                    stringItem newItem = new stringItem();
                    newItem.Item = m_SearchFor;
                    newItem.Enable = true;
                    Core.PhotonGenericRef.SendDebugIntData<stringItem>((int)val, newItem);  
                }

                if (GUILayout.Button("Off"))
                {
                    stringItem newItem = new stringItem();
                    newItem.Item = m_SearchFor;
                    newItem.Enable = false;
                    Core.PhotonGenericRef.SendDebugIntData<stringItem>((int)val, newItem);  
                }
            }
            else
            {
                GUIStyle style = m_Green;
                if (m_State[val] == false)
                {
                    style = m_Red;
                }
                if (GUILayout.Button(val.ToString(), style))
                {
                    m_State[val] = !m_State[val];
                    Core.PhotonGenericRef.SendDebugIntData((int)val, Convert.ToInt32(m_State[val]));  
                }
            }
        }
    }
}
#endif
