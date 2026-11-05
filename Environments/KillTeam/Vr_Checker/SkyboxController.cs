using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    private void Start()
    {
        ApplySkybox();
    }

    public void ApplySkybox()
    {
        Material skyboxMaterial = RenderSettings.skybox;

        if (skyboxMaterial == null)
        {
            Shader shader = Shader.Find("Skybox/Procedural");

            if (shader == null)
            {
                Debug.LogError("Skybox shader not found!");
                return;
            }

            skyboxMaterial = new Material(shader);
            RenderSettings.skybox = skyboxMaterial;
        }

        foreach (Camera cam in Camera.allCameras)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
        }

        DynamicGI.UpdateEnvironment();

        Debug.Log("Skybox applied: " + skyboxMaterial.name);
    }
}