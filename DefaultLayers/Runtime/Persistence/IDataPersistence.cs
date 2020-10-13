using System;
using UnityEngine.GameFoundation.Data;

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    /// <summary>
    ///     Interface for defining load and save methods for persistence.
    /// </summary>
    public interface IDataPersistence
    {
        /// <summary>
        ///     Asynchronously loads GameFoundation's data from the persistence layer.
        /// </summary>
        /// <param name="onLoadCompleted">Called with the loaded data when they have been successfully loaded.</param>
        /// <param name="onLoadFailed">Called with a detailed exception when the loading failed.</param>
        void Load(Action<GameFoundationData> onLoadCompleted = null, Action<Exception> onLoadFailed = null);

        /// <summary>
        ///     Asynchronously saves GameFoundation's data onto the persistence layer.
        /// </summary>
        /// <param name="content">GameFoundation's data to persist.</param>
        /// <param name="onSaveCompleted">Called when data have been successfully saved.</param>
        /// <param name="onSaveFailed">Called with a detailed exception when the save failed.</param>
        void Save(GameFoundationData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null);
    }
}
