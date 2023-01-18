using System.Reflection;
using System.Reflection.Emit;
using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Card.CostProperties;

[HarmonyPatch]
internal static class CostProperties
{
    internal static MethodInfo CardInfoMethodInfo = null;
        
    internal static MethodInfo NewBloodCostMethodInfo = null;
    internal static MethodInfo NewBoneCostMethodInfo = null;
    internal static MethodInfo NewGemCostMethodInfo = null;
    
    internal static MethodInfo OldBloodCost = null;
    internal static FieldInfo OldBoneCost = null;
    internal static MethodInfo OldBoneCost2 = null;
    internal static FieldInfo OldGemCost = null;
    internal static MethodInfo OldGemCost2 = null;
    
    static CostProperties()
    {
        CardInfoMethodInfo = AccessTools.PropertyGetter(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.Info));
        
        DiskCardGame.Card c = null;
        
        NewBloodCostMethodInfo = SymbolExtensions.GetMethodInfo(() => c.BloodCost());
        NewBoneCostMethodInfo = SymbolExtensions.GetMethodInfo(() => c.BoneCost());
        NewGemCostMethodInfo = SymbolExtensions.GetMethodInfo(() => c.GemsCost());
            
        OldBloodCost = AccessTools.PropertyGetter(typeof(CardInfo), nameof(CardInfo.BloodCost));
        OldBoneCost = AccessTools.Field(typeof(CardInfo), nameof(CardInfo.bonesCost));
        OldBoneCost2 = AccessTools.PropertyGetter(typeof(CardInfo), nameof(CardInfo.BonesCost));
        OldGemCost = AccessTools.Field(typeof(CardInfo), nameof(CardInfo.gemsCost));
        OldGemCost2 = AccessTools.PropertyGetter(typeof(CardInfo), nameof(CardInfo.GemsCost));
    }
}

[HarmonyPatch]
internal static class BoardManager_ChooseSacrificesForCard
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        // BoardManager.ChooseSacrificesForCard
        var type = Type.GetType("DiskCardGame.BoardManager+<ChooseSacrificesForCard>d__80, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        yield return AccessTools.Method(type, "MoveNext");
        
        // BoardManager.SacrificesCreateRoomForCard
        yield return AccessTools.Method(typeof(BoardManager), "SacrificesCreateRoomForCard", new Type[]{typeof(PlayableCard), typeof(List<CardSlot>)});
    }
    
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this
        
        // card.Info.BloodCost
        
        // === Into this
        
        // card.BloodCost()
        
        // ===
        
        InscryptionAPIPlugin.Logger.LogInfo("[BoardManager_ChooseSacrificesForCard]");

        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count - 2; i++)
        {
            OpCode currentOpCode = codes[i].opcode;
            object currentOperand = codes[i].operand;

            CodeInstruction next = codes[i+1];
            OpCode nextOpCode = next.opcode;
            object nextOperand = next.operand;
            
            if (currentOpCode == OpCodes.Callvirt && currentOperand == CostProperties.CardInfoMethodInfo && 
                nextOpCode == OpCodes.Callvirt && nextOperand == CostProperties.OldBloodCost)
            {
                InscryptionAPIPlugin.Logger.LogInfo("[BoardManager_ChooseSacrificesForCard] Replaced " + i);
                codes.RemoveAt(i);
                next.operand = CostProperties.NewBloodCostMethodInfo;
                
                i++;
            }
        }
        
        InscryptionAPIPlugin.Logger.LogInfo("[BoardManager_ChooseSacrificesForCard] Done");

        return codes;
    }
}
