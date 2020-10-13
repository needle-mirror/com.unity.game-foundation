using System;
using UnityEngine.GameFoundation.Data;

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    /// <summary>
    ///     Base persistence class derived from IDataPersistence
    /// </summary>
    public abstract class BaseDataPersistence : IDataPersistence
    {
        /// <summary>
        ///     The serialization layer used by the processes of this persistence.
        /// </summary>
        protected IDataSerializer serializer { get; }

        /// <summary>
        ///     Basic constructor that takes in a data serializer which this will use.
        /// </summary>
        /// <param name="serializer">The data serializer to use.</param>
        public BaseDataPersistence(IDataSerializer serializer)
        {
            this.serializer = serializer;
        }

        /// <inheritdoc/>
        public abstract void Load(Action<GameFoundationData> onLoadCompleted = null, Action<Exception> onLoadFailed = null);

        /// <inheritdoc/>
        public abstract void Save(GameFoundationData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null);
    }
}
