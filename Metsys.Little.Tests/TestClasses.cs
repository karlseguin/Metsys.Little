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
   [System.Serializable]
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
}