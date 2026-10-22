
using UnityEngine;
using Autohand;
using System.Collections.Generic;

[RequireComponent(typeof(Grabbable))]
public class MiniatureMovementLine : MonoBehaviour
{
    [Header("Movement")]
    public float maxMovement = 6f * 0.0254f;
    public float lineWidth = 0.005f;
    public float lineHeight = 0.01f;
    public float distance = 0.01f;

    [Header("Pathfinding")]
    public float gridSize = 0.025f;
    public float miniatureRadius = 0.015f;
    public float searchPadding = 0.3f;
    public int maxGridCells = 20000;

    [Header("Colours")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private LineRenderer line;
    private Grabbable grab;

    private Vector3 startPosition;
    private bool isMoving;

    private class Node
    {
        public int x, z;
        public float g = float.MaxValue;
        public float h;
        public Node parent;
        public bool blocked;
        public bool closed;

        public float F => g + h;
    }

    private void Awake()
    {
        GameObject child = new GameObject("MovementLine");
        child.transform.SetParent(transform, false);
        line = child.AddComponent<LineRenderer>();
        Debug.LogError($"MiniatureMovementLine Script A added LineRenderer: {line.GetInstanceID()}");
        grab = GetComponent<Grabbable>();

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.material = new Material(
            Shader.Find("Sprites/Default"));

        line.enabled = false;
    }

    private void OnEnable()
    {
        grab.OnGrabEvent += OnGrab;
        grab.OnReleaseEvent += OnRelease;
    }

    private void OnDisable()
    {
        grab.OnGrabEvent -= OnGrab;
        grab.OnReleaseEvent -= OnRelease;
    }

    private void OnGrab(Hand hand, Grabbable grabbable)
    {
        startPosition = transform.position;
        isMoving = true;
        line.enabled = true;
    }

    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        isMoving = false;
        line.enabled = false;
    }

