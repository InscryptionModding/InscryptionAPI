using GBC;

namespace InscryptionAPI.Encounters;

public class CachedGCBNPCDescriptor
{
    public string ID { get; set; }
    public CardTemple BossTemple { get; set; }
    public bool IsBoss { get; set; }
    public PixelBoardSpriteSetter.BoardTheme BattleBackgroundTheme { get; set; }
    public DialogueSpeaker DialogueSpeaker { get; set; }

    public CachedGCBNPCDescriptor(CardBattleNPC npc)
    {
        ID = npc.ID;
        BossTemple = npc.BossTemple;
        IsBoss = npc.IsBoss;
        BattleBackgroundTheme = npc.BattleBackgroundTheme;
        DialogueSpeaker = npc.DialogueSpeaker;
    }
}