﻿using System;
using System.Reflection;

namespace Metsys.Little
{
      internal class MagicProperty
      {
         private readonly PropertyInfo _property;
         private readonly bool _nullable;
         private readonly bool _hasHeader;         
         private readonly Type _type;         

         public Type Type
         {
            get { return _type; }
         }
         public string Name
         {
            get{ return _property.Name;}
         }
         public bool Nullable
         {
            get { return _nullable; }
         }
         public bool HasHeader
         {
            get { return _hasHeader;}
         }
       
         public Action<object, object> Setter { get; private set; }

         public Func<object, object> Getter { get; private set; }

         public MagicProperty(PropertyInfo property)
         {
            var t = property.PropertyType;
            _property = property;
            _nullable = IsNullable(t);
            _hasHeader = _nullable;
            if (t.IsEnum)
            {
               _type = Enum.GetUnderlyingType(t);
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
               _type = System.Nullable.GetUnderlyingType(t);
            }
            else
            {
               _type = t;
            }
            Getter = CreateGetterMethod(property);
            Setter = CreateSetterMethod(property);
         }
         public static bool IsNullable(Type type)
         {
            return !type.IsValueType ||(type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
         }
         private static Action<object, object> CreateSetterMethod(PropertyInfo property)
         {
            var genericHelper = typeof(MagicProperty).GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
         }
         private static Func<object, object> CreateGetterMethod(PropertyInfo property)
         {
            var genericHelper = typeof(MagicProperty).GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)constructedHelper.Invoke(null, new object[] { property });
         }
         private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
         {
            var m = method.GetSetMethod(true);
            if (m == null) { return null; } //no setter
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
            return (target, param) => func((TTarget)target, (TParam)param);
         }
         private static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
         {
            var m = method.GetGetMethod(true);
            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), m);
            return target => func((TTarget)target);
         }
      }
}