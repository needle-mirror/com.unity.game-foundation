using TMPro;
using UnityEditor;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Components
{
    /// <summary>
    ///     Component that manages displaying an icon and text.
    /// </summary>
    public class ItemView : MonoBehaviour
    {
        /// <summary>
        ///     The Image component to show an icon.
        /// </summary>
        public Image imageField => m_ImageField;

        [SerializeField]
        internal Image m_ImageField;

        /// <summary>
        ///     The TextMeshProUGUI component to show the display name.
        /// </summary>
        public TextMeshProUGUI displayNameTextField => m_DisplayNameTextField;

        [SerializeField]
        internal TextMeshProUGUI m_DisplayNameTextField;

        /// <summary>
        ///     The TextMeshProUGUI component to show the description.
        /// </summary>
        public TextMeshProUGUI descriptionTextField => m_DescriptionTextField;

        [SerializeField]
        internal TextMeshProUGUI m_DescriptionTextField;

        /// <summary>
        ///     Sets icon and text that are displayed in this view.
        /// </summary>
        /// <param name="icon">
        ///     Icon sprite to display
        /// </param>
        /// <param name="displayName">
        ///     Name to display
        /// </param>
        /// <param name="description">
        ///     Description to display.
        /// </param>
        public void SetItemView(Sprite icon, string displayName, string description)
        {
            SetIcon(icon);
            SetDisplayName(displayName);
            SetDescription(description);
        }

        /// <summary>
        ///     Sets icon that is displayed in this view.
        /// </summary>
        /// <param name="icon">
        ///     Icon sprite to display.
        /// </param>
        public void SetIcon(Sprite icon)
        {
            if (m_ImageField is null || m_ImageField.sprite == icon)
                return;

            m_ImageField.sprite = icon;
            m_ImageField.preserveAspect = true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_ImageField);
#endif
        }

        /// <summary>
        ///     Set the display name that is displayed in this view.
        /// </summary>
        /// <param name="displayName">
        ///     Text to display.
        /// </param>
        public void SetDisplayName(string displayName)
        {
            if (m_DisplayNameTextField is null || m_DisplayNameTextField.text == displayName)
                return;

            m_DisplayNameTextField.text = displayName;
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_DisplayNameTextField);
#endif
        }

        /// <summary>
        ///     Set the description that is displayed in this view.
        /// </summary>
        /// <param name="description">
        ///     Text to display.
        /// </param>
        public void SetDescription(string description)
        {
            if (m_DescriptionTextField is null || m_DescriptionTextField.text == description)
                return;

            m_DescriptionTextField.text = description;
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_DescriptionTextField);
#endif
        }
    }
}
