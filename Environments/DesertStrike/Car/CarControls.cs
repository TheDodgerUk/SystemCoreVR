using UnityEngine;
using System.Collections;

public class CarControls : MonoBehaviour
{
    public enum EWheelType
    {
        POWER,
        STEER,
        POWER_STEER,
        POWER_REVERSE_STEER,
        REVERSE_STEER,

    };
    [System.Serializable]
    public class Wheels
    {
        public string m_pairName;
        public WheelCollider m_wheelLeftCollider;
        public WheelCollider m_wheelRightCollider;

        public float m_antiRoll = 5000.0f;
        public float m_suspensionDistance = 0.1f;
        public EWheelType m_wheelType = EWheelType.POWER;
    }

    public Wheels[] m_wheelPair = new Wheels[2];
    public int m_numberOfGears = 6;
    private float[] engineForceValues;
    private float[] m_gears;
    private int m_currentGear = 0;
    private float m_currentEnginePower = 0;


    private Wheels m_powerWheels;
    private Vector3 m_relativeVelocity;

    public AudioSource m_engineSound;
    public Transform m_centreOfMass;


    [System.Serializable]
    public class CLimits
    {

        public float m_inputSteerMax = 20;
        public float m_inputSteerMuliplyer = 10;
        public float m_inputSteerDrag = 0.99f;


        public float m_inputTorqueMuliplyer = 10f;
        public float m_inputTorqueDrag = 0.99f;
        public float m_maxSpeed = 250;
        [HideInInspector]
        public float m_inputSteer = 0;
        [HideInInspector]
        public float m_inputTorque = 0;

    }
    public CLimits m_limits;
    bool m_handbrake = false;


    /// <summary>
    /// ////////////////////////////////////////////////////////////////
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>


    float Convert_Miles_Per_Hour_To_Meters_Per_Second(float value) { return value * 0.44704f; }

    public int GetCurrentGear() { return m_currentGear; }

    public void InputSteer(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);

        if (amount > 0)
        {
            if (amount > m_limits.m_inputSteer / m_limits.m_inputSteerMax)
                m_limits.m_inputSteer += amount * m_limits.m_inputSteerMuliplyer * Time.deltaTime;
        }
        else
        {
            if (amount < m_limits.m_inputSteer / m_limits.m_inputSteerMax)
                m_limits.m_inputSteer += amount * m_limits.m_inputSteerMuliplyer * Time.deltaTime;
        }


