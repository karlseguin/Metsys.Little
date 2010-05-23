using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metsys.Little.Tests
{
   public abstract class BaseFixture
   {
      protected BaseFixture()
      {
         typeof(LittleConfiguration).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
         typeof(TypeHelper).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, LittleConfiguration.Instance);
         typeof(Serializer).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, LittleConfiguration.Instance);
         typeof(Deserializer).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, LittleConfiguration.Instance);
         typeof(TypeHelper).GetField("_cachedTypeLookup", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new Dictionary<Type, TypeHelper>());
      }
   }
}