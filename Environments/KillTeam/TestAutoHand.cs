using UnityEngine;
using Autohand;
using NaughtyAttributes;

public class TestAutoHand : MonoBehaviour
{
    public Grabbable grabbable;

    private void Awake()
    {
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();
    }

    [Button("Grab")]
    public void Grab()
    {
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        Debug.Log("Test Grab");
        grabbable.OnGrabEvent?.Invoke(null, grabbable);
    }

    [Button("Release")]
    public void Release()
    {
        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();

        Debug.Log("Test Release");
        grabbable.OnReleaseEvent?.Invoke(null, grabbable);
    }
}