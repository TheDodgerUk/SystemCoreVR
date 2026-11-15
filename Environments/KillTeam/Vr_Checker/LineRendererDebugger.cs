using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


/*
Create an empty GameObject called VR Debugger.
Attach LineRendererDebugger.
Press Play.
Press and release your right grip.
Look in the Unity Console for LINE RENDERER CHANGED.
Click the warning to locate the GameObject.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */


public class LineRendererDebugger : MonoBehaviour
{
    private class LineState
    {
        public bool enabled;
        public bool active;
        public int positionCount;
        public float width;
    }

    private readonly Dictionary<LineRenderer, LineState> states =
        new Dictionary<LineRenderer, LineState>();

    private InputDevice rightController;

    private bool previousGrip;

    private void Start()
    {
        rightController =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        FindLineRenderers();
    }

    [ContextMenu("Find Line Renderers")]
    public void FindLineRenderers()
    {
        states.Clear();

        LineRenderer[] renderers =
            FindObjectsOfType<LineRenderer>(true);

        foreach (LineRenderer line in renderers)
        {
            states.Add(line, CaptureState(line));

            Debug.Log(
                $"FOUND LineRenderer: {GetPath(line.transform)}",
                line.gameObject);
        }

        Debug.Log(
            $"Monitoring {states.Count} LineRenderers.");
    }

    private void LateUpdate()
    {
        UpdateGrip();

        // Create a snapshot so we can safely update the dictionary.
        var renderers = new List<LineRenderer>(states.Keys);

        foreach (LineRenderer line in renderers)
        {
            if (line == null)
                continue;

            if (!states.TryGetValue(line, out LineState previous))
                continue;

            LineState current = CaptureState(line);

            bool changed =
                previous.enabled != current.enabled ||
                previous.active != current.active ||
                previous.positionCount != current.positionCount ||
                !Mathf.Approximately(
                    previous.width,
                    current.width);

            if (!changed)
                continue;

            Debug.LogWarning(
                $"LINE RENDERER CHANGED\n" +
                $"Object: {GetPath(line.transform)}\n" +
                $"Enabled: {previous.enabled} -> {current.enabled}\n" +
                $"Active: {previous.active} -> {current.active}\n" +
                $"Positions: {previous.positionCount} -> {current.positionCount}\n" +
                $"Width: {previous.width} -> {current.width}\n" +
                $"Right Grip: {GetRightGrip()}\n" +
                $"Frame: {Time.frameCount}",
                line.gameObject);

            PrintScripts(line.transform);

            states[line] = current;
        }
    }

    private LineState CaptureState(LineRenderer line)
    {
        return new LineState
        {
            enabled = line.enabled,
            active = line.gameObject.activeInHierarchy,
            positionCount = line.positionCount,
            width = line.widthMultiplier
        };
    }

    private void UpdateGrip()
    {
        bool grip = GetRightGrip();

        if (grip && !previousGrip)
            Debug.Log("RIGHT GRIP PRESSED");

        if (!grip && previousGrip)
            Debug.Log("RIGHT GRIP RELEASED");

        previousGrip = grip;
    }

    private bool GetRightGrip()
    {
        if (!rightController.isValid)
        {
            rightController =
                InputDevices.GetDeviceAtXRNode(
                    XRNode.RightHand);
        }

        if (!rightController.isValid)
            return false;

        rightController.TryGetFeatureValue(
            CommonUsages.grip,
            out float value);

        return value > 0.5f;
    }

    private void PrintScripts(Transform target)
    {
        Debug.Log("POSSIBLE CONTROLLING SCRIPTS:");

        Transform current = target;

        while (current != null)
        {
            MonoBehaviour[] scripts =
                current.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                if (script == null)
                    continue;

                Debug.Log(
                    $"{script.GetType().FullName} " +
                    $"on {GetPath(current)}",
                    script);
            }

            current = current.parent;
        }
    }

    private string GetPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}