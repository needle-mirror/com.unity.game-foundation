using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Editor utility class for exchange widget.
    /// </summary>
    public static class ExchangeEditor
    {
        static readonly List<TradableDefinitionAsset> k_AvailableTradableDefinitions = new List<TradableDefinitionAsset>();

        static readonly List<ExchangeDefinitionObject> k_CurrencyExchanges = new List<ExchangeDefinitionObject>();

        static readonly List<ExchangeDefinitionObject> k_InventoryItemExchanges = new List<ExchangeDefinitionObject>();

        internal static void DrawExchangeUI(TransactionExchangeDefinitionObject exchangeDefinition)
        {
            k_CurrencyExchanges.Clear();
            k_InventoryItemExchanges.Clear();

            foreach (var exchange in exchangeDefinition.m_Exchanges)
            {
                switch (exchange.catalogItem)
                {
                    case CurrencyAsset _:
                        k_CurrencyExchanges.Add(exchange);
                        break;

                    case InventoryItemDefinitionAsset _:
                        k_InventoryItemExchanges.Add(exchange);
                        break;

                    default:
                        throw new NotSupportedException($"{nameof(ExchangeEditor)}: Cannot draw exchange UI for " +
                            $"{exchange.catalogItem.displayName} because only {nameof(CurrencyAsset)} " +
                            $"and {nameof(InventoryItemDefinitionAsset)} are supported. This item is of type" +
                            $"{exchange.catalogItem.GetType()}.");
                }
            }

            DrawExchanges<CurrencyAsset>(
                "Currencies",
                k_CurrencyExchanges,
                exchangeDefinition,
                "There is no currency to add.");

            DrawExchanges<InventoryItemDefinitionAsset>(
                "Items",
                k_InventoryItemExchanges,
                exchangeDefinition,
                "There is no inventory item to add.");
        }

        static void DrawExchanges<TTradableAsset>(
            string label,
            List<ExchangeDefinitionObject> exchangeDefinitions,
            TransactionExchangeDefinitionObject transactionExchangeDefinition,
            string cannotAddTooltip)
            where TTradableAsset : TradableDefinitionAsset
        {

            CatalogSettings.catalogAsset.GetItems(k_AvailableTradableDefinitions);

            // filter out the items that aren't the right specific type
            // TODO: there might be a better way - GetItems<T> doesn't work with TTradableAsset
            for (var i = k_AvailableTradableDefinitions.Count - 1; i >= 0; i--)
            {
                if (!(k_AvailableTradableDefinitions[i] is TTradableAsset))
                {
                    k_AvailableTradableDefinitions.RemoveAt(i);
                }
            }

            foreach (var exchangeDefinition in exchangeDefinitions)
            {
                k_AvailableTradableDefinitions.Remove(exchangeDefinition.catalogItem);
            }

            var availableNames = new string[k_AvailableTradableDefinitions.Count + 1];
            for (var i = 0; i < k_AvailableTradableDefinitions.Count; i++)
            {
                var tradableDefinition = k_AvailableTradableDefinitions[i];
                availableNames[i + 1] = tradableDefinition.displayName;
            }

            using (new GUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(label);

                using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                {
                    ExchangeDefinitionObject toRemove = null;

                    foreach (var exchangeDefinition in exchangeDefinitions)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            availableNames[0] = exchangeDefinition.catalogItem.displayName;
                            var newIndex = EditorGUILayout.Popup(0, availableNames);
                            if (newIndex != 0)
                            {
                                exchangeDefinition.m_CatalogItem = k_AvailableTradableDefinitions[newIndex - 1];
                                EditorUtility.SetDirty(CatalogSettings.catalogAsset);
                            }

                            var amount = EditorGUILayout.LongField(exchangeDefinition.m_Amount);
                            if (amount != exchangeDefinition.amount)
                            {
                                exchangeDefinition.m_Amount = amount;
                                EditorUtility.SetDirty(CatalogSettings.catalogAsset);
                            }

                            var click = GUILayout.Button((string) null, GameFoundationEditorStyles.deleteButtonStyle);
                            if (click)
                            {
                                toRemove = exchangeDefinition;
                            }
                        }
                    }

                    if (toRemove != null)
                    {
                        transactionExchangeDefinition.m_Exchanges.Remove(toRemove);
                        EditorUtility.SetDirty(CatalogSettings.catalogAsset);
                    }

                    {
                        var click = false;
                        var disabled = k_AvailableTradableDefinitions.Count <= 0;
                        var addButtonContent = new GUIContent("+");
                        if (disabled)
                        {
                            addButtonContent.tooltip = cannotAddTooltip;
                        }

                        using (new EditorGUI.DisabledScope(disabled))
                        {
                            click = GUILayout.Button(addButtonContent);
                        }

                        if (!disabled && click)
                        {
                            var exchange = new ExchangeDefinitionObject
                            {
                                m_Amount = 0,
                                m_CatalogItem = k_AvailableTradableDefinitions[0]
                            };

                            transactionExchangeDefinition.m_Exchanges.Add(exchange);
                            EditorUtility.SetDirty(CatalogSettings.catalogAsset);
                        }
                    }
                }
            }
        }
    }
}
