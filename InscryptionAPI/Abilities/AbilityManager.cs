using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace InscryptionAPI.Abilities;

[HarmonyPatch]
public static class AbilityManager
{
    public class FullAbility
    {
        public Ability Id { get; internal set; }
        public AbilityInfo Info { get; internal set; }
        public Type AbilityBehavior { get; internal set; }
        public Texture Texture { get; internal set; }
    }

    public readonly static ReadOnlyCollection<FullAbility> BaseGameAbilities = new(GenBaseGameAbilityList());
    private readonly static ObservableCollection<FullAbility> NewAbilities = new();
    
    public static List<FullAbility> AllAbilities { get; private set; }
    public static List<AbilityInfo> AllAbilityInfos { get; private set; }

    private static int _abilityCounter = (int)Ability.NUM_ABILITIES;

    static AbilityManager()
    {
        NewAbilities.CollectionChanged += static (_, _) =>
        {
            AllAbilities = BaseGameAbilities.Concat(NewAbilities).ToList();
            AllAbilityInfos = AllAbilities.Select(x => x.Info).ToList();
        };
    }

    private static List<FullAbility> GenBaseGameAbilityList()
    {
        List<FullAbility> baseGame = new();
        var gameAsm = typeof(AbilityInfo).Assembly;
        foreach (var ability in Resources.LoadAll<AbilityInfo>("Data/Abilities"))
        {
            var name = ability.ability.ToString();
            baseGame.Add(new FullAbility
            {
                Id = ability.ability,
                Info = ability,
                AbilityBehavior = gameAsm.GetType($"DiskCardGame.{name}"),
                Texture = AbilitiesUtil.LoadAbilityIcon(name)
            });
        }
        return baseGame;
    }

    public static FullAbility Add(AbilityInfo info, Type behavior, Texture tex)
    {
        FullAbility full = new()
        {
            Info = info,
            AbilityBehavior = behavior,
            Texture = tex,
            Id = (Ability)Interlocked.Increment(ref _abilityCounter)
        };
        full.Info.ability = full.Id;
        NewAbilities.Add(full);
        return full;
    }

    public static void Remove(Ability id) => NewAbilities.Remove(NewAbilities.FirstOrDefault(x => x.Id == id));
    public static void Remove(FullAbility ability) => NewAbilities.Remove(ability);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScriptableObjectLoader<UnityObject>), nameof(ScriptableObjectLoader<UnityObject>.LoadData))]
    [SuppressMessage("Member Access", "Publicizer001", Justification = "Need to set internal list of abilities")]
    private static void AbilityLoadPrefix()
    {
        ScriptableObjectLoader<AbilityInfo>.allData = AllAbilityInfos;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilitiesUtil), nameof(AbilitiesUtil.LoadAbilityIcon))]
    private static bool LoadAbilityIconReplacement(string abilityName, ref Texture __result)
    {
        if (int.TryParse(abilityName, out int abilityId))
        {
            __result = AllAbilities.FirstOrDefault(x => x.Id == (Ability)abilityId).Texture;
        }
        else
        {
            __result = AllAbilities.FirstOrDefault(x => x.Info.name == abilityName).Texture;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilitiesUtil), nameof(AbilitiesUtil.GetLearnedAbilities))]
    private static bool GetLearnedAbilitesReplacement(bool opponentUsable, int minPower, int maxPower, AbilityMetaCategory categoryCriteria, ref List<Ability> __result)
    {
        __result = new();

        foreach (var ability in AllAbilityInfos)
        {
            bool canUse = true;
            canUse &= !opponentUsable || ability.opponentUsable;
            canUse &= minPower <= ability.powerLevel && maxPower >= ability.powerLevel;
            canUse &= ability.metaCategories.Contains(categoryCriteria);
            canUse &= ProgressionData.LearnedAbility(ability.ability);

            if (canUse)
            {
                __result.Add(ability.ability);
            }
        }
        
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.AddAbility), new[] { typeof(Ability) })]
    private static bool AddAbilityReplacement(CardTriggerHandler __instance, Ability ability)
    {
        var full = AllAbilities.FirstOrDefault(x => x.Id == ability);
        if (!__instance.triggeredAbilities.Exists(x => x.Item1 == ability) || full.Info.canStack && !full.Info.passive)
        {
            var reciever = (AbilityBehaviour)__instance.gameObject.GetComponent(full.AbilityBehavior);
            if (!reciever)
            {
                reciever = (AbilityBehaviour)__instance.gameObject.AddComponent(full.AbilityBehavior);
            }
            __instance.triggeredAbilities.Add(new Tuple<Ability, AbilityBehaviour>(ability, reciever));
        }

        return false;
    }
}