    private void LateUpdate()
    {
        if (!isMoving)
            return;

        Vector3 endPosition = transform.position;
        endPosition.y = startPosition.y;

        List<Collider> terrainColliders =
            GetTerrainColliders();

        List<Vector3> path =
            FindPath(startPosition, endPosition, terrainColliders);

        if (path == null || path.Count < 2)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;

        distance = 0f;

        for (int i = 1; i < path.Count; i++)
        {
            distance += Vector3.Distance(
                path[i - 1], path[i]);
        }

        Color color = distance <= maxMovement
            ? validColor
            : invalidColor;

        line.startColor = color;
        line.endColor = color;

        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 point = path[i];
            point.y = startPosition.y + lineHeight;

            line.SetPosition(i, point);
        }
    }

    // Get every collider belonging to terrain objects,
    // including colliders on child GameObjects.
    private List<Collider> GetTerrainColliders()
    {
        List<Collider> result = new List<Collider>();

        KillTeam.Terrian[] terrain =
            FindObjectsOfType<KillTeam.Terrian>();

        foreach (KillTeam.Terrian t in terrain)
        {
            Collider[] colliders =
                t.GetComponentsInChildren<Collider>();

            foreach (Collider col in colliders)
            {
                if (col.enabled &&
                    col.gameObject.activeInHierarchy &&
                    !col.isTrigger)
                {
                    result.Add(col);
                }
            }
        }

        return result;
    }

    private List<Vector3> FindPath(
        Vector3 start,
        Vector3 end,
        List<Collider> terrainColliders)
    {
        float minX = Mathf.Min(start.x, end.x) - searchPadding;
        float maxX = Mathf.Max(start.x, end.x) + searchPadding;
        float minZ = Mathf.Min(start.z, end.z) - searchPadding;
        float maxZ = Mathf.Max(start.z, end.z) + searchPadding;

        // Include the bounds of all terrain colliders.
        foreach (Collider col in terrainColliders)
        {
            Bounds b = col.bounds;

            minX = Mathf.Min(minX, b.min.x - searchPadding);
            maxX = Mathf.Max(maxX, b.max.x + searchPadding);
            minZ = Mathf.Min(minZ, b.min.z - searchPadding);
            maxZ = Mathf.Max(maxZ, b.max.z + searchPadding);
        }

        int width = Mathf.CeilToInt(
            (maxX - minX) / gridSize) + 1;

        int height = Mathf.CeilToInt(
            (maxZ - minZ) / gridSize) + 1;

        if ((long)width * height > maxGridCells)
            return null;

        Node[,] nodes = new Node[width, height];

        // Build grid.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Node node = new Node
                {
                    x = x,
                    z = z
                };

                Vector3 point = new Vector3(
                    minX + x * gridSize,
                    start.y,
                    minZ + z * gridSize);

                node.blocked = IsBlocked(
                    point, terrainColliders);

                nodes[x, z] = node;
            }
        }

        int sx = Mathf.Clamp(
            Mathf.RoundToInt((start.x - minX) / gridSize),
            0, width - 1);

        int sz = Mathf.Clamp(
            Mathf.RoundToInt((start.z - minZ) / gridSize),
            0, height - 1);

        int ex = Mathf.Clamp(
            Mathf.RoundToInt((end.x - minX) / gridSize),
            0, width - 1);

        int ez = Mathf.Clamp(
            Mathf.RoundToInt((end.z - minZ) / gridSize),
            0, height - 1);

        Node first = nodes[sx, sz];
        Node goal = nodes[ex, ez];

        if (first.blocked || goal.blocked)
            return null;

        first.g = 0f;
        first.h = Heuristic(first, goal);

        List<Node> open = new List<Node> { first };

        while (open.Count > 0)
        {
            Node current = open[0];

            foreach (Node n in open)
            {
                if (n.F < current.F)
                    current = n;
            }

            if (current == goal)
                return BuildPath(
                    goal, start, end, minX, minZ);

            open.Remove(current);
            current.closed = true;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0)
                        continue;

                    int nx = current.x + dx;
                    int nz = current.z + dz;

                    if (nx < 0 || nz < 0 ||
                        nx >= width || nz >= height)
                        continue;

                    Node next = nodes[nx, nz];

                    if (next.blocked || next.closed)
                        continue;

                    // Prevent diagonal movement through corners.
                    if (dx != 0 && dz != 0)
                    {
                        if (nodes[current.x + dx, current.z].blocked ||
                            nodes[current.x, current.z + dz].blocked)
                            continue;
                    }

                    float cost = Mathf.Sqrt(
                        dx * dx + dz * dz) * gridSize;

                    float newG = current.g + cost;

                    if (newG < next.g)
                    {
                        next.g = newG;
                        next.h = Heuristic(next, goal);
                        next.parent = current;

                        if (!open.Contains(next))
                            open.Add(next);
                    }
                }
            }
        }

        return null;
    }

    private List<Vector3> BuildPath(
        Node goal,
        Vector3 start,
        Vector3 end,
        float minX,
        float minZ)
    {
        List<Vector3> result = new List<Vector3>();

        Node current = goal;

        while (current != null)
        {
            result.Add(new Vector3(
                minX + current.x * gridSize,
                start.y,
                minZ + current.z * gridSize));

            current = current.parent;
        }

        result.Reverse();

        // Replace grid endpoints with actual miniature positions.
        result[0] = start;

        if (result.Count == 1)
            result.Add(end);
        else
            result[result.Count - 1] = end;

        return result;
    }

    private float Heuristic(Node a, Node b)
    {
        float dx = Mathf.Abs(a.x - b.x);
        float dz = Mathf.Abs(a.z - b.z);

        return gridSize * (
            Mathf.Max(dx, dz) +
            (Mathf.Sqrt(2f) - 1f) * Mathf.Min(dx, dz));
    }

    private bool IsBlocked(
        Vector3 point,
        List<Collider> terrainColliders)
    {
        foreach (Collider col in terrainColliders)
        {
            Bounds b = col.bounds;

            if (point.x >= b.min.x - miniatureRadius &&
                point.x <= b.max.x + miniatureRadius &&
                point.z >= b.min.z - miniatureRadius &&
                point.z <= b.max.z + miniatureRadius)
            {
                return true;
            }
        }

        return false;
    }
}
