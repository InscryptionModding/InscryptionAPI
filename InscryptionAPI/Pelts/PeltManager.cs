using DiskCardGame;

namespace InscryptionAPI.Pelts;

public static class PeltManager
{
    public class CustomPeltData
    {
        public virtual string PluginGUID { get; set; } = null;
        public virtual string CardNameOfPelt { get; set; } = null;
        public virtual int MaxChoices { get; set; } = 8;
        public virtual int AbilityCount { get; set; } = 0;
        public virtual bool AvailableAtTrader { get; set; } = true;

        public virtual List<CardInfo> GetChoices()
        {
            return GetChoicesCallback();
        }
        
        public virtual int Cost()
        {
            return CostCallback();
        }

        public virtual Func<List<CardInfo>> GetChoicesCallback { get; set; }

        public virtual Func<int> CostCallback { get; set; }
    }

    public static List<CustomPeltData> AllNewPelts = new List<CustomPeltData>();

    public static void New(CustomPeltData data)
    {
        AllNewPelts.Add(data);
    }

    public static string[] AllPeltsAvailableAtTrader()
    {
        List<string> peltNames = new List<string>();
        foreach (string peltName in CardLoader.PeltNames)
        {
            CustomPeltData data = AllNewPelts.Find((a) => a.CardNameOfPelt == peltName);
            if (data == null || data.AvailableAtTrader)
            {
                peltNames.Add(peltName);
            }
        }

        return peltNames.ToArray();
    }
}