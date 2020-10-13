using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    sealed class ResourcesAssetTreeViewItem : TreeViewItem
    {
        public readonly string assetPath;

        public ResourcesAssetTreeViewItem(int id, int depth, string displayName, string path)
            : base(id, depth, displayName)
        {
            assetPath = path;
            icon = AssetDatabase.GetCachedIcon(path) as Texture2D;
        }
    }
}
