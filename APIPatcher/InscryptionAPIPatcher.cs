using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace APIPatcher;

public static class InscryptionAPIPatcher
{
    public static IEnumerable<string> TargetDLLs => new[] { "Assembly-CSharp.dll" };

    public static void Patch(AssemblyDefinition asm)
    {
        AddScriptableObjectLoaderEvent(asm);
        AddSingletonWarning(asm);
    }

    private static void AddScriptableObjectLoaderEvent(AssemblyDefinition asm)
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

    private static void AddSingletonWarning(AssemblyDefinition asm)
    {
        var singletonType = asm.MainModule.GetType("Singleton`1");
        var findInstMethod = singletonType.Methods.First(x => x.Name == "FindInstance");

        ILContext ctx = new(findInstMethod);
        ILCursor c = new(ctx);

        c.GotoNext(MoveType.Before,
            x => x.MatchLdtoken(out _),
            x => x.MatchCall(out _),
            x => x.MatchCall("UnityEngine.Object", "FindObjectOfType")
        );

        c.RemoveRange(3);

        // var @object = InscryptionAPIPatcher.GetFrameLoopInstance<T>();

        // if (Object.FindObjectOfType(typeof(T)) == null) {
        c.Emit(OpCodes.Dup);
        c.Emit(OpCodes.Brtrue_S, c.Next);

        // string errorStr = "Got a null instance in Singleton<T>.FindInstance for type: " + typeof(T).FullName;
        c.Emit(OpCodes.Ldstr, "Got a null instance in Singleton<T>.FindInstance for type: ");
        c.Emit(OpCodes.Ldtoken, singletonType.GenericParameters[0]);
        c.Emit(OpCodes.Call, ctx.Import(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))));
        c.Emit(OpCodes.Callvirt, ctx.Import(typeof(Type).GetProperty(nameof(Type.FullName)).GetGetMethod()));
        c.Emit(OpCodes.Call, ctx.Import(typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) })));
        c.Emit(OpCodes.Dup);

        // APIPatcher.EasyLogWarning(errorStr);
        c.Emit(OpCodes.Call, ctx.Import(typeof(InscryptionAPIPatcher).GetMethod(nameof(EasyLogWarning))));

        // throw new NullReferenceException(errorStr);
        c.Emit(OpCodes.Newobj, ctx.Import(typeof(NullReferenceException).GetConstructor(new Type[] { typeof(string) })));
        c.Emit(OpCodes.Throw);

        // }
    }

    private static readonly ManualLogSource Log = Logger.CreateLogSource("APIPatcher");
    public static void EasyLogWarning(string warn) => Log.LogWarning(warn);
}
