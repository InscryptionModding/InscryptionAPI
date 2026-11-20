using GBC;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Utility class for caching NPCs in Act 2. Primarily used for tracking the last triggered battle NPC.
/// </summary>
public class CachedGCBNPCDescriptor
{
    /// <summary>
    /// The ID of this NPC.
    /// </summary>
    public string ID { get; set; }
    /// <summary>
    /// This NPC's <see cref="CardTemple"/>. Used to determine the dialogue box style.
    /// </summary>
    public CardTemple BossTemple { get; set; }
    /// <summary>
    /// If this NPC is a boss.
    /// </summary>
    public bool IsBoss { get; set; }
    /// <summary>
    /// The board style used in this NPC's battle.
    /// </summary>
    public PixelBoardSpriteSetter.BoardTheme BattleBackgroundTheme { get; set; }
    /// <summary>
    /// The dialogue object for the NPC.
    /// </summary>
    public DialogueSpeaker DialogueSpeaker { get; set; }

    /// <summary>
    /// Creates a new 
    /// </summary>
    /// <param name="npc"></param>
    public CachedGCBNPCDescriptor(CardBattleNPC npc)
    {
        ID = npc.ID;
        BossTemple = npc.BossTemple;
        IsBoss = npc.IsBoss;
        BattleBackgroundTheme = npc.BattleBackgroundTheme;
        DialogueSpeaker = npc.DialogueSpeaker;
    }
}