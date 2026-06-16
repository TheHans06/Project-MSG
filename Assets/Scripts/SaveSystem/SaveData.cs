using System;

namespace ProjectMSG.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public const int DefaultStartingMap = 0;
        public const float DefaultStartingMoney = 100000f;

        public string saveName;
        public int map;
        public float money;
        public int saveId;
        public SupplyData supply = new SupplyData();

        public SaveData()
        {
            saveName = "New Save";
            supply = new SupplyData();
        }

        public SaveData(int saveId, string saveName, int map, float money, SupplyData supply = null)
        {
            this.saveId = saveId;
            this.saveName = string.IsNullOrWhiteSpace(saveName) ? $"Save {saveId}" : saveName.Trim();
            this.map = map;
            this.money = money;
            this.supply = supply?.Clone() ?? new SupplyData();
        }

        public static SaveData CreateDefault(int saveId, string saveName = "New Save")
        {
            return new SaveData(saveId, saveName, DefaultStartingMap, DefaultStartingMoney);
        }

        public SaveData Clone()
        {
            return new SaveData(saveId, saveName, map, money, supply);
        }

        public string GetDescription()
        {
            return $"Map {map} | Money {money:0.##}";
        }

        public string GetSupplyDescription()
        {
            EnsureSupply();
            return supply.GetDescription();
        }

        public void EnsureSupply()
        {
            supply ??= new SupplyData();
        }
    }
}
