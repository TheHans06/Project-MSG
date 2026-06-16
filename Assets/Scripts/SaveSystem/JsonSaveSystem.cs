using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectMSG.SaveSystem
{
    public sealed class JsonSaveSystem : MonoBehaviour
    {
        private static JsonSaveSystem instance;

        [Header("Save Defaults")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private int firstSaveId = 1;
        [SerializeField] private string defaultSaveName = "New Save";

        [Header("Scene Flow")]
        [SerializeField] private string prepSceneName = "Prep Scene";
        [SerializeField] private string gameplaySceneName = "Main Gameplay";
        [SerializeField] private string slotNameFormat = "Save {0}";

        [Header("Debug")]
        [SerializeField] private bool debugLogging = true;

        [Header("Prep Scene Supply Sync")]
        [SerializeField] private bool syncSupplyFromPrepManager = true;
        [SerializeField] private bool applySupplyWhenPrepSceneLoads = true;

        private SaveFileRepository repository;

        public static JsonSaveSystem Instance => instance;
        public SaveData CurrentSaveData { get; private set; }
        public SaveFileRepository Repository => repository ??= new SaveFileRepository();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            firstSaveId = Mathf.Max(0, firstSaveId);
            repository ??= new SaveFileRepository();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        public void UseRepository(SaveFileRepository saveRepository)
        {
            repository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
        }

        public SaveData CreateNewSave(string saveName = null)
        {
            int saveId = Repository.GetNextSaveId(firstSaveId);
            string finalSaveName = string.IsNullOrWhiteSpace(saveName) ? defaultSaveName : saveName;
            CurrentSaveData = SaveData.CreateDefault(saveId, finalSaveName);
            LogSaveDebug(BuildSaveDebugMessage("Created unsaved data", CurrentSaveData, true));
            return CurrentSaveData;
        }

        public SaveData CreateNewSaveInSlot(int saveId, string saveName = null)
        {
            string finalSaveName = string.IsNullOrWhiteSpace(saveName) ? FormatSlotName(saveId) : saveName;
            CurrentSaveData = SaveData.CreateDefault(saveId, finalSaveName);
            TryApplySupplyToPrepManager(CurrentSaveData);
            CurrentSaveData = Repository.Save(CurrentSaveData);
            LogSaveDebug(BuildSaveDebugMessage("Success saved new slot", CurrentSaveData, true));
            SaveSystemNotification.Show($"Save successful: {CurrentSaveData.saveName}");
            return CurrentSaveData;
        }

        public SaveData SelectSlot(int saveId)
        {
            SaveSlotSelectionService slotSelection = new SaveSlotSelectionService(Repository, slotNameFormat);
            CurrentSaveData = slotSelection.SelectSlot(saveId);
            LogSaveDebug(BuildSaveDebugMessage("Selected slot", CurrentSaveData, true));
            TryApplySupplyToPrepManager(CurrentSaveData);
            return CurrentSaveData;
        }

        public void SelectSlotAndEnterPrep(int saveId)
        {
            SelectSlot(saveId);
            EnterPrepScene();
        }

        public void SelectSlotAndEnterGameplay(int saveId)
        {
            SelectSlot(saveId);
            EnterGameplayScene();
        }

        public void SetCurrentSave(SaveData saveData)
        {
            CurrentSaveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
        }

        public void SaveCurrentGame()
        {
            EnsureCurrentSave();
            TrySyncSupplyFromPrepManager(CurrentSaveData);
            Repository.Save(CurrentSaveData);
            LogSaveDebug(BuildSaveDebugMessage("Success saved current game", CurrentSaveData, true));
            SaveSystemNotification.Show($"Save successful: {CurrentSaveData.saveName}");
        }

        public void SaveGame(SaveData saveData)
        {
            TrySyncSupplyFromPrepManager(saveData);
            CurrentSaveData = Repository.Save(saveData);
            LogSaveDebug(BuildSaveDebugMessage("Success saved game data", CurrentSaveData, true));
            SaveSystemNotification.Show($"Save successful: {CurrentSaveData.saveName}");
        }

        public void SaveSlot(int saveId)
        {
            EnsureCurrentSave();
            CurrentSaveData.saveId = saveId;

            if (string.IsNullOrWhiteSpace(CurrentSaveData.saveName))
            {
                CurrentSaveData.saveName = $"Save {saveId}";
            }

            TrySyncSupplyFromPrepManager(CurrentSaveData);
            Repository.Save(CurrentSaveData);
            LogSaveDebug(BuildSaveDebugMessage($"Success saved slot {saveId}", CurrentSaveData, true));
            SaveSystemNotification.Show($"Save successful: {CurrentSaveData.saveName}");
        }

        public bool LoadGame(int saveId)
        {
            if (!Repository.TryLoad(saveId, out SaveData saveData))
            {
                LogSaveDebug(BuildFailedDebugMessage("Failed loaded", saveId, "Save file does not exist or cannot be read."));
                return false;
            }

            CurrentSaveData = saveData;
            LogSaveDebug(BuildSaveDebugMessage($"Success loaded slot {saveId}", CurrentSaveData, true));
            TryApplySupplyToPrepManager(CurrentSaveData);
            return true;
        }

        public IReadOnlyList<SaveData> LoadAllSaves()
        {
            return Repository.LoadAll();
        }

        public bool DeleteSave(int saveId)
        {
            bool deleted = Repository.Delete(saveId);

            if (deleted && CurrentSaveData != null && CurrentSaveData.saveId == saveId)
            {
                CurrentSaveData = null;
            }

            LogSaveDebug(deleted
                ? BuildDeletedDebugMessage(saveId)
                : BuildFailedDebugMessage("Failed delete", saveId, "Save file does not exist."));
            return deleted;
        }

        public bool SaveExists(int saveId)
        {
            return Repository.SaveExists(saveId);
        }

        public string GetSavePath(int saveId)
        {
            return Repository.GetSavePath(saveId);
        }

        public string GetSavesDirectory()
        {
            return Repository.SavesDirectory;
        }

        public void LogSavesDirectory()
        {
            LogSaveDebug($"Directory:\n{Repository.SavesDirectory}");
        }

        public void LogJsonList()
        {
            IReadOnlyList<SaveData> saves = Repository.LoadAll();
            LogSaveDebug($"Directory:\n{Repository.SavesDirectory}");

            if (saves.Count == 0)
            {
                LogSaveDebug("JSON list:\nNo save files found.");
                return;
            }

            foreach (SaveData saveData in saves)
            {
                LogSaveDebug(BuildSaveDebugMessage($"JSON list item slot {saveData.saveId}", saveData, true));
            }
        }

        public void SaveSlot1()
        {
            SaveSlot(1);
        }

        public void SaveSlot2()
        {
            SaveSlot(2);
        }

        public void SaveSlot3()
        {
            SaveSlot(3);
        }

        public void LoadSlot1()
        {
            LoadGame(1);
        }

        public void LoadSlot2()
        {
            LoadGame(2);
        }

        public void LoadSlot3()
        {
            LoadGame(3);
        }

        public void SelectSlot1()
        {
            SelectSlotAndEnterPrep(1);
        }

        public void SelectSlot2()
        {
            SelectSlotAndEnterPrep(2);
        }

        public void SelectSlot3()
        {
            SelectSlotAndEnterPrep(3);
        }

        public void DeleteSlot1()
        {
            DeleteSave(1);
        }

        public void DeleteSlot2()
        {
            DeleteSave(2);
        }

        public void DeleteSlot3()
        {
            DeleteSave(3);
        }

        public void SetSaveName(string saveName)
        {
            EnsureCurrentSave();
            CurrentSaveData.saveName = string.IsNullOrWhiteSpace(saveName) ? defaultSaveName : saveName.Trim();
        }

        public void SetMap(int map)
        {
            EnsureCurrentSave();
            CurrentSaveData.map = map;
        }

        public void SetMoney(float amount)
        {
            EnsureCurrentSave();
            CurrentSaveData.money = amount;
        }

        public void AddMoney(float amount)
        {
            EnsureCurrentSave();
            CurrentSaveData.money += amount;
        }

        public void EnterPrepScene()
        {
            if (string.IsNullOrWhiteSpace(prepSceneName))
            {
                Debug.LogWarning("Prep scene name is empty. Cannot enter prep scene.");
                return;
            }

            SceneManager.LoadScene(prepSceneName);
        }

        public void EnterGameplayScene()
        {
            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                Debug.LogWarning("Gameplay scene name is empty. Cannot enter gameplay.");
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        private void EnsureCurrentSave()
        {
            if (CurrentSaveData == null)
            {
                CreateNewSave();
            }
        }

        private string FormatSlotName(int saveId)
        {
            if (string.IsNullOrWhiteSpace(slotNameFormat))
            {
                return defaultSaveName;
            }

            return string.Format(slotNameFormat, saveId);
        }

        private void LogSaveDebug(string message)
        {
            if (debugLogging)
            {
                Debug.Log($"[SaveSystem] {message}");
            }
        }

        private string BuildSaveDebugMessage(string status, SaveData saveData, bool includeJson)
        {
            saveData.EnsureSupply();
            string json = includeJson ? SaveFileRepository.ToJson(saveData) : "(not saved to json yet)";
            return
                $"{status}\n" +
                $"Save Name: {saveData.saveName}\n" +
                $"Desc: {saveData.GetDescription()}\n" +
                $"Supply: {saveData.GetSupplyDescription()}\n" +
                $"Slot: {saveData.saveId}\n" +
                $"Directory: {Repository.SavesDirectory}\n" +
                $"File: {Repository.GetSavePath(saveData.saveId)}\n" +
                $"JSON Format:\n{json}";
        }

        private string BuildDeletedDebugMessage(int saveId)
        {
            return
                $"Success deleted\n" +
                $"Slot: {saveId}\n" +
                $"Directory: {Repository.SavesDirectory}\n" +
                $"File: {Repository.GetSavePath(saveId)}\n" +
                "JSON Format:\n(deleted)";
        }

        private string BuildFailedDebugMessage(string status, int saveId, string reason)
        {
            return
                $"{status}\n" +
                $"Reason: {reason}\n" +
                $"Slot: {saveId}\n" +
                $"Directory: {Repository.SavesDirectory}\n" +
                $"File: {Repository.GetSavePath(saveId)}\n" +
                "JSON Format:\n(no json saved)";
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!applySupplyWhenPrepSceneLoads || scene.name != prepSceneName || CurrentSaveData == null)
            {
                return;
            }

            StartCoroutine(ApplySupplyAfterPrepStart());
        }

        private IEnumerator ApplySupplyAfterPrepStart()
        {
            yield return null;
            TryApplySupplyToPrepManager(CurrentSaveData);
        }

        private void TrySyncSupplyFromPrepManager(SaveData saveData)
        {
            if (!syncSupplyFromPrepManager || saveData == null)
            {
                return;
            }

            if (PrepManagerSaveBridge.TryReadSupply(out SupplyData supply, out string message))
            {
                saveData.supply = supply;
                LogSaveDebug(message);
                return;
            }

            saveData.EnsureSupply();
            LogSaveDebug($"Supply sync skipped: {message}");
        }

        private void TryApplySupplyToPrepManager(SaveData saveData)
        {
            if (!syncSupplyFromPrepManager || saveData == null)
            {
                return;
            }

            if (PrepManagerSaveBridge.TryApplySupply(saveData, out string message))
            {
                LogSaveDebug(message);
                return;
            }

            LogSaveDebug($"Supply apply skipped: {message}");
        }
    }
}
