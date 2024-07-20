using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace InscryptionAPI.Slots;

/// <summary>
/// Manager for card slot modifications
/// </summary>
[HarmonyPatch]
public class SlotModificationManager : MonoBehaviour
{
    private static SlotModificationManager m_instance;

    /// <summary>
    /// Singleton instance of the slot modification manager
    /// </summary>
    public static SlotModificationManager Instance
    {
        get
        {
            if (m_instance != null)
                return m_instance;
            Instantiate();
            return m_instance;
        }
        set => m_instance = value;
    }

    private static void Instantiate()
    {
        if (m_instance != null)
            return;

        GameObject slotModManager = new("SlotModificationManager");
        slotModManager.transform.SetParent(BoardManager.Instance.gameObject.transform);
        m_instance = slotModManager.AddComponent<SlotModificationManager>();
    }

    /// <summary>
    /// Unique identifiers for slot modifications.
    /// </summary>
    public enum ModificationType
    {
        NoModification = 0
    }

    /// <summary>
    /// Contains information about a slot modification.
    /// </summary>
    public class Info
    {
        public string Name { get; internal set; }
        public string ModGUID { get; internal set; }

        /// <summary>
        /// The slot's modified texture in 3D scenes (Leshy, P03, etc) (154x226)
        /// </summary>
        public Dictionary<CardTemple, Texture2D> Texture { get; internal set; }

        /// <summary>
        /// The slots modified textures in 2D scenes (Act 2) (44x58)
        /// </summary>
        /// <remarks>Each NPC has a slightly different theme, so custom slot textures need to accomodate these themes.</remarks>
        public Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> PixelBoardSlotSprites { get; internal set; }

        /// <summary>
        /// Unique identifier for the slot modification. This will be assigned by the API.
        /// </summary>
        public ModificationType ModificationType { get; internal set; }

        /// <summary>
        /// Class that contains the behavior for the slot. This must be a subclass of SlotModificationBehaviour
        /// </summary>
        public Type SlotBehaviour { get; internal set; }

        public Info(string name, string modGuid,
            Dictionary<CardTemple, Texture2D> texture,
            Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> pixelSprites,
            ModificationType modType, Type behaviour)
        {
            this.Name = name;
            this.ModGUID = modGuid;
            this.Texture = texture;
            this.PixelBoardSlotSprites = pixelSprites;
            this.ModificationType = modType;
            this.SlotBehaviour = behaviour;
        }
        public Info Clone()
        {
            return new(this.Name, this.ModGUID,
                this.Texture != null ? new(this.Texture) : null,
                this.PixelBoardSlotSprites != null ? new(this.PixelBoardSlotSprites) : null,
                this.ModificationType, this.SlotBehaviour);
        }
    }

    internal static List<Info> AllSlotModifications = new() {
        new ("NoModification", InscryptionAPIPlugin.ModGUID, null, null, ModificationType.NoModification, null)
    };

    public static List<Info> AllModificationInfos { get; private set; } = new();
    public static List<ModificationType> AllModificationTypes { get; private set; } = new();

    public static void SyncSlotModificationList()
    {
        AllModificationInfos = AllSlotModifications.ConvertAll(x => x.Clone()).ToList();
        AllModificationTypes = AllSlotModifications.ConvertAll(x => x.ModificationType).ToList();
    }

    private static Color ParseHtml(string html)
    {
        if (ColorUtility.TryParseHtmlString(html, out Color c))
            return c;
        return Color.white;
    }

    private static readonly Color TRANSPARENT = new Color(0f, 0f, 0f, 0f);

