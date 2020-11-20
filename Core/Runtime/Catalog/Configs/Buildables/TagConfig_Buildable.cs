using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public partial class TagConfig : IBuildable<Tag>
    {
        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        public Deferred<Tag> Compile()
        {
            Promises.GetHandles<Tag>(out var deferred, out var completer);

            if (!Tools.IsValidId(key))
            {
                completer.Reject(new InvalidKeyException(key, nameof(Tag)));

                return deferred;
            }

            completer.Resolve(new Tag(key));

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        public Deferred Link(Tag runtimeObject, Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);
            completer.Resolve();

            return deferred;
        }
    }
}
