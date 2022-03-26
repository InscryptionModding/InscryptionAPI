namespace InscryptionAPI.Helpers.Extensions;

public static class DialogueEnums
{
    /// <summary>
    /// All possible colors that the game is setup to use currently.
    ///
    /// TODO: Add more colors?
    /// </summary>
    public enum Colors
    {
        None,
        Gold,
        BrightGold,
        Blue,
        BrightBlue,
        DarkBlue,
        Orange,
        BrownOrange,
        Red,
        BrightRed,
        LimeGreen,
        BrightLightGreen,
        DarkLimeGreen,
        DarkSeafoam,
        GlowSeafoam
    }

    /// <summary>
    /// The values for all possible sizes for the text. Max is a size of 5, Giant. 
    /// </summary>
    public enum Size
    {
        None, // 0
        Tiny, // 1
        Small, // 2
        Normal, // 3
        Large, // 4
        Giant // 5
        
    } 
}