        m_limits.m_inputSteer = Mathf.Clamp(m_limits.m_inputSteer, -m_limits.m_inputSteerMax, m_limits.m_inputSteerMax);
    }

    public void InputTorque(float amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        m_limits.m_inputTorque += amount * m_limits.m_inputTorqueMuliplyer * Time.deltaTime;
    }

    public float GetTorque()
    {
        return m_limits.m_inputTorque;
    }

    public void HandBrake()
    {
        m_handbrake = true;

    }

    // Use this for initialization
    void Start()
    {
        //check all wheels added have been added correctly
        for (int i = 0; i < m_wheelPair.Length; i++) // error checking 
        {
            if (!m_wheelPair[i].m_wheelLeftCollider)
                Debug.LogError("Left Collider number: " + i + " Is not valid");
            if (!m_wheelPair[i].m_wheelRightCollider)
                Debug.LogError("Right Collider number: " + i + " Is not valid");
        }

        // Set wheels
        for (int i = 0; i < m_wheelPair.Length; i++)
        {
            m_wheelPair[i].m_wheelLeftCollider.suspensionDistance = m_wheelPair[i].m_suspensionDistance;
            m_wheelPair[i].m_wheelRightCollider.suspensionDistance = m_wheelPair[i].m_suspensionDistance;

            if (m_wheelPair[i].m_wheelType != EWheelType.STEER) // any kind ecept  plain steer
                m_powerWheels = m_wheelPair[i];
        }




        // set Centre of Mass
        if (m_centreOfMass)
            GetComponent<Rigidbody>().centerOfMass = m_centreOfMass.transform.localPosition;
        else
            Debug.LogError("No Centre of mass Placed");




        m_limits.m_maxSpeed = Convert_Miles_Per_Hour_To_Meters_Per_Second(m_limits.m_maxSpeed);
        SetupGears();
    }


    void SetupGears()
    {
        engineForceValues = new float[m_numberOfGears];
        m_gears = new float[m_numberOfGears];

        float tempTopSpeed = m_limits.m_maxSpeed;

        for (var i = 0; i < m_numberOfGears; i++)
        {
            if (i > 0)
                m_gears[i] = tempTopSpeed / 4 + m_gears[i - 1];
            else
                m_gears[i] = tempTopSpeed / 4;

            tempTopSpeed -= tempTopSpeed / 4;
        }

        float engineFactor = m_limits.m_maxSpeed / m_gears[m_gears.Length - 1];

        for (int i = 0; i < m_numberOfGears; i++)
        {
            float maxLinearDrag = m_gears[i] * m_gears[i];// * dragMultiplier.z;
            engineForceValues[i] = maxLinearDrag * engineFactor;
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // drag
        m_limits.m_inputSteer *= m_limits.m_inputSteerDrag;
        m_limits.m_inputTorque *= m_limits.m_inputTorqueDrag;

        m_relativeVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);


        WheelsSuspention();
        RPMCalc();

        ShiftGears();
        Sound();


        ApplyForcesToWheels();
    }


    void CalculateEnginePower()
    {


        if (m_currentGear == 0)
            m_currentEnginePower = Mathf.Clamp(m_currentEnginePower, 0, engineForceValues[0]);
        else
            m_currentEnginePower = Mathf.Clamp(m_currentEnginePower, engineForceValues[m_currentGear - 1], engineForceValues[m_currentGear]);
    }


    void ApplyForcesToWheels()
    {
        for (int i = 0; i < m_wheelPair.Length; i++)
        {
            switch (m_wheelPair[i].m_wheelType)
            {
                case EWheelType.POWER:
                    m_wheelPair[i].m_wheelLeftCollider.motorTorque = m_limits.m_inputTorque / m_gears[m_currentGear] * engineForceValues[m_currentGear];
                    m_wheelPair[i].m_wheelRightCollider.motorTorque = m_limits.m_inputTorque / m_gears[m_currentGear] * engineForceValues[m_currentGear];
                    break;
                case EWheelType.POWER_STEER:
                    m_wheelPair[i].m_wheelLeftCollider.motorTorque = m_limits.m_inputTorque / m_gears[m_currentGear] * engineForceValues[m_currentGear];
                    m_wheelPair[i].m_wheelRightCollider.motorTorque = m_limits.m_inputTorque / m_gears[m_currentGear] * engineForceValues[m_currentGear];
                    m_wheelPair[i].m_wheelLeftCollider.steerAngle = m_limits.m_inputSteer;
                    m_wheelPair[i].m_wheelRightCollider.steerAngle = m_limits.m_inputSteer;
                    break;
                case EWheelType.STEER:
                    //  m_wheelPair[i].m_wheelLeftCollider.motorTorque = m_engine.m_engineTorque / m_gears[m_currentGear] * m_inputTorque;
                    // m_wheelPair[i].m_wheelRightCollider.motorTorque = m_engine.m_engineTorque / m_gears[m_currentGear] * m_inputTorque;
                    m_wheelPair[i].m_wheelLeftCollider.steerAngle = m_limits.m_inputSteer;
                    m_wheelPair[i].m_wheelRightCollider.steerAngle = m_limits.m_inputSteer;
                    break;
                case EWheelType.REVERSE_STEER:
                    m_wheelPair[i].m_wheelLeftCollider.steerAngle = -m_limits.m_inputSteer;
                    m_wheelPair[i].m_wheelRightCollider.steerAngle = -m_limits.m_inputSteer;
                    break;

                case EWheelType.POWER_REVERSE_STEER:
                    m_wheelPair[i].m_wheelLeftCollider.motorTorque = m_limits.m_inputTorque / m_gears[m_currentGear] * engineForceValues[m_currentGear];
                    m_wheelPair[i].m_wheelRightCollider.motorTorque = m_limits.m_inputTorque / m_gears[m_currentGear] * engineForceValues[m_currentGear];
                    m_wheelPair[i].m_wheelLeftCollider.steerAngle = -m_limits.m_inputSteer;
                    m_wheelPair[i].m_wheelRightCollider.steerAngle = -m_limits.m_inputSteer;
                    break;
            }
        }

    }



    void RPMCalc()
    {
        // jsut the irst set of wheels
        // m_engine.m_engineRPM = (m_wheelPair[0].m_wheelLeftCollider.rpm + m_wheelPair[0].m_wheelRightCollider.rpm) / 2 * m_gears[m_currentGear];
    }
    void Sound()
    {
        if (!m_engineSound) return;

        // audio.pitch = Mathf.Abs(m_engine.m_engineRPM / m_engine.m_maxEngineRPM) + 1.0f;
        // this line is just to ensure that the pitch does not reach a value higher than is desired.
        if (GetComponent<AudioSource>().pitch > 2.0)
            GetComponent<AudioSource>().pitch = 2.0f;


    }

    void WheelsSuspention()
    {
        for (int i = 0; i < m_wheelPair.Length; i++)
        {
            Wheels wheel = m_wheelPair[i];
            WheelHit hit;
            float travelL = 1.0f;
            float travelR = 1.0f;

            var groundedL = wheel.m_wheelLeftCollider.GetGroundHit(out hit);
            if (groundedL)
                travelL = (-wheel.m_wheelLeftCollider.transform.InverseTransformPoint(hit.point).y - wheel.m_wheelLeftCollider.radius)
                          / wheel.m_wheelLeftCollider.suspensionDistance;

            var groundedR = wheel.m_wheelRightCollider.GetGroundHit(out hit);
            if (groundedR)
                travelR = (-wheel.m_wheelRightCollider.transform.InverseTransformPoint(hit.point).y - wheel.m_wheelRightCollider.radius)
                          / wheel.m_wheelRightCollider.suspensionDistance;

            var antiRollForce = (travelL - travelR) * wheel.m_antiRoll;

            if (groundedL)
                GetComponent<Rigidbody>().AddForceAtPosition(wheel.m_wheelLeftCollider.transform.up * -antiRollForce, wheel.m_wheelLeftCollider.transform.position);
            if (groundedR)
                GetComponent<Rigidbody>().AddForceAtPosition(wheel.m_wheelRightCollider.transform.up * antiRollForce, wheel.m_wheelRightCollider.transform.position);
        }

    }

    void ShiftGears()
    {
        m_currentGear = 0;
        for (var i = 0; i < m_numberOfGears - 1; i++)
        {
            if (m_relativeVelocity.z > m_gears[i])
                m_currentGear = i + 1;
        }
    }
}
