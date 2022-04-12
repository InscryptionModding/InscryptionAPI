using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Helpers
{
    public class BetterSingleton<T> : ManagedBehaviour where T : ManagedBehaviour
    {
        public static T Instance
        {
            get
            {
                T result;
                if (m_ShuttingDown)
                {
                    result = default;
                    return result;
                }
                object @lock = m_Lock;
                lock (@lock)
                {
                    FindInstance();
                    result = m_Instance;
                }
                return result;
            }
        }
        
        protected static void FindInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = (T)FrameLoopManager.Instance.activeBehaviours.Find(x => x != null && x is T && x.isActiveAndEnabled);
            }
        }

        private void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }

        private static bool m_ShuttingDown = false;
        private static object m_Lock = new();
        private static T m_Instance;
    }
}
