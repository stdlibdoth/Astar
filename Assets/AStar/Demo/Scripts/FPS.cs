using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    [SerializeField] private Text m_text;

    [SerializeField] [Range(0.1f,5)]private float m_updateFreq;
    [SerializeField] [Range(1,60)]private int m_consecSampleCount;

    private List<float> m_intervals;
    private float m_lastUpdateTime;
    private void Awake()
    {
        m_intervals = new List<float>();
    }

    private void Update()
    {
        float updateInterval = 1 / m_updateFreq;

        m_intervals.Insert(0,Time.deltaTime);
        if (m_intervals.Count > m_consecSampleCount)
            m_intervals.RemoveRange(m_consecSampleCount, m_intervals.Count - m_consecSampleCount);

        if (Time.time - m_lastUpdateTime >= updateInterval)
        {
            float sum = 0;
            for (int i = 0; i < m_intervals.Count; i++)
            {
                sum += m_intervals[i];
            }

            m_text.text = "FPS:" + (1.0f/(sum / m_consecSampleCount)).ToString("F2");
            m_lastUpdateTime = Time.time;
        }
    }
}
