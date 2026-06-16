using TMPro;
using UnityEngine;

namespace ProjectMSG.SaveSystem
{
    public sealed class SaveSlotSceneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private JsonSaveSystem saveSystem;
        [SerializeField] private SaveSlotDisplay[] slotDisplays;

        [Header("New Save Popup")]
        [SerializeField] private GameObject namePopup;
        [SerializeField] private TMP_InputField saveNameInput;
        [SerializeField] private TMP_Text popupTitleText;

        [Header("Popup Text")]
        [SerializeField] private string popupTitleFormat = "Name Save Slot {0}";
        [SerializeField] private string emptyNameFallbackFormat = "Save {0}";

        private int pendingNewSaveId = -1;

        private void Awake()
        {
            saveSystem = GetOrCreateSaveSystem();

            if (slotDisplays == null || slotDisplays.Length == 0)
            {
                slotDisplays = FindObjectsByType<SaveSlotDisplay>();
            }
        }

        private void Start()
        {
            HideNamePopup();
            RefreshSlots();
            saveSystem.LogSavesDirectory();
            saveSystem.LogJsonList();
        }

        public void SelectOrCreateSlot(int saveId)
        {
            HighlightSlot(saveId);

            if (saveSystem.SaveExists(saveId))
            {
                LoadSlotAndEnterPrep(saveId);
                return;
            }

            ShowNamePopup(saveId);
        }

        public void LoadSlotAndEnterPrep(int saveId)
        {
            HighlightSlot(saveId);

            if (!saveSystem.LoadGame(saveId))
            {
                Debug.LogWarning($"[SaveSystem] Failed loaded: slot {saveId} does not exist.");
                ShowNamePopup(saveId);
                return;
            }

            saveSystem.EnterPrepScene();
        }

        public void SaveCurrentToSlot(int saveId)
        {
            HighlightSlot(saveId);
            saveSystem.SaveSlot(saveId);
            RefreshSlots();
            saveSystem.LogJsonList();
        }

        public void DeleteSlot(int saveId)
        {
            HighlightSlot(saveId);
            saveSystem.DeleteSave(saveId);
            RefreshSlots();
            saveSystem.LogJsonList();
        }

        public void ConfirmNamePopup()
        {
            if (pendingNewSaveId < 0)
            {
                Debug.LogWarning("[SaveSystem] Failed saved: no pending save slot selected.");
                return;
            }

            string saveName = saveNameInput != null ? saveNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = string.Format(emptyNameFallbackFormat, pendingNewSaveId);
            }

            saveSystem.CreateNewSaveInSlot(pendingNewSaveId, saveName);
            Debug.Log($"[SaveSystem] Success saved new slot {pendingNewSaveId}: {saveName}");

            HideNamePopup();
            RefreshSlots();
            saveSystem.LogJsonList();
            saveSystem.EnterPrepScene();
        }

        public void CancelNamePopup()
        {
            HideNamePopup();
        }

        public void RefreshSlots()
        {
            if (slotDisplays == null)
            {
                return;
            }

            foreach (SaveSlotDisplay display in slotDisplays)
            {
                if (display != null)
                {
                    display.Refresh();
                }
            }
        }

        private void ShowNamePopup(int saveId)
        {
            pendingNewSaveId = saveId;

            if (popupTitleText != null)
            {
                popupTitleText.text = string.Format(popupTitleFormat, saveId);
            }

            if (saveNameInput != null)
            {
                saveNameInput.text = string.Empty;
                saveNameInput.ActivateInputField();
            }

            if (namePopup != null)
            {
                namePopup.SetActive(true);
            }

            Debug.Log($"[SaveSystem] Slot {saveId} is empty. Waiting for save name popup.");
        }

        private void HideNamePopup()
        {
            pendingNewSaveId = -1;

            if (namePopup != null)
            {
                namePopup.SetActive(false);
            }
        }

        private void HighlightSlot(int saveId)
        {
            if (slotDisplays == null)
            {
                return;
            }

            foreach (SaveSlotDisplay display in slotDisplays)
            {
                if (display != null)
                {
                    display.SetHighlighted(display.SaveId == saveId);
                }
            }
        }

        private static JsonSaveSystem GetOrCreateSaveSystem()
        {
            if (JsonSaveSystem.Instance != null)
            {
                return JsonSaveSystem.Instance;
            }

            JsonSaveSystem existingSaveSystem = FindAnyObjectByType<JsonSaveSystem>();
            if (existingSaveSystem != null)
            {
                return existingSaveSystem;
            }

            GameObject saveSystemObject = new GameObject("Save System");
            return saveSystemObject.AddComponent<JsonSaveSystem>();
        }
    }
}
