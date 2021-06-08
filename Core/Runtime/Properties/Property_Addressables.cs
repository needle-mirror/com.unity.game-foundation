using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Property support for Addressables in Game Foundation.
    /// </summary>
    public partial struct Property
    {
        /// <summary>
        ///     Reusuable list for retrieve assets from Addressables package.
        /// </summary>
        static List<string> s_AddressesList = new List<string>();

        /// <summary>
        ///     Unit test support for Addressables.
        /// </summary>
        internal delegate void LoadAssetAsyncUT(string address);
        internal delegate void LoadAssetsListAsyncUT(string address, bool hasCallbackFlag);
        internal delegate void LoadAssetsListAsyncMergeUT(List<string> addresses,
            bool hasCallbackFlag, Addressables.MergeMode mergeMode);

        /// <summary>
        ///     Callbacks set by unit tests to ensure Addressables methods are called correctly.
        /// </summary>
        static internal LoadAssetAsyncUT s_AddressablesLoadAssetAsyncUT = null;
        static internal LoadAssetsListAsyncUT s_AddressablesLoadAssetsListAsyncUT = null;
        static internal LoadAssetsListAsyncMergeUT s_AddressablesLoadAssetsListAsyncMergeUT = null;

        /// <summary>
        ///     Explicitly returns this <see cref="Property"/> as an address for use with Addressables.
        /// </summary>
        /// <returns>
        ///     This Addressables <see cref="Property"/>'s address as a string.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public string AsAddressString()
        {
            if (type == PropertyType.Addressables)
            {
                return m_StringValue;
            }

            throw new InvalidCastException(
                $"Cannot cast this {nameof(Property)} instance into an Addressables address, because it contains a {type} value.");
        }

        /// <summary>
        ///     Explicitly returns this <see cref="Property"/> as an Addressables handle.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of Addressables asset to return.
        /// </typeparam>
        /// <returns>
        ///     Async-operation handle to this Addressable.
        ///     Addressables will be loaded asynchronously, and the returned handle can be used to check status of
        ///     async load and retrieve requested Addressables asset when it's ready.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public AsyncOperationHandle<T> AsAddressable<T>()
        {
            if (type == PropertyType.Addressables)
            {
                // if unit tests are off then call Addressables normally
                if (s_AddressablesLoadAssetAsyncUT is null)
                {
                    return Addressables.LoadAssetAsync<T>(m_StringValue);
                }

                // permit unit testing by calling back with param we would have used to call Addressables
                s_AddressablesLoadAssetAsyncUT(m_StringValue);

                // return default handle of correct type
                return new AsyncOperationHandle<T>();
            }

            throw new InvalidCastException(
                $"Cannot cast this {nameof(Property)} instance into an addressable, because it contains a {type} value.");
        }

        /// <summary>
        ///     Explicitly returns this <see cref="Property"/> as an Addressables handle to list of assets.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of Addressables asset list to return.
        /// </typeparam>
        /// <param name="callback">
        ///     Callback <see cref="Action{T}"/> that is called per load operation.
        /// </param>
        /// <returns>
        ///     Async-operation handle to this Addressable.
        ///     Addressables will be loaded asynchronously, and the returned handle can be used to check status of
        ///     async load and retrieve requested Addressables asset when it's ready.
        ///     Important: whereas <see cref="AsAddressable{T}"/> returns a single asset, this returns a list of
        ///     matching assets so it will be necessary to extract the needed asset from the list. By using the
        ///     filter, you can restrict the call to just a single asset so it can easily be extracted.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public AsyncOperationHandle<IList<T>> AsAddressables<T>(Action<T> callback = null)
        {
            if (type == PropertyType.Addressables)
            {
                // if unit tests are off then call Addressables normally
                if (s_AddressablesLoadAssetsListAsyncUT is null)
                {
                    return Addressables.LoadAssetsAsync<T>(m_StringValue, callback);
                }

                // permit unit testing by calling back with param we would have used to call Addressables
                s_AddressablesLoadAssetsListAsyncUT(m_StringValue, !(callback is null));

                // return default handle of correct type
                return new AsyncOperationHandle<IList<T>>();
            }

            throw new InvalidCastException(
                $"Cannot cast this {nameof(Property)} instance into an Addressable, because it contains a {type} value.");
        }

        /// <summary>
        ///     Explicitly returns this <see cref="Property"/> as an Addressables handle allowing a string
        ///     to be used to filter, usually by Addressables label.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of Addressables asset list to return.
        /// </typeparam>
        /// <param name="label">
        ///     Label to use in filtering. By default, method uses Addressable
        ///     <see cref="Addressables.MergeMode.Intersection"/> to further restrict the output (for example
        ///     based on a label specified to choose between different assets of the same Address).
        /// </param>
        /// <param name="mergeMode">
        ///     Desired merge for labels and Game Foundation Addressables address. Defaults to
        ///     <see cref="Addressables.MergeMode.Intersection"/> to permit finding a specific asset with
        ///     specified label, but any valid <see cref="Addressables.MergeMode"/> enum value can be used.
        /// </param>
        /// <param name="callback">
        ///     Callback <see cref="Action{T}"/> that is called per load operation.
        /// </param>
        /// <returns>
        ///     Async-operation handle to this Addressable.
        ///     Addressables will be loaded asynchronously, and the returned handle can be used to check status of
        ///     async load and retrieve requested Addressables asset list when it's ready.
        ///     Important: whereas <see cref="AsAddressable{T}"/> returns a single asset, this returns a list of
        ///     matching assets so it will be necessary to extract the needed asset from the list. By using the
        ///     filter, you can restrict the call to just a single asset so it can easily be extracted.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public AsyncOperationHandle<IList<T>> AsAddressables<T>(string label,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection,
            Action<T> callback = null)
        {
            if (type == PropertyType.Addressables)
            {
                s_AddressesList.Clear();
                s_AddressesList.Add(m_StringValue);
                s_AddressesList.Add(label);

                if (s_AddressablesLoadAssetsListAsyncMergeUT is null)
                {
                    return Addressables.LoadAssetsAsync<T>(s_AddressesList, callback, mergeMode);
                }

                // permit unit testing by calling back with param we would have used to call Addressables
                s_AddressablesLoadAssetsListAsyncMergeUT(s_AddressesList, !(callback is null), mergeMode);

                // return default handle of correct type
                return new AsyncOperationHandle<IList<T>>();
            }

            throw new InvalidCastException(
                $"Cannot cast this {nameof(Property)} instance into an Addressable, because it contains a {type} value.");
        }

        /// <summary>
        ///     Create a <see cref="Property"/> of type <see cref="PropertyType.Addressables"/>
        ///     using the given <paramref name="address"/>.
        ///     Note that the given <paramref name="address"/> isn't verified--editor or runtime code must ensure
        ///     specified address exists and is of valid type.
        /// </summary>
        /// <param name="address">
        ///     The address of desired Addressables asset.
        /// </param>
        /// <returns>
        ///     Return the created property.
        /// </returns>
        public static Property CreateAddressablesProperty(string address)
        {
            return new Property
            {
                type = PropertyType.Addressables,
                m_StringValue = address
            };
        }
    }
}
