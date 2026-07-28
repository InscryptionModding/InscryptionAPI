using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;

namespace InscryptionCommunityPatch.Tests;

public class DebugCards
{
    /// <summary>
    /// This is an instantiator for a Test Case related to the Steel Trap Death Patch.
    /// </summary>
    /// <remarks>This code is provided by Creator/Chaosyr/SaxbyMod/The Stoat Lord.</remarks>
    public static void CreateTestCards()
    {
        CardInfo TestPelt = CreateCardUtil.CreateCard("Test_Pelt", "Test Pelt", 0, 4, new List<Tribe>() { Tribe.Canine });
        CardInfo Debugger = CreateCardUtil.CreateCard("Debugger", "This is a Debugger", 0, 2, new List<Tribe>() { Tribe.None });
        CardInfo Debugger2 = CreateCardUtil.CreateCard("Debugger", "This is a Debugger", 0, 2, new List<Tribe>() { Tribe.None });
        Debugger2.SetExtendedProperty("SteelTrapPelt", PatchPlugin.ModGUID + "_Test_Pelt");
        Debugger.AddAbilities(Ability.SteelTrap);
    }
    
    /// <summary>
    /// A Utility for Creating Cards (this is for debug purposes only, so we're tossing a OBSOLETE to be safe.
    /// </summary>
    /// <remarks>This code is provided by JamesGames.</remarks>
    [Obsolete("DO NOT USE THIS, THIS IS FOR DEBUGGING ONLY.")]
    private class CreateCardUtil
    {
        // Code from JamesGames
        public static CardInfo CreateCard(string name, string displayName, int attack, int health, List<Tribe> tribe)
        {
            CardInfo info = CardManager.New(PatchPlugin.ModGUID, name, displayName, attack, health);
            info.cardComplexity = CardComplexity.Simple;
            info.AddTraits(Trait.Pelt);
            foreach (Tribe currentTribe in tribe)
            {
                if (currentTribe != Tribe.None)
                {
                    info.AddTribes(currentTribe);
                }
            }
            info.temple = CardTemple.Nature;
            info.AddSpecialAbilities(SpecialTriggeredAbility.SpawnLice);
            info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);

            return info;
        }
    }
    
    /// <summary>
    /// A Utility for Creating Rare Cards (this is for debug purposes only, so we're tossing a OBSOLETE to be safe.
    /// </summary>
    /// <remarks>This code is provided by JamesGames.</remarks>
    [Obsolete("DO NOT USE THIS, THIS IS FOR DEBUGGING ONLY.")]
    private class CreateRareCardUtil
    {
        // Code from JamesGames
        public static CardInfo CreateRareCard(string name, string displayName, int attack, int health, List<Tribe> tribe)
        {
            CardInfo info = CreateCardUtil.CreateCard(name, displayName, attack, health, tribe);
            info.AddAppearances(CardAppearanceBehaviour.Appearance.GoldEmission);

            return info;
        }
    }
    
    /// <summary>
    /// A Utility for Fetching Tribes (this is for debug purposes only, so we're tossing a OBSOLETE to be safe.
    /// </summary>
    /// <remarks>This code is provided by LilySylvee.</remarks>
    [Obsolete("DO NOT USE THIS, THIS IS FOR DEBUGGING ONLY.")]
    private class GetCustomTribeUtil
    {
        // Code from Lily
        public static Tribe GetCustomTribe(string GUID, string name)
        {
            return GuidManager.GetEnumValue<Tribe>(GUID, name);
        }
    }
}
