using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTestingLayout : MonoBehaviour
{
    private List<GameObject> StartStripTestingObjects = new List<GameObject>();


    public void ClearAll()
    {
        foreach (GameObject obj in StartStripTestingObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        StartStripTestingObjects.Clear();
    }


    public void StartStripTesting()
    {

        const int LOOP_NUMBER = 20;

        System.Text.StringBuilder log = new System.Text.StringBuilder();

        foreach (KillTeamGameManager.Player player in
            (KillTeamGameManager.Player[])Enum.GetValues(
                typeof(KillTeamGameManager.Player)))
        {
            if (!KillTeamGameManager.Instance.helpers.playerSetupStrip
                .TryGetValue(player, out GameObject strip))
            {
                Debug.LogWarning($"Missing player setup strip for {player}");
                continue;
            }

            TestBoxCollider(strip, LOOP_NUMBER, log);
        }

        Debug.Log(log.ToString());
    }

    public void StartMainTesting()
    {


        const int LOOP_NUMBER = 50;

        System.Text.StringBuilder log = new System.Text.StringBuilder();

        TestBoxCollider(
            KillTeamGameManager.Instance.helpers.BoardRoot,
            LOOP_NUMBER,
            log
        );

        Debug.Log(log.ToString());
    }


    // Shared method for testing ANY GameObject with a BoxCollider.
    private void TestBoxCollider(
        GameObject target,
        int randomPointCount,
        System.Text.StringBuilder log,
        float cubeSize = 0.005f,
        float edgePadding = 0f)
    {
        if (target == null)
        {
            Debug.LogWarning("TestBoxCollider: Target is null");
            return;
        }

        BoxCollider box = target.GetComponent<BoxCollider>();

        if (box == null)
        {
            Debug.LogWarning($"No BoxCollider on {target.name}");
            return;
        }

        FitBoxColliderToMesh(box);

        Vector3[] corners = box.GetWorldBoxCorners();

        box.GetWorldBoxMinMax(
            out Vector3 min,
            out Vector3 max
        );

        log.AppendLine($"========== {target.name} ==========");
        log.AppendLine($"Transform: {target.transform.position.ToString("F6")}");
        log.AppendLine($"Collider world centre: {target.transform.TransformPoint(box.center).ToString("F6")}");
        log.AppendLine($"MIN: {min.ToString("F6")}");
        log.AppendLine($"MAX: {max.ToString("F6")}");
        log.AppendLine("CORNERS:");

        for (int i = 0; i < corners.Length; i++)
        {
            CreateDebugCube(
                $"Corner_{target.name}_{i}",
                corners[i],
                cubeSize,
                Color.red
            );

            log.AppendLine($"{i}: {corners[i].ToString("F6")}");
        }

        log.AppendLine("RANDOM POSITIONS:");

        for (int i = 0; i < randomPointCount; i++)
        {
            Vector3 position = box.GetRandomPointOnBox(edgePadding);

            CreateDebugCube(
                $"Random_{target.name}_{i + 1}",
                position,
                cubeSize,
                Color.green
            );

            log.AppendLine($"{i}: {position.ToString("F6")}");
        }

        log.AppendLine();
    }




    private void CreateDebugCube(string name, Vector3 position, float size, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * size;
        cube.GetComponent<Renderer>().material.color = color;
        StartStripTestingObjects.Add(cube);
    }


    public void FitBoxColliderToMesh(BoxCollider box)
    {
        if (box == null) return;
        MeshFilter filter = box.GetComponent<MeshFilter>();
        if (filter == null || filter.sharedMesh == null)
        {
            Debug.LogWarning($"Cannot fit collider on {box.name}: missing MeshFilter or mesh.");
            return;
        }

        Bounds meshBounds = filter.sharedMesh.bounds;
        Vector3 size = meshBounds.size;
        // Give planar meshes a little thickness for reliable physics queries.
        if (size.y < 0.001f) size.y = 0.001f;
        box.center = meshBounds.center;
        box.size = size;
    }
}
