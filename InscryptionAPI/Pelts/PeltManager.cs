using DiskCardGame;

namespace InscryptionAPI.Pelts;

public static class PeltManager
{
    public class PeltData
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

    private static List<PeltData> AllNewPelts = new List<PeltData>();
    private static List<PeltData> BasePelts = null;

    internal static List<PeltData> AllPelts()
    {
        BasePelts ??= CreateBasePelts();
        return BasePelts.Concat(AllNewPelts).ToList();
    }
    
    private static List<PeltData> CreateBasePelts()
    {
        List<PeltData> pelts = new List<PeltData>();
        
        string[] peltNames = CardLoader.PeltNames;
        for (int i = 0; i < peltNames.Length; i++)
        {
            int peltIndex = i;
            pelts.Add(new PeltData()
            {
                CardNameOfPelt = peltNames[i],
                MaxChoices = 8,
                AbilityCount = 0,
                AvailableAtTrader = true,
                CostCallback = ()=> SpecialNodeHandler.Instance.buyPeltsSequencer.PeltPrices[peltIndex]
            });
        }

        // Golden Pelt
        pelts[2].AbilityCount = 1;
        pelts[2].MaxChoices = 4;

        return pelts;
    }

    public static void New(PeltData data)
    {
        AllNewPelts.Add(data);
    }

    public static List<PeltData> AllPeltsAvailableAtTrader()
    {
        List<PeltData> peltNames = new List<PeltData>();
        foreach (PeltData data in AllPelts())
        {
            if (data.AvailableAtTrader)
            {
                peltNames.Add(data);
            }
        }

        return peltNames;
    }
    
    public static int GetCostOfPelt(string peltName)
    {
        PeltData pelt = GetPelt(peltName);
        if (pelt == null)
        {
            return 1;
        }
        
        return pelt.Cost();
    }
    
    public static PeltData GetPelt(string peltName)
    {
        return AllPelts().Find((a) => a.CardNameOfPelt == peltName);
    }
}