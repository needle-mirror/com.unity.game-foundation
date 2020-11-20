using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.Configs
{
    public abstract partial class CatalogItemConfig
    {
        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        public abstract Deferred<CatalogItem> CompileGeneric();

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        public abstract Deferred LinkGeneric(CatalogItem runtimeObject, Dictionary<string, CatalogItem> compiledItems);
    }

    /// <summary>
    ///     Base configurator of a <see cref="CatalogItem"/> instance.
    /// </summary>
    /// <typeparam name="TRuntimeItem">
    ///     The type of the configurable <see cref="CatalogItem"/>.
    /// </typeparam>
    public abstract class CatalogItemConfig<TRuntimeItem> : CatalogItemConfig, IBuildable<TRuntimeItem>
        where TRuntimeItem : CatalogItem
    {
        /// <inheritdoc/>
        public override Deferred<CatalogItem> CompileGeneric()
        {
            Promises.GetHandles<CatalogItem>(out var deferred, out var completer);
            using (var subOperation = Compile())
            {
                if (subOperation.isFulfilled)
                {
                    completer.Resolve(subOperation.result);
                }
                else
                {
                    completer.Reject(subOperation.error);
                }
            }

            return deferred;
        }

        /// <inheritdoc/>
        public override Deferred LinkGeneric(CatalogItem runtimeObject, Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);

            if (Tools.RejectIfArgNull(runtimeObject, nameof(runtimeObject), completer))
            {
                return deferred;
            }

            if (!(runtimeObject is TRuntimeItem castItem))
            {
                var message = $"The type of the given catalog item ({runtimeObject.GetType().Name}) isn't handled by this configuration {GetType().Name}.";
                completer.Reject(new ArgumentException(message));

                return deferred;
            }

            using (var subOperation = Link(castItem, compiledItems))
            {
                if (subOperation.isFulfilled)
                {
                    completer.Resolve();
                }
                else
                {
                    completer.Reject(subOperation.error);
                }
            }

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        public Deferred<TRuntimeItem> Compile()
        {
            Promises.GetHandles<TRuntimeItem>(out var deferred, out var completer);

            if (!Tools.IsValidId(key))
            {
                completer.Reject(new InvalidKeyException(key, typeof(TRuntimeItem)));

                return deferred;
            }

            if (DoRequireDisplayName()
                && Tools.RejectIfArgNullOrEmpty(displayName, nameof(displayName), completer))
            {
                return deferred;
            }

            var runtimeItem = CompileItem(completer);

            if (deferred.isDone)
            {
                return deferred;
            }

            runtimeItem.key = key;
            runtimeItem.displayName = displayName;
            runtimeItem.tags = new Tag[tags.Count];

            completer.Resolve(runtimeItem);

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        public Deferred Link(TRuntimeItem runtimeObject, Dictionary<string, CatalogItem> compiledItems)
        {
            Promises.GetHandles(out var deferred, out var completer);

            for (var i = 0; i < tags.Count; i++)
            {
                var tagKey = tags[i];
                if (!Tools.IsValidId(tagKey))
                {
                    completer.Reject(new InvalidKeyException(tagKey, nameof(Tag)));

                    return deferred;
                }

                runtimeObject.tags[i] = new Tag(tagKey);
            }

            foreach (var staticProperty in staticProperties)
            {
                runtimeObject.staticProperties.Add(staticProperty.Key, staticProperty.Value);
            }

            LinkItem(runtimeObject, compiledItems, completer);

            if (!deferred.isDone)
            {
                completer.Resolve();
            }

            return deferred;
        }

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Compile"/>
        protected abstract TRuntimeItem CompileItem(Rejectable rejectable);

        /// <inheritdoc cref="IBuildable{TRuntimeObject}.Link"/>
        protected virtual void LinkItem(
            TRuntimeItem runtimeObject, Dictionary<string, CatalogItem> compiledItems, Rejectable rejectable) { }

        /// <summary>
        ///     Check if this configuration required a display name to compile.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if required, <c>false</c> otherwise.
        /// </returns>
        protected virtual bool DoRequireDisplayName() => true;
    }
}
