using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Vector3 originPosition;
    Quaternion originRotation;

    float shake_decay;
    float shake_intensity;
	 
    void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;

    }

    void Update()
    {
        if (shake_intensity > 0)
        {
            transform.position = originPosition + Random.insideUnitSphere * shake_intensity;
            transform.rotation = new Quaternion(
                            originRotation.x + Random.Range(-shake_intensity, shake_intensity) * 0.2f,
                            originRotation.y + Random.Range(-shake_intensity, shake_intensity) * 0.2f,
                            originRotation.z + Random.Range(-shake_intensity, shake_intensity) * 0.2f,
                            originRotation.w + Random.Range(-shake_intensity, shake_intensity) * 0.2f);
            shake_intensity -= shake_decay;
        }
    }

    public void Shake(float intensity, float decay)
    {

        shake_intensity = intensity;
        shake_decay = decay;
    }
}
