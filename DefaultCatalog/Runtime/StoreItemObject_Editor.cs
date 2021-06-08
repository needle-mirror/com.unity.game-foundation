#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class StoreItemObject
    {
        /// <summary>
        ///     Sets the <see cref="enabled"/> value of the store item.
        /// </summary>
        /// <param name="enable">
        ///     The new value.
        /// </param>
        internal void Editor_SetEnabled(bool enable)
        {
            if (m_Enabled == enable) return;
            m_Enabled = enable;
            EditorUtility.SetDirty(store);
        }

        internal StoreItemObject Clone()
        {
            return new StoreItemObject
            {
                m_Transaction = m_Transaction,
                m_Enabled = m_Enabled,
                store = store
            };
        }

        /// <summary>
        ///     Update all member references to target objects stored in the given <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">
        ///     The catalog containing all required references.
        /// </param>
        internal void RefreshReferences(CatalogAsset owner)
        {
            m_Transaction = (BaseTransactionAsset)owner.FindItem(m_Transaction.key);
            store = (StoreAsset)owner.FindItem(store.key);
        }
    }
}

#endif
