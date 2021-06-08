using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation
{    
    /// <summary>
    ///     Result of addressables pop up draw.
    /// </summary>
    public struct AddressablesDropdownDrawResult
    {
        /// <summary>
        ///     Updated Addressables address after selection from dropdown box.
        /// </summary>
        public string updatedAddressablesAddress;
        
        /// <summary>
        ///     Feedback string to add AT BOTTOM of properties window. This string begins
        ///     empty so if ANY Addressables has significant issues (ie multiple Assets OF THE SAME TYPE),
        ///     the warning can be set and subsequently displayed in UI window to inform the developer.
        /// </summary>
        public string additionalInfo;

        /// <summary>
        ///     Construct a new AddressablesDropdownDrawResult.
        /// </summary>
        /// <param name="updatedAddressablesAddress">
        ///     The string address of the newly selected addressable asset address.
        /// </param>
        /// <param name="additionalInfo">
        ///     A string with additional information about the dropdown state.
        ///     This can include warnings and errors.
        /// </param>
        public AddressablesDropdownDrawResult(string updatedAddressablesAddress, string additionalInfo)
        {
            this.updatedAddressablesAddress = updatedAddressablesAddress;
            this.additionalInfo = additionalInfo;
        }
    }
        
    /// <summary>
    ///     Additional information about how to draw the Addressables property. 
    /// </summary>
    public struct AddressablesUIInfo
    {
        /// <summary>
        ///     The label string shown at right of address field (e.g. Texture2D).
        /// </summary>
        public string label;
        
        /// <summary>
        ///     The tool tip text shown when a developer hovers mouse over the label.
        /// </summary>
        public string tooltip;
        
        /// <summary>
        ///     Boolean for whether the tool tip is a warning or not.
        /// </summary>
        public bool isWarning;
        
        /// <summary>
        ///     Feedback string to add AT BOTTOM of properties window. This string begins
        ///     empty so if ANY Addressables has significant issues (ie multiple Assets OF THE SAME TYPE),
        ///     the warning can be set and subsequently displayed in UI window to inform the developer.
        /// </summary>
        public string additionalInfo;

        /// <summary>
        ///     Construct a new AddressablesUIInfo.
        /// </summary>
        /// <param name="label">
        ///     A label for this addressables UI.
        /// </param>
        /// <param name="tooltip">
        ///     A tooltip for this addressables UI.
        /// </param>
        /// <param name="isWarning">
        ///     True if this addressables dropdown created a warning when used.
        /// </param>
        /// <param name="additionalInfo">
        ///     Additional information about the state of the dropdown.
        ///     Could include errors and warnings.
        /// </param>
        public AddressablesUIInfo(string label, string tooltip, bool isWarning, string additionalInfo)
        {
            this.label = label;
            this.tooltip = tooltip;
            this.isWarning = isWarning;
            this.additionalInfo = additionalInfo;
        }
    }

    /// <summary>
    ///     Draws a dropdown for picking an addressable asset.
    /// </summary>
    public sealed class AddressablesDropdownDrawer
    {
        
        const float k_AddressablesLabelWidth = 80;
        
        const string k_AddressablesManyMatchesWarning =
            "          <i>* Multiple Addressables assets found - see tooltip for details.</i>";

        static string k_AddressablesInitializingToolTip =
            "Initializing Addressables package. Please wait.";
        
        static StringBuilder s_TempStringBuilder = new StringBuilder();

        /// <summary>
        ///     Draws the dropdown box for selecting Addressables.
        /// </summary>
        /// <param name="addressablesAddress">
        ///     Address of addressables
        /// </param>
        /// <param name="selectPropertyClicked">
        ///     Whether or not the addressables property is clicked.
        /// </param>
        /// <returns>
        ///     Returns an AddressablesDropdownDrawResult containing new addressables address
        ///     and additional info such as warnings
        /// </returns>
        public AddressablesDropdownDrawResult Draw(string addressablesAddress, bool selectPropertyClicked)
        {
            var drawResult = new AddressablesDropdownDrawResult(addressablesAddress, "");

            GUIContent[] addressablesGUIContent = null;
            GUIContent[] addressablesErrorGUIContent = null;

            // access cached list of Addressables assets
            var addressableAssets = AddressablesUtility.addressableAssets;

            // if Addressables assets have not been initialized
            if (addressableAssets is null)
            {
                // show single-line drop down with current value
                if (addressablesErrorGUIContent is null)
                {
                    addressablesErrorGUIContent = new GUIContent[1];
                }

                addressablesErrorGUIContent[0] = new GUIContent(
                    addressablesAddress, k_AddressablesInitializingToolTip);

                EditorGUILayout.Popup(0, addressablesErrorGUIContent,
                    GameFoundationEditorStyles.addressablesDropDownStyle);

                GUILayout.Label(new GUIContent("initializing",
                    k_AddressablesInitializingToolTip),
                    GameFoundationEditorStyles.addressablesLabelStyle,
                    GUILayout.Width(k_AddressablesLabelWidth));

                return drawResult;
            }

            // if this is the first Addressables property then allocate gui content (drop down list)
            var addressablesCount = addressableAssets.Count;
            if (addressablesGUIContent is null)
            {
                addressablesGUIContent = new GUIContent[addressablesCount];
                addressablesErrorGUIContent = new GUIContent[addressablesCount + 1];

                int i = 0;
                foreach (var kv in addressableAssets)
                {
                    // set the address for each line. note: tooltip is left blank, but set below as needed
                    addressablesGUIContent[i] = new GUIContent(kv.Key, "");
                    addressablesErrorGUIContent[i + 1] = new GUIContent(kv.Key, "");
                    ++i;
                }
            }

            var indexOfAddress = addressablesGUIContent.IndexOf(addressablesAddress);
            bool wasFound = indexOfAddress >= 0;
            
            var uiInfo = GetAddressablesUIInfo(addressablesAddress);
            drawResult.additionalInfo = uiInfo.additionalInfo;

            int option;
            GUIStyle labelStyle;
            if (wasFound)
            {
                addressablesGUIContent[indexOfAddress].tooltip = uiInfo.tooltip;
                option = EditorGUILayout.Popup(indexOfAddress, addressablesGUIContent,
                    GameFoundationEditorStyles.addressablesDropDownStyle);
                    labelStyle = uiInfo.isWarning ?
                        GameFoundationEditorStyles.addressablesWarningLabelStyle :
                        GameFoundationEditorStyles.addressablesLabelStyle;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(addressablesAddress))
                {
                    addressablesErrorGUIContent[0] = new GUIContent(
                        "None",
                        uiInfo.tooltip);
                    labelStyle = GameFoundationEditorStyles.addressablesLabelStyle;
                }
                else
                {
                    addressablesErrorGUIContent[0] = new GUIContent(
                        addressablesAddress,
                        uiInfo.tooltip);
                    labelStyle = GameFoundationEditorStyles.addressablesErrorLabelStyle;
                }
                option = EditorGUILayout.Popup(0, addressablesErrorGUIContent,
                    GameFoundationEditorStyles.addressablesDropDownStyle) - 1;
            }

            if (option >= 0)
            {
                drawResult.updatedAddressablesAddress = addressablesGUIContent[option].text;
            }

            // if notification area is clicked (between value and delete button ('X')
            var tooltip = wasFound ? "Click to select. " + uiInfo.tooltip : uiInfo.tooltip;

            // show label (i.e. "Texture2D", "AudioClip*", "3 Matches*")
            // note: detect clicks so we can select the associated Addressables asset
            if (GUILayout.Button(new GUIContent(uiInfo.label, tooltip), labelStyle,
                GUILayout.Width(k_AddressablesLabelWidth))
                && wasFound)
            {
                selectPropertyClicked = true;
            }

            // if user clicked the key, type or label then select the associated asset in proj window
            if (selectPropertyClicked && wasFound)
            {
                var mainAsset = AddressablesUtility.
                    GetNextAddressableAsset(drawResult.updatedAddressablesAddress, Selection.activeObject)?.
                    MainAsset;
                Selection.activeObject = mainAsset;
            }

            return drawResult;
        }

        /// <summary>
        ///     Setup UI feedback based on the specified address.
        /// </summary>
        /// <param name="address">
        ///     Addressables address to use as input.
        /// </param>
        /// <returns>
        ///     Struct containing the label string (shown at right of address field), the tool tip text
        ///     (shown when dev hovers mouse over the label), and additional info such as warnings.
        /// </returns>
        static AddressablesUIInfo GetAddressablesUIInfo(string address)
        {
            // validate the Address
            var addressablesUIInfo = new AddressablesUIInfo("", "", true, "");

            // if address is empty then add longer prompt
            if (string.IsNullOrEmpty(address))
            {
                addressablesUIInfo.label = "";
                addressablesUIInfo.tooltip = "Please select Addressables address to use for this property.";
            }

            // if address was NOT empty
            else
            {
                addressablesUIInfo.label = "<b><i>Invalid</i></b>";
                addressablesUIInfo.tooltip = "Please select a valid Addressables address to use for this property. The current " +
                    "address is not found in list of Addressables addresses from Addressables Groups window.";
                
                var list = new List<(System.Type type, int count)>();

                if (AddressablesUtility.GetAddressablesAssetsTypes(address, list,
                    out var totalCount, out var maxCountOfAnyType))
                {
                    if (list.Count == 1)
                    {
                        if (totalCount == 1)
                        {
                            addressablesUIInfo.label = ToUiString(list);
                            addressablesUIInfo.tooltip = $"The address \"{address}\" refers to only one asset. As always, be sure to " +
                                $"specify the correct type (i.e. \"{list[0].type}\") when retrieving the asset from " +
                                $"runtime Game Foundation.";
                            addressablesUIInfo.isWarning = false;
                        }
                        else
                        {
                            var typeStr = ToUiString(list);
                            addressablesUIInfo.label = "<b><i>" + typeStr + "*</i></b>";
                            addressablesUIInfo.tooltip = $"Warning: The address \"{address}\" refers to {totalCount} different assets " +
                                $"of the same type ({typeStr}). To access the desired Addressables asset, Game " +
                                $"Foundation will need more information at runtime (such as a label) to distinguish " +
                                $"between assets and ensure the correct one is retrieved.";
                            addressablesUIInfo.additionalInfo = k_AddressablesManyMatchesWarning;
                        }
                    }
                    else if (maxCountOfAnyType == 1)
                    {
                        addressablesUIInfo.label = $"{totalCount} Types";
                        addressablesUIInfo.tooltip = $"The address \"{address}\" refers to {totalCount} assets of different types " +
                            $"({ToUiString(list)}). As always, be sure to specify the correct type when retrieving " +
                            $"the asset from runtime Game Foundation API.";
                        addressablesUIInfo.isWarning = false;
                    }
                    else
                    {
                        addressablesUIInfo.label = $"<b><i>{totalCount} Matches*</i></b>";
                        addressablesUIInfo.tooltip = $"Warning: The address \"{address}\" refers to {totalCount} total unique assets of " +
                            $"{list.Count} different types ({ToUiString(list)}). To access the desired Addressables " +
                            $"asset for those addresses with multiple assets OF THE SAME TYPE, Game Foundation will " +
                            $"need more information at runtime (such as a label) to distinguish between assets and " +
                            $"ensure the correct one is retrieved.";
                        addressablesUIInfo.additionalInfo = k_AddressablesManyMatchesWarning;
                    }
                }
            }

            return addressablesUIInfo;
        }
        
        static string ToUiString(List<(System.Type type, int count)> list)
        {
            s_TempStringBuilder.Clear();

            for (int i = 0; i < list.Count; ++ i)
            {
                var typeStr = list[i].type.ToString();

                if (s_TempStringBuilder.Length > 0)
                {
                    if (i < list.Count - 1)
                    {
                        s_TempStringBuilder.Append(", ");
                    }
                    else
                    {
                        s_TempStringBuilder.Append(" and ");
                    }
                }

                var lastDot = typeStr.LastIndexOf('.');
                if (lastDot >= 0 && lastDot < typeStr.Length - 1)
                {
                    s_TempStringBuilder.Append(typeStr.Substring(lastDot + 1));
                }
                else
                {
                    s_TempStringBuilder.Append(typeStr);
                }
            }

            return s_TempStringBuilder.ToString();
        }
    }
}
