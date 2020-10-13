using System;

namespace UnityEngine.GameFoundation
{
    public partial struct Property
    {
        /// <summary>
        ///     Explicitly casts this <see cref="Property"/> into
        ///     an asset of the given <typeparamref name="TAsset"/>.
        /// </summary>
        /// <returns>
        ///     The asset reference.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        /// <typeparam name="TAsset">
        ///     The type of the returned asset.
        /// </typeparam>
        public TAsset AsAsset<TAsset>() where TAsset : Object
        {
            switch (type)
            {
                case PropertyType.ResourcesAsset:
                    return Resources.Load<TAsset>(m_StringValue);

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into an asset, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Create a <see cref="Property"/> of type <see cref="PropertyType.ResourcesAsset"/>
        ///     using the given <paramref name="path"/>.
        ///     Note that the given <paramref name="path"/> isn't verified.
        /// </summary>
        /// <param name="path">
        ///     A path to an asset from the Resources folder.
        /// </param>
        /// <returns>
        ///     Return the created property.
        /// </returns>
        public static Property CreateAssetProperty(string path)
        {
            return new Property
            {
                type = PropertyType.ResourcesAsset,
                m_StringValue = path
            };
        }
    }
}
