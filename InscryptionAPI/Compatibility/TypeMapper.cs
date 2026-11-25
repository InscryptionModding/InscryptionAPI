using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System.Reflection;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Obsolete("Unncessary", true)]
[AttributeUsage(AttributeTargets.Field)]
public class IgnoreMappingAttribute : Attribute { }

[Obsolete("Unnecessary", true)]
public static class TypeMapper<S, D> where S : class where D : class {
    private static Dictionary<string, MethodInfo> _accessors = null;
    private static Dictionary<string, MethodInfo> FieldAccessors {
        get {
            if (_accessors is null) {
                _accessors = new();

                foreach (var _field in AccessTools.GetDeclaredFields(typeof(S)).Where(x => !x.GetCustomAttributes(typeof(IgnoreMappingAttribute), false).Any()))
                {
                    var accessor = new DynamicMethodDefinition("get_" + _field.Name, typeof(object), new Type[] { typeof(S) });
                    var il = accessor.GetILProcessor();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, accessor.Module.ImportReference(_field));
                    if (_field.FieldType.IsValueType)
                        il.Emit(OpCodes.Box, _field.FieldType);
                    il.Emit(OpCodes.Ret);
                    _accessors.Add(_field.Name, accessor.Generate());
                }
            }
            return _accessors;
        }
    }

    private static Dictionary<string, MethodInfo> _setters = null;
    private static Dictionary<string, MethodInfo> FieldSetters {
        get {
            if (_setters == null) {
                _setters = new();

                foreach (var _field in AccessTools.GetDeclaredFields(typeof(D)))
                {
                    var setter = new DynamicMethodDefinition("set_" + _field.Name, typeof(void), new Type[] { typeof(D), typeof(object) });
                    var il = setter.GetILProcessor();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Unbox_Any, setter.Module.ImportReference(_field.FieldType));
                    il.Emit(OpCodes.Stfld, setter.Module.ImportReference(_field));
                    il.Emit(OpCodes.Ret);
                    _setters.Add(_field.Name, setter.Generate());
                }
            }
            return _setters;
        }
    }

    public static D Convert(S source, D destination)
    {
        foreach (var _field in FieldAccessors)
        {
            object val = _field.Value.Invoke(null, new object[] { source });
            if (val is not null && FieldSetters.ContainsKey(_field.Key))
            {
                FieldSetters[_field.Key].Invoke(null, new object[] { destination, val });
            }
        }

        return destination;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member