    private static readonly Dictionary<PixelBoardSpriteSetter.BoardTheme, Tuple<Color, Color>> DEFAULT_COLORS = new()
    {
        { PixelBoardSpriteSetter.BoardTheme.Tech, new(ParseHtml("#446969"), ParseHtml("#B4FFEC")) },
        { PixelBoardSpriteSetter.BoardTheme.P03, new(ParseHtml("#446969"), ParseHtml("#B4FFEC")) },
        { PixelBoardSpriteSetter.BoardTheme.Nature, new(ParseHtml("#FF9226"), ParseHtml("#F7C376")) },
        { PixelBoardSpriteSetter.BoardTheme.Wizard, new(ParseHtml("#C1D080"), ParseHtml("#EEF4C6")) },
        { PixelBoardSpriteSetter.BoardTheme.Undead, new(ParseHtml("#C1D080"), ParseHtml("#EEF4C6")) },
        { PixelBoardSpriteSetter.BoardTheme.Finale, new(ParseHtml("#E14C89"), ParseHtml("#F779AD")) },
    };

    private static Texture2D ConvertAct2TextureColor(Texture2D tex, Color targetColor)
    {
        Texture2D newTex = TextureHelper.DuplicateTexture(tex);
        newTex.filterMode = FilterMode.Point;
        for (int x = 0; x < newTex.width; x++)
        {
            for (int y = 0; y < newTex.height; y++)
            {
                var pix = tex.GetPixel(x, y);
                if (pix.a < 1)
                    newTex.SetPixel(x, y, TRANSPARENT);
                else if (pix == Color.black)
                    newTex.SetPixel(x, y, Color.black);
                else
                    newTex.SetPixel(x, y, targetColor);
            }
        }
        newTex.Apply();
        return newTex;
    }

    private static PixelBoardSpriteSetter.BoardThemeSpriteSet GetSpriteSetFromTexture(Texture2D tex, PixelBoardSpriteSetter.BoardTheme theme)
    {
        int offset = theme == PixelBoardSpriteSetter.BoardTheme.Finale ? 4 : (int)theme;
        bool hasOpponentSlots = tex.height == 232;

        Sprite playerSprite = Sprite.Create(tex, new Rect(0f + (float)offset * 44f, tex.height - 58f, 44f, 58f), new Vector2(0.5f, 0.5f));
        Sprite playerHoverSprite = Sprite.Create(tex, new Rect(0f + (float)offset * 44f, tex.height - 116f, 44f, 58f), new Vector2(0.5f, 0.5f));

        Sprite opponentSprite = !hasOpponentSlots ? playerSprite : Sprite.Create(tex, new Rect(0f + (float)offset * 44f, tex.height - 174f, 44f, 58f), new Vector2(0.5f, 0.5f));
        Sprite opponentHoverSprite = !hasOpponentSlots ? playerHoverSprite : Sprite.Create(tex, new Rect(0f + (float)offset * 44f, 0f, 44f, 58f), new Vector2(0.5f, 0.5f));

        List<PixelBoardSpriteSetter.BoardThemeSpriteSet.SpecificSlotSprites> opponentSprites = new();
        if (hasOpponentSlots)
        {
            for (int i = 0; i < 4; i++)
            {
                opponentSprites.Add(new()
                {
                    playerSlot = false,
                    index = i,
                    slotDefault = opponentSprite,
                    slotHighlight = opponentHoverSprite
                });
            }
        }

        return new()
        {
            id = theme,
            slotDefault = playerSprite,
            slotHighlight = playerHoverSprite,
            specificSlotSprites = opponentSprites,
            flipPlayerSlotSpriteX = false,
            flipPlayerSlotSpriteY = false
        };
    }

    /// <summary>
    /// Converts a sheet of act 2 slot textures into a full collection of sprite sets. READ THE DOCUMENTATION.
    /// </summary>
    /// <param name="texture"></param>
    /// <remarks>This expects a texture with 8 slot textures in it, arranged in either 2 or 4 rows of 5. Each slot texture is 44x58.
    /// The first row contains all of the the normal slot sprites and the second row contains all of the hover sprites.
    /// If you want the opponent sprites to be different, you have to provide 4 rows; the third row contains normal opponent slot sprites and the fourth row contains hovered opponent slot sprites.
    /// Each row must go in this order: nature, undead, tech, wizard, finale.
    public static Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> BuildAct2SpriteSetFromSpriteSheetTexture(Texture2D texture)
    {
        Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> retval = new();

        List<PixelBoardSpriteSetter.BoardTheme> themes = Enum.GetValues(typeof(PixelBoardSpriteSetter.BoardTheme)).Cast<PixelBoardSpriteSetter.BoardTheme>().ToList();
        themes.Remove(PixelBoardSpriteSetter.BoardTheme.P03);
        themes.Remove(PixelBoardSpriteSetter.BoardTheme.NUM_THEMES);

        foreach (var theme in themes)
            retval[theme] = GetSpriteSetFromTexture(texture, theme);

        return retval;
    }

