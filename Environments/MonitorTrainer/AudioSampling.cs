using UnityEngine;

namespace MonitorTrainer
{
    public class AudioSampling : MonoBehaviour
    {
        private const int NUM_AUDIO_SAMPLES = 512;
        private FFTWindow m_FFTWindow = FFTWindow.Hamming;
        private AudioSource m_Audio;
        private float[] m_AudioSamples;

        private float[] m_FreqBand = new float[8];
        private float m_FreqBandTotal = 0;

        public float GetFrequencyBand(int index) => m_FreqBand[index];

        public float FreqBandTotal => m_FreqBandTotal;

        public void Init(AudioSource source)
        {
            m_Audio = source;
            m_AudioSamples = new float[NUM_AUDIO_SAMPLES];
        }

        private void Update()
        {
            if (null != m_Audio)
            {
                m_Audio.GetSpectrumData(m_AudioSamples, 0, m_FFTWindow);
                OnSpectrumUpdate(m_AudioSamples);
            }
        }

        public void OnSpectrumUpdate(float[] samples)
        {
            int index = 0;
            for (int i = 0; i < 8; i++)
            {
                float average = 0;
                int sampleCount = (int)Mathf.Pow(2, i) * 2;

                if (i == 7)
                {
                    sampleCount += 2;
                }

                for (int j = 0; j < sampleCount; j++)
                {
                    average += samples[index] * (index + 1);
                    index++;
                }
                average /= index;
                m_FreqBand[i] = average * 10;
            }

            m_FreqBandTotal = 0;
            for (int i = 0; i < 8; i++)
            {
                m_FreqBandTotal += m_FreqBand[i];
            }

            m_FreqBandTotal /= 8;
        }
    }
}