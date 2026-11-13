using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.RuleBook;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// Manager class for handling stat icons.
/// </summary>
[HarmonyPatch]
public static class StatIconManager
{
    private static readonly Dictionary<SpecialStatIcon, SpecialTriggeredAbility> BASE_GAME_ABILITIES = new()
    {
        { SpecialStatIcon.Ants, SpecialTriggeredAbility.Ant },
        { SpecialStatIcon.Bell, SpecialTriggeredAbility.BellProximity },
        { SpecialStatIcon.Bones, SpecialTriggeredAbility.Lammergeier },
        { SpecialStatIcon.CardsInHand, SpecialTriggeredAbility.CardsInHand },
        { SpecialStatIcon.GreenGems, SpecialTriggeredAbility.GreenMage },
        { SpecialStatIcon.Mirror, SpecialTriggeredAbility.Mirror },
        { SpecialStatIcon.SacrificesThisTurn, SpecialTriggeredAbility.SacrificesThisTurn }
    };

    /// <summary>
    /// A utility class that holds all of the required information about a stat icon in order to be able to use it in-game.
    /// </summary>
    public class FullStatIcon
    {
        /// <summary>
        /// Enum ID for this stat icon.
        /// </summary>
        public readonly SpecialStatIcon Id;
        /// <summary>
        /// Special ability enum ID for this stat icon.
        /// </summary>
        public readonly SpecialTriggeredAbility AbilityId;
        /// <summary>
        /// The description object for this stat icon.
        /// </summary>
        public readonly StatIconInfo Info;
        /// <summary>
        /// A subclass of VariableStatBehaviour that implements the logic for this stat icon.
        /// </summary>
        public readonly Type VariableStatBehavior;

        /// <summary>
        /// Tracks all rulebook redirects that this ability's description will have. Explanation of the variables is as follows:
        /// Key (string): the text that will be recoloured to indicate that it's clickable.
        /// Tuple.Item1 (PageRangeType): the type of page the redirect will go to. Use PageRangeType.Unique if you want to redirect to a custom rulebook page using its pageId.
        /// Tuple.Item2 (Color): the colour the Key text will be recoloured to.
        /// Tuple.Item3 (string): the id that the API will match against to find the redirect page. Eg, for ability redirects this will be the Ability id as a string.
        /// </summary>
        public Dictionary<string, RuleBookManager.RedirectInfo> RulebookDescriptionRedirects = new();

        /// <summary>
        /// Creates a new instance of FullStatIcon and registers its behaviour type with the <see cref="TypeManager"/>.
        /// </summary>
        /// <param name="id">The unique ID for this stat icon.</param>
        /// <param name="abilityId">The unique ID for this stat icon's associated special ability.</param>
        /// <param name="info">The description object for this ability.</param>
        /// <param name="variableStatBehavior">A subclass of VariableStatBehaviour that implements the logic for the ability.</param>
        public FullStatIcon(SpecialStatIcon id, SpecialTriggeredAbility abilityId, StatIconInfo info, Type variableStatBehavior)
        {
            Id = id;
            AbilityId = abilityId;
            Info = info;
            VariableStatBehavior = variableStatBehavior;

            TypeManager.Add(id.ToString(), variableStatBehavior);
        }
    }

    /// <summary>
    /// All of the vanilla game's stat icons.
    /// </summary>
    public readonly static ReadOnlyCollection<FullStatIcon> BaseGameStatIcons = new(GenBaseGameStatIconList());
    private readonly static ObservableCollection<FullStatIcon> NewStatIcons = new();

    /// <summary>
    /// The current processed list of all stat icons in the game, including vanilla and mod-added abilities.
    /// </summary>
    public static List<FullStatIcon> AllStatIcons { get; private set; } = BaseGameStatIcons.ToList();
    /// <summary>
    /// The current processed list of all StatIconInfos in the game, including vanilla and mod-added abilities.
    /// </summary>
    public static List<StatIconInfo> AllStatIconInfos { get; private set; } = BaseGameStatIcons.Select(x => x.Info).ToList();

    static StatIconManager()
    {
        NewStatIcons.CollectionChanged += static (_, _) =>
        {
            AllStatIcons = BaseGameStatIcons.Concat(NewStatIcons).ToList();
            AllStatIconInfos = AllStatIcons.Select(x => x.Info).ToList();
        };

        // Let's help people fix the most common mistake with special stat icons
        CardManager.ModifyCardList += delegate (List<CardInfo> cards)
        {
            foreach (CardInfo card in cards)
            {
                if (card.specialStatIcon != SpecialStatIcon.None)
                {
                    FullStatIcon icon = AllStatIcons.FirstOrDefault(i => i.Id == card.specialStatIcon);
                    if (icon != null && icon.AbilityId != SpecialTriggeredAbility.None)
                    {
                        card.specialAbilities = new(card.specialAbilities);
                        card.AddSpecialAbilities(icon.AbilityId);
                    }
                }
            }
            return cards;
        };
    }

