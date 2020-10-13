using System;
using System.Collections.Generic;

namespace UnityEngine.Promise
{
    /// <summary>
    ///     A simple pooling class.
    /// </summary>
    /// <typeparam name="TPooled">
    ///     The type of object to pool.
    /// </typeparam>
    class Pool<TPooled> where TPooled : class
    {
        /// <summary>
        ///     Provides an intuitive solution to release pooled objects.
        ///     Returned by <see cref="Pool{TPooled}.Get(out TPooled)"/>, it can be used in a <c>using</c> block as it
        ///     implements <see cref="IDisposable"/>.
        ///     Disposing this handle is releasing the related <typeparamref name="TPooled"/> instance back to its
        ///     <see cref="Pool{TPooled}"/> instance.
        ///     It is also possible to call <see cref="Release"/> manually.
        /// </summary>
        public struct Handle : IDisposable
        {
            /// <summary>
            ///     The <see cref="Pool{TPooled}"/> instance the <see cref="m_Poolable"/> instance belongs to.
            /// </summary>
            Pool<TPooled> m_Pool;

            /// <summary>
            ///     The poolable object to release by using this handle.
            /// </summary>
            TPooled m_Poolable;

            /// <summary>
            ///     Initializes a new instance of the <see cref="Handle"/> type.
            /// </summary>
            /// <param name="pool">
            ///     The <see cref="Pool{TPooled}"/> instance <paramref name="poolable"/> belongs to.
            /// </param>
            /// <param name="poolable">
            ///     The poolable object to release by using this handle.
            /// </param>
            internal Handle(Pool<TPooled> pool, TPooled poolable)
            {
                m_Pool = pool;
                m_Poolable = poolable;
            }

            /// <inheritdoc/>
            void IDisposable.Dispose() => Release();

            /// <summary>
            ///     Releases <see cref="m_Poolable"/> back to its <see cref="m_Pool"/>.
            /// </summary>
            public void Release()
            {
                m_Pool?.Release(m_Poolable);
                m_Pool = null;
            }
        }

        /// <summary>
        ///     The pooled objects.
        /// </summary>
        readonly Queue<TPooled> m_Pool = new Queue<TPooled>();

        /// <summary>
        ///     A method called to create a new object.
        /// </summary>
        readonly Func<TPooled> m_Create;

        /// <summary>
        ///     A method called when releasing an object.
        /// </summary>
        readonly Action<TPooled> m_Release;

        /// <summary>
        ///     Creates a new pool.
        /// </summary>
        /// <param name="create">
        ///     A method called to create a new object.
        /// </param>
        /// <param name="release">
        ///     A method called on the object when released.
        /// </param>
        public Pool(Func<TPooled> create, Action<TPooled> release)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }

            m_Create = create;
            m_Release = release;
        }

        /// <summary>
        ///     Gets a pooled object (or creates it).
        /// </summary>
        /// <returns>
        ///     A <see cref="TPooled"/> object.
        /// </returns>
        public TPooled Get()
        {
            if (m_Pool.Count > 0)
            {
                return m_Pool.Dequeue();
            }

            return m_Create();
        }

        /// <summary>
        ///     Gets a pooled object (or create it), and returns a disposable handle.
        ///     It makes pool easier to use within a using() block.
        /// </summary>
        /// <param name="poolable">
        ///     A <see cref="TPooled"/> object.
        /// </param>
        /// <returns>
        ///     The disposable handle.
        /// </returns>
        public Handle Get(out TPooled poolable)
        {
            poolable = Get();
            return new Handle(this, poolable);
        }

        /// <summary>
        ///     Releases a poolable object.
        /// </summary>
        /// <param name="pooled">
        ///     The object to release.
        /// </param>
        public void Release(TPooled pooled)
        {
            m_Release?.Invoke(pooled);
            m_Pool.Enqueue(pooled);
        }
    }
}
