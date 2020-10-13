using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Sample;
using UnityEngine;

namespace UnityEditor.GameFoundation.Sample
{
    [CustomEditor(typeof(ChilliConnectSample))]
    public class ChilliConnectSampleEditor : Editor
    {
        const string k_ChilliConnectPackageGuid = "33ef068b0c8cbc440bf98e7ec1737ef0";
        const string k_ChilliConnectDefine = "CHILLICONNECT_ENABLED";

        Action m_Action;

        bool m_AreChilliConnectPackagesImported;

        bool m_IsSymbolDefined;

        void OnEnable()
        {
            m_AreChilliConnectPackagesImported = IsChilliConnectSDKImported && IsChilliConnectAdaptersImported;
            m_IsSymbolDefined = IsSymbolDefinedFor(k_ChilliConnectDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        public override void OnInspectorGUI()
        {
            if (!IsChilliConnectSDKImported)
            {
                var clicked = GUILayout.Button("Get the ChilliConnect SDK Package");
                if (clicked)
                {
                    m_Action = () => Application.OpenURL("https://docs.chilliconnect.com/guide/sdks/#unity");
                }
            }
            else
            {
                if (IsChilliConnectAdaptersImported)
                {
                    if (m_IsSymbolDefined)
                    {
                        var clicked = GUILayout.Button("Use default data access layer");
                        if (clicked)
                        {
                            m_Action = () => UndefineSymbolFor(k_ChilliConnectDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
                        }
                    }
                    else
                    {
                        var clicked = GUILayout.Button("Use ChilliConnect data access layer");
                        if (clicked)
                        {
                            m_Action = () => DefineSymbolFor(k_ChilliConnectDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
                        }
                    }
                }
                else
                {
                    var clicked = GUILayout.Button("Install ChilliConnect Adapters Package");
                    if (clicked)
                    {
                        m_Action = () => InstallAdaptersPackage(); 
                    }
                }
            }

            base.OnInspectorGUI();

            PerformAction();
        }

        void PerformAction()
        {
            if (m_Action is null) return;

            try
            {
                m_Action();
            }
            finally
            {
                m_Action = null;
            }
        }

        bool IsSymbolDefinedFor(string symbol, BuildTargetGroup targetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            return defines.Contains(symbol);
        }

        static bool IsChilliConnectSDKImported => AssetDatabase.FindAssets("ChilliConnectSdk t:Script").Length > 0;

        static bool IsChilliConnectAdaptersImported => AssetDatabase.FindAssets("ChilliConnectCloudSync t:Script").Length > 0;

        static void DefineSymbolFor(string symbol, BuildTargetGroup targetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            var oldDefinedSymbols = defines.Split(
                new[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            var newDefinedSymbols = new List<string>(oldDefinedSymbols);
            newDefinedSymbols.Add(symbol);

            defines = string.Join(";", newDefinedSymbols.ToArray());

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }

        static void UndefineSymbolFor(string symbol, BuildTargetGroup targetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            var oldDefinedSymbols = defines.Split(
                new[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            var newDefinedSymbols = new List<string>(oldDefinedSymbols);
            newDefinedSymbols.Remove(symbol);

            defines = string.Join(";", newDefinedSymbols.ToArray());

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }

        static void InstallAdaptersPackage()
        {
            var packageFullPath = AssetDatabase.GUIDToAssetPath(k_ChilliConnectPackageGuid);
            AssetDatabase.ImportPackage(packageFullPath, true);
        }
    }
}
