using System;

namespace ProjectMSG.SaveSystem
{
    public sealed class SaveSlotSelectionService
    {
        private const string DefaultSlotNameFormat = "Save {0}";

        private readonly SaveFileRepository repository;
        private readonly string slotNameFormat;

        public SaveSlotSelectionService(SaveFileRepository repository, string slotNameFormat = DefaultSlotNameFormat)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.slotNameFormat = string.IsNullOrWhiteSpace(slotNameFormat) ? DefaultSlotNameFormat : slotNameFormat;
        }

        public SaveData SelectSlot(int saveId)
        {
            if (repository.TryLoad(saveId, out SaveData existingSave))
            {
                return existingSave;
            }

            SaveData newSave = SaveData.CreateDefault(saveId, string.Format(slotNameFormat, saveId));
            return repository.Save(newSave);
        }
    }
}
