using System.Collections.Generic;
using UnityEngine;

public class ColliderStorage
{
    private readonly Dictionary<Collider, bool> colliderStates =
        new Dictionary<Collider, bool>();

    // Search root and ALL children, including inactive objects
    public void Store(GameObject root)
    {
        colliderStates.Clear();

        if (root == null)
            return;

        Collider[] colliders =
            root.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            colliderStates[col] = col.enabled;
        }

        Debug.Log($"Stored {colliderStates.Count} colliders from {root.name}");
    }

    public void DisableAll()
    {
        foreach (var pair in colliderStates)
        {
            if (pair.Key != null)
                pair.Key.enabled = false;
        }
    }

    public void Restore()
    {
        foreach (var pair in colliderStates)
        {
            if (pair.Key != null)
                pair.Key.enabled = pair.Value;
        }
    }

    public void LogAll()
    {
        foreach (var pair in colliderStates)
        {
            if (pair.Key == null)
                continue;

            Debug.Log(
                $"Object: {pair.Key.gameObject.name} | " +
                $"Collider: {pair.Key.GetType().Name} | " +
                $"Original: {pair.Value} | " +
                $"Current: {pair.Key.enabled}"
            );
        }
    }
}