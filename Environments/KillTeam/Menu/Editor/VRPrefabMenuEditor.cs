
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(VRPrefabMenu))]
public class VRPrefabMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VRPrefabMenu menu = (VRPrefabMenu)target;

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Button Testing",
            EditorStyles.boldLabel
        );

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode and create the menu before testing.",
                MessageType.Info
            );

            return;
        }

        int buttonCount = menu.GetButtonCount();

        EditorGUILayout.LabelField(
            $"Created Buttons: {buttonCount}"
        );

        if (buttonCount == 0)
        {
            EditorGUILayout.HelpBox(
                "No buttons have been created.",
                MessageType.Warning
            );

            return;
        }

        for (int i = 0; i < buttonCount; i++)
        {
            int index = i;

            string buttonName = menu.GetButtonName(index);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"{index}: {buttonName}",
                GUILayout.MinWidth(150)
            );

            if (GUILayout.Button(
                "Test Button Click",
                GUILayout.Width(130),
                GUILayout.Height(25)))
            {
                menu.TestButtonClick(index);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
