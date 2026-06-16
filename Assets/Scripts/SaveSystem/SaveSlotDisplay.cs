using TMPro;
using UnityEngine;

namespace ProjectMSG.SaveSystem
{
    public sealed class SaveSlotDisplay : MonoBehaviour
    {
        [SerializeField] private int saveId = 1;
        [SerializeField] private TMP_Text saveNameText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private GameObject highlighter;
        [SerializeField] private Transform progressBarFill;
        [SerializeField] private int maxMapIndex = 5;
        [SerializeField] private string emptySlotText = "Empty Slot";

        public int SaveId => saveId;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            SaveFileRepository repository = JsonSaveSystem.Instance != null
                ? JsonSaveSystem.Instance.Repository
                : new SaveFileRepository();

            if (!repository.TryLoad(saveId, out SaveData saveData))
            {
                SetText(saveNameText, emptySlotText);
                SetText(detailText, $"Slot {saveId} | No save file");
                SetProgress(0f);
                return;
            }

            SetText(saveNameText, saveData.saveName);
            SetText(detailText, $"Map: {saveData.map} | Money: {saveData.money:0.##}");
            SetProgress(CalculateProgress(saveData.map));
        }

        public void SetHighlighted(bool isHighlighted)
        {
            if (highlighter != null)
            {
                highlighter.SetActive(isHighlighted);
            }
        }

        public static void RefreshAll()
        {
            SaveSlotDisplay[] displays = FindObjectsByType<SaveSlotDisplay>();
            foreach (SaveSlotDisplay display in displays)
            {
                display.Refresh();
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private float CalculateProgress(int map)
        {
            if (maxMapIndex <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(map / (float)maxMapIndex);
        }

        private void SetProgress(float value)
        {
            if (progressBarFill == null)
            {
                return;
            }

            Vector3 scale = progressBarFill.localScale;
            scale.x = Mathf.Clamp01(value);
            progressBarFill.localScale = scale;
        }
    }
}