    /// <summary>
    /// Converts a single act 2 slot texture into a full collection of sprite sets by repeatedly recoloring the texture
    /// </summary>
    /// <param name="texture"></param>
    public static Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> BuildAct2SpriteSetFromTexture(Texture2D texture)
    {
        Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> retval = new();
        foreach (PixelBoardSpriteSetter.BoardTheme theme in Enum.GetValues(typeof(PixelBoardSpriteSetter.BoardTheme)))
        {
            if (theme == PixelBoardSpriteSetter.BoardTheme.NUM_THEMES)
                continue;

            Texture2D slotTexture = ConvertAct2TextureColor(texture, DEFAULT_COLORS[theme].Item1);
            Texture2D highlightedTexture = ConvertAct2TextureColor(texture, DEFAULT_COLORS[theme].Item2);

            PixelBoardSpriteSetter.BoardThemeSpriteSet spriteSet = new();
            spriteSet.id = theme;
            spriteSet.slotDefault = Sprite.Create(slotTexture, new Rect(0f, 0f, 44f, 58f), new Vector2(0.5f, 0.5f));
            spriteSet.slotHighlight = Sprite.Create(highlightedTexture, new Rect(0f, 0f, 44f, 58f), new Vector2(0.5f, 0.5f));
            spriteSet.specificSlotSprites = new();

            retval[theme] = spriteSet;
        }
        return retval;
    }

    internal readonly Dictionary<CardSlot, Tuple<ModificationType, SlotModificationBehaviour>> SlotReceivers = new();

    /// <summary>
    /// Creates a new card slot modification
    /// </summary>
    /// <param name="modGuid">Unique ID for the mod creating the slot modification</param>
    /// <param name="modificationName">Reference name for the slot modification</param>
    /// <param name="behaviour">The class that controls the behavior for the new slot</param>
    /// <param name="slotTexture">The 3D scene slot texture (154x226)</param>
    /// <param name="slotPixelTexture">The 2D scene slot texture</param>
    /// <returns>Unique identifier for the modification type; used to set the slot modification in the future</returns>
    public static ModificationType New(string modGuid, string modificationName, Type behaviour, Dictionary<CardTemple, Texture2D> slotTexture, Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> pixelBoardSlotSprites)
    {
        if (!behaviour.IsSubclassOf(typeof(SlotModificationBehaviour)))
            throw new InvalidOperationException("The slot behavior must be a subclass of SlotModificationBehaviour");

        ModificationType mType = GuidManager.GetEnumValue<ModificationType>(modGuid, modificationName);

        AllSlotModifications.Add(new(modificationName, modGuid, slotTexture, pixelBoardSlotSprites, mType, behaviour));
        return mType;
    }

    /// <summary>
    /// Creates a new card slot modification
    /// </summary>
    /// <param name="modGuid">Unique ID for the mod creating the slot modification</param>
    /// <param name="modificationName">Reference name for the slot modification</param>
    /// <param name="behaviour">The class that controls the behavior for the new slot</param>
    /// <param name="slotTexture">The 3D scene slot texture (154x226)</param>
    /// <param name="slotPixelTexture">The 2D scene slot texture</param>
    /// <returns>Unique identifier for the modification type; used to set the slot modification in the future</returns>
    public static ModificationType New(string modGuid, string modificationName, Type behaviour, Texture2D slotTexture, Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> pixelBoardSlotSprites)
    {
        Dictionary<CardTemple, Texture2D> templeMap = new();
        templeMap[CardTemple.Nature] = slotTexture;
        templeMap[CardTemple.Tech] = slotTexture;
        templeMap[CardTemple.Undead] = slotTexture;
        templeMap[CardTemple.Wizard] = slotTexture;
        return New(modGuid, modificationName, behaviour, templeMap, pixelBoardSlotSprites);
    }

