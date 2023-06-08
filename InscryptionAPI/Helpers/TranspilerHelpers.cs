using HarmonyLib;

namespace InscryptionAPI.Helpers;

public static class TranspilerHelpers
{
    public static void LogCodeInscryptions(this List<CodeInstruction> codes, string prefix = null)
    {
        string p = prefix ?? "";
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction code = codes[i];
            string codeOperand = code.operand == null ? "" : code.operand.ToString();
            InscryptionAPIPlugin.Logger.LogInfo($"{p}{i}: {code.opcode} {codeOperand}");
        }
    }
}
