using UnityEngine;
using Autohand;

public class TestAutoHand : MonoBehaviour
{
    public Grabbable grabbable;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Test Grab");
            grabbable.OnGrabEvent?.Invoke(null, grabbable);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Test Release");
            grabbable.OnReleaseEvent?.Invoke(null, grabbable);
        }
    }
}