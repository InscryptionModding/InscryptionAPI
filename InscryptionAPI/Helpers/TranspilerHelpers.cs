using HarmonyLib;

namespace InscryptionAPI.Helpers;

/// <summary>
/// A Helper Set related to the Transpiler.
/// </summary>
public static class TranspilerHelpers
{
    /// <summary>
    /// A function used to log OpCode Intructions.
    /// </summary>
    /// <param name="codes">The List of OpCodes to Log.</param>
    /// <param name="prefix">An Optional Prefix to help indicate where the Logs Originate, or what the OpCodes relate to.</param>
    public static void LogCodeInstructions(this List<CodeInstruction> codes, string prefix = null)
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
