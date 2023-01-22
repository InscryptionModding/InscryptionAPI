using DiskCardGame;

#nullable enable
namespace InscryptionAPI.TalkingCards.Helpers;

internal class CardHelpers
{
    public static CardInfo? Get(string name)
    {
        CardInfo? card;
        try
        {
            card = CardLoader.GetCardByName(name);
        }
        catch (Exception)
        {
            LogHelpers.LogError($"Couldn't find a card of name {name ?? "(null)"}!");
            return null;
        }
        return card;
    }
}
