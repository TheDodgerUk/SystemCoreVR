#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MonitorTrainer;
using UnityEngine.UI;
using static MonitorTrainer.MenuManager;

public class MenuEditorBase : Editor
{
    public void Display(Button button)
    {
        if ((button != null) && (button.gameObject.activeInHierarchy == true))
        {
            if (GUILayout.Button(button.name))
            {
                button.onClick.Invoke();
            }
        }
    }

    public void Display(Toggle toggle)
    {
        if ((toggle != null) && (toggle.gameObject.activeInHierarchy == true))
        {
            if (GUILayout.Button(toggle.name))
            {
                toggle.isOn = true;
            }
        }
    }
    public void Display(HorizontalData data)
    {
        if (data.m_Root.gameObject.activeInHierarchy == true)
        {
            var list = data.m_TogglePool.GetPublicList();
            foreach (var item in list)
            {
                if (GUILayout.Button(item.name))
                {
                    item.isOn = true;
                }
            }
        }
    }
}
#endif
