using System;
using System.Reflection;

namespace Metsys.Little
{
      internal class MagicField
      {
         private readonly FieldInfo _property;
         private readonly bool _nullable;
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
       
         public Action<object, object> Setter { get; private set; }

         public Func<object, object> Getter { get; private set; }

         public MagicField(FieldInfo field)
         {
            var t = field.FieldType;
            _property = field;
            _nullable = IsNullable(t);
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
            Getter = CreateGetterMethod(field);
            Setter = CreateSetterMethod(field);
         }
         public static bool IsNullable(Type type)
         {
            return !type.IsValueType ||(type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
         }
         private static Action<object, object> CreateSetterMethod(FieldInfo field)
         {
            return field.SetValue;
         }
         private static Func<object, object> CreateGetterMethod(FieldInfo field)
         {
             return field.GetValue;
         }
      }
}