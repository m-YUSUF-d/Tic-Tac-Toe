using System.Collections;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.UI;

public class LifecycleControl : MonoBehaviour
{
    private IAdaptivePerformance ap;

    public Toggle adaptivePerformanceInitialized;
    public Toggle adaptivePerformanceRunning;

    public Text provider;
    public Toggle providerInitialized;
    public Toggle providerRunning;

    public Text lastLifecycleEventReceived;
    public Text lastCommandAttempted;

    void Start()
    {
        Holder.LifecycleEventHandler += OnAdaptivePerformanceLifecycleEventHandler;

        StartCoroutine(TestTimeout());

        // if "Initialize Adaptive Performance on Startup" is unchecked, early exit will occur here
        // and Adaptive Performance must be initialized manually
        if (Holder.Instance == null)
            return;

        ap = Holder.Instance;
        UpdateAdaptivePerformanceStateInfo();

        // subscribe to any event handlers on new instance
        // example: ap.ThermalStatus.ThermalEvent += OnThermalEvent;
    }

    void OnDestroy()
    {
        Holder.LifecycleEventHandler -= OnAdaptivePerformanceLifecycleEventHandler;
        Holder.Deinitialize();
    }

    void OnAdaptivePerformanceLifecycleEventHandler(IAdaptivePerformance instance, LifecycleChangeType changeType)
    {
        if (changeType == LifecycleChangeType.Destroyed)
        {
            // unsubscribe from any event handlers on old instance
            // example: ap.ThermalStatus.ThermalEvent -= OnThermalEvent;

            ap = null;
        }
        else if(changeType == LifecycleChangeType.Created)
        {
            ap = instance;

            // reset any settings
            ap.DevelopmentSettings.Logging = true;
            ap.DevicePerformanceControl.AutomaticPerformanceControl = false;

            // subscribe to any event handlers on new instance
            // example: ap.ThermalStatus.ThermalEvent += OnThermalEvent;
        }

        UpdateLastLifecycleEventReceived(changeType);
        UpdateAdaptivePerformanceStateInfo();
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

    private void UpdateAdaptivePerformanceStateInfo()
    {
        var settings = AdaptivePerformanceGeneralSettings.Instance;

        adaptivePerformanceInitialized.isOn = ap != null && ap.Initialized;
        adaptivePerformanceRunning.isOn = ap != null && ap.Active;

        var activeLoader = $"{settings.Manager.activeLoader}";
        if (string.IsNullOrWhiteSpace(activeLoader))
            activeLoader = "N/A";

        provider.text = activeLoader;
        providerInitialized.isOn = settings.IsProviderInitialized;
        providerRunning.isOn = settings.IsProviderStarted;
    }

    private void UpdateLastCommandAttemptInfo(string command)
    {
        lastCommandAttempted.text = $"Last Command: {command}";
    }

    private void UpdateLastLifecycleEventReceived(LifecycleChangeType changeType)
    {
        lastLifecycleEventReceived.text = $"Last Lifecycle Event: {changeType}";
    }

    public void InitializeAdaptivePerformance()
    {
        if (ap != null || Holder.Instance != null)
        {
            Debug.Log("Adaptive Performance is already initialized.");
            return;
        }

        Holder.Initialize();

        UpdateLastCommandAttemptInfo("Initialize");
        UpdateAdaptivePerformanceStateInfo();
    }

    public void DeinitializeAdaptivePerformance()
    {
        if (ap == null)
        {
            Debug.Log("You must initialize Adaptive Performance before attempting to deinitialize.");
            return;
        }

        Holder.Deinitialize();

        UpdateLastCommandAttemptInfo("Deinitialize");
        UpdateAdaptivePerformanceStateInfo();
    }

    public void StartAdaptivePerformance()
    {
        if (ap == null)
        {
            Debug.Log("You must initialize Adaptive Performance before attempting to start.");
            return;
        }

        if (ap.Active)
        {
            Debug.Log("Adaptive Performance is already running.");
            return;
        }

        ap.StartAdaptivePerformance();

        UpdateLastCommandAttemptInfo("Start");
        UpdateAdaptivePerformanceStateInfo();
    }

    public void StopAdaptivePerformance()
    {
        if (ap == null)
        {
            Debug.Log("You must start Adaptive Performance before attempting to stop.");
            return;
        }

        if (!ap.Active)
        {
            Debug.Log("Adaptive Performance is already stopped.");
            return;
        }

        ap.StopAdaptivePerformance();

        UpdateLastCommandAttemptInfo("Stop");
        UpdateAdaptivePerformanceStateInfo();
    }
}
