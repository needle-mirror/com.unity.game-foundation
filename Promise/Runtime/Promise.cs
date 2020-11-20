using System;
using System.Collections;

namespace UnityEngine.Promise
{
    /// <summary>
    ///     Data contract for an asynchronous operation.
    /// </summary>
    class Promise : IEnumerator
    {
        /// <summary>
        ///     The token used to keep a soft reference between this promise and its <see cref="Completer"/> and
        ///     <see cref="Deferred"/> instances.
        /// </summary>
        internal ulong m_Token;

        /// <summary>
        ///     The total number of steps the process controling the completer needs to achieve.
        /// </summary>
        internal int m_TotalSteps;

        /// <summary>
        ///     The index of the current step the process is at.
        /// </summary>
        internal int m_CurrentStep;

        /// <summary>
        ///     A reference to this error that has prevented this <see cref="Promise"/> to be fulfilled.
        /// </summary>
        Exception m_Error;

        /// <summary>
        ///     Used by the <see cref="CustomYieldInstruction"/> to control the lifecycle of the instruction.
        /// </summary>
        protected bool m_Done;

        /// <inheritdoc cref="Deferred.isDone"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if done, <c>false</c> otherwise.
        /// </returns>
        internal bool IsDone(ulong tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Done;
        }

        /// <inheritdoc cref="Deferred.isFulfilled"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if fulfilled, <c>false</c> otherwise.
        /// </returns>
        internal bool IsFulfilled(ulong tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Done && null == m_Error;
        }

        /// <inheritdoc cref="Deferred.error"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance.
        /// </param>
        /// <returns>
        ///     The error.
        /// </returns>
        internal Exception GetError(ulong tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Error;
        }

        /// <inheritdoc cref="Completer.SetProgression(int, int)"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Completer"/> instance.
        /// </param>
        internal void SetProgression(ulong tokenValue, int currentStep, int totalSteps)
        {
            ValidateToken(tokenValue);
            m_CurrentStep = currentStep;
            m_TotalSteps = totalSteps;
        }

        /// <inheritdoc cref="Deferred.currentStep"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance.
        /// </param>
        /// <returns>
        ///     The current step index.
        /// </returns>
        internal int GetCurrentStep(ulong tokenValue)
        {
            ValidateToken(tokenValue);
            return m_CurrentStep;
        }

        /// <inheritdoc cref="Deferred.totalSteps"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance.
        /// </param>
        /// <returns>
        ///     The total steps.
        /// </returns>
        internal int GetTotalSteps(ulong tokenValue)
        {
            ValidateToken(tokenValue);
            return m_TotalSteps;
        }

        /// <inheritdoc cref="Deferred.GetProgression(out int, out int)"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance.
        /// </param>
        internal void GetProgression(ulong tokenValue, out int currentStep, out int totalSteps)
        {
            ValidateToken(tokenValue);
            currentStep = m_CurrentStep;
            totalSteps = m_TotalSteps;
        }

        /// <inheritdoc cref="Completer.Resolve\"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Completer"/> instance.
        /// </param>
        internal void Resolve(ulong tokenValue)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_Done = true;
        }

        /// <inheritdoc cref="Completer.Reject(Exception)\"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Completer"/> instance.
        /// </param>
        internal void Reject(ulong tokenValue, Exception reason)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_Error = reason;
            m_Done = true;
        }

        /// <inheritdoc cref="Deferred.Release"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Completer"/> instance.
        /// </param>
        internal void Release(ulong tokenValue)
        {
            ValidateToken(tokenValue);

            m_Token++;
            m_CurrentStep = default;
            m_TotalSteps = default;
            m_Error = default;
            m_Done = false;

            ResetData();

            Promises.Release(this);
        }

        internal IEnumerator Wait(ulong tokenValue)
        {
            ValidateToken(tokenValue);
            return this;
        }

        /// <summary>
        ///     Resets the data of the promise.
        /// </summary>
        protected virtual void ResetData() { }

        /// <summary>
        ///     Checks if the token sent by a <see cref="Deferred"/> instance or a <see cref="Completer"/> instance is
        ///     valid (the same than the one stored in this promise).
        /// </summary>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred"/> instance or a <see cref="Completer"/> instance.
        /// </param>
        /// <param name="throwError">
        ///     Defines if the check throws an exception in case the token is not validated.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the token is valid, <c>false</c> otherwise.
        /// </returns>
        protected bool ValidateToken(ulong tokenValue, bool throwError = true)
        {
            var valid = m_Token == tokenValue;

            if (!valid && throwError)
            {
                throw new NullReferenceException("This promise has been released and cannot be used anymore.");
            }

            return valid;
        }

        object IEnumerator.Current => default;

        bool IEnumerator.MoveNext() => !m_Done;

        void IEnumerator.Reset() { }
    }

    /// <inheritdoc cref="Promise"/>
    /// <typeparam name="TResult">
    ///     Type of the result of the async operation.
    /// </typeparam>
    class Promise<TResult> : Promise
    {
        /// <summary>
        ///     The result of the promise if fulfilled.
        /// </summary>
        TResult m_Result;

        /// <inheritdoc cref="Deferred{TResult}.result"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Deferred{TResult}"/> instance.
        /// </param>
        /// <returns>
        ///     The result.
        /// </returns>
        internal TResult GetResult(ulong tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Result;
        }

        /// <inheritdoc cref="Completer{TResult}.Resolve(TResult)"/>
        /// <param name="tokenValue">
        ///     The token coming from a <see cref="Completer{TResult}"/> instance.
        /// </param>
        internal void Resolve(ulong tokenValue, TResult resultValue)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_Result = resultValue;

            m_Done = true;
        }

        /// <inheritdoc cref="Promise.ResetData"/>
        protected override void ResetData()
        {
            m_Result = default;
        }
    }
}
