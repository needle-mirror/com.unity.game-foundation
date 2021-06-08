#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class CatalogAsset
    {
        const string k_BaseFileName = "GameFoundationCatalog";

        [MenuItem("Assets/Create/Game Foundation/Catalog Asset")]
        static void Editor_Create()
        {
            string path = null;

            var selection = Selection.activeObject;
            if (EditorUtility.IsPersistent(selection))
            {
                path = AssetDatabase.GetAssetPath(selection);

                if (!AssetDatabase.IsValidFolder(path))
                {
                    var selectionName = Path.GetFileName(path);
                    path = path.Substring(0, path.Length - selectionName.Length);
                }
                else
                {
                    path += "/";
                }
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                path = "Assets/";
            }

            var names = new DirectoryInfo(path).GetFiles().Select(f => f.Name.Substring(0, f.Name.Length - f.Extension.Length)).ToArray();

            var catalogAssetName = ObjectNames.GetUniqueName(names, k_BaseFileName);

            path += catalogAssetName + ".asset";

            var catalogAsset = CreateInstance<CatalogAsset>();
            catalogAsset.name = catalogAssetName;

            catalogAsset.Editor_Save(path);

            Selection.SetActiveObjectWithContext(catalogAsset, null);
            EditorUtility.FocusProjectWindow();
        }

        /// <summary>
        ///     Saves the catalog asset and all its sub-assets in the same asset file.
        /// </summary>
        /// <param name="path">
        ///     The path of the output file.
        /// </param>
        internal void Editor_Save(string path = null)
        {
            var oldPath = AssetDatabase.GetAssetPath(this);
            var doCreateFile = string.IsNullOrEmpty(oldPath);

            if (doCreateFile
                && string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path),
                    $"{nameof(CatalogAsset)}: Database cannot be saved because {nameof(path)} parameter is null or empty.");
            }

            var save = false;
            if (doCreateFile)
            {
                AssetDatabase.CreateAsset(this, path);
                save = true;
            }

            var subAssets = new List<Object> { m_TagCatalog };

            m_TagCatalog.Editor_GetSubAssets(subAssets);

            if (m_Items != null)
            {
                foreach (var item in m_Items)
                {
                    if (item is null)
                        continue;

                    subAssets.Add(item);
                }
            }

            foreach (var subAsset in subAssets)
            {
                if (AssetDatabase.IsSubAsset(subAsset))
                    continue;

                AssetDatabase.AddObjectToAsset(subAsset, this);

                if (string.IsNullOrWhiteSpace(subAsset.name))
                {
                    if (subAsset is CatalogItemAsset catalogItem)
                    {
                        catalogItem.name = catalogItem.editorAssetName;
                    }
                    else if (subAsset is TagAsset tag)
                    {
                        tag.name = tag.editorAssetName;
                    }
                }

                save = true;
            }

            if (save)
            {
                EditorUtility.SetDirty(this);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        ///     Create a new instance of the given <typeparamref name="TCatalogItem"/>
        ///     using the given <paramref name="key"/> and add it to this catalog.
        /// </summary>
        /// <param name="key">
        ///     The key to give to the created item.
        /// </param>
        /// <typeparam name="TCatalogItem">
        ///     The type of catalog item to create.
        /// </typeparam>
        /// <returns>
        ///     Return the created catalog item.
        /// </returns>
        /// <remarks>
        ///     <c>new()</c> could be another constraint on <typeparamref name="TCatalogItem"/>
        ///     but we can't have this constraint due to our current editor implementations.
        /// </remarks>
        internal TCatalogItem Editor_CreateItem<TCatalogItem>(string key)
            where TCatalogItem : CatalogItemAsset
        {
            GFTools.ThrowIfArgNullOrEmpty(key, nameof(key));

            var item = CreateInstance<TCatalogItem>();
            item.Editor_Initialize(key, key);

            Editor_AddItem(item);

            return item;
        }

        /// <summary>
        ///     Adds the <paramref name="item"/> to this catalog.
        /// </summary>
        /// <param name="item">
        ///     The <see cref="CatalogItemAsset"/> to add.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown if a duplicate definition is given.
        /// </exception>
        internal void Editor_AddItem(CatalogItemAsset item)
        {
            GFTools.ThrowIfArgNull(item, nameof(item));

            if (!(FindItem(item.key) is null))
            {
                throw new ArgumentException(
                    $"{nameof(CatalogAsset)}_Editor: The object is already registered within this Catalog. (key: {item.key})");
            }

            m_Items.Add(item);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Remove the given <paramref name="item"/> from this catalog.
        /// </summary>
        /// <param name="item">
        ///     The item to remove from this catalog.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if the item has been successfully removed;
        ///     return <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveItem(CatalogItemAsset item)
        {
            var isRemoved = m_Items.Remove(item);
            if (!isRemoved)
            {
                return false;
            }

            EditorUtility.SetDirty(this);

            //Remove sub asset from main.
            if (AssetDatabase.IsSubAsset(item))
            {
                var itemPath = AssetDatabase.GetAssetPath(item);
                var mainAsset = AssetDatabase.LoadAssetAtPath<Object>(itemPath);
                if (ReferenceEquals(mainAsset, this))
                {
                    AssetDatabase.RemoveObjectFromAsset(item);
                }
            }

            return true;
        }

        /// <summary>
        ///     Creates a new <see cref="TagAsset"/> instance and adds it to the catalog asset.
        /// </summary>
        /// <param name="id">
        ///     The key of the new <see cref="TagAsset"/> instance.
        /// </param>
        /// <returns>
        ///     The new <see cref="TagAsset"/> instance.
        /// </returns>
        internal TagAsset Editor_CreateTag(string id)
        {
            var tag = CreateInstance<TagAsset>();
            tag.Editor_Initialize(m_TagCatalog, id);
            m_TagCatalog.Editor_AddTag(tag);

            EditorUtility.SetDirty(this);

            return tag;
        }

        /// <summary>
        ///     Remove the given <paramref name="tag"/> from this catalog.
        /// </summary>
        /// <param name="tag">
        ///     The tag to remove.
        /// </param>
        /// <returns>
        ///     Return <c>true</c> if the tag was removed successfully;
        ///     return <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_RemoveTag(TagAsset tag)
        {
            var isRemoved = tagCatalog.Editor_RemoveTag(tag);

            if (!isRemoved)
            {
                return false;
            }

            //Remove sub asset from main.
            if (AssetDatabase.IsSubAsset(tag))
            {
                var itemPath = AssetDatabase.GetAssetPath(tag);
                var mainAsset = AssetDatabase.LoadAssetAtPath<Object>(itemPath);
                if (ReferenceEquals(mainAsset, this))
                {
                    AssetDatabase.RemoveObjectFromAsset(tag);
                }
            }

            //Propagate change through items.
            foreach (var catalogItem in m_Items)
            {
                catalogItem.Editor_RemoveTag(tag);
            }

            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        ///     Deep copy this whole catalog.
        /// </summary>
        /// <returns>
        ///     Return a deep copy of this instance.
        /// </returns>
        internal CatalogAsset Clone()
        {
            var clone = CreateInstance<CatalogAsset>();

            clone.m_TagCatalog = m_TagCatalog.Clone();
            clone.m_TagCatalog.RefreshReferences(clone);

            clone.m_Items = new List<CatalogItemAsset>(m_Items.Count);
            foreach (var catalogItem in m_Items)
            {
                var cloneCatalogItem = (CatalogItemAsset)CreateInstance(catalogItem.GetType());
                cloneCatalogItem.Editor_Initialize(catalogItem.key, catalogItem.displayName);
                catalogItem.CopyValues(cloneCatalogItem);

                clone.m_Items.Add(cloneCatalogItem);
            }

            //Once all items and tags are created: refresh references.
            foreach (var clonedItem in clone.m_Items)
            {
                clonedItem.RefreshReferences(this);
            }

            return clone;
        }
    }
}

#endif