    /// <summary>
    /// Creates a new card slot modification
    /// </summary>
    /// <param name="modGuid">Unique ID for the mod creating the slot modification</param>
    /// <param name="modificationName">Reference name for the slot modification</param>
    /// <param name="behaviour">The class that controls the behavior for the new slot</param>
    /// <param name="slotTexture">The 3D scene slot texture (154x226)</param>
    /// <param name="pixelSlotTexture">The 2D scene slot texture. If it is the size of a single slot (44x58) it will be color converted to match each theme. If it is the size of a slot sprite sheet (220x116) it will be sliced into individual sprites.</param>
    /// <returns>Unique identifier for the modification type; used to set the slot modification in the future</returns>
    public static ModificationType New(string modGuid, string modificationName, Type behaviour, Texture2D slotTexture, Texture2D pixelSlotTexture)
    {
        Dictionary<PixelBoardSpriteSetter.BoardTheme, PixelBoardSpriteSetter.BoardThemeSpriteSet> spriteSet = null;
        if (pixelSlotTexture != null)
        {
            if (pixelSlotTexture.width == 44 && pixelSlotTexture.height == 58)
                spriteSet = BuildAct2SpriteSetFromTexture(pixelSlotTexture);
            else if (pixelSlotTexture.width == 220 && (pixelSlotTexture.height == 116 || pixelSlotTexture.height == 232))
                spriteSet = BuildAct2SpriteSetFromSpriteSheetTexture(pixelSlotTexture);
            else
                throw new InvalidOperationException($"Cannot create slot mod {modGuid}/{modificationName}. The pixel slot texture must either be a single slot (44x58) or a 5x2 sprite sheet (220x116) or a 5x4 sprite sheet (220x232)");
        }
        return New(modGuid, modificationName, behaviour, slotTexture, spriteSet);
    }

    /// <summary>
    /// Creates a new card slot modification
    /// </summary>
    /// <param name="modGuid">Unique ID for the mod creating the slot modification</param>
    /// <param name="modificationName">Reference name for the slot modification</param>
    /// <param name="behaviour">The class that controls the behavior for the new slot</param>
    /// <param name="slotTexture">The 3D scene slot texture (154x226)</param>
    /// <returns>Unique identifier for the modification type; used to set the slot modification in the future</returns>
    public static ModificationType New(string modGuid, string modificationName, Type behaviour, Texture2D slotTexture) => New(modGuid, modificationName, behaviour, slotTexture, (Texture2D)null);

    /// <summary>
    /// Creates a new card slot modification
    /// </summary>
    /// <param name="modGuid">Unique ID for the mod creating the slot modification</param>
    /// <param name="modificationName">Reference name for the slot modification</param>
    /// <param name="behaviour">The class that controls the behavior for the new slot</param>
    /// <returns>Unique identifier for the modification type; used to set the slot modification in the future</returns>
    public static ModificationType New(string modGuid, string modificationName, Type behaviour) => New(modGuid, modificationName, behaviour, null, (Texture2D)null);

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
    [HarmonyPostfix]
    private static IEnumerator CleanupSlots(IEnumerator sequence)
    {
        foreach (CardSlot slot in BoardManager.Instance.AllSlots)
        {
            if (slot.GetSlotModification() != ModificationType.NoModification)
                yield return slot.SetSlotModification(ModificationType.NoModification);
        }

        // At this point, all of the receivers should be clear, but regardless, we double check
        if (GlobalTriggerHandler.Instance.StackSize > 0)
            yield return new WaitUntil(() => GlobalTriggerHandler.Instance.StackSize == 0);
        foreach (var kvp in Instance.SlotReceivers)
            GameObject.Destroy(kvp.Value.Item2);
        Instance.SlotReceivers.Clear();

        yield return sequence;
    }

