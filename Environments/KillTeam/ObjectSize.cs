using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
[ExecuteAlways]
public class ObjectSize : MonoBehaviour
{
    public enum Dimension { Width, Height, Depth }

    [Header("Measured Object Size (Read Only)")]
    [SerializeField] private Vector3 size;
    [SerializeField] private Vector3 sizeCm;
    public Vector3 Size => size;
    public Vector3 SizeCm => sizeCm;

    [Header("Resize Settings")]
    public Dimension resizeBy = Dimension.Height;
    [Min(0.0001f)] public float targetSize = 0.035f;

    [Header("Target Base")]
    [Min(1f)] public float targetDiameterMM = 40f;
    [Header("Base Detection")]
    [Min(0.1f)] public float scanHeightMM = 5f;
    [Min(3)] public int minimumVertices = 12;
    [Header("Options")]
    public bool alignBottomToZero = true;


    [SerializeField] private float measuredDiameterMM;
    [SerializeField] private float lastScaleFactor;
    [SerializeField] private int sampledVertices;
    public float MeasuredDiameterMM => measuredDiameterMM;

    private Transform rootModel;
    private Transform placementCollision;

    private void Reset() => FindChildren();

    private void Awake()
    {
        if (FindChildren())
        {
            UpdateSize();
            AlignModelBase();
        }
    }

    public bool FindChildren()
    {
        rootModel = transform.FindChildRecursive("RootModel");
        if (rootModel == null)
        {
            Debug.LogError($"RootModelfound {this.name}", this);
            return false;
        }
        placementCollision = transform.FindChildRecursive("PlacementCollision");
        if (placementCollision == null)
        {
            Debug.LogError($"PlacementCollision not found. {this.name}", this);
            return false;
        }
        return true;
    }

    private void RecordChanges(string label)
    {
#if UNITY_EDITOR
        Undo.RecordObjects(new UnityEngine.Object[]
        {
            this, transform, rootModel, placementCollision
        }, label);
#endif
    }

    // Transfer any scale on the root to the children without changing their
    // positions or sizes in the root's parent space.
    private void NormalizeRootScale()
    {
        Vector3 scale = transform.localScale;
        if (scale == Vector3.one) return;

        RecordChanges("Transfer Root Scale To Children");
        rootModel.localPosition = Vector3.Scale(rootModel.localPosition, scale);
        placementCollision.localPosition = Vector3.Scale(placementCollision.localPosition, scale);
        rootModel.localScale = Vector3.Scale(rootModel.localScale, scale);
        placementCollision.localScale = Vector3.Scale(placementCollision.localScale, scale);
        transform.localScale = Vector3.one;
    }

    private void ApplyScale(float factor)
    {
        NormalizeRootScale();
        rootModel.localScale *= factor;
        placementCollision.localScale *= factor;
    }

    // Public operations save themselves, so they also work when called from other editor code.
    public void UpdateRotation(bool badXRotation)
    {
        if (!FindChildren()) return;
        RecordChanges("Update Model Rotation");
        Quaternion rotation = Quaternion.Euler(badXRotation ? -90f : 0f, 0f, 0f);
        rootModel.localRotation = rotation;
        placementCollision.localRotation = rotation;
        FinishChanges();
    }

