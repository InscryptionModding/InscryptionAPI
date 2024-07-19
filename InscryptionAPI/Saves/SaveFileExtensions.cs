using GBC;
using InscryptionAPI.Encounters;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.Saves;

public static class SaveFileExtensions
{
    /// <summary>
    /// Gets the player's current location as a CardTemple
    /// </summary>
    /// <returns>The temple of the player's current location OR null if the player is in an ambiguous location</returns>
    public static CardTemple? GetSceneAsCardTemple(this SaveFile save)
    {
        // Easy stuff
        if (save.IsGrimora)
            return CardTemple.Undead;
        if (save.IsMagnificus)
            return CardTemple.Wizard;
        if (save.IsPart1)
            return CardTemple.Nature;
        if (save.IsPart3)
            return CardTemple.Tech;

        // Now the hard part; if this is Act 2
        if (save.IsPart2)
        {
            // If there is an active battle, we should be able to get it from the NPC
            var npc = GBCEncounterManager.Instance.GetTriggeringNPC();
            if (npc != null)
            {
                // Translate the theme to a card temlpe
                if (npc.BattleBackgroundTheme == PixelBoardSpriteSetter.BoardTheme.Nature)
                    return CardTemple.Nature;
                if (npc.BattleBackgroundTheme == PixelBoardSpriteSetter.BoardTheme.Tech)
                    return CardTemple.Tech;
                if (npc.BattleBackgroundTheme == PixelBoardSpriteSetter.BoardTheme.P03)
                    return CardTemple.Tech;
                if (npc.BattleBackgroundTheme == PixelBoardSpriteSetter.BoardTheme.Undead)
                    return CardTemple.Undead;
                if (npc.BattleBackgroundTheme == PixelBoardSpriteSetter.BoardTheme.Wizard)
                    return CardTemple.Wizard;

                // A bit of an arbitrary choice here for "finale"
                // P03 takes over so...
                if (npc.BattleBackgroundTheme == PixelBoardSpriteSetter.BoardTheme.Finale)
                    return CardTemple.Tech;
            }

            // Okay, let's try to figure it out from the scene name
            string sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();
            if (sceneName.Contains("nature"))
                return CardTemple.Nature;
            if (sceneName.Contains("tech"))
                return CardTemple.Tech;
            if (sceneName.Contains("wizard"))
                return CardTemple.Wizard;
            if (sceneName.Contains("undead"))
                return CardTemple.Undead;
        }

        // And now we're at the point where there's no way to figure it out.
        // You're either in a neutral area of the Act 2 map
        // Or you're not in a game scene at all.
        return null;
    }
}