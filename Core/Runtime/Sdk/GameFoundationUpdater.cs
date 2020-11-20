namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     This object is used by <see cref="GameFoundationSdk"/> to update and launch coroutines.
    /// </summary>
    class GameFoundationUpdater : MonoBehaviour
    {
        void Awake()
        {
            var cachedGameObject = gameObject;
            cachedGameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

            DontDestroyOnLoad(cachedGameObject);
        }
    }
}
