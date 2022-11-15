using DiskCardGame;

namespace InscryptionAPI.Pelts;

public interface ICustomPeltData
{
    public string PluginGUID { get; }
    public string CardNameOfPelt { get; }
    public int MaxChoices { get; }
    public int AbilityCount { get; }
    public bool AvailableAtTrader { get; }
    public int Cost();
    public List<CardInfo> GetChoices();
}

public static class PeltManager
{
    public class CustomPeltData : ICustomPeltData
    {
        public string PluginGUID { get; set; }
        public string CardNameOfPelt { get; set; }
        public int MaxChoices { get; set; }
        public int AbilityCount { get; set; }
        public bool AvailableAtTrader { get; set; }

        public List<CardInfo> GetChoices()
        {
            return GetChoicesCallback();
        }
        
        public int Cost()
        {
            return CostCallback();
        }

        public Func<List<CardInfo>> GetChoicesCallback { get; set; }

        public Func<int> CostCallback { get; set; }
    }

    public static List<ICustomPeltData> AllNewPelts = new List<ICustomPeltData>();

    public static void New(ICustomPeltData data)
    {
        AllNewPelts.Add(data);
    }

    public static string[] AllPeltsAvailableAtTrader()
    {
        List<string> peltNames = new List<string>();
        foreach (string peltName in CardLoader.PeltNames)
        {
            ICustomPeltData data = AllNewPelts.Find((a) => a.CardNameOfPelt == peltName);
            if (data == null || data.AvailableAtTrader)
            {
                peltNames.Add(peltName);
            }
        }

        return peltNames.ToArray();
    }
}