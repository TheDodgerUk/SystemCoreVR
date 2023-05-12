using UnityEngine;
using System.Collections;

public class Rotors : MonoBehaviour {

    float m_RotorSpeedSlowDown = 200;

    public enum RotorState
    {
        PowerUp,
        Constant,
        PowerDown,
    }

    public enum EDirection
    {
        ROTATE_X,
        ROTATE_Y,
        ROTATE_Z,
    };
    [System.Serializable]
    public class CRotors
    {
        public GameObject  m_rotor;
        public float       m_rotorSpeed = 3000;
        public float       m_rotorSpeedMax = 4000;
        public float       m_maxPitch = 2;
        public float       m_volume = 2;
        public EDirection  m_direction  = EDirection.ROTATE_X;
        public AudioClip   m_sound;
        [HideInInspector]
        public AudioSource m_audioSource;
    };

    public CRotors[] m_rotors;
    private RotorState m_RotorState = RotorState.Constant;
    Vector3 m_rotationDirection;
	// Use this for initialization
	void Start() 
    {

        for (int i = 0 ; i < m_rotors.Length ; i++)
        {

            if (m_rotors[i].m_sound)
            {
                m_rotors[i].m_audioSource              = gameObject.AddComponent<AudioSource>();
                m_rotors[i].m_audioSource.loop         = true;
                m_rotors[i].m_audioSource.clip         = m_rotors[i].m_sound;
                // m_rotors[i].m_audioSource.volume    = m_rotors[i].m_volume;
                m_rotors[i].m_audioSource.rolloffMode  = AudioRolloffMode.Logarithmic;
                m_rotors[i].m_audioSource.spatialBlend = 1;
                m_rotors[i].m_audioSource.minDistance  = 40;
                m_rotors[i].m_audioSource.maxDistance  = 500;
                m_rotors[i].m_audioSource.Play();   
            }
        }
	}

    //--------------------------------------------------------------------------------------------------------------------
    void Update () 
    {

        //-----------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------
        foreach (CRotors clone in m_rotors)
        {
            switch(m_RotorState)
            {
                case RotorState.Constant:
                    break;

                case RotorState.PowerDown:
                    clone.m_rotorSpeed -= Time.deltaTime * m_RotorSpeedSlowDown;
                    clone.m_rotorSpeed = Mathf.Clamp(clone.m_rotorSpeed, 0, m_rotors[0].m_rotorSpeedMax);
                    break;
            }
        }

        //-----------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------
        for(int i = 0; i < m_rotors.Length; i++)
        {
            m_rotationDirection = GetRotationDirection(i, true);
            m_rotors[i].m_rotor.transform.Rotate(m_rotationDirection);
            //if (m_rotors[i].m_sound)
            //    m_rotors[i].m_audioSource.pitch = ((m_rotors[i].m_rotorSpeed / m_rotors[i].m_rotorSpeedMax) * m_rotors[i].m_maxPitch);
        }


	}

    //--------------------------------------------------------------------------------------------------------------------
    public Vector3 GetRotationDirection(int lIndex, bool lWithDeltaTime)
    {
        float lTime = 1;
        if (lWithDeltaTime == true)
        {
            lTime = Time.deltaTime;
        }

        Vector3 lDirection = Vector3.zero;
        switch (m_rotors[lIndex].m_direction)
        {
            case EDirection.ROTATE_X: lDirection = new Vector3(m_rotors[lIndex].m_rotorSpeed * lTime, 0, 0); break;
            case EDirection.ROTATE_Y: lDirection = new Vector3(0, m_rotors[lIndex].m_rotorSpeed * lTime, 0); break;
            case EDirection.ROTATE_Z: lDirection = new Vector3(0, 0, m_rotors[lIndex].m_rotorSpeed * lTime); break;
        }
        return lDirection;
    }


    //--------------------------------------------------------------------------------------------------------------------
    public void SetRotorState(RotorState lRotorState)
    {
        m_RotorState = lRotorState;
    }
}