    private static List<FullStatIcon> GenBaseGameStatIconList()
    {
        List<FullStatIcon> baseGame = new();
        Assembly gameAsm = typeof(AbilityInfo).Assembly;
        foreach (var staticon in Resources.LoadAll<StatIconInfo>("Data/staticons"))
        {
            string name = staticon.iconType.ToString();
            SpecialTriggeredAbility ab = BASE_GAME_ABILITIES.ContainsKey(staticon.iconType) ? BASE_GAME_ABILITIES[staticon.iconType] : (SpecialTriggeredAbility)Enum.Parse(typeof(SpecialTriggeredAbility), staticon.iconType.ToString());
            baseGame.Add(new FullStatIcon(staticon.iconType, ab, staticon, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }

    /// <summary>
    /// Creates a new stat icon and registers it to be able to be added to cards
    /// </summary>
    /// <param name="guid">The guid of the mod adding the ability.</param>
    /// <param name="info">An instance of StatIconInfo describing the ability.</param>
    /// <param name="behavior">A subclass of VariableStatBehaviour that implements the logic for the stat icon.</param>
    /// <returns>An instance of FullStatIcon containing all necessary data for the stat icon to work..</returns>
    public static FullStatIcon Add(string guid, StatIconInfo info, Type behavior)
    {
        // Register the special ability at the same time
        SpecialTriggeredAbility abilityId = SpecialTriggeredAbilityManager.Add(guid, info.rulebookName, behavior).Id;

        FullStatIcon full = new(GuidManager.GetEnumValue<SpecialStatIcon>(guid, info.rulebookName), abilityId, info, behavior);
        full.Info.iconType = full.Id;
        info.name = $"{guid}_{info.rulebookName}";
        NewStatIcons.Add(full);
        return full;
    }

    ///     /// <summary>
    /// Creates a new stat icon and registers it to be able to be added to cards
    /// </summary>
    /// <param name="guid">The guid of the mod adding the ability.</param>
    /// <param name="rulebookName">The name of the stat icon when it appears in the Rulebook.</param>
    /// <param name="rulebookDescription">The description of the stat icon when it appears in the Rulebook.</param>
    /// <param name="behavior">A subclass of VariableStatBehaviour that implements the logic for the stat icon.</param>
    /// <returns>An instance of StatIconInfo describing the new ability.</returns>
    public static StatIconInfo New(string guid, string rulebookName, string rulebookDescription, Type behavior)
    {
        StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();
        info.rulebookDescription = rulebookDescription;
        info.rulebookName = rulebookName;

        return Add(guid, info, behavior).Info;
    }

    /// <summary>
    /// Removes a stat icon from the game based on stat icon ID. Can only remove mod-added stat icons, not vanilla stat icons.
    /// </summary>
    /// <param name="id">The unique ID of the stat icon to remove.</param>
    public static void Remove(SpecialStatIcon id) => NewStatIcons.Remove(NewStatIcons.FirstOrDefault(x => x.Id == id));
    /// <summary>
    /// Removes an stat icon from the game based on its FullStatIcon. Can only remove mod-added stat icons, not vanilla stat icons.
    /// </summary>
    /// <param name="ability">The instance of the ability to remove.</param>
    public static void Remove(FullStatIcon ability) => NewStatIcons.Remove(ability);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StatIconInfo), nameof(StatIconInfo.LoadAbilityData))]
    private static void AbilityLoadPrefix()
    {
        StatIconInfo.allIconInfo = AllStatIconInfos;
    }

    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
    [HarmonyPostfix]
    private static void AddNewStatIconsToRuleBook(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
    {
        if (NewStatIcons.Count > 0)
        {
            foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
            {
                if (pageRangeInfo.type == PageRangeType.StatIcons)
                {
                    int insertPosition = __result.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
                    int curPageNum = (int)SpecialStatIcon.NUM_ICONS;
                    foreach (FullStatIcon fab in NewStatIcons.Where(x => __instance.StatIconShouldBeAdded((int)x.Id, metaCategory)))
                    {
                        RuleBookPageInfo info = new();
                        info.pagePrefab = pageRangeInfo.rangePrefab;
                        info.headerText = string.Format(Localization.Translate("APPENDIX XII, SUBSECTION VII - VARIABLE STATS {0}"), curPageNum);
                        __instance.FillStatIconPage(info, pageRangeInfo, (int)fab.Id);
                        __result.Insert(insertPosition, info);
                        curPageNum += 1;
                        insertPosition += 1;
                    }
                }
            }
        }
    }
}
