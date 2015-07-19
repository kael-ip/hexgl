using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace HexTex.OpenGL {

    public interface IImplProvider {
        object Invoke(Type dtype, object[] args);
    }

    class CodeGenerator {
        AssemblyBuilder assembly;
        ModuleBuilder module;
        public CodeGenerator(string asmName, string modName)
            : this(AppDomain.CurrentDomain, new AssemblyName(asmName), modName) {
        }
        public CodeGenerator(AppDomain domain, AssemblyName asmName, string modName) {
            this.assembly = domain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            this.module = assembly.DefineDynamicModule(modName);
        }
        static void CreateStandardDelegate(TypeBuilder tdelegate, Type returnType, Type[] parameterTypes) {
            var argsBeginInvoke = new Type[parameterTypes.Length + 2];
            Array.Copy(parameterTypes, argsBeginInvoke, parameterTypes.Length);
            argsBeginInvoke[parameterTypes.Length] = typeof(AsyncCallback);
            argsBeginInvoke[parameterTypes.Length + 1] = typeof(object);
            MethodBuilder methodBeginInvoke = tdelegate.DefineMethod("BeginInvoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                typeof(IAsyncResult), argsBeginInvoke);
            methodBeginInvoke.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            MethodBuilder methodEndInvoke = tdelegate.DefineMethod("EndInvoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                returnType, new Type[] { typeof(IAsyncResult) });
            methodEndInvoke.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            MethodBuilder methodInvoke = tdelegate.DefineMethod("Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                CallingConventions.Standard, returnType, parameterTypes);
            methodInvoke.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            ConstructorBuilder constructorBuilder = tdelegate.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { typeof(object), typeof(System.IntPtr) });
            constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
        }
        public Type DefineDelegate(string name, Type returnType, Type[] parameterTypes) {
            var tdelegate = module.DefineType(name,
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.Public,
                typeof(System.MulticastDelegate));
            CreateStandardDelegate(tdelegate, returnType, parameterTypes);
            return tdelegate.CreateType();
        }
        public static Type[] GetMethodParameterTypes(MethodInfo mi) {
            //return mi.GetParameters().Select(p => p.ParameterType).ToArray();
            var parameters = mi.GetParameters();
            var parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                parameterTypes[i] = parameters[i].ParameterType;
            }
            return parameterTypes;
        }
        public Dictionary<MethodInfo, Type> DefineDelegates(Type type) {
            var dict = new Dictionary<MethodInfo, Type>();
            foreach (var mi in type.GetMethods()) {
                var dtype = DefineDelegate(mi.Name, mi.ReturnType, GetMethodParameterTypes(mi));
                dict.Add(mi, dtype);
            }
            return dict;
        }
        public Type CreateImplementor(string name, Type interfaceType, Dictionary<MethodInfo, Type> delegates, Type providerType) {
            var methodInvoke = providerType.GetMethod("Invoke");
            TypeBuilder typeBuilder = module.DefineType(name, TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit,
                typeof(object), new Type[] { interfaceType });
            var fProvider = typeBuilder.DefineField("provider", providerType, FieldAttributes.Private);
            {
                var constructor = typeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard, new Type[] { providerType });
                ILGenerator generator = constructor.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[0]));
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, fProvider);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ret);
            }
            foreach (var mi in delegates.Keys) {
                var dtype = delegates[mi];
                var parameterTypes = GetMethodParameterTypes(mi);
                var method = typeBuilder.DefineMethod(mi.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    mi.ReturnType, parameterTypes);
                ILGenerator generator = method.GetILGenerator();
                var local0 = generator.DeclareLocal(typeof(object[]));
                if (mi.ReturnType != null && !typeof(void).IsAssignableFrom(mi.ReturnType)) {
                    var local1 = generator.DeclareLocal(mi.ReturnType);
                }
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fProvider);
                generator.Emit(OpCodes.Ldtoken, dtype);
                generator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                if (parameterTypes.Length < 256) {
                    generator.Emit(OpCodes.Ldc_I4_S, (byte)parameterTypes.Length);
                } else {
                    generator.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
                }
                generator.Emit(OpCodes.Newarr, typeof(object));
                generator.Emit(OpCodes.Stloc_0);
                for (int i = 0; i < parameterTypes.Length; i++) {
                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    //generator.Emit(OpCodes.Ldarg_1);//i+1
                    if (i < 255) {
                        generator.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                    } else {
                        generator.Emit(OpCodes.Ldarg, i + 1);
                    }
                    if (parameterTypes[i].IsValueType) {
                        generator.Emit(OpCodes.Box, parameterTypes[i]);
                    }
                    generator.Emit(OpCodes.Stelem_Ref);
                }
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Callvirt, methodInvoke);
                if (mi.ReturnType != null && !typeof(void).IsAssignableFrom(mi.ReturnType)) {
                    if (mi.ReturnType.IsValueType) {
                        generator.Emit(OpCodes.Unbox_Any, mi.ReturnType);
                    }
                    generator.Emit(OpCodes.Stloc_1);
                    generator.Emit(OpCodes.Nop);
                    generator.Emit(OpCodes.Ldloc_1);
                } else {
                    generator.Emit(OpCodes.Pop);
                }
                generator.Emit(OpCodes.Ret);

            }
            return typeBuilder.CreateType();
        }
    }

}
