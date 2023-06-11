using BepInEx;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace InscryptionAPI.Guid;

[HarmonyPatch]
public static class TypeManager
{
    private static Dictionary<string, Type> TypeCache = new();

    internal static void Add(string key, Type value)
    {
        if (TypeCache.ContainsKey(key))
        {
            if (TypeCache[key] == value) return;
            TypeManager.Replace(key, value);

            return;
        }

        TypeCache.Add(key, value);
    }

    internal static void Replace(string key, Type value)
    {
        if (TypeCache.ContainsKey(key))
            TypeCache.Remove(key);

        Add(key, value);
    }

    private static Dictionary<string, string> ModIds = new();

    private static string GetModIdFromAssembly(Assembly assembly)
    {
        if (ModIds.ContainsKey(assembly.FullName))
            return ModIds[assembly.FullName];

        foreach (var t in assembly.GetTypes())
        {
            foreach (var d in t.GetCustomAttributes<BepInPlugin>())
            {
                if (d.GUID == InscryptionAPIPlugin.ModGUID)
                    continue;

                ModIds.Add(assembly.FullName, d.GUID);
                return d.GUID;
            }
        }

        ModIds.Add(assembly.FullName, default(string));
        return default(string);
    }

    public static string GetModIdFromCallstack(Assembly callingAssembly)
    {
        string cacheVal = GetModIdFromAssembly(callingAssembly);
        if (!string.IsNullOrEmpty(cacheVal))
            return cacheVal;

        StackTrace trace = new StackTrace();
        foreach (var frame in trace.GetFrames())
        {
            string newVal = GetModIdFromAssembly(frame.GetMethod().DeclaringType.Assembly);
            if (!string.IsNullOrEmpty(newVal))
                return newVal;
        }

        return default(string);
    }


    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(CustomType), nameof(CustomType.GetType), new Type[] { typeof(string), typeof(string) })]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Type OriginalGetType(string nameSpace, string typeName) { throw new NotImplementedException(); }

    [HarmonyPatch(typeof(CustomType), nameof(CustomType.GetType), new Type[] { typeof(string), typeof(string) })]
    [HarmonyPrefix]
    private static bool GetCustomType(string nameSpace, string typeName, ref Type __result)
    {
        //InscryptionAPIPlugin.Logger.LogInfo($"Trying to get type for {nameSpace}.{typeName}");

        if (TypeCache.ContainsKey(typeName))
        {
            __result = TypeCache[typeName];
            return false;
        }

        int enumValue;
        if (int.TryParse(typeName, out enumValue))
        {
            //InscryptionAPIPlugin.Logger.LogInfo($"This appears to be a custom type");
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