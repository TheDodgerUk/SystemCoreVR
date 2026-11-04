
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class DynamicImagePlane : MonoBehaviour
{
    [Header("Plane Settings")]
    [SerializeField]
    private Vector2 planeSize = new Vector2(0.1f, 0.1f);

    [SerializeField]
    private Texture2D initialImage;

    [SerializeField]
    private Vector3 planePosition;

    [SerializeField]
    private Vector3 planeRotation;

    private GameObject imagePlane;
    private MeshRenderer meshRenderer;
    private Material materialInstance;
    private Texture currentTexture;

    public void CreatePlane(
        Vector3 position,
        Vector2 size,
        Texture2D image)
    {
        planePosition = position;
        planeSize = size;
        initialImage = image;

        RegeneratePlane();
    }

    public void RegeneratePlane()
    {
        // Find an existing Quad, including after
        // Unity recompiles scripts.
        if (imagePlane == null)
        {
            Transform existing = transform.Find("DynamicImagePlane");

            if (existing != null)
                imagePlane = existing.gameObject;
        }

        // Remove existing Quad.
        if (imagePlane != null)
        {
            if (Application.isPlaying)
                Destroy(imagePlane);
            else
                DestroyImmediate(imagePlane);
        }

        // Create new Quad.
        imagePlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        imagePlane.name = "DynamicImagePlane";

        imagePlane.transform.SetParent(transform, false);
        imagePlane.transform.position = planePosition;
        imagePlane.transform.rotation =
            Quaternion.Euler(planeRotation);

        // Remove collider.
        Collider collider = imagePlane.GetComponent<Collider>();

        if (collider != null)
        {
            if (Application.isPlaying)
                Destroy(collider);
            else
                DestroyImmediate(collider);
        }

        // Create material.
        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Texture");

        materialInstance = new Material(shader);

        meshRenderer = imagePlane.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = materialInstance;

        currentTexture = initialImage;

        if (currentTexture != null)
            materialInstance.mainTexture = currentTexture;

        UpdateScale();
    }

    private void UpdateScale()
    {
        if (imagePlane == null)
            return;

        float width = planeSize.x;
        float height = planeSize.y;

        if (currentTexture != null && currentTexture.height > 0)
        {
            float aspectRatio =
                (float)currentTexture.width /
                currentTexture.height;

            height = width / aspectRatio;
        }

        imagePlane.transform.localScale =
            new Vector3(width, height, 1f);
    }

    public void SetImage(Texture2D image)
    {
        initialImage = image;
        ApplyTexture(image);
    }

    public void SetImage(RenderTexture image)
    {
        ApplyTexture(image);
    }

    private void ApplyTexture(Texture image)
    {
        currentTexture = image;

        if (materialInstance != null)
            materialInstance.mainTexture = image;

        UpdateScale();
    }

    public void SetRotation(Vector3 rotation)
    {
        planeRotation = rotation;

        if (imagePlane != null)
            imagePlane.transform.rotation = Quaternion.Euler(rotation);
    }

    public void SetPosition(Vector3 position)
    {
        planePosition = position;

        if (imagePlane != null)
            imagePlane.transform.position = position;
    }

    public void SetSize(Vector2 size)
    {
        planeSize = size;
        UpdateScale();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(DynamicImagePlane))]
public class DynamicImagePlaneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Regenerate Plane", GUILayout.Height(35)))
        {
            DynamicImagePlane plane =
                (DynamicImagePlane)target;

            Undo.RegisterFullObjectHierarchyUndo(
                plane.gameObject,
                "Regenerate Image Plane");

            plane.RegeneratePlane();

            EditorUtility.SetDirty(plane);

            if (PrefabUtility.IsPartOfPrefabInstance(plane))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    plane);
            }

            if (plane.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(
                    plane.gameObject.scene);
            }
        }
    }
}
#endif
