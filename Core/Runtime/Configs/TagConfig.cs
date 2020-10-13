using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    ///     Configuration object for a <see cref="Tag"/> instance.
    /// </summary>
    public class TagConfig
    {
        /// <summary>
        ///     Checks the configuration and builds the <typeparamref name="Tag"/> instance.
        /// </summary>
        protected internal virtual Tag CompileItem()
        {
            var tag = new Tag(key);

            return tag;
        }

        /// <summary>
        ///     The identifier of the <see cref="Tag"/> instance.
        /// </summary>
        public string key;

        /// <summary>
        ///     The tag built by this configurator.
        /// </summary>
        internal Tag runtimeTag;

        /// <summary>
        ///     Checks the configuration and builds the <see cref="Tag"/> instance.
        /// </summary>
        internal void Compile()
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));

            if (!Tools.IsValidId(key))
            {
                throw new Exception($"Key {key} is not valid");
            }

            runtimeTag = new Tag(key);
        }
    }
}
