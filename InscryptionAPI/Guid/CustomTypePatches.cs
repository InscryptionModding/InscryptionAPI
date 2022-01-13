using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using System.Runtime.CompilerServices;

namespace InscryptionAPI.Guid;

[HarmonyPatch]
public static class TypeManager
{
    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(CustomType), nameof(CustomType.GetType), new Type[] { typeof(string), typeof(string) } )]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Type OriginalGetType(string nameSpace, string typeName) { throw new NotImplementedException(); }

    [HarmonyPatch(typeof(CustomType), nameof(CustomType.GetType), new Type[] { typeof(string), typeof(string) } )]
    [HarmonyPrefix]
    public static bool GetCustomType(string nameSpace, string typeName, ref Type __result)
    {
        InscryptionAPIPlugin.Logger.LogInfo($"Trying to get type for {nameSpace}.{typeName}");
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
                    InscryptionAPIPlugin.Logger.LogInfo($"I found behavior {__result.Name}");
                    return false;
                }
            }

            if (enumType == typeof(SpecialStatIcon))
            {
                StatIconManager.FullStatIcon staticon = StatIconManager.AllStatIcons.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (staticon != null)
                {
                    __result = staticon.VariableStatBehavior;
                    return false;
                }
            }

            if (enumType == typeof(SpecialTriggeredAbility))
            {
                SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility ability = SpecialTriggeredAbilityManager.AllSpecialTriggers.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (ability != null)
                {
                    __result = ability.AbilityBehaviour;
                    return false;
                }
            }

            if (enumType == typeof(CardAppearanceBehaviour.Appearance))
            {
                CardAppearanceBehaviourManager.FullCardAppearanceBehaviour ability = CardAppearanceBehaviourManager.AllAppearances.FirstOrDefault(fab => (int)fab.Id == enumValue);
                if (ability != null)
                {
                    __result = ability.AppearanceBehaviour;
                    return false;
                }
            }
        }
        
        __result = AccessTools.TypeByName($"{nameSpace}.{typeName}");
        return false;
    }
}