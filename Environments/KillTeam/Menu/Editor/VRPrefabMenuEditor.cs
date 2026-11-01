using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VRPrefabMenu))]
public class VRPrefabMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Test Button Click", GUILayout.Height(30)))
            {
                ((VRPrefabMenu)target).TestButtonClick();
            }
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play Mode and create the menu before testing.", MessageType.Info);
    }
}
