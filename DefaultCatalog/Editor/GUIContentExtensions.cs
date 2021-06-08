using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     This contains extension methods used to extend
    ///     functionality of classes based on GUIContent.
    /// </summary>
    public static class GUIContentExtensions
    {
        /// <summary>
        ///     Returns index of GUIContent in a GUIContent array that matches the given text
        /// </summary>
        /// <param name="guiContentArray">
        ///     GUIContent array to search.
        /// </param>
        /// <param name="stringToFind">
        ///     GUIContext.text to find in the GUIContent array.
        /// </param>
        /// <returns>
        ///     Index of GUIContent with matching string as GUIContent.text
        /// </returns>
        public static int IndexOf(this GUIContent[] guiContentArray, string stringToFind)
        {
            for (int i = 0; i < guiContentArray.Length; ++i)
            {
                if (guiContentArray[i].text == stringToFind)
                {
                    return i;
                }
            }
            
            return -1;
        }
    }
}
