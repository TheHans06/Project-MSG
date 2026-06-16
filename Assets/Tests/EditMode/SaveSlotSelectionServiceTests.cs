using System;
using System.IO;
using NUnit.Framework;

namespace ProjectMSG.SaveSystem.Tests
{
    public sealed class SaveSlotSelectionServiceTests
    {
        private string testDirectory;
        private SaveFileRepository repository;
        private SaveSlotSelectionService slotSelection;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), "ProjectMSG_SlotFlowTests", Guid.NewGuid().ToString("N"));
            repository = new SaveFileRepository(testDirectory);
            slotSelection = new SaveSlotSelectionService(repository);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        [Test]
        public void SelectSlot_LoadsExistingSaveForThatSlot()
        {
            SaveData existingSave = new SaveData(2, "Existing Slot", map: 3, money: 1250f);

            repository.Save(existingSave);

            SaveData selectedSave = slotSelection.SelectSlot(2);

            Assert.That(selectedSave.saveId, Is.EqualTo(2));
            Assert.That(selectedSave.saveName, Is.EqualTo("Existing Slot"));
            Assert.That(selectedSave.map, Is.EqualTo(3));
            Assert.That(selectedSave.money, Is.EqualTo(1250f));
        }

        [Test]
        public void SelectSlot_CreatesDefaultSaveForEmptySlot()
        {
            SaveData selectedSave = slotSelection.SelectSlot(3);

            Assert.That(selectedSave.saveId, Is.EqualTo(3));
            Assert.That(selectedSave.saveName, Is.EqualTo("Save 3"));
            Assert.That(selectedSave.map, Is.EqualTo(SaveData.DefaultStartingMap));
            Assert.That(selectedSave.money, Is.EqualTo(SaveData.DefaultStartingMoney));
            Assert.That(repository.SaveExists(3), Is.True);
        }

        [Test]
        public void SelectSlot_DoesNotUseFirstFreeSlotWhenSpecificSlotIsEmpty()
        {
            repository.Save(SaveData.CreateDefault(1, "Slot 1"));

            SaveData selectedSave = slotSelection.SelectSlot(3);

            Assert.That(selectedSave.saveId, Is.EqualTo(3));
            Assert.That(repository.SaveExists(2), Is.False);
            Assert.That(repository.SaveExists(3), Is.True);
        }
    }
}
