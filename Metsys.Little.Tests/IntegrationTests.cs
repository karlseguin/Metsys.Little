using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Metsys.Little.Tests
{
   public class IntegrationTests : BaseFixture
   {
      [Fact]
      public void ProcessIsLossless()
      {
         var expected = new ComplexObject
                           {
                              Enabled = true,
                              Id = 433,
                              Name = "Goku"
                           };
         byte[] data = Serializer.Serialize(expected);
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
         byte[] data = Serializer.Serialize(expected);
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
         byte[] data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<ComplexObject>(data);

         Assert.Equal(expected.Security, actual.Security);
         Assert.Equal(expected.Roles, actual.Roles);
      }
      [Fact]
      public void NullCollectionIsPreserved()
      {
         var expected = new ComplexObject();
         byte[] data = Serializer.Serialize(expected);
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
         customer.Orders.Add(new Order {Ordered = new DateTime(2009, 12, 31), Price = 99});
         byte[] data = Serializer.Serialize(customer);
         var actual = Deserializer.Deserialize<Customer>(data);

         Assert.Equal(customer.Id, actual.Id);
         Assert.Equal(customer.Address.StreetName, actual.Address.StreetName);
         Assert.Equal(customer.Address.StreetNumber, actual.Address.StreetNumber);
         Assert.Equal(customer.Orders.Count, actual.Orders.Count);
         Assert.Equal(customer.Orders[0].Ordered.ToString(), actual.Orders[0].Ordered.ToString());
         Assert.Equal(customer.Orders[0].Price, actual.Orders[0].Price);
         Assert.Equal(customer.Orders[1].Ordered.ToString(), actual.Orders[1].Ordered.ToString());
         Assert.Equal(customer.Orders[1].Price, actual.Orders[1].Price);
      }
      [Fact]
      public void CanReadWriteMultipleToSingleStream()
      {
         var customers = new[]
                            {
                               new Customer(10) {Address = new Address {StreetName = "Abc St.", StreetNumber = 123},},
                               new Customer(20) {Address = new Address {StreetName = "Def St.", StreetNumber = 456},},
                               new Customer(30) {Address = new Address {StreetName = "Sesame St.", StreetNumber = 789},},
                            };
         var stream = new MemoryStream();
         foreach (Customer customer in customers)
         {
            Serializer.Serialize(customer, stream);
         }
         stream.Position = 0;
         int i = 0;
         while (true)
         {
            var customer = Deserializer.Deserialize<Customer>(stream);
            if (customer == null)
            {
               break;
            }
            Assert.Equal(customers[i].Id, customer.Id);
            Assert.Equal(customers[i].Address.StreetName, customer.Address.StreetName);
            Assert.Equal(customers[i].Address.StreetNumber, customer.Address.StreetNumber);
            i++;
         }
         Assert.Equal(3, i);
      }
      [Fact]
      public void ListOfAmbiguousTypesArePreserved()
      {
         var expected = new List<IAmAnInterface>
                         {
                            new ImplementationA {Name = "first"}, new ImplementationB {IsEnabled = true},
                            new ImplementationA {Name = "second"}, new ImplementationB {IsEnabled = false},
                         };
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<List<IAmAnInterface>>(data);
         Assert.Equal(4, actual.Count);
         Assert.Equal("first", ((ImplementationA)actual[0]).Name);
         Assert.Equal(true, ((ImplementationB)actual[1]).IsEnabled);
         Assert.Equal("second", ((ImplementationA)actual[2]).Name);
         Assert.Equal(false, ((ImplementationB)actual[3]).IsEnabled);
      }
      [Fact]
      public void AbstractPropertyIsPreserved()
      {
         var expected = new AbstractContainer {Implementation = new ImplementationA {Name = "Working"}};
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<AbstractContainer>(data);
         Assert.Equal("Working", ((ImplementationA)actual.Implementation).Name);
      }
      [Fact]
      public void AbstractNullPropertyIsSetToNull()
      {
         var expected = new AbstractContainer();
         var data = Serializer.Serialize(expected);
         var actual = Deserializer.Deserialize<AbstractContainer>(data);
         Assert.Equal(null, actual.Implementation);
      }

       [Fact]
       public void RoundtripMixedFieldsAndProperties() {
           var expected = new FieldsAndProperties(123) {
               Name = "bob",
               Email = "a@b.com",
               Ref = Guid.NewGuid()
           };
           var data = Serializer.Serialize(expected);
           var actual = Deserializer.Deserialize<FieldsAndProperties>(data);
           Assert.Equal(expected.Id, actual.Id);
           Assert.Equal(expected.Name, actual.Name);
           Assert.Equal(expected.Email, actual.Email);
      }
   }
}