using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Metsys.Little.Tests
{
   public class IntegrationTests
   {
      public IntegrationTests()
      {
         typeof(LittleConfiguration).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
         typeof(TypeHelper).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, LittleConfiguration.Instance);
         typeof(TypeHelper).GetField("_cachedTypeLookup", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new Dictionary<Type, TypeHelper>());
      }

      [Fact]
      public void ProcessIsLossless()
      {
         var expected = new ComplexObject
            {
               Enabled = true,
               Id = 433,
               Name = "Goku"
            };
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<ComplexObject>(data);

         Assert.Equal(true, actual.Enabled);
         Assert.Equal(433, actual.Id);
         Assert.Equal("Goku", actual.Name);
         Assert.Equal(null, actual.Initial);
         Assert.Equal(null, actual.Description);
      }

      [Fact]
      public void IgnoredPropertiesArentPreserved()
      {
         LittleConfiguration.ForType<ComplexObject>(c => c.Ignore(u => u.Name).Ignore(u => u.Id));

         var expected = new ComplexObject
            {
               Enabled = false,
               Id = 433,
               Initial = 'z',
               Name = "Goku"
         };
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<ComplexObject>(data);

         Assert.Equal(false, actual.Enabled);
         Assert.Equal(0, actual.Id);
         Assert.Equal(null, actual.Name);
         Assert.Equal('z', actual.Initial);
         Assert.Equal(null, actual.Description);
      }

   }  
}