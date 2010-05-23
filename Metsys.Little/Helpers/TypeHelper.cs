using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metsys.Little
{
   internal class TypeHelper
   {
      private static readonly IDictionary<Type, TypeHelper> _cachedTypeLookup = new Dictionary<Type, TypeHelper>();
      private static readonly LittleConfiguration _configuration = LittleConfiguration.Instance;

      private readonly IList<MagicProperty> _properties;

      public IEnumerable<MagicProperty> Properties
      {
         get{ return _properties;}
      }

      private TypeHelper(Type type)
      {
         var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
         _properties = LoadMagicProperties(type, properties);
      }

      public static TypeHelper GetHelperForType(Type type)
      {
         TypeHelper helper;
         if (!_cachedTypeLookup.TryGetValue(type, out helper))
         {
            helper = new TypeHelper(type);
            _cachedTypeLookup[type] = helper;
         }
         return helper;
      }

      private static IList<MagicProperty> LoadMagicProperties(Type type, ICollection<PropertyInfo> properties)
      {
         var magic = new List<MagicProperty>(properties.Count);
         foreach (var property in properties)
         {
            if (property.GetIndexParameters().Length > 0) { continue; }
            if (_configuration.IsIgnored(type, property.Name)) { continue; }
            magic.Add(new MagicProperty(property));
         }
         magic.Sort((a, b) => string.Compare(a.Name, b.Name));
         return magic;
      }
   }
}
