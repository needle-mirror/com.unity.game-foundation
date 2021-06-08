using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying an icon and text.
    /// </summary>
    public class ImageInfoView : MonoBehaviour
    {
        /// <summary>
        ///     The Image component to show an icon.
        /// </summary>
        public Image imageField => m_ImageField;

        [SerializeField]
        internal Image m_ImageField;

        /// <summary>
        ///     The TextMeshProUGUI component to show a text.
        /// </summary>
        public TextMeshProUGUI textField => m_TextField;

        [SerializeField]
        internal TextMeshProUGUI m_TextField;

        /// <summary>
        ///     Instance of the GameFoundationDebug class to use for logging.
        /// </summary>
        static readonly GameFoundationDebug k_GFLogger = GameFoundationDebug.Get<ImageInfoView>();

        /// <summary>
        ///     Sets icon and text that are displayed in this view.
        /// </summary>
        /// <param name="icon">Icon sprite to display</param>
        /// <param name="text">Text to display</param>
        public void SetView(Sprite icon, string text)
        {
            SetIcon(icon);
            SetText(text);
        }

        /// <summary>
        ///     Sets icon and text that are displayed in this view.
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="imageProperty">Property to get sprite image from</param>
        public void SetView(string text, Property imageProperty)
        {
            SetText(text);
            LoadAndSetIconSprite(imageProperty);
        }

        /// <summary>
        ///     Sets icon that is displayed in this view.
        /// </summary>
        /// <param name="icon">Icon sprite to display</param>
        public void SetIcon(Sprite icon)
        {
            if (m_ImageField is null || m_ImageField.sprite == icon)
            {
                return;
            }

            m_ImageField.sprite = icon;
            m_ImageField.preserveAspect = true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_ImageField);
#endif
        }

        /// <summary>
        ///     Set text that is displayed in this view.
        /// </summary>
        /// <param name="text">Text to display</param>
        public void SetText(string text)
        {
            if (m_TextField is null || m_TextField.text == text)
            {
                return;
            }

            m_TextField.gameObject.SetActive(!string.IsNullOrEmpty(text));
            m_TextField.text = text;
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_TextField);
#endif
        }
        
        /// <summary>
        ///     Loads a sprite from a Property and calls SetIcon if load is successful.
        /// </summary>
        void LoadAndSetIconSprite(Property imageProperty)
        {
            PrefabTools.LoadSprite(imageProperty, SetIcon, OnSpriteLoadFailed);
        }

        /// <summary>
        ///     Callback for if there is an error while trying to load a sprite from its Property.
        /// </summary>
        /// <param name="errorMessage">
        ///     The error message returned by <see cref="PrefabTools.LoadSprite"/>.
        /// </param>
        void OnSpriteLoadFailed(string errorMessage)
        {
            k_GFLogger.LogWarning(errorMessage);
        }
    }
}
