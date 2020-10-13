using System;
using System.IO;
using System.Text;
using UnityEngine.GameFoundation.Data;

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    /// <summary>
    ///     This saves data locally onto the user's device.
    /// </summary>
    public class LocalPersistence : BaseDataPersistence
    {
        /// <summary>
        ///     Suffix used for the backup save file.
        /// </summary>
        public const string kBackupSuffix = "_backup";

        /// <summary>
        ///     The relative path from <see cref="Application.persistentDataPath"/> to the save file.
        /// </summary>
        public string filename { get; }

        /// <summary>
        ///     The absolute path to the save file.
        /// </summary>
        public string fullpath { get; }

        /// <summary>
        ///     Initializes the local persistence with the given file path &amp; serializer.
        /// </summary>
        /// <param name="filename">
        ///     The relative path from <see cref="Application.persistentDataPath"/> to the save file.
        /// </param>
        /// <param name="serializer">
        ///     The data serializer to use.
        /// </param>
        public LocalPersistence(string filename, IDataSerializer serializer)
            : base(serializer)
        {
            this.filename = filename;
            fullpath = $"{Application.persistentDataPath}/{filename}";
        }

        /// <inheritdoc/>
        public override void Save(
            GameFoundationData content,
            Action onSaveCompleted = null,
            Action<Exception> onSaveFailed = null)
        {
            string pathBackup = $"{fullpath}{kBackupSuffix}";

            try
            {
                using (var sw = new StreamWriter(fullpath, false, Encoding.Default))
                {
                    serializer.Serialize(content, sw);
                }

                File.Copy(fullpath, pathBackup, true);
            }
            catch (Exception e)
            {
                onSaveFailed?.Invoke(e);
                return;
            }

            onSaveCompleted?.Invoke();
        }

        /// <inheritdoc/>
        public override void Load(
            Action<GameFoundationData> onLoadCompleted = null,
            Action<Exception> onLoadFailed = null)
        {
            string path;
            string pathBackup = $"{fullpath}{kBackupSuffix}";

            //If the main file doesn't exist we check for backup
            if (File.Exists(fullpath))
            {
                path = fullpath;
            }
            else if (File.Exists(pathBackup))
            {
                path = pathBackup;
            }
            else
            {
                onLoadFailed?.Invoke(new FileNotFoundException($"There is no file at the path \"{fullpath}\"."));
                return;
            }

            GameFoundationData data;

            var fileInfo = new FileInfo(path);
            using (var sr = new StreamReader(fileInfo.OpenRead(), Encoding.Default))
            {
                data = serializer.Deserialize(sr);
            }

            onLoadCompleted?.Invoke(data);
        }

        /// <summary>
        ///     Asynchronously delete data from the persistence layer.
        /// </summary>
        /// <param name="onDeletionCompleted">
        ///     Called when the deletion is completed with success.
        /// </param>
        /// <param name="onDeletionFailed">
        ///     Called with a detailed exception when the deletion failed.
        /// </param>
        public void Delete(Action onDeletionCompleted = null, Action<Exception> onDeletionFailed = null)
        {
            try
            {
                TryDeleteFile(fullpath);
                TryDeleteFile($"{fullpath}{kBackupSuffix}");
            }
            catch (Exception e)
            {
                onDeletionFailed?.Invoke(e);

                return;
            }

            //Since success callback shouldn't call the fail callback when it fails it is out of the try block.
            onDeletionCompleted?.Invoke();
        }

        static bool TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }
    }
}
