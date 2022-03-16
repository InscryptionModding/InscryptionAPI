using BepInEx;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace APIPatcher;

public static class InscryptionAPIPatcher
{
    public static IEnumerable<string> TargetDLLs => new[] { "Assembly-CSharp.dll" };

    public static void Patch(AssemblyDefinition asm)
    {
        var apiPath = Directory.GetFiles(Paths.PluginPath, "InscryptionAPI.dll", SearchOption.AllDirectories)[0];
        using var pluginAsm = AssemblyDefinition.ReadAssembly(apiPath);

        var eventInvoker = pluginAsm.MainModule.GetType("InscryptionAPI.InscryptionAPIPlugin").Methods.First(x => x.Name == "InvokeSOLEvent");

        var sol = asm.MainModule.GetType("ScriptableObjectLoader`1");
        var allDataGetter = sol.Properties.First(x => x.Name == "AllData").GetMethod;

        ILContext ctx = new(allDataGetter);
        ILCursor c = new(ctx);

        c.Emit(OpCodes.Ldtoken, sol.GenericParameters[0]);
        c.Emit(OpCodes.Call, ctx.Import(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))));
        c.Emit(OpCodes.Call, asm.MainModule.ImportReference(eventInvoker));
    }
}
