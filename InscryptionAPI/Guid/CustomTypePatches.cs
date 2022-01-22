using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using System.Runtime.CompilerServices;

namespace InscryptionAPI.Guid;

[HarmonyPatch]
public static class TypeManager
{
    private static Dictionary<string, Type> TypeCache = new();

    internal static void Add(string key, Type value)
    {
        if (TypeCache.ContainsKey(key) && TypeCache[key] == value)
            return;

        TypeCache.Add(key, value);
    }


    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(CustomType), nameof(CustomType.GetType), new Type[] { typeof(string), typeof(string) } )]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Type OriginalGetType(string nameSpace, string typeName) { throw new NotImplementedException(); }

    [HarmonyPatch(typeof(CustomType), nameof(CustomType.GetType), new Type[] { typeof(string), typeof(string) } )]
    [HarmonyPrefix]
    public static bool GetCustomType(string nameSpace, string typeName, ref Type __result)
    {
        InscryptionAPIPlugin.Logger.LogInfo($"Trying to get type for {nameSpace}.{typeName}");

        if (TypeCache.ContainsKey(typeName))
        {
            __result = TypeCache[typeName];
            return false;
        }

        int enumValue;
        if (int.TryParse(typeName, out enumValue))
        {
            InscryptionAPIPlugin.Logger.LogInfo($"This appears to be a custom type");
            Type enumType = GuidManager.GetEnumType(enumValue);

            if (enumType == typeof(Ability))
            {
                AbilityManager.FullAbility ability = AbilityManager.AllAbilities.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (ability != null)
                {
                    __result = ability.AbilityBehavior;
                    TypeCache.Add(typeName, __result);
                    return false;
                }
            }

            if (enumType == typeof(SpecialStatIcon))
            {
                StatIconManager.FullStatIcon staticon = StatIconManager.AllStatIcons.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (staticon != null)
                {
                    __result = staticon.VariableStatBehavior;
                    TypeCache.Add(typeName, __result);
                    return false;
                }
            }

            if (enumType == typeof(SpecialTriggeredAbility))
            {
                SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility ability = SpecialTriggeredAbilityManager.AllSpecialTriggers.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (ability != null)
                {
                    __result = ability.AbilityBehaviour;
                    TypeCache.Add(typeName, __result);
                    return false;
                }
            }

            if (enumType == typeof(CardAppearanceBehaviour.Appearance))
            {
                CardAppearanceBehaviourManager.FullCardAppearanceBehaviour ability = CardAppearanceBehaviourManager.AllAppearances.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (ability != null)
                {
                    __result = ability.AppearanceBehaviour;
                    TypeCache.Add(typeName, __result);
                    return false;
                }
            }
        }
        
        __result = AccessTools.TypeByName($"{nameSpace}.{typeName}");
        TypeCache.Add(typeName, __result);
        return false;
    }
}