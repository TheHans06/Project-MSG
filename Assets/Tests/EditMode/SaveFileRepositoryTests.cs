using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace ProjectMSG.SaveSystem.Tests
{
    public sealed class SaveFileRepositoryTests
    {
        private string testDirectory;
        private SaveFileRepository repository;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), "ProjectMSG_SaveTests", Guid.NewGuid().ToString("N"));
            repository = new SaveFileRepository(testDirectory);
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
        public void DefaultSavesDirectory_UsesDocumentsMyGamesFolder()
        {
            string savesDirectory = SaveFileRepository.GetDefaultSavesDirectory();

            Assert.That(savesDirectory, Does.Contain("My Games"));
            Assert.That(Path.GetFileName(savesDirectory), Is.EqualTo("Saves"));
        }

        [Test]
        public void Save_WritesRequiredJsonFields()
        {
            SaveData saveData = CreateSampleSave(3);

            repository.Save(saveData);

            string json = File.ReadAllText(repository.GetSavePath(3));
            Assert.That(json, Does.Contain("\"saveName\""));
            Assert.That(json, Does.Contain("\"map\""));
            Assert.That(json, Does.Contain("\"money\""));
            Assert.That(json, Does.Contain("\"saveId\""));
            Assert.That(json, Does.Not.Contain("\"bakso\""));
            Assert.That(json, Does.Not.Contain("\"bahanBakso\""));
        }

        [Test]
        public void Load_ReturnsSavedData()
        {
            SaveData saveData = CreateSampleSave(1);

            repository.Save(saveData);
            SaveData loaded = repository.Load(1);

            Assert.That(loaded.saveName, Is.EqualTo("Warung Day 4"));
            Assert.That(loaded.map, Is.EqualTo(2));
            Assert.That(loaded.money, Is.EqualTo(4500.5f).Within(0.001f));
            Assert.That(loaded.saveId, Is.EqualTo(1));
        }

        [Test]
        public void LoadAll_ReturnsSavesSortedById()
        {
            repository.Save(CreateSampleSave(3));
            repository.Save(CreateSampleSave(1));
            repository.Save(CreateSampleSave(2));

            var saves = repository.LoadAll();

            Assert.That(saves, Has.Count.EqualTo(3));
            Assert.That(saves[0].saveId, Is.EqualTo(1));
            Assert.That(saves[1].saveId, Is.EqualTo(2));
            Assert.That(saves[2].saveId, Is.EqualTo(3));
        }

        [Test]
        public void Delete_RemovesSaveFile()
        {
            repository.Save(CreateSampleSave(5));

            bool deleted = repository.Delete(5);

            Assert.That(deleted, Is.True);
            Assert.That(repository.SaveExists(5), Is.False);
        }

        [Test]
        public void GetNextSaveId_ReturnsFirstFreeSlot()
        {
            repository.Save(CreateSampleSave(1));
            repository.Save(CreateSampleSave(2));

            int nextSaveId = repository.GetNextSaveId();

            Assert.That(nextSaveId, Is.EqualTo(3));
        }

        [Test]
        public void Save_RejectsNegativeSaveId()
        {
            SaveData saveData = CreateSampleSave(-1);

            Assert.Throws<ArgumentOutOfRangeException>(() => repository.Save(saveData));
        }

        [Test]
        public void TryLoad_ReturnsFalseWhenMissing()
        {
            bool loaded = repository.TryLoad(99, out SaveData saveData);

            Assert.That(loaded, Is.False);
            Assert.That(saveData, Is.Null);
        }

        [Test]
        public void JsonUtility_CanDeserializeExpectedShape()
        {
            string json = "{ \"saveName\": \"Slot A\", \"map\": 4, \"money\": 99.5, \"saveId\": 7 }";

            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            Assert.That(saveData.saveName, Is.EqualTo("Slot A"));
            Assert.That(saveData.map, Is.EqualTo(4));
            Assert.That(saveData.money, Is.EqualTo(99.5f).Within(0.001f));
            Assert.That(saveData.saveId, Is.EqualTo(7));
        }

        private static SaveData CreateSampleSave(int saveId)
        {
            return new SaveData(
                saveId,
                "Warung Day 4",
                map: 2,
                money: 4500.5f);
        }
    }
}
