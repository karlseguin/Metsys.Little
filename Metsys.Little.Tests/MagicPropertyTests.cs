using Xunit;

namespace Metsys.Little.Tests
{
   public class MagicPropertyTests
   {
      [Fact]
      public void ReferenceTypesAreNullable()
      {
         Assert.Equal(true, MagicProperty.IsNullable(typeof(object)));
         Assert.Equal(true, MagicProperty.IsNullable(typeof(string)));
         Assert.Equal(true, MagicProperty.IsNullable(typeof(SimpleNullClass)));
      }
      [Fact]
      public void NullableTypesAreNullable()
      {
         Assert.Equal(true, MagicProperty.IsNullable(typeof(int?)));
         Assert.Equal(true, MagicProperty.IsNullable(typeof(PowerLevels?)));
      }
      [Fact]
      public void ValueTypesArentNullable()
      {
         Assert.Equal(false, MagicProperty.IsNullable(typeof(int)));
         Assert.Equal(false, MagicProperty.IsNullable(typeof(PowerLevels)));
      }
   }
}