    public void UpdateSize()
    {
        if (!FindChildren())
        {
            size = Vector3.zero;
            sizeCm = Vector3.zero;
            return;
        }

        NormalizeRootScale();
        Physics.SyncTransforms();

        Renderer[] renderers = rootModel.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds bounds = new Bounds();
        foreach (Renderer renderer in renderers)
        {
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        size = found ? bounds.size : Vector3.zero;
        sizeCm = new Vector3(
            Mathf.Round(size.x * 1000f) / 10f,
            Mathf.Round(size.y * 1000f) / 10f,
            Mathf.Round(size.z * 1000f) / 10f);
    }

    public void ResizeObject()
    {
        if (!FindChildren() || targetSize <= 0f) return;

        RecordChanges("Resize Object");
        NormalizeRootScale();
        Physics.SyncTransforms();
        UpdateSize();

        float current = resizeBy == Dimension.Width ? size.x :
                        resizeBy == Dimension.Height ? size.y : size.z;

        if (current <= 0.000001f)
        {
            Debug.LogWarning("Selected dimension is zero.", this);
            return;
        }

        float factor = targetSize / current;
        ApplyScale(factor);
        FinishChanges();
        Debug.Log($"Resized {name}: multiplier {factor:F5}, final size {size}", this);
    }

    private bool TryGetLowestY(out float lowestY)
    {
        lowestY = float.PositiveInfinity;
        bool found = false;
        foreach (MeshFilter filter in rootModel.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh mesh = filter.sharedMesh;
            if (mesh == null) continue;
            Vector3[] vertices;
            try { vertices = mesh.vertices; }
            catch (Exception)
            {
                Debug.LogWarning($"Cannot read mesh {mesh.name}. Enable Read/Write.", filter);
                continue;
            }
            Matrix4x4 matrix = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            foreach (Vector3 vertex in vertices)
            {
                lowestY = Mathf.Min(lowestY, matrix.MultiplyPoint3x4(vertex).y);
                found = true;
            }
        }
        foreach (SkinnedMeshRenderer renderer in rootModel.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            Bounds b = renderer.localBounds;
            Matrix4x4 matrix = transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3(x, y, z));
                        lowestY = Mathf.Min(lowestY, matrix.MultiplyPoint3x4(corner).y);
                        found = true;
                    }
        }
        return found;
    }

    private void OffsetChildrenY(float offset)
    {
        rootModel.localPosition += Vector3.up * offset;
        placementCollision.localPosition += Vector3.up * offset;
    }

    public void AlignModelBase()
    {
        if (!FindChildren() || !TryGetLowestY(out float bottom))
        {
            Debug.LogWarning("Unable to find readable model geometry.", this);
            return;
        }
        RecordChanges("Align Model Base");
        OffsetChildrenY(-bottom);
        FinishChanges();
    }

    private bool TryMeasure(out float diameter, out float bottomY, out int count)
    {
        diameter = 0f;
        bottomY = 0f;
        count = 0;
        if (!FindChildren() || !TryGetLowestY(out bottomY)) return false;

        float maxY = bottomY + scanHeightMM / (1000f * Mathf.Max(Mathf.Abs(transform.lossyScale.y), 0.000001f));
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
        foreach (MeshFilter filter in rootModel.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh mesh = filter.sharedMesh;
            if (mesh == null) continue;
            Vector3[] vertices;
            try { vertices = mesh.vertices; }
            catch (Exception) { continue; }
            Matrix4x4 matrix = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            foreach (Vector3 vertex in vertices)
            {
                Vector3 p = matrix.MultiplyPoint3x4(vertex);
                if (p.y < bottomY - 0.000001f || p.y > maxY) continue;
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z);
                maxZ = Mathf.Max(maxZ, p.z);
                count++;
            }
        }
        if (count < minimumVertices) return false;
        diameter = Mathf.Max(maxX - minX, maxZ - minZ);
        return diameter > 0.000001f;
    }

    public void MeasureBase()
    {
        if (!TryMeasure(out float diameter, out float bottom, out int count))
        {
            Debug.LogWarning("Unable to measure base. Check mesh Read/Write and scan height.", this);
            return;
        }
        RecordChanges("Measure Base");
        measuredDiameterMM = diameter * 1000f * Mathf.Abs(transform.lossyScale.x);
        sampledVertices = count;
        FinishChanges();
        Debug.Log($"Base diameter: {measuredDiameterMM:F2} mm, bottom Y: {bottom:F5}, vertices: {count}", this);
    }

    public void ScaleToTarget()
    {
        if (!FindChildren() || targetDiameterMM <= 0f) return;

        RecordChanges("Scale Miniature Base");
        NormalizeRootScale();
        Physics.SyncTransforms();

        if (!TryMeasure(out float diameter, out _, out _))
        {
            Debug.LogWarning("Unable to measure base.", this);
            return;
        }

        // TryMeasure returns a diameter in parent-local coordinates.
        // Convert to world units before calculating the new parent scale.
        float currentWorldDiameterMM = diameter * 1000f *
            Mathf.Abs(transform.lossyScale.x);
        if (currentWorldDiameterMM <= 0.000001f) return;

        float factor = targetDiameterMM / currentWorldDiameterMM;
        if (float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0f) return;

        ApplyScale(factor);
        Physics.SyncTransforms();

        if (TryMeasure(out float newDiameter, out float bottom, out int count))
        {
            if (alignBottomToZero) OffsetChildrenY(-bottom);
            measuredDiameterMM = newDiameter * 1000f * Mathf.Abs(transform.lossyScale.x);
            sampledVertices = count;
        }

        lastScaleFactor = factor;
        FinishChanges();
        Debug.Log($"Scaled base to {targetDiameterMM:F2} mm, factor {factor:F5}", this);
    }

    private void FinishChanges()
    {
        Physics.SyncTransforms();
        UpdateSize();
#if UNITY_EDITOR
        SaveEverything();
#endif
    }
