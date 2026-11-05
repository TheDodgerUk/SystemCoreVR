
using UnityEngine;
using UnityEngine.XR;

public class XRTrackingDebug : MonoBehaviour
{
    [Header("Settings")]
    public float cubeSize = 0.1f;
    public Transform trackingSpace;

    [Header("Button Threshold")]
    [Range(0f, 1f)]
    public float triggerThreshold = 0.5f;

    private GameObject headCube;
    private GameObject leftCube;
    private GameObject rightCube;

    private Renderer headRenderer;
    private Renderer leftRenderer;
    private Renderer rightRenderer;

    private void Start()
    {
        headCube = CreateCube(
            "XR Head", Color.red, out headRenderer);

        leftCube = CreateCube(
            "XR Left Controller", Color.green,
            out leftRenderer);

        rightCube = CreateCube(
            "XR Right Controller", Color.blue,
            out rightRenderer);
    }

    private GameObject CreateCube(
        string name,
        Color color,
        out Renderer cubeRenderer)
    {
        GameObject cube = GameObject.CreatePrimitive(
            PrimitiveType.Cube);

        cube.name = name;
        cube.transform.localScale = Vector3.one * cubeSize;

        cubeRenderer = cube.GetComponent<Renderer>();

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.SetColor("_BaseColor", color);

        cubeRenderer.material = material;

        Collider collider = cube.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        return cube;
    }

    private void Update()
    {
        UpdateHead();
        UpdateController(
            XRNode.LeftHand,
            leftCube,
            leftRenderer,
            Color.green,
            "Left");

        UpdateController(
            XRNode.RightHand,
            rightCube,
            rightRenderer,
            Color.blue,
            "Right");
    }

    private void UpdateHead()
    {
        InputDevice device =
            InputDevices.GetDeviceAtXRNode(XRNode.Head);

        UpdateTracking(headCube, device);
    }

    private void UpdateController(
        XRNode node,
        GameObject cube,
        Renderer cubeRenderer,
        Color defaultColor,
        string controllerName)
    {
        InputDevice device =
            InputDevices.GetDeviceAtXRNode(node);

        UpdateTracking(cube, device);

        if (!device.isValid)
            return;

        Color color = defaultColor;

        bool primary = GetButton(
            device, CommonUsages.primaryButton);

        bool secondary = GetButton(
            device, CommonUsages.secondaryButton);

        bool gripButton = GetButton(
            device, CommonUsages.gripButton);

        bool triggerButton = GetButton(
            device, CommonUsages.triggerButton);

        float grip = GetAxis(
            device, CommonUsages.grip);

        float trigger = GetAxis(
            device, CommonUsages.trigger);

        // Priority when multiple buttons are pressed.
        if (primary)
            color = Color.yellow;
        else if (secondary)
            color = Color.magenta;
        else if (gripButton || grip > triggerThreshold)
            color = Color.cyan;
        else if (triggerButton || trigger > triggerThreshold)
            color = new Color(1f, 0.5f, 0f);

        cubeRenderer.material.SetColor("_BaseColor", color);
    }

    private bool GetButton(
        InputDevice device,
        InputFeatureUsage<bool> usage)
    {
        return device.TryGetFeatureValue(
            usage, out bool pressed) && pressed;
    }

    private float GetAxis(
        InputDevice device,
        InputFeatureUsage<float> usage)
    {
        return device.TryGetFeatureValue(
            usage, out float value) ? value : 0f;
    }

    private void UpdateTracking(
        GameObject cube,
        InputDevice device)
    {
        if (!device.isValid)
        {
            cube.SetActive(false);
            return;
        }

        bool hasPosition = device.TryGetFeatureValue(
            CommonUsages.devicePosition,
            out Vector3 position);

        bool hasRotation = device.TryGetFeatureValue(
            CommonUsages.deviceRotation,
            out Quaternion rotation);

        cube.SetActive(hasPosition && hasRotation);

        if (!hasPosition || !hasRotation)
            return;

        if (trackingSpace != null)
        {
            cube.transform.position =
                trackingSpace.TransformPoint(position);

            cube.transform.rotation =
                trackingSpace.rotation * rotation;
        }
        else
        {
            cube.transform.position = position;
            cube.transform.rotation = rotation;
        }
    }

    private void OnDestroy()
    {
        if (headCube != null) Destroy(headCube);
        if (leftCube != null) Destroy(leftCube);
        if (rightCube != null) Destroy(rightCube);
    }
}
