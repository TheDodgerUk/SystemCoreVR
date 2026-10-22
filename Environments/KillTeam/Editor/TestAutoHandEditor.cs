using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestAutoHand))]
public class TestAutoHandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestAutoHand script = (TestAutoHand)target;

        if (GUILayout.Button("Grab"))
            script.Grab();

        if (GUILayout.Button("Release"))
            script.Release();
    }
}