using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AdaptivePerformance;

public class PerformanceModeVisualisation : MonoBehaviour
{
    public Text performanceModeLabel;
    public Image performanceModePanel;

    IAdaptivePerformance m_AP;

    void OnPerformanceModeEvent(PerformanceMode performanceMode)
    {
        performanceModeLabel.text = performanceMode.ToString();
        switch (performanceMode)
        {
            case PerformanceMode.Optimize:
            case PerformanceMode.CPU:
            case PerformanceMode.GPU:
                performanceModePanel.color = Color.green;
                break;
            case PerformanceMode.Unknown:
                performanceModePanel.color = Color.magenta;
                break;
            case PerformanceMode.Standard:
                performanceModePanel.color = Color.cyan;
                break;
            case PerformanceMode.Battery:
                performanceModePanel.color = Color.yellow;
                break;
        }
    }

    void Start()
    {
        m_AP = Holder.Instance;
        if (m_AP == null)
        {
            Debug.Log("[Performance Mode Visualization] Warning Adaptive Performance Manager was not found and does not report");
            return;
        }

        m_AP.PerformanceModeStatus.PerformanceModeEvent += OnPerformanceModeEvent;
        OnPerformanceModeEvent(m_AP.PerformanceModeStatus.PerformanceMode);
    }
}
