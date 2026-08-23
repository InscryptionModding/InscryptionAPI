using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// Patches the Steal Trap Death system to drop a differed pelt on the case the opposing card has the "SteelTrapPelt" extended property.
/// </summary>
/// <remarks>This code is provided by Snowi, and SpecialAPI.</remarks>
[HarmonyPatch]
internal static class SteelTrapDeathPatch
{
    /// <summary>
    /// A Transpiler that patches in two functions into the SteelTrap.OnDie function.
    /// </summary>
    /// <param name="ctx">The ILContext at the time its being called.</param>
    /// <remarks>This code is provided by Snowi, and SpecialAPI.</remarks>
    [HarmonyPatch(typeof(SteelTrap), nameof(SteelTrap.OnDie), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CustomSteelTrapPelts_Transpiler(ILContext ctx)
    {
        var crs = new ILCursor(ctx);
 
        if (!crs.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<AbilityBehaviour>(nameof(AbilityBehaviour.PreSuccessfulTriggerSequence))))
            return;
 
                                   // curr: IEnumerator
        crs.Emit(OpCodes.Ldloc_1); // SteelTrap
        crs.Emit(OpCodes.Call, AccessTools.Method(typeof(SteelTrapDeathPatch), nameof(CustomSteelTrapPelts_SetDyingCard))); // ret: void
 
        if (!crs.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<DrawCreatedCard>(nameof(DrawCreatedCard.CreateDrawnCard))))
            return;
 
                                   // curr: IEnumerator
        crs.Emit(OpCodes.Ldloc_1); // SteelTrap
        crs.Emit(OpCodes.Call, AccessTools.Method(typeof(SteelTrapDeathPatch), nameof(CustomSteelTrapPelts_CustomDrawCard))); // ret: IEnumerator
    }
 
    /// <summary>
    /// A function which sets the dying card.
    /// </summary>
    /// <param name="trap">The SteelTrap ability.</param>
    /// <remarks>This code is provided by Snowi, and SpecialAPI.</remarks>
    private static void CustomSteelTrapPelts_SetDyingCard(SteelTrap trap)
    {
        var holder = trap.GetComponent<DyingCardHolder>();
        if (holder == null)
            holder = trap.gameObject.AddComponent<DyingCardHolder>();
 
        holder.dyingCard = trap?.Card?.Slot?.opposingSlot?.Card?.Info;
    }
 
    /// <summary>
    /// The functionality that allows you to get a differed card when a Trap dies, and the current card is the one that perished via "SteelTrapPelt".
    /// </summary>
    /// <param name="orig">The IEnumerator origin.</param>
    /// <param name="trap">The Steel Trap Ability</param>
    /// <returns>Either the default Wolf Pelt or a custom card if the card that died had a value set for "SteelTrapPelt".</returns>
    /// <remarks>This code is provided by Snowi, and SpecialAPI.</remarks>
    private static IEnumerator CustomSteelTrapPelts_CustomDrawCard(IEnumerator orig, SteelTrap trap)
    {
        var holder = trap.GetComponent<DyingCardHolder>();
        if (holder == null || holder.dyingCard == null)
        {
            yield return orig;
            yield break;
        }
 
        var peltName = holder.dyingCard.GetSteelTrapPelt();
        if (string.IsNullOrEmpty(peltName))
        {
            yield return orig;
            yield break;
        }
 
        yield return new WaitForSeconds(0.5f);
        if (Singleton<ViewManager>.Instance.CurrentView != View.Default)
        {
            yield return new WaitForSeconds(0.2f);
            Singleton<ViewManager>.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.2f);
        }
        
        var cardToDraw = CardLoader.GetCardByName(peltName);
        yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(cardToDraw, null);
        yield return new WaitForSeconds(0.45f);
    }
 
    /// <summary>
    /// The Holder of the dyingCard.
    /// </summary>
    /// <remarks>This code is provided by Snowi, and SpecialAPI.</remarks>
    private class DyingCardHolder : MonoBehaviour
    {
        public CardInfo dyingCard;
    }
}