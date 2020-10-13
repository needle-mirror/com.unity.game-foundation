#if UNITY_EDITOR

using System;
using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class IAPTransactionAsset
    {
        /// <inheritdoc/>
        internal override string editorAssetPrefix => "IAPTransaction";

        /// <summary>
        ///     Set the <see cref="appleId"/> of this instance.
        /// </summary>
        /// <param name="id">
        ///     The Apple ID to set.
        /// </param>
        internal void Editor_SetAppleId(string id)
        {
            m_AppleId = id;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the <see cref="googleId"/> of this instance.
        /// </summary>
        /// <param name="id">
        ///     The Google ID to set.
        /// </param>
        internal void Editor_SetGoogleId(string id)
        {
            m_GoogleId = id;
            EditorUtility.SetDirty(this);
        }

        /// <inheritdoc/>
        internal override void CopyValues(CatalogItemAsset target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target),
                    $"{nameof(IAPTransactionAsset)}: The {nameof(CatalogItemAsset)} target parameter cannot be null.");
            }

            var iapTransactionTarget = target as IAPTransactionAsset;

            if (iapTransactionTarget == null)
            {
                throw new InvalidCastException(
                    $"{nameof(IAPTransactionAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            iapTransactionTarget.m_AppleId = m_AppleId;
            iapTransactionTarget.m_GoogleId = m_GoogleId;

            base.CopyValues(iapTransactionTarget);
        }
    }
}

#endif
