using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor.GameFoundation
{
    class ResourcesAssetTreeView : TreeView
    {
        readonly ResourcesAssetDrawer m_Drawer;

        readonly ResourcesAssetPopup m_Popup;

        public ResourcesAssetTreeView(TreeViewState state, ResourcesAssetDrawer drawer, ResourcesAssetPopup popup)
            : base(state)
        {
            m_Drawer = drawer;
            m_Popup = popup;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null
                || selectedIds.Count != 1)
                return;

            var selectedNode = FindItem(selectedIds[0], rootItem);
            if (selectedNode is ResourcesAssetTreeViewItem assetNode)
            {
                m_Drawer.m_SelectedAssetFullPath = assetNode.assetPath;

                m_Popup.ForceClose();
            }
            else if (selectedNode != null)
            {
                SetExpanded(selectedNode.id, true);
            }
            else
            {
                m_Popup.ForceClose();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var resourcesFolderPaths = ResourcesAssetUtility.GetAllResourcesFolderPaths();

            var depth = -1;
            var root = new TreeViewItem("root".GetHashCode(), depth);
            ++depth;

            var emptyNode = new ResourcesAssetTreeViewItem(
                ResourcesAssetDrawer.noAssetString.GetHashCode(),
                depth,
                ResourcesAssetDrawer.noAssetString,
                string.Empty);
            root.AddChild(emptyNode);

            var pathHash = new HashSet<string>();
            foreach (var resourcesFolderPath in resourcesFolderPaths)
            {
                pathHash.Clear();

                var folderNode = new TreeViewItem(resourcesFolderPath.GetHashCode(), depth, resourcesFolderPath);
                root.AddChild(folderNode);

                ++depth;
                var assetGuids = AssetDatabase.FindAssets("", new[] { resourcesFolderPath });
                foreach (var assetGuid in assetGuids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                    //Sub assets are returned too with GUIDToAssetPath so we use
                    //a hash to make sure we don't handle the same asset twice.
                    if (!pathHash.Add(assetPath))
                        continue;

                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (var asset in allAssets)
                    {
                        //Resources doesn't handle sub asset.
                        if (asset == null
                            || AssetDatabase.IsSubAsset(asset))
                            continue;

                        var assetNode = new ResourcesAssetTreeViewItem(
                            assetGuid.GetHashCode(), depth, asset.name, assetPath);
                        folderNode.AddChild(assetNode);
                    }
                }

                --depth;
            }

            return root;
        }
    }
}
