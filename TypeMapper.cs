using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace CardLoaderPlugin
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreMappingAttribute : Attribute {}
    public static unsafe class TypeMapper<S, D> where S : class where D : class, new()
    {
        private struct GetFieldDelegate
        {
            public delegate*<S, object> Del;
            public Type FieldType;
        }

        private struct SetFieldDelegate
        {
            public delegate*<D, object, void> Del;
            public Type FieldType;
        }

        private static Dictionary<string, GetFieldDelegate> _accessors = null;
        private static Dictionary<string, GetFieldDelegate> FieldAccessors
        {
            get
            {
                if (_accessors is null)
                {
                    _accessors = new();

                    foreach (var field in AccessTools.GetDeclaredFields(typeof(S)).Where(x => !x.GetCustomAttributes(typeof(IgnoreMappingAttribute), false).Any()))
                    {
                        var accessor = new DynamicMethodDefinition("get_" + field.Name, typeof(object), new Type[] { typeof(S) });
                        var il = accessor.GetILProcessor();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, accessor.Module.ImportReference(field));
                        if (field.FieldType.IsValueType)
                            il.Emit(OpCodes.Box);
                        il.Emit(OpCodes.Ret);
                        _accessors.Add(field.Name, new GetFieldDelegate
                        {
                            Del = (delegate*<S, object>)accessor.Generate().MethodHandle.GetFunctionPointer(),
                            FieldType = field.FieldType
                        });
                    }
                }
                return _accessors;
            }
        }

        private static Dictionary<string, SetFieldDelegate> _setters = null;
        private static Dictionary<string, SetFieldDelegate> FieldSetters
        {
            get
            {
                if (_setters == null)
                {
                    _setters = new();

                    foreach (var field in AccessTools.GetDeclaredFields(typeof(D)))
                    {
                        var fieldType = field.FieldType;
                        var setter = new DynamicMethodDefinition("set_" + field.Name, typeof(void), new Type[] { typeof(D), typeof(object) });
                        var il = setter.GetILProcessor();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        if (FieldAccessors.TryGetValue(field.Name, out var getter) && getter.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>) && getter.FieldType.GetGenericArguments()[0] == field.FieldType)
                        {
                            il.Emit(OpCodes.Call, AccessTools.DeclaredPropertyGetter(fieldType, "Value"));
                            fieldType = getter.FieldType.GetGenericArguments()[0];
                        }
                        else if (fieldType.IsValueType)
                            il.Emit(OpCodes.Unbox, setter.Module.ImportReference(field.FieldType));
                        else
                            il.Emit(OpCodes.Castclass, setter.Module.ImportReference(field.FieldType));
                        il.Emit(OpCodes.Stfld, setter.Module.ImportReference(field));
                        il.Emit(OpCodes.Ret);
                        _setters.Add(field.Name, new SetFieldDelegate
                        {
                            Del = (delegate*<D, object, void>)setter.Generate().MethodHandle.GetFunctionPointer(),
                            FieldType = field.FieldType
                        });
                    }
                }
                return _setters;
            }
        }

        public static D Convert(S source, D destination = null)
        {
            destination ??= new();
            
            foreach (var field in FieldAccessors)
            {
                object val = field.Value.Del(source);
                if (val is not null)
                {
                    FieldSetters[field.Key].Del(destination, val);
                }
            }

            return destination;
        }
    }
}
