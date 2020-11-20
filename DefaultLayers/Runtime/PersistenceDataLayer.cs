using System;
using System.IO;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Data layer for GameFoundation using an <see cref="IDataPersistence"/> as its data source.
    /// </summary>
    public class PersistenceDataLayer : BaseMemoryDataLayer
    {
        /// <summary>
        ///     The persistence object used by this data layer to save &amp; load data.
        /// </summary>
        public IDataPersistence persistence { get; }

        /// <summary>
        ///     Create a data layer with the given catalog provider that will use the given persistence object to save
        ///     &amp; load GameFoundation's data.
        /// </summary>
        /// <param name="catalogAsset">
        ///     The catalog asset to use as the source of truth.
        /// </param>
        /// <param name="persistence">
        ///     Persistence used by this data layer.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the given persistence is null.
        /// </exception>
        public PersistenceDataLayer(IDataPersistence persistence, CatalogAsset catalogAsset = null)
            : base(catalogAsset)
        {
            this.persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        }

        /// <inheritdoc/>
        public override void Initialize(Completer completer)
        {
            void InitializeWith(GameFoundationData data)
            {
                try
                {
                    InitializeInventoryDataLayer(data.inventoryManagerData);
                    InitializeWalletDataLayer(data.walletData);

                    // this must be called *after* inventory and wallet
                    InitializeRewardDataLayer(data.rewardManagerData);

                    m_Version = data.version;

                    completer.Resolve();
                }
                catch (Exception e)
                {
                    completer.Reject(e);
                }
            }

            persistence.Load(
                InitializeWith,
                error =>
                {
                    switch (error)
                    {
                        case FileNotFoundException _:
                            InitializeWith(m_CatalogAsset.CreateDefaultData());
                            break;

                        default:
                            completer.Reject(error);
                            break;
                    }
                });
        }

        /// <summary>
        ///     Save GameFoundation's data using the persistence object.
        /// </summary>
        /// <returns>
        ///     Returns a <see cref="Deferred"/> to track the progression of the save process.
        /// </returns>
        public Deferred Save()
        {
            var data = GetData();

            Promises.GetHandles(out var deferred, out var completer);

            persistence.Save(data, completer.Resolve, completer.Reject);

            return deferred;
        }
    }
}
