using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    ///     This class is not an actual sample, but provides functionality for all other samples as needed.
    ///     It is included in this sample because it needs to be brought in for any other samples to work for the database.
    /// </summary>
    public static class SamplesHelper
    {
        /// <summary>
        ///     This method verifies that the database in use is correct.
        ///     It also handles any situations where the settings were not setup correctly and may result in errors.
        /// </summary>
        /// <returns>
        ///     If the database in use is the sample catalog.
        /// </returns>
        public static bool VerifyDatabase()
        {
            string catalogAssetName;

            try
            {
                catalogAssetName = CatalogSettings.catalogAsset.name;
            }
            catch
            {
                return false;
            }

            return catalogAssetName == "SampleCatalog";
        }
    }
}
