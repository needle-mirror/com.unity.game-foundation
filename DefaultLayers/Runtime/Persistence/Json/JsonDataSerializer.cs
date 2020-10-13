using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.MiniJson;

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    /// <summary>
    ///     DataSerializer to serialize GameFoundation's data to and from Json.
    /// </summary>
    public sealed class JsonDataSerializer : IDataSerializer
    {
        static readonly List<MethodInfo> s_Methods;

        static JsonDataSerializer()
        {
            s_Methods = new List<MethodInfo>();

            foreach (var typeInfo in Assembly.GetExecutingAssembly().GetTypes())
            foreach (var methodInfo in typeInfo.GetMethods())
            {
                if (methodInfo.GetCustomAttributes(typeof(MigrateFromAttribute), false).Length > 0)
                {
                    s_Methods.Add(methodInfo);
                }
            }
        }

        /// <inheritdoc/>
        public void Serialize(GameFoundationData data, TextWriter writer)
        {
            writer.Write(JsonUtility.ToJson(data));
        }

        /// <inheritdoc/>
        public GameFoundationData Deserialize(TextReader reader)
        {
            var jsonString = MigrateMiniJsonString(reader.ReadToEnd());

            var gameFoundationData = JsonUtility.FromJson<GameFoundationData>(jsonString);

            if (gameFoundationData.version != GameFoundationData.k_CurrentSchemaVersion)
            {
                throw new ArgumentException(
                    $"The provided json's schema version ({gameFoundationData.version}) doesn't match the current " +
                    $"{nameof(GameFoundationData)} schema version ({GameFoundationData.k_CurrentSchemaVersion}), " +
                    "or schema migration failed.");
            }

            return gameFoundationData;
        }

        /// <summary>
        ///     Migrate parsed MiniJson data to the latest GameFoundationData schema version.
        ///     Always start at version 0 and migrate each version until we are current.
        ///     Players could wait a very long time between updating their game,
        ///     so we need to always be able to migrate from version 0.
        /// </summary>
        /// <param name="jsonString">
        ///     The json string to migrate to the current schema.
        /// </param>
        /// <returns>
        ///     An object that can be serialized by MiniJson.
        /// </returns>
        static string MigrateMiniJsonString(string jsonString)
        {
            var miniJsonObject = Json.Deserialize(jsonString);

            if (miniJsonObject is Dictionary<string, object> parsedRoot
                && Convert.ToInt64(parsedRoot["version"]) != GameFoundationData.k_CurrentSchemaVersion)
            {
                for (var i = 0; i <= GameFoundationData.k_CurrentSchemaVersion; i++)
                {
                    foreach (var method in s_Methods)
                    {
                        if (method.GetCustomAttribute<MigrateFromAttribute>()?.fromVersion == i)
                        {
                            miniJsonObject = method.Invoke(null, new[] { miniJsonObject });
                            break;
                        }
                    }
                }

                jsonString = Json.Serialize(miniJsonObject);
            }

            return jsonString;
        }
    }
}
