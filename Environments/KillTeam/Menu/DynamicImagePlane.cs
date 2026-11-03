
using UnityEngine;

public class DynamicImagePlane : MonoBehaviour
{
    private GameObject imagePlane;
    private MeshRenderer meshRenderer;
    private Material materialInstance;

    public void CreatePlane(
        Vector3 position,
        Vector2 size,
        Texture2D image)
    {
        // Remove existing plane.
        if (imagePlane != null)
            Destroy(imagePlane);

        // Create a plane.
        imagePlane = GameObject.CreatePrimitive(PrimitiveType.Quad);

        imagePlane.name = "DynamicImagePlane";

        imagePlane.transform.SetParent(transform, false);
        imagePlane.transform.position = position;

        // Quad is 1x1 units.
        imagePlane.transform.localScale =
            new Vector3(size.x, size.y, 1f);

        // Remove collider if not needed.
        Collider collider = imagePlane.GetComponent<Collider>();

        if (collider != null)
            Destroy(collider);

        // Create URP unlit material.
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Texture");

        materialInstance = new Material(shader);

        meshRenderer = imagePlane.GetComponent<MeshRenderer>();

        meshRenderer.material = materialInstance;

        // Assign initial image.
        SetImage(image);
    }

    public void SetImage(Texture2D image)
    {
        if (materialInstance == null || image == null)
            return;

        materialInstance.mainTexture = image;
    }

    public void SetImage(RenderTexture image)
    {
        if (materialInstance == null || image == null)
            return;

        materialInstance.mainTexture = image;
    }

    public void SetRotation(Quaternion rotation)
    {
        if (imagePlane != null)
            imagePlane.transform.rotation = rotation;
    }

    public void SetPosition(Vector3 position)
    {
        if (imagePlane != null)
            imagePlane.transform.position = position;
    }

    public void SetSize(Vector2 size)
    {
        if (imagePlane != null)
        {
            imagePlane.transform.localScale =
                new Vector3(size.x, size.y, 1f);
        }
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);

        if (imagePlane != null)
            Destroy(imagePlane);
    }
}
