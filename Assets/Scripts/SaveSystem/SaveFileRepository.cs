using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectMSG.SaveSystem
{
    public sealed class SaveFileRepository
    {
        private const string FilePrefix = "save_";
        private const string FileExtension = ".json";
        private const string MyGamesFolderName = "My Games";
        private const string DefaultGameFolderName = "Project MSG";
        private const string SavesFolderName = "Saves";

        public string SavesDirectory { get; }

        public SaveFileRepository()
            : this(GetDefaultSavesDirectory())
        {
        }

        public SaveFileRepository(string savesDirectory)
        {
            if (string.IsNullOrWhiteSpace(savesDirectory))
            {
                throw new ArgumentException("Save directory cannot be empty.", nameof(savesDirectory));
            }

            SavesDirectory = savesDirectory;
        }

        public static string GetDefaultSavesDirectory()
        {
            string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(documentsDirectory))
            {
                documentsDirectory = Application.persistentDataPath;
            }

            string gameFolderName = string.IsNullOrWhiteSpace(Application.productName)
                ? DefaultGameFolderName
                : SanitizePathSegment(Application.productName);

            return Path.Combine(documentsDirectory, MyGamesFolderName, gameFolderName, SavesFolderName);
        }

        public static string ToJson(SaveData saveData)
        {
            ValidateSaveData(saveData);
            return JsonUtility.ToJson(saveData, true);
        }

        public SaveData Save(SaveData saveData)
        {
            ValidateSaveData(saveData);

            Directory.CreateDirectory(SavesDirectory);

            string json = ToJson(saveData);
            File.WriteAllText(GetSavePath(saveData.saveId), json);

            return saveData;
        }

        public SaveData Load(int saveId)
        {
            if (!TryLoad(saveId, out SaveData saveData))
            {
                throw new FileNotFoundException($"Save file with id {saveId} was not found.", GetSavePath(saveId));
            }

            return saveData;
        }

        public bool TryLoad(int saveId, out SaveData saveData)
        {
            ValidateSaveId(saveId);

            saveData = null;
            string savePath = GetSavePath(saveId);

            if (!File.Exists(savePath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData == null)
                {
                    return false;
                }

                saveData.EnsureSupply();
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is IOException)
            {
                saveData = null;
                return false;
            }
        }

        public IReadOnlyList<SaveData> LoadAll()
        {
            if (!Directory.Exists(SavesDirectory))
            {
                return Array.Empty<SaveData>();
            }

            List<SaveData> saves = new List<SaveData>();
            string[] saveFiles = Directory.GetFiles(SavesDirectory, $"{FilePrefix}*{FileExtension}");

            foreach (string saveFile in saveFiles)
            {
                if (!TryGetSaveIdFromPath(saveFile, out int saveId))
                {
                    continue;
                }

                if (TryLoad(saveId, out SaveData saveData))
                {
                    saves.Add(saveData);
                }
            }

            saves.Sort((left, right) => left.saveId.CompareTo(right.saveId));
            return saves;
        }

        public bool Delete(int saveId)
        {
            ValidateSaveId(saveId);

            string savePath = GetSavePath(saveId);
            if (!File.Exists(savePath))
            {
                return false;
            }

            File.Delete(savePath);
            return true;
        }

        public bool SaveExists(int saveId)
        {
            ValidateSaveId(saveId);
            return File.Exists(GetSavePath(saveId));
        }

        public int GetNextSaveId(int startAt = 1)
        {
            ValidateSaveId(startAt);

            int saveId = startAt;
            while (SaveExists(saveId))
            {
                saveId++;
            }

            return saveId;
        }

        public string GetSavePath(int saveId)
        {
            ValidateSaveId(saveId);
            return Path.Combine(SavesDirectory, $"{FilePrefix}{saveId}{FileExtension}");
        }

        private static void ValidateSaveData(SaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            ValidateSaveId(saveData.saveId);

            if (string.IsNullOrWhiteSpace(saveData.saveName))
            {
                throw new ArgumentException("Save name cannot be empty.", nameof(saveData));
            }

            saveData.EnsureSupply();
        }

        private static void ValidateSaveId(int saveId)
        {
            if (saveId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(saveId), saveId, "Save id cannot be negative.");
            }
        }

        private static bool TryGetSaveIdFromPath(string savePath, out int saveId)
        {
            string fileName = Path.GetFileNameWithoutExtension(savePath);
            string idText = fileName.StartsWith(FilePrefix, StringComparison.Ordinal)
                ? fileName.Substring(FilePrefix.Length)
                : string.Empty;

            return int.TryParse(idText, out saveId);
        }

        private static string SanitizePathSegment(string pathSegment)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            char[] sanitizedCharacters = pathSegment.Trim().ToCharArray();

            for (int i = 0; i < sanitizedCharacters.Length; i++)
            {
                if (Array.IndexOf(invalidCharacters, sanitizedCharacters[i]) >= 0)
                {
                    sanitizedCharacters[i] = '_';
                }
            }

            string sanitizedPathSegment = new string(sanitizedCharacters);
            return string.IsNullOrWhiteSpace(sanitizedPathSegment) ? DefaultGameFolderName : sanitizedPathSegment;
        }
    }
}