    [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.CleanUp))]
    [HarmonyPostfix]
    private static IEnumerator CleanUpModifiedSlots(IEnumerator sequence)
    {
        foreach (Info defn in AllModificationInfos.Where(m => m.SlotBehaviour != null))
        {
            Component comp = BoardManager.Instance.gameObject.GetComponent(defn.SlotBehaviour);
            if (!comp.SafeIsUnityNull())
                UnityEngine.Object.Destroy(comp);
        }

        yield return sequence;
    }

    internal static readonly Dictionary<CardTemple, Texture> DefaultSlotTextures = new()
    {
        { CardTemple.Nature, ResourceBank.Get<Texture>("Art/Cards/card_slot") },
        { CardTemple.Tech, ResourceBank.Get<Texture>("Art/Cards/card_slot_tech") },
        { CardTemple.Wizard, ResourceBank.Get<Texture>("Art/Cards/card_slot_undead") },
        { CardTemple.Undead, ResourceBank.Get<Texture>("Art/Cards/card_slot_wizard") }
    };

    internal static readonly Dictionary<CardTemple, List<Texture>> PlayerOverrideSlots = new();
    internal static readonly Dictionary<CardTemple, List<Texture>> OpponentOverrideSlots = new();

    private static void ConditionallyResetAllSlotTextures()
    {
        if (BoardManager.m_Instance != null)
        {
            foreach (var slot in BoardManager.Instance.AllSlotsCopy)
            {
                if (slot.GetSlotModification() == ModificationType.NoModification)
                    slot.ResetSlotTexture();
            }
        }
    }

    /// <summary>
    /// Allows you to change the default slot texture for a given 3D scene
    /// </summary>
    /// <param name="temple">Indicator for which scene/scrybe this should apply to</param>
    /// <param name="playerSlot">Texture for each player slot. Null textures will be replaced with the game's default.</param>
    /// <param name="opponentSlot">Texture for each opponent slot. Null textures will be replaced with the game's default.</param>
    /// <remarks>This does not work for Act 2. The base game uses the <c>CardBattleNPC</c> to
    /// set default textures. It's possible that a future API update would allow for custom Act 2 
    /// NPCs, so this will deliberately not touch Act 2</remarks>
    public static void OverrideDefaultSlotTexture(CardTemple temple, Texture playerSlot, Texture opponentSlot)
    {
        OverrideDefaultSlotTexture(
            temple,
            playerSlot == null ? null : new List<Texture>() { playerSlot, playerSlot, playerSlot, playerSlot },
            opponentSlot == null ? null : new List<Texture>() { opponentSlot, opponentSlot, opponentSlot, opponentSlot }
        );
    }

    /// <summary>
    /// Allows you to change the default slot texture for a given 3D scene
    /// </summary>
    /// <param name="temple">Indicator for which scene/scrybe this should apply to</param>
    /// <param name="playerSlots">Textures for each player slot. Null textures will be replaced with the game's default.</param>
    /// <param name="opponentSlots">Textures for each opponent slot. Null textures will be replaced with the game's default.</param>
    /// <remarks>This does not work for Act 2. The base game uses the <c>CardBattleNPC</c> to
    /// set default textures. It's possible that a future API update would allow for custom Act 2 
    /// NPCs, so this will deliberately not touch Act 2</remarks>
    public static void OverrideDefaultSlotTexture(CardTemple temple, List<Texture> playerSlots, List<Texture> opponentSlots)
    {
        if (playerSlots != null && playerSlots.Any(t => t != null))
            PlayerOverrideSlots[temple] = new(playerSlots);

        if (opponentSlots != null && opponentSlots.Any(t => t != null))
            OpponentOverrideSlots[temple] = new(opponentSlots);

        ConditionallyResetAllSlotTextures();
    }

    /// <summary>
    /// Resets the default slot texture back to the base game's default texture.
    /// </summary>
    /// <param name="temple">The scene/scrybe to reset</param>
    public static void ResetDefaultSlotTexture(CardTemple temple)
    {
        if (PlayerOverrideSlots.ContainsKey(temple))
            PlayerOverrideSlots.Remove(temple);
        if (OpponentOverrideSlots.ContainsKey(temple))
            OpponentOverrideSlots.Remove(temple);
        ConditionallyResetAllSlotTextures();
    }
}