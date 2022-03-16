using System.Collections.ObjectModel;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class StatIconManager
{
    public class FullStatIcon
    {
        public readonly SpecialStatIcon Id;
        public readonly StatIconInfo Info;
        public readonly Type VariableStatBehavior;

        public FullStatIcon(SpecialStatIcon id, StatIconInfo info, Type variableStatBehavior)
        {
            Id = id;
            Info = info;
            VariableStatBehavior = variableStatBehavior;

            TypeManager.Add(id.ToString(), variableStatBehavior);
        }
    }

    public readonly static ReadOnlyCollection<FullStatIcon> BaseGameStatIcons = new(GenBaseGameStatIconList());
    private readonly static ObservableCollection<FullStatIcon> NewStatIcons = new();

    public static List<FullStatIcon> AllStatIcons { get; private set; } = BaseGameStatIcons.ToList();
    public static List<StatIconInfo> AllStatIconInfos { get; private set; } = BaseGameStatIcons.Select(x => x.Info).ToList();

    static StatIconManager()
    {
        NewStatIcons.CollectionChanged += static (_, _) =>
        {
            AllStatIcons = BaseGameStatIcons.Concat(NewStatIcons).ToList();
            AllStatIconInfos = AllStatIcons.Select(x => x.Info).ToList();
        };
    }

    private static List<FullStatIcon> GenBaseGameStatIconList()
    {
        List<FullStatIcon> baseGame = new();
        var gameAsm = typeof(AbilityInfo).Assembly;
        foreach (var iconInfo in Resources.LoadAll<StatIconInfo>("Data/staticons"))
        {
            var name = iconInfo.iconType.ToString();
            baseGame.Add(new FullStatIcon(iconInfo.iconType, iconInfo, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }

    public static FullStatIcon Add(string guid, StatIconInfo info, Type behavior)
    {
        FullStatIcon full = new(GuidManager.GetEnumValue<SpecialStatIcon>(guid, info.rulebookName), info, behavior);
        full.Info.iconType = full.Id;
        NewStatIcons.Add(full);
        return full;
    }

    public static StatIconInfo New(string guid, string rulebookName, string rulebookDescription, Type behavior)
    {
        StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();
        info.rulebookDescription = rulebookDescription;
        info.rulebookName = rulebookName;

        return Add(guid, info, behavior).Info;
    }

    public static void Remove(SpecialStatIcon id) => NewStatIcons.Remove(NewStatIcons.FirstOrDefault(x => x.Id == id));
    public static void Remove(FullStatIcon ability) => NewStatIcons.Remove(ability);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StatIconInfo), nameof(StatIconInfo.LoadAbilityData))]
    private static void AbilityLoadPrefix()
    {
        StatIconInfo.allIconInfo = AllStatIconInfos;
    }

    [HarmonyPatch(typeof(RuleBookInfo), nameof(RuleBookInfo.ConstructPageData), typeof(AbilityMetaCategory))]
    [HarmonyPostfix]
    public static void FixRulebook(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
    {
        if (NewStatIcons.Count > 0)
        {
            foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
            {
                // regular abilities
                if (pageRangeInfo.type == PageRangeType.StatIcons)
                {
                    int insertPosition = __result.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
                    int curPageNum = (int)SpecialStatIcon.NUM_ICONS;
                    foreach (FullStatIcon fab in NewStatIcons.Where(x => __instance.StatIconShouldBeAdded((int)x.Id, metaCategory)))
                    {
                        RuleBookPageInfo info = new()
                        {
                            pagePrefab = pageRangeInfo.rangePrefab,
                            headerText = string.Format(Localization.Translate("APPENDIX XII, SUBSECTION VII - VARIABLE STATS {0}"), curPageNum)
                        };
                        __instance.FillAbilityPage(info, pageRangeInfo, (int)fab.Id);
                        __result.Insert(insertPosition, info);
                        curPageNum += 1;
                        insertPosition += 1;
                    }
                }
            }
        }
    }
}
