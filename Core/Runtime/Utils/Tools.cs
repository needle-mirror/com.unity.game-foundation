using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    static class Tools
    {
        public class Pools
        {
            public static readonly Pool<List<string>> stringList = new Pool<List<string>>(
                () => new List<string>(),
                list => list.Clear());

            public static readonly Pool<List<object>> objectList = new Pool<List<object>>(
                () => new List<object>(),
                list => list.Clear());

            public static readonly Pool<List<InventoryItem>> inventoryItemList = new Pool<List<InventoryItem>>(
                () => new List<InventoryItem>(),
                list => list.Clear());

            public static readonly Pool<List<Currency>> currencyList = new Pool<List<Currency>>(
                () => new List<Currency>(),
                list => list.Clear());

            public static readonly Pool<List<Reward>> rewardList = new Pool<List<Reward>>(
                () => new List<Reward>(),
                list => list.Clear());

            public static readonly Pool<Dictionary<string, RewardItemState>> rewardItemStateDictionary = new Pool<Dictionary<string, RewardItemState>>(
                () => new Dictionary<string, RewardItemState>(),
                list => list.Clear());

            public static readonly Pool<List<RewardItemDefinition>> rewardItemsList = new Pool<List<RewardItemDefinition>>(
                () => new List<RewardItemDefinition>(),
                list => list.Clear());

            public static readonly Pool<List<InventoryItemDefinition>> inventoryItemDefinitionList = new Pool<List<InventoryItemDefinition>>(
                () => new List<InventoryItemDefinition>(),
                list => list.Clear());

            public static readonly Pool<List<IItemCollection>> collectionList = new Pool<List<IItemCollection>>(
                () => new List<IItemCollection>(),
                list => list.Clear());

            public static readonly Pool<List<ITradable>> tradableList = new Pool<List<ITradable>>(
                () => new List<ITradable>(),
                list => list.Clear());
        }

        /// <summary>
        ///     Checks to see if the argument is a valid Id.
        ///     Valid Ids are alphanumeric with optional dashes or underscores.
        ///     No whitespace is permitted
        /// </summary>
        /// <param name="id">
        ///     id to check.
        /// </param>
        /// <returns>
        ///     true if the given <paramref name="id"/> is valid;
        ///     false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[a-zA-Z][a-zA-Z0-9\-_]*$");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNull(object o, string name)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name, $"{name} cannot be null");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNull(object o, string name, int index)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name, $"{name}#{index} cannot be null");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowIfOutOfRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
            {
                throw new IndexOutOfRangeException
                    ($"{paramName} ({value}) is out of the range [{min}, {max}]");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNegative(long value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, "Cannot be negative");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Cannot be null or empty", name);
            }
        }

        /// <summary>
        ///     Reject the given <paramref name="rejectable"/> if
        ///     the given <paramref name="value"/> is null, empty or white space.
        /// </summary>
        /// <param name="value">
        ///     String to check for null, empty or white space.
        /// </param>
        /// <param name="name">
        ///     Name of the caller's argument that is checked.
        /// </param>
        /// <param name="rejectable">
        ///     The object to reject if the given <paramref name="value"/> is null, empty or white space.
        /// </param>
        /// <returns>
        ///     Return true if the rejectable have been rejected;
        ///     return false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RejectIfArgNullOrEmpty(string value, string name, Rejectable rejectable)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                rejectable.Reject(new ArgumentException("Incorrect value", name));

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Reject the given <paramref name="rejectable"/> if
        ///     the given <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="value">
        ///     The value to null check.
        /// </param>
        /// <param name="name">
        ///     Name of the caller's argument that is checked.
        /// </param>
        /// <param name="rejectable">
        ///     The object to reject if the given <paramref name="value"/> is null.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the rejectable have been rejected, <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RejectIfArgNull(object value, string name, Rejectable rejectable)
        {
            if (value is null)
            {
                rejectable.Reject(new ArgumentNullException(name));
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Reject the given <paramref name="rejectable"/> if
        ///     the given <paramref name="value"/> is negative.
        /// </summary>
        /// <param name="value">
        ///     Number to check the sign of.
        /// </param>
        /// <param name="name">
        ///     Name of the caller's argument that is checked.
        /// </param>
        /// <param name="rejectable">
        ///     The object to reject if the given <paramref name="value"/> is negative.
        /// </param>
        /// <returns>
        ///     Return true if the rejectable have been rejected;
        ///     return false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RejectIfArgNegative(long value, string name, Rejectable rejectable)
        {
            if (value < 0)
            {
                var exception = new ArgumentOutOfRangeException(name, "Cannot be negative");

                rejectable.Reject(exception);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Copies a collection into a new array.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the collection.
        /// </typeparam>
        /// <param name="collection">
        ///     The collection of elements to copy into the new array.
        /// </param>
        /// <returns>
        ///     The new array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(ICollection<T> collection)
        {
            var copy = new T[collection.Count];
            collection.CopyTo(copy, 0);
            return copy;
        }

        /// <summary>
        ///     Copies a collection into another.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the collections.
        /// </typeparam>
        /// <param name="from">
        ///     The collection of elements to copy.
        /// </param>
        /// <param name="to">
        ///     The collection to copy the element into.
        /// </param>
        /// <param name="clearTo">
        ///     If <c>true</c>, it clears the <paramref name="to"/> collection before populating.
        /// </param>
        /// <returns>
        ///     The number of copied items.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy<T>(ICollection<T> from, ICollection<T> to = null, bool clearTo = true)
        {
            if (clearTo) to?.Clear();

            if (from is null)
            {
                return 0;
            }

            if (to != null)
            {
                foreach (var o in from)
                {
                    to.Add(o);
                }
            }

            return from.Count;
        }

        public static void Optimize<TElement>(IList<TElement> list)
            where TElement : class
        {
            for (var i = 0; i < list.Count;)
            {
                var element = list[i];
                if (element is null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InventoryItem GetItemOrDie(string id, string paramName)
        {
            ThrowIfArgNullOrEmpty(id, paramName);

            var item = GameFoundationSdk.inventory.FindItem(id);
            if (item is null)
            {
                throw new InventoryItemNotFoundException(id);
            }

            return item;
        }

        /// <summary>
        ///     Gets a <see cref="CatalogItem"/> from its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the definition to find.
        /// </param>
        /// <param name="paramName">
        ///     Name of <paramref name="key"/>'s variable in the caller.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the given <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="CatalogItemNotFoundException">
        ///     If the given <paramref name="key"/> is not found.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TCatalogItem GetCatalogItemOrDie<TCatalogItem>(string key, string paramName)
            where TCatalogItem : CatalogItem
        {
            ThrowIfArgNull(key, paramName);

            var catalogItem = GameFoundationSdk.catalog.Find<TCatalogItem>(key);
            if (catalogItem is null)
            {
                throw new CatalogItemNotFoundException(key);
            }

            return catalogItem;
        }

        /// <summary>
        ///     Checks to see specified namespace exists.
        /// </summary>
        /// <param name="testNamespace">
        ///     Namespace to check.
        /// </param>
        /// <returns>
        ///     Whether or not specified namespace exists.
        /// </returns>
        public static bool NamespaceExists(string testNamespace)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Namespace == testNamespace)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks to see specified type exists.
        /// </summary>
        /// <param name="testType">
        ///     Type to check.
        /// </param>
        /// <returns>
        ///     Whether or not specified namespace exists.
        /// </returns>
        public static bool TypeExists(string testType)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var typeStr = type.ToString();
                    if (testType == typeStr)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Determine if <see cref="Exception"/> should be rethrown instead of handled (usually because it is
        ///     an 'internal' GUI exception designed to pass control when user opens a dialog, etc.
        /// </summary>
        /// <param name="e">
        ///     <see cref="Exception"/> to check.
        /// </param>
        /// <returns>
        ///     True if exception should be simply rethrown instead of handled as a normal exception.
        /// </returns>
        internal static bool ShouldRethrowException(Exception e) 
        {
            return IsExitGUIException(e);
        }

        /// <summary>
        ///     Determines if specified <see cref="Exception"/> is an internal GUI exception.
        /// </summary>
        /// <param name="e">
        ///     <see cref="Exception"/> to check.
        /// </param>
        /// <returns>
        ///     True if exception designed as an internal 'exit GUI' signal and, as such, should not be 
        ///     treated as an actual exception.
        /// </returns>
        internal static bool IsExitGUIException(Exception e)
        {
            while (e is TargetInvocationException && !(e.InnerException is null))
            {   
                e = e.InnerException;
            }

            return e is ExitGUIException;
        }
    }
}
