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
      [Fact]
      public void CollectionsAreLossless()
      {
         var expected = new ComplexObject
            {
               Security = new[] {'a', 'z', '4', '*'},
         };
         expected.Roles.Add("abc123");
         expected.Roles.Add("lkasdk");
         expected.Roles.Add(null);
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<ComplexObject>(data);

         Assert.Equal(expected.Security, actual.Security);
         Assert.Equal(expected.Roles, actual.Roles);
      }
      [Fact]
      public void NullCollectionIsPreserved()
      {
         var expected = new ComplexObject();
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<ComplexObject>(data);

         Assert.Equal(null, actual.Security);
         Assert.Equal(0, actual.Roles.Count);
      }
      [Fact]
      public void NestedTypesWithCollectionOfObjects()
      {
         var customer = new Customer(43)
            {
               Address = new Address {StreetName = "Its Over", StreetNumber = 9000},
            };
         customer.Orders.Add(new Order {Ordered = new DateTime(2010, 1, 4, 4, 59, 4), Price = 32.99m});
         customer.Orders.Add(new Order { Ordered = new DateTime(2009, 12, 31), Price = 99 });
         var data = Serializer.Serialize(customer);
         var actual = Deserializer.Deserialize<Customer>(data);

         Assert.Equal(customer.Id, actual.Id);
         Assert.Equal(customer.Address.StreetName, actual.Address.StreetName);
         Assert.Equal(customer.Address.StreetNumber, actual.Address.StreetNumber);
         Assert.Equal(customer.Orders.Count, actual.Orders.Count);
         Assert.Equal(customer.Orders[0].Ordered, actual.Orders[0].Ordered);
         Assert.Equal(customer.Orders[0].Price, actual.Orders[0].Price);
         Assert.Equal(customer.Orders[1].Ordered, actual.Orders[1].Ordered);
         Assert.Equal(customer.Orders[1].Price, actual.Orders[1].Price);
      }
   }  
}