using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class Act2EvolveNumbers
{
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(Evolve), nameof(Evolve.OnUpkeep));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_I4_5 && codes[i + 1].opcode == OpCodes.Ldstr)
            {
                MethodInfo customMethod = AccessTools.Method(typeof(Act2EvolveNumbers), nameof(Act2EvolveNumbers.AddGBCSupport),
                    new Type[] { typeof(Evolve), typeof(int) });
                i -= 2;
                codes.RemoveRange(i, 9);
                codes.Insert(i, new(OpCodes.Ldloc_3));
                codes.Insert(i + 1, new(OpCodes.Call, customMethod));
                break;
            }
        }

        return codes;
    }

    private static void AddGBCSupport(Evolve instance, int num2)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            Texture evolveOverride = ResourceBank.Get<Texture>("Art/Cards/AbilityIcons/ability_evolve_" + num2);
            instance.Card.RenderInfo.OverrideAbilityIcon(Ability.Evolve, evolveOverride);
            return;
        }

        instance.Card.GetComponentInChildren<PixelCardAbilityIcons>().DisplayAbilities(instance.Card.RenderInfo, instance.Card);
    }
}

[HarmonyPatch]
internal class PixelEvolvePatches
{
    [HarmonyPatch(typeof(PixelCardAbilityIcons), nameof(PixelCardAbilityIcons.DisplayAbilities), new Type[] { typeof(List<Ability>), typeof(PlayableCard) })]
    [HarmonyPrefix]
    private static bool FixEvolveSprite(PixelCardAbilityIcons __instance, List<Ability> abilities, PlayableCard card)
    {
        if (!abilities.Contains(Ability.Evolve) || __instance.abilityIconGroups.Count <= 0)
            return true;

        foreach (GameObject abilityIconGroup in __instance.abilityIconGroups)
            abilityIconGroup.gameObject.SetActive(value: false);

        if (abilities.Count > 0 && abilities.Count - 1 < __instance.abilityIconGroups.Count)
        {
            GameObject obj = __instance.abilityIconGroups[abilities.Count - 1];
            obj.gameObject.SetActive(value: true);
            SpriteRenderer[] componentsInChildren = obj.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                AbilityInfo info = AbilitiesUtil.GetInfo(abilities[i]);
                componentsInChildren[i].sprite = GetPixelEvolveTex(info, card);

                if (info.flipYIfOpponent && card != null && card.OpponentCard)
                {
                    if ((bool)info.customFlippedPixelIcon)
                        componentsInChildren[i].sprite = info.customFlippedPixelIcon;
                    else
                        componentsInChildren[i].flipY = true;
                }
                else
                    componentsInChildren[i].flipY = false;
            }
        }
        __instance.conduitIcon.SetActive(abilities.Exists(x => AbilitiesUtil.GetInfo(x).conduit));
        Ability ability = abilities.Find(x => AbilitiesUtil.GetInfo(x).activated);

        if (ability == 0)
            __instance.activatedAbilityButton.gameObject.SetActive(value: false);
        else
        {
            __instance.activatedAbilityButton.gameObject.SetActive(value: true);
            __instance.activatedAbilityButton.SetAbility(ability);
        }

        return false;
    }
    private static Sprite GetPixelEvolveTex(AbilityInfo info, PlayableCard card)
    {
        if (info.ability != Ability.Evolve)
            return info.pixelIcon;

        int turnsInPlay = card.GetComponentInChildren<Evolve>()?.numTurnsInPlay ?? 0;
        int turnsToEvolve = Mathf.Max(1, (card.Info.evolveParams == null ? 1 : card.Info.evolveParams.turnsToEvolve) - turnsInPlay);

        int pngIndex = turnsToEvolve > 3 ? 0 : turnsToEvolve;

        info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("pixel_evolve_" + pngIndex + ".png", typeof(PixelEvolvePatches).Assembly));

        return info.pixelIcon;
    }
}
