using System.Collections.Generic;
using UnityEngine;

public class PrefabRowSpawner : MonoBehaviour
{
    [Tooltip("0.5 means a gap of half the previous collider size.")]
    [SerializeField, Min(0f)]
    private float spacingMultiplier = 0.5f;

    [SerializeField]
    private Vector3 fallbackSize = Vector3.one;

    private class Item
    {
        public GameObject instance;
        public Bounds bounds;
        public float x;
    }

    public void Spawn(Vector3 centrePoint, List<GameObject> gameObjects)
    {
        if (gameObjects == null)
            return;

        List<GameObject> validObjects =
            gameObjects.FindAll(obj => obj != null);

        int count = validObjects.Count;

        if (count == 0)
            return;

        Physics.SyncTransforms();

        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rowCount = Mathf.CeilToInt(count / (float)columns);

        var rows = new List<List<Item>>();
        var rowDepths = new List<float>();

        float gapMultiplier = Mathf.Max(0f, spacingMultiplier);
        int index = 0;

        // Measure the existing objects and calculate each row.
        for (int row = 0; row < rowCount; row++)
        {
            var items = new List<Item>();

            float rightEdge = 0f;
            float previousWidth = 0f;
            float depth = 0f;

            for (int column = 0;
                 column < columns && index < count;
                 column++)
            {
                GameObject instance = validObjects[index++];
                Bounds bounds = GetColliderBounds(instance);

                float width = bounds.size.x;

                if (column > 0)
                    rightEdge += previousWidth * gapMultiplier;

                items.Add(new Item
                {
                    instance = instance,
                    bounds = bounds,
                    x = rightEdge + width * 0.5f
                });

                rightEdge += width;
                previousWidth = width;
                depth = Mathf.Max(depth, bounds.size.z);
            }

            // Centre each row.
            foreach (Item item in items)
                item.x -= rightEdge * 0.5f;

            rows.Add(items);
            rowDepths.Add(depth);
        }

        float totalDepth = 0f;

        for (int row = 0; row < rowCount; row++)
        {
            totalDepth += rowDepths[row];

            if (row < rowCount - 1)
                totalDepth += rowDepths[row] * gapMultiplier;
        }

        float frontEdge = totalDepth * 0.5f;

        // Reposition the existing objects.
        for (int row = 0; row < rowCount; row++)
        {
            float z = frontEdge - rowDepths[row] * 0.5f;

            foreach (Item item in rows[row])
            {
                // Place the bottom of the collider at centrePoint.y.
                Vector3 targetBoundsCentre = new Vector3(
                    centrePoint.x + item.x,
                    centrePoint.y + item.bounds.extents.y,
                    centrePoint.z + z
                );

                // Account for the collider's offset from the object pivot.
                item.instance.transform.position +=
                    targetBoundsCentre - item.bounds.center;
            }

            frontEdge -= rowDepths[row] * (1f + gapMultiplier);
        }

        Physics.SyncTransforms();
    }

    private Bounds GetColliderBounds(GameObject instance)
    {
        Transform collisionTransform =
            instance.transform.Find("PlacementCollision");

        if (collisionTransform != null)
        {
            BoxCollider collider =
                collisionTransform.GetComponentInChildren<BoxCollider>();
            Bounds b;
            if (collider != null)
            {
                if(collider.enabled == false)
                {
                    collider.enabled = true;
                    b = collider.bounds;
                    collider.enabled = false;
                }
                else
                {
                    b = collider.bounds;
                }  

                return b;
            }
        }

        Debug.LogWarning(
            $"No active PlacementCollision BoxCollider found on {instance.name}. " +
            "Using fallback size.",
            instance
        );

        return new Bounds(instance.transform.position, fallbackSize);
    }
}