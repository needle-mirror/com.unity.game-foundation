using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using Object = UnityEngine.Object;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Collection Editor Tools class provides cross-class editor tools for the
    ///     creation/display/rule enforcing of different aspects of game foundation systems.
    /// </summary>
    static class CollectionEditorTools
    {
        /// <summary>
        ///     Converts a given display name to a definition Key which follows variable naming rules.
        ///     Conversions it makes:
        ///     1. Removes special characters with the exception of underscore
        ///     2. Removes any non English-alphabet characters at the beginning of the string, until it gets to an alphabet
        ///     character
        ///     3. Converts to camelCase
        ///     4. Removes white space
        /// </summary>
        /// <param name="displayName">
        ///     The string value entered in by the user as a display name. All types
        ///     of characters are possible for this value.
        /// </param>
        /// <returns>
        ///     A "cleaned" version of the display name, without special characters (underscores being
        ///     the exception), with the first character being a letter, no spaces, and camelCase format.
        /// </returns>
        public static string ConvertNameToKey(string displayName)
        {
            string convertedKey = displayName;

            if (convertedKey != null)
            {
                // Remove special characters
                // Regex matches anything not A-Z, a-z 0-9, underscore or space
                // and replaces with an empty string
                Regex noSpecialCharacters = new Regex(@"[^\w\s_]", RegexOptions.ECMAScript);
                convertedKey = noSpecialCharacters.Replace(convertedKey, "");

                // Remove any numbers, spaces or underscores as the first character of the string
                // Regex captures beginning of string (any character that is not a US letter)(until it reaches a US letter)
                // the replace removes everything in the first capturing group, leaving only what's in the second capturing group
                convertedKey = Regex.Replace(convertedKey, @"^([^A-z]*)([A-z]*)", "$2");

                // Convert first letter of string to lowercase
                // Regex matches the start of the string followed by a capital letter
                // and replaces it with lower case version of the same letter
                convertedKey = Regex.Replace(convertedKey, @"^[A-Z]", m => m.ToString().ToLower());

                // Convert to camel case
                // Regex finds any not newline followed by a word boundary followed by a lower case letter,
                // then replaces it with the capital version of the letter
                convertedKey = Regex.Replace(convertedKey, @"[^\n](\b[a-z])", m => m.ToString().ToUpper());

                // Remove white space
                // Removes any space character grouped in 1 or more occurences and replaces with an empty string
                convertedKey = Regex.Replace(convertedKey, @"\s+", "");
            }

            return convertedKey;
        }

        /// <summary>
        ///     This method checks the given <paramref name="definitionKey"/>
        ///     against a list of <paramref name="existingKeys"/>, making sure that the given
        ///     one doesn't conflict with any existing ones.
        ///     If it does conflict, it will add a number to the end corresponding to when there is no longer a conflict.
        /// </summary>
        /// <param name="definitionKey">
        ///     The definition Key that needs to be checked to see if it exists yet.
        /// </param>
        /// <param name="existingKeys">
        ///     The list of existing objects to check their Keys against the new definitonKey.
        /// </param>
        /// <returns>
        ///     The definition Key that is not a duplicate, either the original one passed in, or one with a valid number at the
        ///     end.
        /// </returns>
        public static string DeDuplicateNewKey(string definitionKey, HashSet<string> existingKeys)
        {
            string testKey = definitionKey;

            if (existingKeys != null && existingKeys.Count > 0)
            {
                int i = 0;
                bool conflict = existingKeys.Contains(testKey);
                while (conflict)
                {
                    i++;
                    testKey = definitionKey + i;
                    conflict = existingKeys.Contains(testKey);
                }
            }

            return testKey;
        }

        /// <summary>
        ///     Helper method which calls <see cref="ConvertNameToKey(string)"/>
        ///     followed by <see cref="DeDuplicateNewKey(string, HashSet{string})"/>.
        /// </summary>
        /// <param name="displayName">
        ///     The string value entered in by the user as a display name. All types
        ///     of characters are possible for this value.
        /// </param>
        /// <param name="existingKeys">
        ///     The list of existing objects to check their Keys against the new definitonKey.
        /// </param>
        /// <returns>
        ///     The definition Key that is not a duplicate, either the original one passed in, or one with a valid number at the
        ///     end.
        /// </returns>
        public static string CraftUniqueKey(string displayName, HashSet<string> existingKeys)
        {
            string newKey = ConvertNameToKey(displayName);
            return DeDuplicateNewKey(newKey, existingKeys);
        }

        /// <summary>
        ///     Helper method to create an path with a uniquely incremented index at the end"/>
        /// </summary>
        /// <param name="filePath">
        ///     The path being checked for uniquness. Expects the format "Assets/Resources/{NameOfFile}.asset"
        /// </param>
        /// <returns>
        ///     A new string with the full, unique path.
        /// </returns>
        public static string CreateUniqueCatalogPath(string filePath)
        {
            int index = 0;
            string testPath = filePath;
            int insertIndex = filePath.LastIndexOf(".asset");

            if (insertIndex < 0)
            {
                throw new ArgumentException(
                    $"{nameof(CollectionEditorTools)}: Cannot create a unique catalog path with given filepath argument, {filePath}. Filepath must end in .asset");
            }

            while (File.Exists(Path.Combine(Application.dataPath, testPath.Substring(7))))
            {
                index++;
                testPath = filePath.Insert(insertIndex, index.ToString());
            }

            return testPath;
        }

        /// <summary>
        ///     Helper method for adding an object to a ScriptableObject asset in the Asset Database.
        /// </summary>
        /// <param name="objectToAdd">
        ///     Object to be added to the asset.
        /// </param>
        /// <param name="asset">
        ///     Asset to which the object should be added.
        /// </param>
        public static void AssetDatabaseAddObject(Object objectToAdd, ScriptableObject asset)
        {
            AssetDatabase.AddObjectToAsset(objectToAdd, asset);
            EditorUtility.SetDirty(asset);
            AssetDatabaseUpdate();
        }

        /// <summary>
        ///     Helper method for updating the Asset Database. Calls AssetDatabase.SaveAssets() and AssetDatabase.Refresh().
        /// </summary>
        public static void AssetDatabaseUpdate()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Show the user a dialog that confirms whether they want to discard the current progress of creating a new item.
        /// </summary>
        /// <returns>
        ///     True if we should cancel the current item creation, otherwise false.
        /// </returns>
        public static bool ConfirmDiscardingNewItem()
        {
            return EditorUtility.DisplayDialog(
                "New Item In Progress",
                "You are creating a new item. Are you sure you want to discard the new item?",
                "Discard",
                "Stay");
        }

        /// <summary>
        ///     Sets GUI.enabled to whatever value is being passed in only if Application.isPlaying is false.
        /// </summary>
        /// <param name="enabled">
        ///     Boolean for whether or not to enable the GUI.
        /// </param>
        public static void SetGUIEnabledAtEditorTime(bool enabled)
        {
            if (Application.isPlaying)
            {
                return;
            }

            GUI.enabled = enabled;
        }

        /// <summary>
        ///     Sets GUI.enabled to whatever value is being passed in only if Application.isPlaying is true.
        /// </summary>
        /// <param name="enabled">
        ///     Boolean for whether or not to enable the GUI.
        /// </param>
        public static void SetGUIEnabledAtRunTime(bool enabled)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            GUI.enabled = enabled;
        }

        /// <summary>
        ///     Checks to see if the argument is a valid Id. Valid Ids are alphanumeric with optional dashes or underscores.
        ///     No whitespace or empty strings are permitted
        /// </summary>
        /// <param name="id">
        ///     id to check
        /// </param>
        /// <returns>
        ///     whether Id is valid or not
        /// </returns>
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[a-zA-Z][a-zA-Z0-9\-_]*$");
        }

        internal static bool IsNarrowWindow(EditorWindow window)
        {
            if (window == null)
            {
                return false;
            }

            return window.position.width <= 600;
        }
    }
}
