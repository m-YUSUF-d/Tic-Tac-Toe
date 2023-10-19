using System;
using System.Reflection;

namespace UnityEngine.AdaptivePerformance
{
    internal class AdaptivePerformanceManagerSpawner : ScriptableObject
    {
        public const string AdaptivePerformanceManagerObjectName = "AdaptivePerformanceManager";

        GameObject m_ManagerGameObject;

        public GameObject ManagerGameObject { get { return m_ManagerGameObject; } }

        void OnEnable()
        {
            if (m_ManagerGameObject != null)
                return;

            m_ManagerGameObject = GameObject.Find(AdaptivePerformanceManagerObjectName);
        }

        public void Initialize(bool isCheckingProvider)
        {
            if (m_ManagerGameObject != null)
                return;

            m_ManagerGameObject = new GameObject(AdaptivePerformanceManagerObjectName);
            var apm = m_ManagerGameObject.AddComponent<AdaptivePerformanceManager>();

            if (isCheckingProvider)
            {
                // if no provider was found we can disable AP and destroy the game object, otherwise continue with initialization.
                if (apm.Indexer == null)
                {
                    Deinitialize();

                    return;
                }
            }

            Holder.Instance = apm;
            InstallScalers();
            DontDestroyOnLoad(m_ManagerGameObject);
        }
        public void Deinitialize()
        {
            if (m_ManagerGameObject == null)
                return;

            Destroy(m_ManagerGameObject);

            m_ManagerGameObject = null;
        }

        void InstallScalers()
        {
            Type ti = typeof(AdaptivePerformanceScaler);
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (ti.IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        ScriptableObject.CreateInstance(t);
                    }
                }
            }
        }
    }

    internal static class AdaptivePerformanceInitializer
    {
        static AdaptivePerformanceManagerSpawner s_Spawner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoInitialize()
        {
            InitializeSpawner(isAuto: true);
        }

        public static void Initialize()
        {
            InitializeSpawner(isAuto: false);
        }

        public static void Deinitialize()
        {
            if (s_Spawner == null)
                return;

            s_Spawner.Deinitialize();
            UnityEngine.Object.Destroy(s_Spawner);
            s_Spawner = null;
        }

        static void InitializeSpawner(bool isAuto)
        {
            if (s_Spawner == null)
                s_Spawner = ScriptableObject.CreateInstance<AdaptivePerformanceManagerSpawner>();

            if (s_Spawner != null && s_Spawner.ManagerGameObject != null)
                return;

            // isAuto translates to isCheckingProvider:
            //    - IsAuto=True, then isCheckingProvider=true; DO check if provider is present
            //    - IsAuto=False, then isCheckingProvider=false; DON'T check if provider is present
            // the reason for this is the auto startup process initializes providers, and should be available at the
            // time of the check.  During a manual startup, the providers have not yet been initialized, so skipping
            // the check so that the AP Manager is created and available to be initialized and the provider is then
            // made available.
            s_Spawner.Initialize(isCheckingProvider: isAuto);
        }
    }
}
