using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Linq;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Helper methods related to addressables.
    /// </summary>
    public static class AddressablesUtility
    {
        /// <summary>
        ///     Dictionary of address => list of all Addressables with same address.
        ///     This permits queries such as 'how many?' and 'what different types of assets exist?'
        /// </summary>
        static Dictionary<string, List<AddressableAssetEntry>> s_AddressableAssets = null;
        internal static Dictionary<string, List<AddressableAssetEntry>>
            addressableAssets => s_AddressableAssets;

        /// <summary>
        ///     Temp Addressables data to reuse when updating addressables.
        /// </summary>
        static Dictionary<string, List<AddressableAssetEntry>> s_TempAddressableAssets
             = new Dictionary<string, List<AddressableAssetEntry>>();

        /// <summary>
        ///     Temp list of Addressables asset entries. Reused whenever addressables are updated.
        /// </summary>
        static List<AddressableAssetEntry> s_AddressablesList = new List<AddressableAssetEntry>();

        /// <summary>
        ///     When returning an addressables, prefer specified types by this ordered array.
        /// </summary>
        static System.Type[] k_AddressablesPreferredTypes = new System.Type[] {
            typeof(Texture), typeof(GameObject), typeof(AudioClip)
        };

        /// <summary>
        ///     Checks if addressables assets have been initialized.
        /// </summary>
        /// <returns>
        ///     True if addressables assets have been initialized.
        /// </returns>
        internal static bool AddressablesAssetsReady()
        {
            return !(s_AddressableAssets is null);
        }

        /// <summary>
        ///     Update internal list of all Addressables (by address) with types and
        ///     trigger repaint of last focus window if anything changes.
        /// </summary>
        internal static void UpdateAddressablesAssets()
        {
            if (s_AddressableAssets is null)
            {
                s_AddressableAssets = new Dictionary<string, List<AddressableAssetEntry>>();
            }
            else
            {
                s_AddressablesList.Clear();
            }

            // get updated list of Addressables
            try
            {
                AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(s_AddressablesList, true);
            }

            // this throw occurs if the Addressables folder is completely removed, but catch prevents needless logging
            catch (System.NullReferenceException)
            {
                return;
            }

            foreach (var entry in s_AddressablesList)
            {
                if (!entry.IsSubAsset)
                {
                    if (!s_TempAddressableAssets.TryGetValue(entry.address, out var list))
                    {
                        list = new List<AddressableAssetEntry>();
                        s_TempAddressableAssets[entry.address] = list;
                    }

                    list.Add(entry);
                }
            }

            // check if anything changed
            bool areIdentical = s_TempAddressableAssets.Count == s_AddressableAssets.Count;
            if (areIdentical)
            {
                foreach(var kv in s_TempAddressableAssets)
                {
                    if (!s_AddressableAssets.TryGetValue(kv.Key, out var oldVal))
                    {
                        areIdentical = false;
                        break;
                    }
                    var newVal = kv.Value;
                    if (newVal.Count != oldVal.Count)
                    {
                        areIdentical = false;
                        break;
                    }
                    foreach(var address in oldVal)
                    {
                        if (! newVal.Exists(a => a.guid == address.guid))
                        {
                            areIdentical = false;
                            break;
                        }
                    }
                    if (!areIdentical)
                    {
                        break;
                    }
                }
            }

            // if nothing changed then skip refresh
            if (areIdentical)
            {
                // setup to reuse the temp Addressables assets list
                s_TempAddressableAssets.Clear();

                return;
            }

            // save off old Addressables for recycling
            var assets = s_AddressableAssets;

            // use updated Addressables data
            s_AddressableAssets = s_TempAddressableAssets;

            // recycle the Addressables assets list
            s_TempAddressableAssets = assets;
            s_TempAddressableAssets.Clear();

            // repaint the focus window to show current state of Addressables in properties
            CollectionEditorWindowBase.RepaintLastFocusWindow();
        }

        /// <summary>
        ///     Return list of ALL Addressables that match specified address.
        /// </summary>
        /// <param name="address">
        ///     Address for which to search.
        /// </param>
        /// <param name="types">
        ///     List to fill with all types of Addressables matching specified address along with count of each type.
        /// </param>
        /// <param name="totalCount">
        ///     Total count of Addressables using specified address.
        /// </param>
        /// <param name="maxCountOfAnyType">
        ///     Max count of any 1 type of Addressable.
        /// </param>
        /// <returns>
        ///     'true' if any Addressables found with specified address, else false.
        /// </returns>
        public static bool GetAddressablesAssetsTypes(string address,
            List<(System.Type type, int count)> types, out int totalCount, out int maxCountOfAnyType)
        {
            types.Clear();
            totalCount = 0;
            maxCountOfAnyType = 0;

            if (s_AddressableAssets is null)
            {
                return false;
            }
            if (!s_AddressableAssets.TryGetValue(address, out var addressablesList))
            {
                return false;
            }

            totalCount = addressablesList.Count;
            if (totalCount < 1)
            {
                return false;
            }

            maxCountOfAnyType = 1;

            foreach(var addressables in addressablesList)
            {
                var type = addressables?.TargetAsset?.GetType();
                if (!(type is null))
                {
                    int index = types.FindIndex(a => a.type == type);

                    if (index == -1)
                    {
                        types.Add((type, 1));
                    }
                    else
                    {
                        int count = types[index].count + 1;
                        types[index] = (type, count);
                        if (count > maxCountOfAnyType)
                        {
                            maxCountOfAnyType = count;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Return a single Addressables that match specified address.
        ///     Note: this prioritizes assets based on internal preferred types list so, for example,
        ///     <see cref="Texture"/>s are returned before <see cref="GameObject"/>s and unexpected types
        ///     are returned last.
        /// </summary>
        /// <param name="address">
        ///     Address for which to search.
        /// </param>
        /// <param name="currentObject">
        ///     The currently selected addressable asset to exclude from the collection.
        /// </param>
        /// <returns>
        ///     'Best' Addressable asset entry if any are found or null if address is not found.
        /// </returns>
        public static AddressableAssetEntry GetNextAddressableAsset(string address, Object currentObject)
        {
            // error check and return null for any failure
            if (s_AddressableAssets is null)
            {
                return null;
            }
            if (!s_AddressableAssets.TryGetValue(address, out var addressablesList))
            {
                return null;
            }
            if (addressablesList is null || addressablesList.Count == 0)
            {
                return null;
            }
            if (addressablesList.Count == 1)
            {
                return addressablesList[0];
            }

            var ordered = addressablesList.OrderBy(x => AddressablesTypePriority(x));
            var next = ordered.SkipWhile(x => x.MainAsset != currentObject).Skip(1);

            return next.Any() ? next.First() : ordered.First();
        }

        /// <summary>
        ///     Determine internal UI priority for Addressable type so often-needed Addressables will
        ///     appear first when label is clicked.
        /// </summary>
        /// <param name="addressablesAddress">
        ///     Addressables address to check.
        /// </param>
        /// <returns>
        ///     Priority of specified Addressables address (lowest value should appear first when label is clicked).
        /// </returns>
        static int AddressablesTypePriority(AddressableAssetEntry addressablesAddress)
        {
            for (int i = 0; i < k_AddressablesPreferredTypes.Length; ++i)
            {
                if (k_AddressablesPreferredTypes[i].IsAssignableFrom(addressablesAddress.MainAsset.GetType()))
                {
                    return i;
                }
            }
            return k_AddressablesPreferredTypes.Length;
        }







    }
}
