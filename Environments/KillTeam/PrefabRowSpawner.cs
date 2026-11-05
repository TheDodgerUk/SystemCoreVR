
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

    private const float MIN_SIZE = 0.0001f;

    private class Item
    {
        public GameObject instance;
        public Bounds bounds;
        public float x;
        public float z;
    }

    public void Spawn(Vector3 centrePoint, List<GameObject> gameObjects)
    {
        if (gameObjects == null || gameObjects.Count == 0)
            return;

        List<GameObject> validObjects = new List<GameObject>();
        HashSet<GameObject> seen = new HashSet<GameObject>();

        foreach (GameObject obj in gameObjects)
        {
            if (obj == null)
                continue;

            if (!seen.Add(obj))
            {
                Debug.LogWarning(
                    $"Duplicate GameObject ignored: {obj.name}",
                    obj
                );
                continue;
            }

            validObjects.Add(obj);
        }

        int count = validObjects.Count;

        if (count == 0)
            return;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rowCount = Mathf.CeilToInt(count / (float)columns);

        float gapMultiplier = Mathf.Max(0f, spacingMultiplier);

        List<List<Item>> rows = new List<List<Item>>();
        List<float> rowDepths = new List<float>();

        int index = 0;

        // BUILD ROWS
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

                Bounds bounds = GetColliderBounds(instance);

                float width = Mathf.Max(bounds.size.x, MIN_SIZE);
                float depth = Mathf.Max(bounds.size.z, MIN_SIZE);

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

                maxDepth = Mathf.Max(maxDepth, depth);
            }

            float rowCentre = rightEdge * 0.5f;

            foreach (Item item in items)
                item.x -= rowCentre;

            rows.Add(items);
            rowDepths.Add(maxDepth);
        }

        // CALCULATE TOTAL DEPTH
        float totalDepth = 0f;

        for (int row = 0; row < rowCount; row++)
        {
            totalDepth += rowDepths[row];

            if (row < rowCount - 1)
                totalDepth += rowDepths[row] * gapMultiplier;
        }

        // POSITION OBJECTS
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

                Vector3 offset =
                    targetBoundsCentre - item.bounds.center;

                Vector3 position = item.instance.transform.position;

                position.x += offset.x;
                position.y = centrePoint.y;
                position.z += offset.z;

                item.instance.transform.position = position;
            }

            currentZ -= depth * (1f + gapMultiplier);
        }

        // FINAL CENTRING CORRECTION
        Bounds combinedBounds = GetColliderBounds(validObjects[0]);

        for (int i = 1; i < validObjects.Count; i++)
            combinedBounds.Encapsulate(GetColliderBounds(validObjects[i]));

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

            obj.transform.position = position;
        }

        Physics.SyncTransforms();

        Debug.Log(
            $"PrefabRowSpawner: Positioned {count} objects " +
            $"in {rowCount} rows and {columns} columns."
        );
    }

    // GET WORLD-SPACE COLLIDER BOUNDS
    // Works with any scale, rotation, or disabled collider.
    private Bounds GetColliderBounds(GameObject instance)
    {
        Transform collisionTransform =
            instance.transform.Find("PlacementCollision");

        if (collisionTransform != null)
        {
            BoxCollider box =
                collisionTransform.GetComponentInChildren<BoxCollider>(true);

            if (box != null)
            {
                Bounds bounds = CalculateWorldBounds(box);

                if (bounds.size.x > MIN_SIZE &&
                    bounds.size.z > MIN_SIZE)
                {
                    return bounds;
                }

                Debug.LogWarning(
                    $"Invalid collider dimensions on {instance.name}: " +
                    $"{bounds.size}. Using fallback.",
                    instance
                );
            }
        }

        Debug.LogWarning(
            $"No valid PlacementCollision BoxCollider found on " +
            $"{instance.name}. Using fallback size.",
            instance
        );

        return new Bounds(
            instance.transform.position,
            new Vector3(
                Mathf.Max(Mathf.Abs(fallbackSize.x), MIN_SIZE),
                Mathf.Max(Mathf.Abs(fallbackSize.y), MIN_SIZE),
                Mathf.Max(Mathf.Abs(fallbackSize.z), MIN_SIZE)
            )
        );
    }

    // CALCULATE BOUNDS FROM ALL 8 BOX CORNERS
    private Bounds CalculateWorldBounds(BoxCollider box)
    {
        Vector3 centre = box.center;
        Vector3 halfSize = box.size * 0.5f;

        Transform t = box.transform;

        Vector3 firstCorner = t.TransformPoint(
            centre + new Vector3(
                -halfSize.x,
                -halfSize.y,
                -halfSize.z
            )
        );

        Bounds bounds = new Bounds(firstCorner, Vector3.zero);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = centre + new Vector3(
                        halfSize.x * x,
                        halfSize.y * y,
                        halfSize.z * z
                    );

                    bounds.Encapsulate(t.TransformPoint(localCorner));
                }
            }
        }

        return bounds;
    }
}
