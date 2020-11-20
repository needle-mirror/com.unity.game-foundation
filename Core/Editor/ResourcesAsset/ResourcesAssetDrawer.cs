using System;
using UnityEngine;
using UnityEngine.GameFoundation;
using Object = UnityEngine.Object;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    ///     Draws a resource asset.
    /// </summary>
    public sealed class ResourcesAssetDrawer
    {
        /// <summary>
        ///     Text displayed when the drawer has no asset to draw.
        /// </summary>
        public const string noAssetString = "None (Asset)";

        internal string m_SelectedAssetFullPath;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<ResourcesAssetDrawer>();

        /// <inheritdoc cref="Draw(Rect, Rect, string, UnityEngine.Object)"/>
        /// <param name="label">
        ///     \
        ///     The title of the widget.
        /// </param>
        public string Draw(Rect position, GUIContent label, string serializedPath, Object target)
        {
            var objectPickerRect = EditorGUI.PrefixLabel(position, label);

            return Draw(position, objectPickerRect, serializedPath, target);
        }

        /// <inheritdoc cref="Draw(Rect, Rect, string, Object)"/>
        public string Draw(Rect position, string serializedPath, Object target)
        {
            return Draw(position, position, serializedPath, target);
        }

        /// <summary>
        ///     Draws the asset.
        /// </summary>
        /// <param name="position">
        ///     The position of the widget in the editor.
        /// </param>
        /// <param name="positionWithoutLabel">
        ///     The position of the widget, label excluded.
        /// </param>
        /// <param name="serializedPath">
        ///     The path of the asset.
        /// </param>
        /// <param name="target">
        ///     The target object.
        /// </param>
        /// <returns>
        ///     The new serialized path.
        /// </returns>
        string Draw(Rect position, Rect positionWithoutLabel, string serializedPath, Object target)
        {
            //Serialize selected path.
            if (m_SelectedAssetFullPath != null)
            {
                if (target != null)
                {
                    EditorUtility.SetDirty(target);

                    //Don't forget to set the GUI changed since we override the default behaviour.
                    GUI.changed = true;
                }

                //Remove Resources path.
                const string resourcesFolder = "/Resources/";
                var startIndex = m_SelectedAssetFullPath.IndexOf(resourcesFolder, StringComparison.Ordinal) + resourcesFolder.Length;
                if (startIndex >= 0
                    && startIndex < m_SelectedAssetFullPath.Length)
                {
                    m_SelectedAssetFullPath = m_SelectedAssetFullPath.Substring(startIndex);
                }

                //Remove file extension.
                startIndex = m_SelectedAssetFullPath.LastIndexOf('.');
                if (startIndex >= 0
                    && startIndex < m_SelectedAssetFullPath.Length)
                {
                    m_SelectedAssetFullPath = m_SelectedAssetFullPath.Remove(startIndex);
                }

                serializedPath = m_SelectedAssetFullPath;

                m_SelectedAssetFullPath = null;
            }

            var currentEvent = Event.current;
            var isMouseHover = position.Contains(currentEvent.mousePosition);
            var isDragging = currentEvent.type == EventType.DragUpdated
                && isMouseHover;
            var isDropping = currentEvent.type == EventType.DragPerform
                && isMouseHover;

            DrawControl(positionWithoutLabel, isDragging, isDropping, serializedPath);

            if (isDragging)
            {
                HandleDrag();
            }

            if (isDropping)
            {
                HandleDrop();
            }

            return serializedPath;
        }

        void DrawControl(Rect position, bool isDragging, bool isDropping, string serializedPath)
        {
            const float iconWidth = 20f;
            var iconRect = position;
            iconRect.width = iconWidth;
            iconRect.x = position.xMax - iconWidth;

            var currentEvent = Event.current;
            var isPickerPressed = currentEvent.type == EventType.MouseDown
                && currentEvent.button == 0
                && iconRect.Contains(currentEvent.mousePosition);

            if (isPickerPressed
                || isDragging
                || isDropping)
            {
                // To override ObjectField's default behavior
                currentEvent.Use();
            }

            //It is important to draw the ObjectField after using the event to prevent its default behaviour.
            var asset = Resources.Load<Object>(serializedPath);
            var displayName = GetDisplayName(serializedPath, asset);

            if (asset != null)
            {
                var assetName = asset.name;
                asset.name = displayName;

                EditorGUI.ObjectField(position, asset, asset.GetType(), false);

                asset.name = assetName;
            }
            else
            {
                EditorGUI.ObjectField(position, null, typeof(Object), false);
            }

            if (isPickerPressed)
            {
                PopupWindow.Show(position, new ResourcesAssetPopup(position, this));
            }
        }

        void HandleDrop()
        {
            if (DragAndDrop.paths == null
                || DragAndDrop.paths.Length != 1)
                return;

            var path = DragAndDrop.paths[0];
            if (ResourcesAssetUtility.IsResourcesAsset(path))
            {
                m_SelectedAssetFullPath = path;
            }
            else
            {
                k_GFLogger.LogWarning("Dragged asset is not in a Resources folder. " + path);
            }
        }

        static string GetDisplayName(string serializedPath, Object asset)
        {
            var assetName = noAssetString;
            if (asset != null)
            {
                assetName = asset.name;
            }
            else if (!string.IsNullOrEmpty(serializedPath))
            {
                assetName = $"Missing Asset ({serializedPath})";
            }

            return assetName;
        }

        static void HandleDrag()
        {
            //During the drag, doing a light check on asset validity.
            //The in-depth check happens during a drop, and should include a log if it fails.
            var doRejectedDrag = DragAndDrop.paths.Length != 1
                || !ResourcesAssetUtility.IsResourcesAsset(DragAndDrop.paths[0]);

            DragAndDrop.visualMode = doRejectedDrag ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Copy;
        }
    }
}
