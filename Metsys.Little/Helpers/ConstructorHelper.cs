using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Metsys.Little
{
    //taken from:
    //http://blogs.msdn.com/zelmalki/archive/2008/12/12/reflection-fast-object-creation.aspx
    internal static class ConstructorHelper
    {
        private static CreateDelegateHandler _createDelegate;

        static ConstructorHelper()
        {
            var methodInfo = typeof(Delegate).GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(object), typeof(RuntimeMethodHandle) }, null);
            _createDelegate = Delegate.CreateDelegate(typeof(CreateDelegateHandler), methodInfo) as CreateDelegateHandler;
        }
        public static Delegate CreateDelegate(this ConstructorInfo constructor, Type delegateType)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }
            if (delegateType == null)
            {
                throw new ArgumentNullException("delegateType");
            }

            // Validate the delegate return type
            var delMethod = delegateType.GetMethod("Invoke");

            // Validate the signatures
            var delParams = delMethod.GetParameters();
            var constructorParam = constructor.GetParameters();
            if (delParams.Length != constructorParam.Length)
            {
                throw new InvalidOperationException("The delegate signature does not match that of the constructor");
            }
            for (var i = 0; i < constructorParam.Length; i++)
            {
                if (delParams[i].ParameterType != constructorParam[i].ParameterType || // Probably other things we should check ??
                    delParams[i].IsOut)
                {
                    throw new InvalidOperationException("The delegate signature does not match that of the constructor");
                }
            }

            // Create the dynamic method
            var method = new DynamicMethod(
                    string.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")),
                    constructor.DeclaringType,
                    Array.ConvertAll(constructorParam, p => p.ParameterType),
                    true
                    );

            // Create the il
            var gen = method.GetILGenerator();
            for (var i = 0; i < constructorParam.Length; i++)
            {
                if (i < 4)
                {
                    switch (i)
                    {
                        case 0:
                            gen.Emit(OpCodes.Ldarg_0);
                            break;
                        case 1:
                            gen.Emit(OpCodes.Ldarg_1);
                            break;
                        case 2:
                            gen.Emit(OpCodes.Ldarg_2);
                            break;
                        case 3:
                            gen.Emit(OpCodes.Ldarg_3);
                            break;
                    }
                }
                else
                {
                    gen.Emit(OpCodes.Ldarg_S, i); // Only up to 255 args :)
                }
            }
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            // Return the delegate :)
            return method.CreateDelegate(delegateType);
        }
        private delegate Delegate CreateDelegateHandler(Type type, object target, RuntimeMethodHandle handle);
    }
}