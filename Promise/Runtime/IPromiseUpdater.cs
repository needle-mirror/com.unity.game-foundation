using System.Collections;

namespace UnityEngine.Promise
{
    interface IPromiseUpdater
    {
        void HandleRoutine(IEnumerator routine);
    }
}
