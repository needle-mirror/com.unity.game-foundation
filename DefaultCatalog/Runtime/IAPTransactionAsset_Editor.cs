#if UNITY_EDITOR

using System;
using UnityEditor;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEditor.Purchasing;
using UnityEngine.Purchasing;
#endif

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class IAPTransactionAsset
    {
        /// <inheritdoc/>
        internal override string editorAssetPrefix => "IAPTransaction";

        void OnEnable()
        {
            RefreshProductType();
        }

        internal void RefreshProductType()
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

            if (!PurchasingSettings.enabled)
                return;

            var catalog = ProductCatalog.LoadDefaultCatalog();

            if (catalog == null)
                return;

            foreach (var productCatalogItem in catalog.allValidProducts)
            {
                var matched = productCatalogItem.id == m_AppleId || productCatalogItem.id == m_GoogleId;

                foreach (var storeId in productCatalogItem.allStoreIDs)
                {
                    switch (storeId.store)
                    {
                        case AppleAppStore.Name when storeId.id == m_AppleId:
                        case GooglePlay.Name when storeId.id == m_GoogleId:
                        {
                            matched = true;
                            break;
                        }
                    }
                }

                if (matched)
                {
                    // convert UnityEngine.Purchasing.ProductType
                    // to UnityEngine.GameFoundation.IAPProductType
                    switch (productCatalogItem.type)
                    {
                        case ProductType.Consumable:
                            productType = IAPProductType.Consumable;

                            EditorUtility.SetDirty(this);
                            return;

                        case ProductType.NonConsumable:
                            productType = IAPProductType.NonConsumable;

                            if (payout.m_Exchanges.Count > 0)
                            {
                                Debug.LogWarning(
                                    $"Deleted payouts from IAP Transaction '{m_Key}' " +
                                    "because it changed to a NonConsumable product type.");
                            }
                            payout.m_Exchanges.Clear();

                            EditorUtility.SetDirty(this);
                            return;

                        case ProductType.Subscription:
                            // not supported by GF yet,
                            // but lets keep track anyway,
                            // we could use this to show a warning in the UI
                            productType = IAPProductType.Subscription;

                            if (payout.m_Exchanges.Count > 0)
                            {
                                Debug.LogWarning(
                                    $"Deleted payouts from IAP Transaction '{m_Key}' " +
                                    "because it changed to a Subscription product type.");
                            }
                            payout.m_Exchanges.Clear();

                            EditorUtility.SetDirty(this);
                            return;

                        default: break; // ignore
                    }
                }
            }

            // if we reached here, then the product wasn't found in the IAP Catalog
            productType = IAPProductType.Undetermined;
            EditorUtility.SetDirty(this);
#else
            // In this case, the dev is editing an IAP Transaction without the IAP SDK activated.
            // This method will silently do nothing.
#endif
        }

        /// <summary>
        ///     Set the <see cref="appleId"/> of this instance.
        /// </summary>
        /// <param name="id">
        ///     The Apple ID to set.
        /// </param>
        internal void Editor_SetAppleId(string id)
        {
            m_AppleId = id;
            m_AppleIdWrapper = new ExternalizableValue<string>(m_AppleId);

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
            m_GoogleIdWrapper = new ExternalizableValue<string>(m_GoogleId);

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

            if (!(target is IAPTransactionAsset iapTransactionTarget))
            {
                throw new InvalidCastException(
                    $"{nameof(IAPTransactionAsset)}: The target object {target.displayName} of type " +
                    $"'{target.GetType()}' could not be cast to {GetType()}.");
            }

            iapTransactionTarget.Editor_SetAppleId(m_AppleId);
            iapTransactionTarget.Editor_SetGoogleId(m_GoogleId);
            iapTransactionTarget.RefreshProductType();

            base.CopyValues(iapTransactionTarget);
        }
    }
}

#endif
