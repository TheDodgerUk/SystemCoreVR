using UnityEngine;
using System.Collections;

public class WheelAlignment : MonoBehaviour
{


    // Define the variables used in the script, the Corresponding collider is the wheel collider at the position of
    // the visible wheel, the slip prefab is the prefab instantiated when the wheels slide, the rotation value is the
    // value used to rotate the wheel around it's axel.
    public WheelCollider m_correspondingCollider;
    public GameObject    m_slipPrefab;
    private float        m_rotationValue = 0.0f;

    void Start()
    {
        if (!m_correspondingCollider)
            Debug.LogError("collider not set , Please Set :" + this);
    }


    void FixedUpdate()
    {

        // define a hit point for the raycast collision
        RaycastHit hit;
        // Find the collider's center point, you need to do this because the center of the collider might not actually be
        // the real position if the transform's off.
        Vector3 ColliderCenterPoint = m_correspondingCollider.transform.TransformPoint(m_correspondingCollider.center);

        // now cast a ray out from the wheel collider's center the distance of the suspension, if it hit something, then use the "hit"
        // variable's data to find where the wheel hit, if it didn't, then se tthe wheel to be fully extended along the suspension.
        if (Physics.Raycast(ColliderCenterPoint, -m_correspondingCollider.transform.up, out hit, m_correspondingCollider.suspensionDistance + m_correspondingCollider.radius))
        {
            transform.position = hit.point + (m_correspondingCollider.transform.up * m_correspondingCollider.radius);
        }
        else
        {
            transform.position = ColliderCenterPoint - (m_correspondingCollider.transform.up * m_correspondingCollider.suspensionDistance);
        }

        // now set the wheel rotation to the rotation of the collider combined with a new rotation value. This new value
        // is the rotation around the axle, and the rotation from steering input.
        transform.rotation = m_correspondingCollider.transform.rotation * Quaternion.Euler(m_rotationValue, m_correspondingCollider.steerAngle, 0);
        // increase the rotation value by the rotation speed (in degrees per second)
        m_rotationValue += m_correspondingCollider.rpm * (360 / 60) * Time.deltaTime;

        // define a wheelhit object, this stores all of the data from the wheel collider and will allow us to determine
        // the slip of the tire.
        WheelHit CorrespondingGroundHit;
        m_correspondingCollider.GetGroundHit(out CorrespondingGroundHit);

        // if the slip of the tire is greater than 2.0, and the slip prefab exists, create an instance of it on the ground at
        // a zero rotation.
        if (Mathf.Abs(CorrespondingGroundHit.sidewaysSlip) > 2.0)
        {
            if (m_slipPrefab)
            {
                Instantiate(m_slipPrefab, CorrespondingGroundHit.point, Quaternion.identity);
            }
        }

    }
}