using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UnityEngine.Promise
{
    /// <summary>
    ///     Simple component only existing for the routines to be updated.
    /// </summary>
    class RuntimePromiseUpdater : MonoBehaviour, IPromiseUpdater
    {
        void Awake()
        {
            // This needs to be called in the awake instead of being called by InitUpdater() to be editor unit tests
            // compliant.
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
            EditorApplication.update += CheckMode;
#endif
        }

#if UNITY_EDITOR
        void CheckMode()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.update -= CheckMode;
                DestroyImmediate(gameObject, false);
                Promises.m_Updater = null;
            }
        }
#endif

        public void HandleRoutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}