#if UNITY_EDITOR
    public void SaveEverything()
    {
        if (!FindChildren())
            return;

        Physics.SyncTransforms();
        UpdateSize();

        UnityEngine.Object[] changedObjects =
        {
        this,
        transform,
        rootModel,
        placementCollision
    };

        // Mark every modified object as dirty.
        foreach (UnityEngine.Object obj in changedObjects)
        {
            if (obj == null)
                continue;

            EditorUtility.SetDirty(obj);

            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            }
        }

        GameObject prefabRoot =
            PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);

        // CASE 1: Editing an instance inside a scene.
        if (prefabRoot != null &&
            !PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            string prefabPath =
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                    prefabRoot
                );

            if (!string.IsNullOrEmpty(prefabPath))
            {
                // Apply the modified components and transforms
                // to the source prefab.
                foreach (UnityEngine.Object obj in changedObjects)
                {
                    if (obj == null)
                        continue;

                    if (!PrefabUtility.IsPartOfPrefabInstance(obj))
                        continue;

                    PrefabUtility.ApplyObjectOverride(
                        obj,
                        prefabPath,
                        InteractionMode.AutomatedAction
                    );
                }

                AssetDatabase.SaveAssets();

                Debug.Log(
                    "Applied changes to prefab: " + prefabPath,
                    this
                );
            }
        }
        // CASE 2: Editing the prefab asset directly.
        else if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            GameObject assetRoot = transform.root.gameObject;

            PrefabUtility.SavePrefabAsset(assetRoot);
            AssetDatabase.SaveAssets();

            Debug.Log(
                "Saved prefab asset: " + assetRoot.name,
                this
            );
        }

        // CASE 3: Save changes to the scene.
        if (gameObject.scene.IsValid() &&
            gameObject.scene.isLoaded &&
            !PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);

            if (!string.IsNullOrEmpty(gameObject.scene.path))
            {
                EditorSceneManager.SaveScene(gameObject.scene);
            }
            else
            {
                Debug.LogWarning(
                    "Scene has not been saved to disk yet.",
                    this
                );
            }
        }

        Debug.Log("ObjectSize changes saved.", this);
    }


    private void OnDrawGizmosSelected()
    {
        if (!FindChildren()) return;
        Renderer[] renderers = rootModel.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        Vector3 centre = new Vector3(b.center.x, b.min.y + scanHeightMM / 1000f, b.center.z);
        float half = Mathf.Max(b.size.x, b.size.z) * 0.75f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(centre + Vector3.left * half, centre + Vector3.right * half);
        Gizmos.DrawLine(centre + Vector3.back * half, centre + Vector3.forward * half);
        Handles.Label(centre + Vector3.up * 0.005f, $"Scan Height: {scanHeightMM:F1} mm");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjectSize))]
public class ObjectSizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "size", "sizeCm",
            "measuredDiameterMM", "lastScaleFactor", "sampledVertices");
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Measured Results (Read Only)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            foreach (string property in new[] { "size", "sizeCm", "measuredDiameterMM", "lastScaleFactor", "sampledVertices" })
                EditorGUILayout.PropertyField(serializedObject.FindProperty(property));
        }

        ObjectSize item = (ObjectSize)target;
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Object Tools", EditorStyles.boldLabel);
        if (GUILayout.Button("Update Size", GUILayout.Height(30)))
        {
            Undo.RecordObject(item, "Update Size");
            item.UpdateSize();
            item.AlignModelBase();
            item.SaveEverything();
        }

        if (GUILayout.Button("Resize Object", GUILayout.Height(30)))
        {
            item.ResizeObject();
            item.AlignModelBase();
            item.SaveEverything();
        }

        if (GUILayout.Button("Bad Rotation (-90 X)", GUILayout.Height(30)))
        {
            item.UpdateRotation(true);
            item.AlignModelBase();
            item.SaveEverything();
        }

        if (GUILayout.Button("Normal Rotation (0 X)", GUILayout.Height(30)))
        {
            item.UpdateRotation(false);
            item.AlignModelBase();
            item.SaveEverything();
        }

        if (GUILayout.Button("Fix Model Base Y = 0", GUILayout.Height(30)))
        {
            item.AlignModelBase();
            item.SaveEverything();
        }

        EditorGUILayout.Space(15);

        if (GUILayout.Button("Measure Current Base", GUILayout.Height(30)))
        {
            item.MeasureBase();
            item.SaveEverything();
        }

        if (GUILayout.Button("Scale Base To Target Size", GUILayout.Height(36)))
        {
            item.ScaleToTarget();
            item.AlignModelBase();
            item.SaveEverything();
        }
    }
}
#endif
