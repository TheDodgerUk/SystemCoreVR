
using System.Collections.Generic;
using UnityEngine;

public class PrefabRowSpawner : MonoBehaviour
{

    [Header("Spacing")]
    [Tooltip("0.5 = gap of half the previous collider size")]
    [SerializeField, Min(0f)]
    private float spacingMultiplier = 0.5f;

    [Header("Fallback")]
    [SerializeField]
    private Vector3 fallbackSize = Vector3.one;

    private class Item
    {
        public GameObject instance;
        public Bounds bounds;
        public float x;
        public float z;
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

        float gapMultiplier = Mathf.Max(0f, spacingMultiplier);

        List<List<Item>> rows = new List<List<Item>>();
        List<float> rowDepths = new List<float>();

        int index = 0;

        // ---------------------------------------------
        // BUILD ROWS
        // ---------------------------------------------

        for (int row = 0; row < rowCount; row++)
        {
            List<Item> items = new List<Item>();

            float rightEdge = 0f;
            float previousWidth = 0f;
            float maxDepth = 0f;

            for (int col = 0;
                 col < columns && index < count;
                 col++)
            {
                GameObject instance = validObjects[index++];

                Bounds bounds = GetColliderBounds(instance, out float axis);

                float width = bounds.size.x;

                if (col > 0)
                    rightEdge += previousWidth * gapMultiplier;

                Item item = new Item
                {
                    instance = instance,
                    bounds = bounds,
                    x = rightEdge + width * 0.5f
                };

                items.Add(item);

                rightEdge += width;
                previousWidth = width;

                maxDepth = Mathf.Max(
                    maxDepth,
                    bounds.size.z
                );
            }

            // Centre each row around X = 0
            float rowCentre = rightEdge * 0.5f;

            foreach (Item item in items)
            {
                item.x -= rowCentre;
            }

            rows.Add(items);
            rowDepths.Add(maxDepth);
        }

        // ---------------------------------------------
        // CALCULATE TOTAL DEPTH
        // ---------------------------------------------

        float totalDepth = 0f;

        for (int row = 0; row < rowCount; row++)
        {
            totalDepth += rowDepths[row];

            if (row < rowCount - 1)
            {
                totalDepth +=
                    rowDepths[row] * gapMultiplier;
            }
        }

        // ---------------------------------------------
        // POSITION OBJECTS
        // ---------------------------------------------

        float currentZ = totalDepth * 0.5f;

        for (int row = 0; row < rowCount; row++)
        {
            float depth = rowDepths[row];

            float rowZ = currentZ - depth * 0.5f;

            foreach (Item item in rows[row])
            {
                item.z = rowZ;

                Vector3 targetBoundsCentre = new Vector3(
                    centrePoint.x + item.x,
                    item.bounds.center.y,
                    centrePoint.z + item.z
                );

                // Calculate X/Z movement from collider centre
                Vector3 offset =
                    targetBoundsCentre - item.bounds.center;

                // Set Y EXACTLY to the passed centrePoint.y
                Vector3 position = item.instance.transform.position;

                position.x += offset.x;
                position.y = centrePoint.y;
                position.z += offset.z;

                item.instance.transform.position = position;
            }

            currentZ -= depth * (1f + gapMultiplier);
        }

        Physics.SyncTransforms();

        // ---------------------------------------------
        // FINAL CENTRING CORRECTION (X/Z ONLY)
        // ---------------------------------------------

        Bounds combinedBounds =
            GetColliderBounds(validObjects[0], out float axis0);

        for (int i = 1; i < validObjects.Count; i++)
        {
            combinedBounds.Encapsulate(
                GetColliderBounds(validObjects[i], out float axis2)
            );
        }

        Vector3 correction = new Vector3(
            centrePoint.x - combinedBounds.center.x,
            0f,
            centrePoint.z - combinedBounds.center.z
        );

        foreach (GameObject obj in validObjects)
        {
            Vector3 position = obj.transform.position;

            position.x += correction.x;
            position.y = centrePoint.y;
            position.z += correction.z;
            Debug.Log($"{obj.gameObject.name}: Correction: {correction}, Position: {position}, CentrePoint: {centrePoint}, CombinedBoundsCenter: {combinedBounds.center} . new y {position.y += axis0}");
            position.y += axis0;

            obj.transform.position = position;

        }

        Physics.SyncTransforms();
    }

    // ---------------------------------------------
    // GET COLLIDER BOUNDS
    // ---------------------------------------------

    private Bounds GetColliderBounds(GameObject instance, out float offset)
    {
        // Default value - required for an out parameter
        offset = 0;

        Transform collisionTransform =
            instance.transform.Find("PlacementCollision");

        if (collisionTransform != null)
        {


            BoxCollider collider =
                collisionTransform.GetComponentInChildren<BoxCollider>(true);

            if (collisionTransform.rotation != Quaternion.identity)
            {
                offset -= collider.size.z / 2;
            }
            else
            {
                offset -= collider.size.y / 2;
            }


            if (collider != null)
            {
                bool wasEnabled = collider.enabled;

                if (!wasEnabled)
                {
                    collider.enabled = true;
                    Physics.SyncTransforms();
                }

                Bounds bounds = collider.bounds;

                if (!wasEnabled)
                {
                    collider.enabled = false;
                }

                return bounds;
            }
        }

        Debug.LogWarning(
            $"No PlacementCollision BoxCollider found on " +
            $"{instance.name}. Using fallback size.",
            instance
        );

        return new Bounds(
            instance.transform.position,
            fallbackSize
        );
    }
}
