using System;
using System.Collections;

namespace UnityEngine.Promise
{
    /// <summary>
    ///     Handle to read data of a promise.
    /// </summary>
    public struct Deferred : IDisposable
    {
        /// <summary>
        ///     The token is stored because <see cref="Promise"/> instances can be recycled.
        ///     A deferred can read its promise only if its token matches the promise's.
        /// </summary>
        ulong m_Token;

        /// <summary>
        ///     A reference to the promise this <see cref="Completer"/> reads.
        /// </summary>
        Promise m_Promise;

        /// <summary>
        ///     Tells whether the <see cref="Deferred"/> is still active or not.
        ///     A <see cref="Deferred"/> is active if it is not released.
        /// </summary>
        public bool isActive => m_Promise != null && m_Promise.m_Token == m_Token;

        /// <summary>
        ///     A flag to determine if the handled promise has been fulfilled or
        ///     rejected.
        /// </summary>
        public bool isDone => m_Promise.IsDone(m_Token);

        /// <summary>
        ///     A flag to determine if the handled promise has been fulfilled.
        /// </summary>
        public bool isFulfilled => m_Promise.IsFulfilled(m_Token);

        /// <summary>
        ///     The exception that prevented the handled promise to be fulfilled if it has been rejected.
        /// </summary>
        public Exception error => m_Promise.GetError(m_Token);

        /// <summary>
        ///     Gets the index of the current step of the promise.
        /// </summary>
        public int currentStep => m_Promise.GetCurrentStep(m_Token);

        /// <summary>
        ///     Gets the number of steps of the promise.
        /// </summary>
        public int totalSteps => m_Promise.GetTotalSteps(m_Token);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Deferred"/> struct.
        /// </summary>
        /// <param name="promise">
        ///     The promise this deferred will read.
        /// </param>
        internal Deferred(Promise promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.m_Token;
        }

        /// <summary>
        ///     Resets the handled promise in order to recycle it.
        /// </summary>
        public void Release()
        {
            m_Promise?.Release(m_Token);
            m_Promise = null;
        }

        /// <summary>
        ///     Gets the progression of the promise.
        /// </summary>
        /// <param name="currentStep">
        ///     The index of the current step the promise is at.
        /// </param>
        /// <param name="totalSteps">
        ///     The number of steps of the promise.
        /// </param>
        public void GetProgression(out int currentStep, out int totalSteps)
            => m_Promise.GetProgression(m_Token, out currentStep, out totalSteps);

        /// <summary>
        ///     Returns an <see cref="IEnumerator"/> instance so it can be used in the context of a coroutine.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator"/> instance to use in a coroutine.
        /// </returns>
        public IEnumerator Wait() => m_Promise?.Wait(m_Token) ?? default;

        /// <summary>
        ///     Release the handled promise if it is still active.
        /// </summary>
        public void Dispose()
        {
            if (isActive)
            {
                Release();
            }
        }
    }

    /// <inheritdoc cref="Deferred"/>
    /// <typeparam name="TResult">
    ///     Type of the result this deferred retrieves from its <see cref="Promise{TResult}"/>.
    /// </typeparam>
    public struct Deferred<TResult> : IDisposable
    {
        /// <summary>
        ///     Implicitly converts the <see cref="Deferred{TResult}"/> into a classic <see cref="Deferred"/>.
        ///     Useful if the only thing you need to know is whether or not the promise is fulfill or not (ignoring the
        ///     result).
        /// </summary>
        /// <param name="deferred">
        ///     The <see cref="Deferred{TResult}"/> to convert.
        /// </param>
        /// <returns>
        ///     A classic <see cref="Deferred"/> instance.
        /// </returns>
        public static implicit operator Deferred(Deferred<TResult> deferred) => new Deferred(deferred.m_Promise);

        /// <inheritdoc cref="Deferred.m_Token"/>
        ulong m_Token;

        /// <inheritdoc cref="Deferred.m_Promise"/>
        Promise<TResult> m_Promise;

        /// <inheritdoc cref="Deferred.isActive"/>
        public bool isActive => m_Promise != null && m_Promise.m_Token == m_Token;

        /// <inheritdoc cref="Deferred.isDone"/>
        public bool isDone => m_Promise.IsDone(m_Token);

        /// <inheritdoc cref="Deferred.isFulfilled"/>
        public bool isFulfilled => m_Promise.IsFulfilled(m_Token);

        /// <summary>
        ///     The result of the async operation if the handled promise could be fulfilled.
        /// </summary>
        public TResult result => m_Promise.GetResult(m_Token);

        /// <inheritdoc cref="Deferred.error"/>
        public Exception error => m_Promise.GetError(m_Token);

        /// <inheritdoc cref="Deferred.currentStep"/>
        public int currentStep => m_Promise.GetCurrentStep(m_Token);

        /// <inheritdoc cref="Deferred.totalSteps"/>
        public int totalSteps => m_Promise.GetTotalSteps(m_Token);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Deferred"/> struct.
        /// </summary>
        /// <param name="promise">
        ///     The promise this deferred will read.
        /// </param>
        internal Deferred(Promise<TResult> promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.m_Token;
        }

        /// <inheritdoc cref="Deferred.Release"/>
        public void Release()
        {
            m_Promise?.Release(m_Token);
            m_Promise = null;
        }

        /// <inheritdoc cref="Deferred.GetProgression(out int, out int)"/>
        public void GetProgression(out int currentStep, out int totalSteps)
            => m_Promise.GetProgression(m_Token, out currentStep, out totalSteps);

        /// <summary>
        ///     Returns an <see cref="IEnumerator"/> instance so it can be used in the context of a coroutine.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator"/> instance to use in a coroutine.
        /// </returns>
        public IEnumerator Wait() => m_Promise?.Wait(m_Token) ?? default;

        /// <summary>
        ///     Release the handled promise if it is still active.
        /// </summary>
        public void Dispose()
        {
            if (isActive)
            {
                Release();
            }
        }
    }
}
