using System;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    [Serializable]
    class CatalogSnapshotContainer
    {
        public CatalogSnapshot CatalogSnapshot;
    }

    [Serializable]
    struct CatalogSnapshot : IEquatable<CatalogSnapshot>
    {
        public static bool operator ==(CatalogSnapshot a, CatalogSnapshot b) =>
            a.inventoryItemCount == b.inventoryItemCount &&
            a.storeCount == b.storeCount &&
            a.propertyCount == b.propertyCount &&
            a.virtualTransactionCount == b.virtualTransactionCount &&
            a.iapTransactionCount == b.iapTransactionCount &&
            a.parameterCount == b.parameterCount &&
            a.rewardCount == b.rewardCount &&
            a.rewardItemCount == b.rewardItemCount;

        public static bool operator !=(CatalogSnapshot a, CatalogSnapshot b) =>
            a.inventoryItemCount != b.inventoryItemCount ||
            a.storeCount != b.storeCount ||
            a.propertyCount != b.propertyCount ||
            a.virtualTransactionCount != b.virtualTransactionCount ||
            a.virtualTransactionCount != b.virtualTransactionCount ||
            a.parameterCount != b.parameterCount ||
            a.rewardCount != b.rewardCount ||
            a.rewardItemCount != b.rewardItemCount;

        public int inventoryItemCount;
        public int storeCount;
        public int propertyCount;
        public int virtualTransactionCount;
        public int iapTransactionCount;
        public int parameterCount;
        public int rewardCount;
        public int rewardItemCount;

        public bool Equals(CatalogSnapshot other) => this == other;

        public override bool Equals(object obj) => obj is CatalogSnapshot other && Equals(other);

        public override int GetHashCode() =>
            (inventoryItemCount & 0b11111111) << 24 | //  255 inventory items  (8 bits)
            (storeCount & 0b1111) << 20 | //   15 stores           (4 bits)
            (virtualTransactionCount & 0b1111) << 16 | //   15 v transactions   (4 bits)
            (iapTransactionCount & 0b1111) << 12 | //   15 iap transactions (4 bits)
            (propertyCount & 0b11111111) << 4 | //  255 properties       (8 bits)
            (parameterCount & 0b1111) << 0 //   15 parameters       (4 bits)
        ;
    }
}
