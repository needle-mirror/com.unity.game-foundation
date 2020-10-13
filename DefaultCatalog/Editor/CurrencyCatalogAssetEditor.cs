using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class CurrencyCatalogAssetEditor : BaseCatalogAssetEditor<CurrencyAsset>
    {
        static readonly GUIContent k_InitialAllocationText =
            new GUIContent("Initial allocation", "The amount of this currency the player will have when starting playing.");

        static readonly GUIContent k_MaximumAllocationText = new GUIContent(
            "Maximum allocation",
            "The maximum of this currency the player can own. " +
            "0 means no limit.");

        static readonly GUIContent k_TypeText = new GUIContent(
            "Type",
            "Tells if this currency is Soft or Hard for informational purposes. " +
            "Has no effect on how it is managed by Game Foundation.");

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Currencies;

        public CurrencyCatalogAssetEditor(string name, EditorWindow window)
            : base(name, window) { }

        protected override void DrawGeneralFields(CurrencyAsset currency)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var initialChanged = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Initial allocation");
                    var initialAllocation = EditorGUILayout.LongField(k_InitialAllocationText, currency.m_InitialBalance);
                    if (initialChanged.changed)
                    {
                        currency.Editor_SetInitialBalance(initialAllocation);
                    }
                }

                using (var maxChanged = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Maximum allocation");
                    var maximumAllocation = EditorGUILayout.LongField(k_MaximumAllocationText, currency.m_MaximumBalance);
                    if (maxChanged.changed)
                    {
                        currency.Editor_SetMaximumBalance(maximumAllocation);
                    }
                }

                using (var typeChanged = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Type");
                    var type = EditorGUILayout.EnumPopup(k_TypeText, currency.m_Type);
                    if (typeChanged.changed)
                    {
                        currency.Editor_SetType((CurrencyType)type);
                    }
                }
            }
        }
    }
}
