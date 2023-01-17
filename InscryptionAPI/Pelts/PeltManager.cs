using BepInEx;
using DiskCardGame;
using InscryptionAPI.Guid;
using System.Reflection;

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

    /// <summary>
    /// Creates a new instance of CustomPeltData.
    /// </summary>
    /// <param name="info">The CardInfo of the pelt.</param>
    /// <param name="pluginGuid">GUID of the mod adding this data.</param>
    /// <param name="availableAtTrader">Whether this type of pelt can be purchased at the Trapper.</param>
    /// <param name="maxChoices">GUID of the mod adding this data.</param>
    /// <param name="abilityCount">GUID of the mod adding this data.</param>
    /// <returns>A new instance of CustomPeltData with its values filled.</returns>
    public static CustomPeltData New(
        CardInfo info, string pluginGuid = null,
        bool availableAtTrader = true, int maxChoices = 8, int abilityCount = 0)
    {
        CustomPeltData peltData = new()
        {
            PluginGUID = pluginGuid ?? TypeManager.GetModIdFromCallstack(Assembly.GetCallingAssembly()),
            CardNameOfPelt = info.name,
            AvailableAtTrader = availableAtTrader,
            MaxChoices = maxChoices,
            AbilityCount = abilityCount
        };
        AllNewPelts.Add(peltData);
        return peltData;
    }
    public static void Add(CustomPeltData data)
    {
        data.PluginGUID ??= TypeManager.GetModIdFromCallstack(Assembly.GetCallingAssembly());

        AllNewPelts.Add(data);
    }

    [Obsolete("Deprecated. Use Add(...) instead.")]
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