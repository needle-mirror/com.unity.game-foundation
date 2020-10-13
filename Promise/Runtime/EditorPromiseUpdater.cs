#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEditor.Promise
{
    class EditorPromiseUpdater : IPromiseUpdater
    {
        List<IEnumerator> m_Routines = new List<IEnumerator>();

        public EditorPromiseUpdater()
        {
            EditorApplication.update += Update;
        }

        public void HandleRoutine(IEnumerator routine)
        {
            m_Routines.Add(routine);
        }

        void Update()
        {
            for (var i = 0; i < m_Routines.Count;)
            {
                var routine = m_Routines[i];
                if (!routine.MoveNext())
                {
                    m_Routines[i] = m_Routines[m_Routines.Count - 1];
                    m_Routines.RemoveAt(m_Routines.Count - 1);
                }
                else
                {
                    i++;
                }
            }
        }
    }
}
#endif
