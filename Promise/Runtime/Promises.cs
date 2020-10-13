using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor.Promise;

#endif

namespace UnityEngine.Promise
{
    /// <summary>
    ///     A <see cref="Promise"/> pool manager.
    /// </summary>
    public static class Promises
    {
        /// <summary>
        ///     A reference of the routine updater.
        /// </summary>
        internal static IPromiseUpdater m_Updater;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void TryInitUpdater()
        {
            if (m_Updater is null)
            {
                InitUpdater();
            }
        }

        /// <summary>
        ///     Initializes the updater of the routines.
        /// </summary>
        static void InitUpdater()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                m_Updater = new EditorPromiseUpdater();
            }
            else
            {
#endif
                var go = new GameObject
                {
                    hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector
                };
                m_Updater = go.AddComponent<RuntimePromiseUpdater>();
#if UNITY_EDITOR
            }
#endif
        }

        /// <summary>
        ///     Stores the already created promises for reuse.
        /// </summary>
        static readonly Dictionary<Type, Pool<Promise>> m_PromisePools = new Dictionary<Type, Pool<Promise>>();

        /// <summary>
        ///     A list of reusable list of deferred, used for the Wait method.
        /// </summary>
        static readonly Pool<List<Deferred>> m_DeferredListPool = new Pool<List<Deferred>>(
            () => new List<Deferred>(),
            list => list.Clear());

        /// <summary>
        ///     A list of reusable list of exceptions, used for the rejection of the Wait deferred.
        /// </summary>
        static readonly Pool<List<Exception>> m_ErrorListPool = new Pool<List<Exception>>(
            () => new List<Exception>(),
            list => list.Clear());

        /// <summary>
        ///     Take a promise out of the pool, or create one if it is empty, and set the two given handlers to handle
        ///     this promise.
        /// </summary>
        /// <param name="deferred">
        ///     A handler to read data from the taken promise.
        /// </param>
        /// <param name="completer">
        ///     A handler to write data to the taken promise.
        /// </param>
        public static void GetHandles(out Deferred deferred, out Completer completer)
        {
            TryInitUpdater();

            var promiseType = typeof(Promise);

            if (!m_PromisePools.TryGetValue(promiseType, out var pool))
            {
                pool = new Pool<Promise>(() => new Promise(), null);

                m_PromisePools.Add(promiseType, pool);
            }

            var promise = pool.Get();

            deferred = new Deferred(promise);
            completer = new Completer(promise);
        }

        /// <summary>
        ///     Extract a promise from the pool, or create one if it is empty, and set the two given handlers to handle
        ///     this promise.
        /// </summary>
        /// <param name="deferred">
        ///     A handler to read data from the taken promise.
        /// </param>
        /// <param name="completer">
        ///     A handler to write data to the taken promise.
        /// </param>
        /// <typeparam name="TResult">
        ///     Type of the result of the promise.
        /// </typeparam>
        public static void GetHandles<TResult>(out Deferred<TResult> deferred, out Completer<TResult> completer)
        {
            TryInitUpdater();

            var promiseType = typeof(Promise<TResult>);

            if (!m_PromisePools.TryGetValue(promiseType, out var pool))
            {
                pool = new Pool<Promise>(() => new Promise<TResult>(), null);
                m_PromisePools.Add(promiseType, pool);
            }

            var promise = pool.Get() as Promise<TResult>;

            deferred = new Deferred<TResult>(promise);
            completer = new Completer<TResult>(promise);
        }

        /// <summary>
        ///     Waits for a list of deferred to be done.
        /// </summary>
        /// <param name="deferreds">
        ///     The list of deferred to wait for.
        /// </param>
        /// <param name="tolerant">
        ///     It true, the returned deferred is fulfilled even if some deferred from the list are rejected.
        ///     Otherwise, one rejection will also reject the returned deferred.
        ///     In this case, the errors from all the rejected deferred are associated in an
        ///     <see cref="AggregateException"/>.
        /// </param>
        /// <returns>
        ///     A single deferred, done when the given list of deferred are done.
        /// </returns>
        public static Deferred Wait(IEnumerable<Deferred> deferreds, bool tolerant = true)
        {
            TryInitUpdater();

            var list = m_DeferredListPool.Get();
            list.AddRange(deferreds);

            GetHandles(out var deferred, out var completer);

            var routine = WaitRoutine(list, tolerant, completer);
            if (!deferred.isDone)
            {
                m_Updater.HandleRoutine(routine);
            }

            return deferred;
        }

        /// <summary>
        ///     This routine waits for all the given deferred to be done, and resolves or reject a single completer.
        /// </summary>
        /// <param name="deferreds">
        ///     The list of deferred to check
        /// </param>
        /// <param name="tolerant">
        ///     If <c>true</c>, rejects the given completer with the list of errors.
        /// </param>
        /// <param name="completer">
        ///     The completer which completes when all the deferred are done.
        /// </param>
        static IEnumerator WaitRoutine(List<Deferred> deferreds, bool tolerant, Completer completer)
        {
            using (m_ErrorListPool.Get(out var errorList))
            {
                for (var i = 0; i < deferreds.Count; i++)
                {
                    var deferred = deferreds[i];

                    while (!deferred.isDone)
                    {
                        yield return null;
                    }

                    if (!deferred.isFulfilled && !tolerant)
                    {
                        errorList.Add(deferred.error);
                    }
                }

                if (errorList.Count > 0)
                {
                    var error = new AggregateException(errorList);
                    completer.Reject(error);
                }
                else
                {
                    completer.Resolve();
                }
            }

            m_DeferredListPool.Release(deferreds);
        }

        /// <summary>
        ///     Releases the given promise for later reuse.
        /// </summary>
        /// <param name="promise">
        ///     The promise to release.
        /// </param>
        internal static void Release(Promise promise)
        {
            var promiseType = promise.GetType();

            m_PromisePools.TryGetValue(promiseType, out var pool);

            pool.Release(promise);
        }
    }
}
