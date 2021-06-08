using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    abstract class CollectionEditorBase<T> : ICollectionEditor
        where T : class
    {
        public string name { get; }

        protected T selectedItem { get; private set; }

        T m_PreviouslySelectedItem;

        protected readonly List<T> m_Items = new List<T>();

        Vector2 m_ScrollPosition;
        Vector2 m_ScrollPositionDetail;

        Rect m_SidebarItemOffset;

        T m_ItemToRemove;

        protected bool m_ClickedCreateButton;

        protected EditorWindow m_Window;
        public bool isCreating { get; private set; }

        protected string m_NewItemDisplayName = string.Empty;
        protected string m_NewItemKey = string.Empty;
        protected T m_ItemToDuplicate;

        protected ReadableNameKeyEditor m_ReadableNameKeyEditor;

        protected readonly TagFilterEditor m_TagFilterEditor = new TagFilterEditor();

        protected abstract GameFoundationAnalytics.TabName tabName { get; }

        protected CollectionEditorBase(string name, EditorWindow window)
        {
            this.name = name;
            m_Window = window;
            SubscribeToCatalogUpdates();
        }

        protected abstract List<T> GetValidItems();

        public virtual void Draw()
        {
            ClearAndRemoveItems();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    DrawSidebar();
                    DrawContent();
                }
            }
        }

        protected abstract void DrawDetail(T item);

        void DrawCreateForm()
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                DrawCreateInputFields();

                DrawCustomCreateFormUI();

                GUILayout.Space(6f);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Cancel", GUILayout.Width(120f)))
                    {
                        isCreating = false;
                        SelectItem(m_PreviouslySelectedItem);
                        m_PreviouslySelectedItem = null;
                    }

                    GUILayout.Space(6f);

                    if (string.IsNullOrEmpty(m_NewItemDisplayName)
                        || string.IsNullOrEmpty(m_NewItemKey)
                        || m_ReadableNameKeyEditor.HasRegisteredKey(m_NewItemKey)
                        || !CollectionEditorTools.IsValidId(m_NewItemKey))
                    {
                        CollectionEditorTools.SetGUIEnabledAtEditorTime(false);
                    }

                    if (GUILayout.Button("Create", GUILayout.Width(120f)))
                    {
                        CreateNewItemFinalize();

                        isCreating = false;
                    }

                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
                }
            }
        }

        protected virtual void DrawCustomCreateFormUI()
        {
            EditorGUILayout.HelpBox(
                "Once the Create button is clicked, the Key cannot be changed.",
                MessageType.Warning);
        }

        protected virtual void DrawCreateInputFields()
        {
            m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_NewItemKey, ref m_NewItemDisplayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("displayName");
            }
        }

        public void OnWillEnter()
        {
            RefreshView();
            GameFoundationAnalytics.SendOpenTabEvent(tabName);
        }

        public virtual void OnWillExit()
        {
            isCreating = false;

            SelectItem(null);
        }

        void DrawSidebar()
        {
            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.sideBarStyle, GUILayout.Width(GameFoundationEditorStyles.sideBarWidth)))
            {
                DrawSidebarContent();
            }
        }

        void DrawSidebarContent()
        {
            DrawSidebarUtils();

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawSidebarList();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+", GameFoundationEditorStyles.createButtonStyle))
            {
                m_ClickedCreateButton = true;

                if (!isCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    m_PreviouslySelectedItem = selectedItem;
                    isCreating = true;
                    m_NewItemDisplayName = string.Empty;
                    m_NewItemKey = string.Empty;
                    SelectItem(null);
                    CreateNewItem();
                }
            }
        }

        protected virtual void DrawSidebarUtils() { }

        void DrawSidebarList()
        {
            var validItems = GetValidItems();

            if (validItems == null) return;
            foreach (T item in validItems)
            {
                DrawSidebarListItem(item);
            }
        }

        protected abstract void DrawSidebarListItem(T item);

        protected void BeginSidebarItem(T item, Vector2 backgroundSize, Vector2 contentMargin)
        {
            var rect = EditorGUILayout.GetControlRect(true, backgroundSize.y);
            rect.width = backgroundSize.x;

            GUI.backgroundColor =
                item.Equals(selectedItem) ? new Color(0.1f, 0.1f, 0.1f, .2f) : new Color(0, 0, 0, 0.0f);

            // make it look like a button, but do not use a GUI.Button
            // (it gets in the way of ContextClick and other things)
            GUI.Box(rect, "", GUI.skin.button);

            CollectionEditorTools.SetGUIEnabledAtRunTime(true);

            if (GUI.enabled
                && rect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (Event.current.button == 0)
                        {
                            isCreating = false;
                            EditorApplication.delayCall += () =>
                            {
                                SelectItem(item);
                            };
                        }

                        break;

                    case EventType.ContextClick: // happens on mouse down, has to be mouse down because of Mac UX

                        isCreating = false;

                        EditorApplication.delayCall += () =>
                        {
                            SelectItem(item);
                        };

                        switch (item)
                        {
                            case CatalogItemAsset catalogItemAsset:
                                SidebarItemContextMenu(catalogItemAsset);
                                break;
                            case TagAsset tagAsset:
                                SidebarItemContextMenu(tagAsset);
                                break;
                        }

                        break;
                }
            }

            CollectionEditorTools.SetGUIEnabledAtRunTime(false);

            GUI.backgroundColor = Color.white;

            m_SidebarItemOffset = rect;
            m_SidebarItemOffset.x += contentMargin.x;
            m_SidebarItemOffset.y += contentMargin.y;

            GUI.color = selectedItem == item ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.6f);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(backgroundSize.y));
        }

        void SidebarItemContextMenu(CatalogItemAsset catalogItemAsset)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Duplicate"), false, () =>
            {
                EditorApplication.delayCall += () =>
                {
                    StartDuplicating(catalogItemAsset);
                };
            });

            if (catalogItemAsset.m_Key == CatalogAsset.k_MainStoreDefinitionKey)
            {
                menu.AddDisabledItem(new GUIContent("Delete"));
            }
            else
            {
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                        "Are you sure?", "Do you want to delete " + catalogItemAsset.displayName + "?", "Yes", "No"))
                    {
                        m_ItemToRemove = catalogItemAsset as T;
                    }
                });
            }

            menu.ShowAsContext();
        }

        void SidebarItemContextMenu(TagAsset tagAsset)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                if (EditorUtility.DisplayDialog(
                    "Are you sure?", "Do you want to delete " + tagAsset.key + "?", "Yes", "No"))
                {
                    m_ItemToRemove = tagAsset as T;
                }
            });

            menu.ShowAsContext();
        }

        protected virtual void StartDuplicating(CatalogItemAsset item)
        {
            m_ClickedCreateButton = true;

            if (!isCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
            {
                m_PreviouslySelectedItem = item as T;
                isCreating = true;
                m_NewItemDisplayName = item.displayName + " Copy";
                m_NewItemKey = ""; // leave this blank so a unique key will be auto-generated
                m_ItemToDuplicate = item as T;
                SelectItem(null);
                CreateNewItem();
            }
        }

        protected void EndSidebarItem()
        {
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        protected void DrawSidebarItemLabel(string text, int width, GUIStyle style, int height = -1)
        {
            m_SidebarItemOffset.width = width;
            m_SidebarItemOffset.height = height == -1 ? m_SidebarItemOffset.height : height;

            if (style == null)
            {
                EditorGUI.LabelField(m_SidebarItemOffset, text);
            }
            else
            {
                EditorGUI.LabelField(m_SidebarItemOffset, text, style);
            }

            m_SidebarItemOffset.x += width;
        }

        void DrawContent()
        {
            using (var scrollViewScope = new GUILayout.ScrollViewScope
                (m_ScrollPositionDetail, false, false, GUILayout.MaxWidth(GameFoundationEditorStyles.contentDetailMaxWidth)))
            {
                m_ScrollPositionDetail = scrollViewScope.scrollPosition;
                DrawContentDetail();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        public void ValidateSelection()
        {
            // it's possible that the selected item was deleted or a new database was loaded
            if (m_Items != null && !m_Items.Contains(selectedItem))
            {
                SelectItem(null);
            }
        }

        void DrawContentDetail()
        {
            if (selectedItem != null)
            {
                isCreating = false;

                DrawDetail(selectedItem);
            }
            else if (isCreating)
            {
                DrawCreateForm();

                if (Event.current.type == EventType.Repaint)
                {
                    m_ClickedCreateButton = false;
                }
            }
            else
            {
                try
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.FlexibleSpace();

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();

                            GUILayout.Label("No object selected.");

                            GUILayout.FlexibleSpace();
                        }

                        GUILayout.FlexibleSpace();
                    }
                }
                catch (ArgumentException)
                {
                    // this except occurs if deleting item, filtering items or changing selected item ...
                    // ... while in the process of creating a new item.  consume the exception.
                }
            }
        }

        protected abstract void CreateNewItem();
        protected abstract void CreateNewItemFinalize();

        /// <summary>
        ///     Remove the given <paramref name="item"/> from this collection.
        /// </summary>
        /// <param name="item">
        ///     The item to remove.
        /// </param>
        protected abstract void OnRemoveItem(T item);

        /// <summary>
        ///     This updates the cached list of items.
        ///     The base implementation only constructs and clears the list.
        ///     The inherited implementation should populate the list.
        /// </summary>
        public virtual void RefreshItems()
        {
            m_Items.Clear();
        }

        /// <summary>
        ///     Refreshes the Item list shown in the left nav bar and the selected item shown in the right content view.
        /// </summary>
        private void RefreshView()
        {
            RefreshItems();
            isCreating = false;
            SelectItem(null);
            SelectValidItem(0);
        }

        void ClearAndRemoveItems()
        {
            if (m_ItemToRemove is null)
            {
                return;
            }

            OnRemoveItem(m_ItemToRemove);
            m_ItemToRemove = null;
            SelectItem(null);
            RefreshItems();
        }

        protected virtual void SelectItem(T item)
        {
            selectedItem = item;

            GUI.FocusControl(null);

            m_Window.Repaint();
        }

        protected void SelectValidItem(int listIndex)
        {
            var validItems = GetValidItems();

            if (validItems != null && validItems.Count > listIndex && listIndex >= 0)
            {
                SelectItem(validItems[listIndex]);
            }
            else
            {
                SelectItem(null);
            }
        }

        /// <summary>
        ///     Subscribes to the Catalog Settings event that is triggered anytime the catalog asset is changed.
        /// </summary>
        private void SubscribeToCatalogUpdates()
        {
            CatalogSettings.onCatalogChanged += RefreshView;
        }

        /// <summary>
        ///     Unsubscribes from the Catalog Settings event that is triggered anytime the catalog asset is changed.
        /// </summary>
        public void UnsubscribeFromCatalogUpdates()
        {
            CatalogSettings.onCatalogChanged -= RefreshView;
        }
    }
}
