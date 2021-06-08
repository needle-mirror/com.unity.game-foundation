using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Abstract class which sets up an editor window with a tab bar at the top.
    /// </summary>
    abstract class CollectionEditorWindowBase : EditorWindow
    {
        /// <summary>
        ///     Minimum amount of time away from GF editor windows before refesh will be required
        ///     for Addressables list in GF. If less than this interval, it's assumed that dev
        ///     didn't have time to make significant changes that would effect UI.
        ///     Note: even if update does NOT occur, dev can still enter values since the internal
        ///     list is only used to give UI feedback about state of Addressables in GF properties.
        /// </summary>
        const float k_RefreshAddressablesNeededTime = 1;

        /// <summary>
        ///     Add minor delay before updating to permit UI to update to help hide any minor
        ///     performance glitches caused by updating the Addressables list.
        /// </summary>
        const float k_RefreshAddressablesDelay = .25f;

        int m_TabBarIndex { get; set; }
        int m_OldTabBarIndex { get; set; }

        protected List<ICollectionEditor> m_Editors { get; } = new List<ICollectionEditor>();

        /// <summary>
        ///     Delay to refresh Addressables to permit window draw before pausing to update
        ///     the interal Addressables data.
        /// </summary>
        static float s_UpdateAddressablesTime = float.MaxValue;

        /// <summary>
        ///     Set when we gained focus after losing focus for significant amount of time to
        ///     trigger refresh of Addressables. Since it's not set for quick window changes,
        ///     we do not have to refresh if dev changes between GF windows as this causes minor
        ///     delay in updating.
        /// </summary>
        static bool s_AddressablesRefreshFlag = true;

        /// <summary>
        ///     When did the GF editor window lose focus.
        /// </summary>
        static float s_LostFocusTime = float.MinValue;

        /// <summary>
        ///     Last collection editor window to receive focus. Needed to issue repaint command
        ///     if Addressables list changes in deferred updating.
        /// </summary>
        static CollectionEditorWindowBase s_LastFocusWindow = null;


        string[] editorNames
        {
            get
            {
                var items = new string[m_Editors.Count];
                for (int i = 0; i < m_Editors.Count; i++)
                {
                    items[i] = m_Editors[i].name;
                }

                return items;
            }
        }

        protected virtual void OnEnable()
        {
            minSize = new Vector2(850f, 400f);

            m_TabBarIndex = 0;
            m_OldTabBarIndex = -1;

            CreateEditors();
        }

        protected virtual void OnFocus()
        {
            foreach (var collectionEditor in m_Editors)
            {
                collectionEditor.ValidateSelection();
            }

            if (m_Editors != null && m_Editors.Count > 0 && m_TabBarIndex < m_Editors.Count)
            {
                m_Editors[m_TabBarIndex].RefreshItems();
            }

            // save off focus window so we can redraw if needed
            s_LastFocusWindow = this;

            // if we lost focus for a significant amount of time then refresh Addressables list
            // note: we do NOT refresh for momentary focus changes to avoid the 1-2 sec pause caused by refreshing
            if (Time.realtimeSinceStartup >= s_LostFocusTime + k_RefreshAddressablesNeededTime)
            {
                s_AddressablesRefreshFlag = true;
            }
            s_LostFocusTime = float.MaxValue;
        }

        /// <summary>
        ///     Trigger repaint on last focus window.  Needed when Addressables update (they do so after a delay
        ///     and require async wait so must callback when ready).
        /// </summary>
        internal static void RepaintLastFocusWindow()
        {
            s_LastFocusWindow?.Repaint();
        }

        protected virtual void Update()
        {
            // if focus changed and we need to update our internal list of Addressables
            if (s_AddressablesRefreshFlag)
            {
                s_AddressablesRefreshFlag = false;

                // if we've already initialized Addressables data
                if (AddressablesUtility.AddressablesAssetsReady())
                {
                    // delay updating a fraction of a sec to permit current redraw to finish
                    // (otherwise we lurch and do not show ui changes which looks very bad)
                    s_UpdateAddressablesTime = Time.realtimeSinceStartup + k_RefreshAddressablesDelay;
                }

                // update immediately if first time
                else
                {
                    AddressablesUtility.UpdateAddressablesAssets();
                }
            }

            // if we need to refresh and the appropriate minor delay has passed
            else if (Time.realtimeSinceStartup >= s_UpdateAddressablesTime)
            {
                // reset timer so we do not refresh again and refresh now
                s_UpdateAddressablesTime = float.MaxValue;
                AddressablesUtility.UpdateAddressablesAssets();
            }
        }

        protected virtual void OnLostFocus()
        {
            s_LostFocusTime = Time.realtimeSinceStartup;
        }

        private void OnDestroy()
        {
            s_LastFocusWindow = null;
            foreach (var editor in m_Editors)
            {
                editor.UnsubscribeFromCatalogUpdates();
            }
        }

        void OnGUI()
        {
            // TAB BAR

            if (m_Editors.Count > 1)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    m_TabBarIndex = GUILayout.Toolbar(m_TabBarIndex, editorNames, GameFoundationEditorStyles.topToolbarStyle);
                }
            }

            if (m_TabBarIndex < 0 || m_TabBarIndex >= m_Editors.Count || m_Editors.Count == 0)
            {
                m_TabBarIndex = 0;
            }

            EditorGUILayout.Space();

            if (m_TabBarIndex != m_OldTabBarIndex)
            {
                // if trying to switch tabs while in the middle of creating a new item, show a confirmation dialog
                if (m_OldTabBarIndex >= 0
                    && m_Editors[m_OldTabBarIndex].isCreating
                    && !CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    // clicked "Stay" - prevent switching tabs - change the index back
                    m_TabBarIndex = m_OldTabBarIndex;
                    return;
                }

                if (m_OldTabBarIndex >= 0 && m_OldTabBarIndex < m_Editors.Count)
                {
                    m_Editors[m_OldTabBarIndex].OnWillExit();
                }

                m_Editors[m_TabBarIndex].OnWillEnter();

                m_OldTabBarIndex = m_TabBarIndex;
            }

            m_Editors[m_TabBarIndex].Draw();

            // if you click anywhere that isn't a GUI control, then clear the focus
            // do not use a big button here - it will block the context click no matter what
            // if (GUI.Button(new Rect(0f, 0f, position.width, position.height), "", GUIStyle.none))
            if (new Rect(0f, 0f, position.width, position.height).Contains(Event.current.mousePosition)
                && Event.current.type == EventType.MouseUp)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        /// <summary>
        ///     Abstract method where editors for the implementing system's tabs should be added to the window.
        /// </summary>
        protected abstract void CreateEditors();
    }
}
