using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        ///     The part of the data layer dedicated to the inventory management.
        /// </summary>
        InventoryDataLayer m_InventoryDataLayer;

        /// <summary>
        ///     Initializes the data layer for <see cref="IInventoryManager"/>.
        /// </summary>
        /// <param name="data">
        ///     InventoryManager's serializable data.
        /// </param>
        /// <param name="catalogAsset">
        ///     The catalog asset to be used as source of truth.
        /// </param>
        protected void InitializeInventoryDataLayer(InventoryManagerData data, CatalogAsset catalogAsset)
        {
            m_InventoryDataLayer = new InventoryDataLayer(data, catalogAsset);
        }

        /// <inheritdoc/>
        InventoryManagerData IInventoryDataLayer.GetData()
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).GetData();
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItem(string definitionKey, string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).CreateItem(definitionKey, itemId, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.SetQuantity(string itemId, long quantity, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).SetQuantity(itemId, quantity, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.DeleteItem(string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).DeleteItem(itemId, completer);
        }

        /// <inheritdoc/>
        Property IInventoryDataLayer.GetMutablePropertyValue(string itemId, string propertyKey)
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).GetMutablePropertyValue(itemId, propertyKey);
        }

        /// <inheritdoc/>
        bool IInventoryDataLayer.TryGetMutablePropertyValue(string itemId, string propertyKey, out Property propertyValue)
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).TryGetMutablePropertyValue(itemId, propertyKey, out propertyValue);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.SetMutablePropertyValue(string itemId, string propertyKey, Property value, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).SetMutablePropertyValue(itemId, propertyKey, value, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItemList(string id, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).CreateItemList(id, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.DeleteItemList(string id, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).DeleteItemList(id, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListAddItem(string listId, string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemListAddItem(listId, itemId, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListRemoveItem(string listId, string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemListRemoveItem(listId, itemId, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListSwapIndexes(string listId, int index0, int index1, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemListSwapIndexes(listId, index0, index1, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemListClear(string listId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemListClear(listId, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItemMap(string id, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).CreateItemMap(id, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.DeleteItemMap(string id, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).DeleteItemMap(id, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapAddItem(string mapId, string slot, string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemMapAddItem(mapId, slot, itemId, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapRemoveItem(string mapId, string slot, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemMapRemoveItem(mapId, slot, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapChangeSlot(string mapId, string oldSlot, string newSlot, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemMapChangeSlot(mapId, oldSlot, newSlot, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapSwapSlots(string mapId, string slot0, string slot1, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemMapSwapSlots(mapId, slot0, slot1, completer);
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.ItemMapClear(string mapId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).ItemMapClear(mapId, completer);
        }
    }
}
