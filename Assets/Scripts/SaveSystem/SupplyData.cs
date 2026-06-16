using System;

namespace ProjectMSG.SaveSystem
{
    [Serializable]
    public class SupplyData
    {
        public const int DefaultSupplyAmount = 4;

        public int bakso;
        public int mie;
        public int mangkok;
        public int msg;

        public SupplyData()
        {
            bakso = DefaultSupplyAmount;
            mie = DefaultSupplyAmount;
            mangkok = DefaultSupplyAmount;
            msg = DefaultSupplyAmount;
        }

        public SupplyData(int bakso, int mie, int mangkok, int msg)
        {
            this.bakso = bakso;
            this.mie = mie;
            this.mangkok = mangkok;
            this.msg = msg;
        }

        public SupplyData Clone()
        {
            return new SupplyData(bakso, mie, mangkok, msg);
        }

        public string GetDescription()
        {
            return $"Bakso {bakso} | Mie {mie} | Mangkok {mangkok} | MSG {msg}";
        }
    }
}
