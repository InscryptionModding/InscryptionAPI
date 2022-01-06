using DiskCardGame;

namespace InscryptionAPI.Card;

public static class CardExtensions
{
    public static CardInfo CardByName(this List<CardInfo> cards, string name) => cards.FirstOrDefault(x => x.name == name);
}
