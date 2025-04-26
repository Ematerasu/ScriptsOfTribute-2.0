using UnityEngine;
using ScriptsOfTribute;
using Unity.VisualScripting;

public static class CardUtils
{
    public static Sprite LoadCardSprite(PatronId deck, CardId cardId)
    {
        string deckName = deck.ToString().Replace("_", string.Empty).ToLower().FirstCharacterToUpper();
        string cardName = cardId.ToString().ToLower().Replace("_", "-");
        string path = $"Sprites/Cards/{deckName}/{cardName}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Nie znaleziono sprite'a dla ścieżki: {path}");
        }

        return sprite;
    }

    public static string GetDeckDisplayName(PatronId deck)
    {
        return deck switch
        {
            PatronId.ANSEI => "Ansei",
            PatronId.DUKE_OF_CROWS => "Duke of Crows",
            PatronId.RAJHIN => "Rajhin",
            PatronId.PSIJIC => "Psijic",
            PatronId.ORGNUM => "Orgnum",
            PatronId.HLAALU => "Hlaalu",
            PatronId.PELIN => "Pelin",
            PatronId.RED_EAGLE => "Red Eagle",
            PatronId.SAINT_ALESSIA => "Saint Alessia",
            PatronId.TREASURY => "Treasury",
            _ => "Unknown"
        };
    }

    public static string GetFullDeckDisplayName(PatronId id)
    {
        return id switch
        {
            PatronId.ANSEI => "Ansei Frandar Hunding",
            PatronId.DUKE_OF_CROWS => "Duke of Crows",
            PatronId.RAJHIN => "Rajhin, the Purring Liar",
            PatronId.PSIJIC => "Psijic Loremaster Celarus",
            PatronId.ORGNUM => "Sorcerer-King Orgnum",
            PatronId.HLAALU => "Grandmaster Delmene Hlaalu",
            PatronId.PELIN => "Saint Pelin",
            PatronId.RED_EAGLE => "Red Eagle, King of the Reach",
            PatronId.SAINT_ALESSIA => "Saint Alessia",
            PatronId.TREASURY => "Treasury",
            _ => "Treasury"
        };
    }
}
