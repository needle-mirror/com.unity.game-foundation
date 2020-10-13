using System;
using System.Collections.Generic;

// use this class template to create migrations as needed

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    static class MigrationFromV0
    {
        static GameFoundationDebug k_GFLogger = GameFoundationDebug.Get(typeof(MigrationFromV0));

        // When you want to make a real migration, you'll need this attribute.
        // [MigrateFrom(0)]
        public static object Migrate(object dataV0)
        {
            if (!(dataV0 is Dictionary<string, object> parsedRoot))
            {
                throw new ArgumentException(
                    "Invalid data encountered when trying to migrate the parsed JSON schema from version 0.");
            }

            if (!parsedRoot.ContainsKey("version") || !(parsedRoot["version"] is long parsedVersion))
            {
                throw new ArgumentException(
                    "Invalid version value encountered when trying to migrate the parsed JSON schema from version 0.");
            }

            if (parsedVersion != 0)
            {
                Debug.Log("Skipped migrating persistent json from schema version 0 because given data is not at schema version 0.");

                return dataV0;
            }

            // here is where some particular migration would happen
            // ...
            // ...
            // ...
            // ...

            // when successfully migrated, set the target version
            parsedRoot["version"] = (long)1;

            k_GFLogger.Log("Successfully migrated Game Foundation persisted data from schema version 0 to version 1.");

            return dataV0;
        }
    }
}
