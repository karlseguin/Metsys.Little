using System;
using System.Collections.Generic;

namespace Metsys.Little.Tests
{
   public class SimpleNullClass
   {
      public string NullString { get; set; }
      public int? NullInt { get; set; }
   }
   public class SimpleClass<T>
   {
      public T Value { get; set; }
   }
   public enum PowerLevels
   {
      One,
      Two,
      Over9000,
   }

   public class ComplexObject
   {
      private IList<string> _roles;
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public bool Enabled { get; set; }
      public char? Initial { get; set; }
      public char[] Security { get; set; }
      public IList<string> Roles
      {
         get
         {
            if (_roles == null)
            {
               _roles = new List<string>();
            }
            return _roles;
         }
      }
   }

   public class Customer
   {
      private IList<Order> _orders;
      public int Id { get; private set; }
      public Address Address { get; set; }
      public IList<Order> Orders
      {
         get
         {
            if (_orders == null)
            {
               _orders = new List<Order>();
            }
            return _orders;
         }
      }

      private Customer(){}
      public Customer(int id)
      {
         Id = id;
      }
   }

   public class Address
   {
      public int StreetNumber { get; set; }
      public string StreetName { get; set; }
   }
   public class Order
   {
      public decimal Price { get; set; }
      public DateTime Ordered { get; set; }
   }
}