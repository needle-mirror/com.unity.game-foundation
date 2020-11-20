using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    ///     Utility methods for the Resource assets.
    /// </summary>
    static class ResourcesAssetUtility
    {
        static ListRequest s_Request;

        static List<string> s_AllResourcesFolderPaths;

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            s_Request = Client.List();
        }

        /// <summary>
        ///     Gets the list of all the Resources folders contained in the project (Assets & Packages).
        /// </summary>
        /// <returns>
        ///     A list of the sub-folders.
        /// </returns>
        public static List<string> GetAllResourcesFolderPaths()
        {
            if (s_AllResourcesFolderPaths != null)
                return new List<string>(s_AllResourcesFolderPaths);

            s_AllResourcesFolderPaths = new List<string>();

            void FindResourcesFolder(string folderPath)
            {
                var subFolders = AssetDatabase.GetSubFolders(folderPath);

                foreach (var subFolderPath in subFolders)
                {
                    var split = subFolderPath.Split('/');
                    var subFolderName = split[split.Length - 1];
                    if (subFolderName == "Resources")
                    {
                        s_AllResourcesFolderPaths.Add(subFolderPath);
                    }
                    else
                    {
                        FindResourcesFolder(subFolderPath);
                    }
                }
            }

            FindResourcesFolder("Assets");

            if (!(s_Request?.Result is null))
            {
                foreach (var packageInfo in s_Request.Result)
                {
                    FindResourcesFolder($"Packages/{packageInfo.name}");
                }
            }

            return new List<string>(s_AllResourcesFolderPaths);
        }

        /// <summary>
        ///     Tells whether or not the specified <paramref name="fullPath"/> leads to an asset stored in the Resources
        ///     folder.
        /// </summary>
        /// <param name="fullPath">
        ///     The path of the asset.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the path leads to a Resources asset, otherwise <c>false</c>.
        /// </returns>
        public static bool IsResourcesAsset(string fullPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);

            return asset != null && fullPath.Contains("/Resources/");
        }
    }
}
