using System.Collections;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class PerformanceModeControl : MonoBehaviour
{
    public SampleFactory objectFactory;

    IAdaptivePerformance m_AP;
    int m_OriginalLimitCount;

    void OnPerformanceModeEvent(PerformanceMode performanceMode)
    {
        switch (performanceMode)
        {
            case PerformanceMode.Optimize:
            case PerformanceMode.CPU:
            case PerformanceMode.GPU:
                objectFactory.LimitCount = m_OriginalLimitCount;
                break;
            case PerformanceMode.Unknown:
                objectFactory.LimitCount = m_OriginalLimitCount / 10;
                break;
            case PerformanceMode.Standard:
                objectFactory.LimitCount = m_OriginalLimitCount / 4;
                break;
            case PerformanceMode.Battery:
                objectFactory.LimitCount = m_OriginalLimitCount / 25;
                break;
        }
    }

    void Start()
    {
        m_AP = Holder.Instance;
        if (m_AP == null)
        {
            Debug.Log("[Performance Mode Control] Warning Adaptive Performance Manager was not found and does not report");
            return;
        }

        m_OriginalLimitCount = objectFactory.LimitCount;
        m_AP.PerformanceModeStatus.PerformanceModeEvent += OnPerformanceModeEvent;
        OnPerformanceModeEvent(m_AP.PerformanceModeStatus.PerformanceMode);

        StartCoroutine(TestTimeout());
    }

    IEnumerator TestTimeout()
    {
        while (true)
        {
            yield return new WaitForSeconds(300);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
