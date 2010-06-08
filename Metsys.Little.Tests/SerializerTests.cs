using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Metsys.Little.Tests
{
   public class SerializerTests : BaseFixture
   {      
      [Fact]
      public void NullGetsSerialized()
      {
         var data = Serializer.Serialize(new SimpleNullClass());
         Assert.Equal(128, data[0] & 128);
         Assert.Equal(128, data[1] & 128);
      }
      [Fact]
      public void NullFieldsGetsSerialized()
      {
         var data = Serializer.Serialize(new SimpleNullFieldClass());
         Assert.Equal(128, data[0] & 128);
         Assert.Equal(128, data[1] & 128);
      }
       [Fact]
      public void NullableGetsPrefixedWhenNull()
      {
         var data = Serializer.Serialize(new { x = (bool?)null });
         Assert.Equal(128, data[0] & 128);
         Assert.Equal(1, data.Length);
      }
      [Fact]
      public void NullableGetsPrefixedWhenNotNull()
      {
         var data = Serializer.Serialize(new { x = (bool?)true });
         Assert.Equal(0, data[0] & 128);
         Assert.Equal(1, data[1]);
      }
      [Fact]
      public void TrueGetsSerialized()
      {
         var data = Serializer.Serialize(new {x = true});
         Assert.Equal(1, data[0]); 
      }
      [Fact]
      public void FalseGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = false });
         Assert.Equal(0, data[0]);        
      }
      [Fact]
      public void IntegerGetsSerialized()
      {
         var data = Serializer.Serialize(new {x = 145});
         Assert.Equal(145, BitConverter.ToInt32(data, 0));
      }
      [Fact]
      public void ShortGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = (short)22 });
         Assert.Equal(22, BitConverter.ToInt16(data, 0));
      }
      [Fact]
      public void LongGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = long.MinValue });
         Assert.Equal(long.MinValue, BitConverter.ToInt64(data, 0));
      }
      [Fact]
      public void FloatGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = float.MinValue });
         Assert.Equal(float.MinValue, BitConverter.ToSingle(data, 0));
      }
      [Fact]
      public void DoubleGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = Double.MaxValue });
         Assert.Equal(double.MaxValue, BitConverter.ToDouble(data, 0));
      }
      [Fact]
      public void DecimalGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = Decimal.MaxValue });
         var bits = new int[4];
         Buffer.BlockCopy(data, 0, bits, 0, 16);
         Assert.Equal(decimal.MaxValue, new Decimal(bits));
      }
      [Fact]
      public void ByteGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = (byte)49 });
         Assert.Equal(49, data[0]);
      }
      [Fact]
      public void EnumGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = PowerLevels.Over9000 });
         Assert.Equal((int) PowerLevels.Over9000, BitConverter.ToInt32(data, 0));
      }
      [Fact]
      public void StringGetsSerialized()
      {
         var data = Serializer.Serialize(new {x = "abc123"});
         var actual = Encoding.Default.GetString(data, 2, data[1]);
         Assert.Equal(0, data[0] & 128);
         Assert.Equal("abc123", actual);
      }
      [Fact]
      public void CharGetsSerialized()
      {
         var data = Serializer.Serialize(new { x = 'w' });
         Assert.Equal('w', (char)data[0]);
      }
      [Fact]
      public void DateTimeGetsDetailedSerialized()
      {
         LittleConfiguration.Global(g => g.DateTime(DateTimeMode.Detailed));
         var now = DateTime.Now;
         var data = Serializer.Serialize(new {x = now});
         Assert.Equal(now, DateTime.FromBinary(BitConverter.ToInt64(data, 0)));
      }
      [Fact]
      public void DateTimeGetsSecondsPrecisionSerialized()
      {
         var now = DateTime.Now;
         var data = Serializer.Serialize(new { x = now });
         Assert.Equal(now.ToString(), Helper.Epoch.AddSeconds(BitConverter.ToInt32(data, 0)).ToString());
      }
      [Fact]
      public void ArrayOfIntegersGetsSerialized()
      {
         var data = Serializer.Serialize(new {x = new[] {1, 5, 9, 19}});
         Assert.Equal(0, data[0] & 128);
         Assert.Equal(4, BitConverter.ToInt32(data, 1));
         Assert.Equal(1, BitConverter.ToInt32(data, 5));
         Assert.Equal(5, BitConverter.ToInt32(data, 9));
         Assert.Equal(9, BitConverter.ToInt32(data, 13));
         Assert.Equal(19, BitConverter.ToInt32(data, 17));
      }
      [Fact]
      public void NullArrayGetsSerialized()
      {
         var data = Serializer.Serialize(new SimpleClass<IList<byte>>());
         Assert.Equal(128, data[0] & 128);
         Assert.Equal(1, data.Length);
      }
      [Fact]
      public void NullArrayFieldGetsSerialized() {
          var data = Serializer.Serialize(new SimpleFieldClass<IList<byte>>());
          Assert.Equal(128, data[0] & 128);
          Assert.Equal(1, data.Length);
      }
      [Fact(Skip = "took out this feature")]
      public void SerializesArrayWithMixOfNullsAndNotNulls()
      {
         var data = Serializer.Serialize(new[] {"ab", null, "cd", null, null});
         Assert.Equal(5, BitConverter.ToInt32(data, 0));
         Assert.Equal(1, data[4]);
         Assert.Equal(2, data[5]);
         Assert.Equal("ab", Encoding.Default.GetString(data, 6, 2));
         Assert.Equal(0, data[8]);
         Assert.Equal(1, data[9]);
         Assert.Equal(2, data[10]);
         Assert.Equal("cd", Encoding.Default.GetString(data, 11, 2));
         Assert.Equal(0, data[13]);
         Assert.Equal(0, data[14]);
      }
      [Fact]
      public void GuidGetsSerialized()
      {
          var guid = Guid.NewGuid();
          var data = Serializer.Serialize(new { x = guid });
          Assert.Equal(guid, new Guid(data));
      }
      [Fact]
      public void InterfaceGetsFlaggedAsAmbiguous()
      {
          var data = Serializer.Serialize(new InterfaceContainer {Implementation = new ImplementationA()});
          Assert.Equal(0, data[0] & 128);
          Assert.Equal(64, data[0] & 64);
          var actual = Encoding.Default.GetString(data, 2, data[1]);
          Assert.Equal("Metsys.Little.Tests.ImplementationA, Metsys.Little.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", actual);
      }
      [Fact]
      public void NonAbstractDoesntHaveAmbiguousFlag()
      {
          var data = Serializer.Serialize(new {x = (bool?)null}); //nullable but not abstract
          Assert.Equal(128, data[0] & 128);
          Assert.Equal(0, data[0] & 64);          
      }
      [Fact]
      public void AbstractGetsFlaggedAsAmbiguous()
      {
          var data = Serializer.Serialize(new AbstractContainer { Implementation = new ImplementationB() });
          Assert.Equal(0, data[0] & 128);
          Assert.Equal(64, data[0] & 64);
          var actual = Encoding.Default.GetString(data, 2, data[1]);
          Assert.Equal("Metsys.Little.Tests.ImplementationB, Metsys.Little.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", actual);
      }
      [Fact]
      public void ScalarIntegerGetsSerialized()
      {
         var data = Serializer.Serialize(58);
         Assert.Equal(new byte[]{58, 0, 0, 0}, data);
      }
      [Fact]
      public void ScalarBoolGetsSerialized()
      {
         var data = Serializer.Serialize(true);
         Assert.Equal(new byte[] { 1 }, data);
      }
      [Fact]
      public void ScalarStringGetsSerialized()
      {
         var data = Serializer.Serialize("abc123");
         Assert.Equal(new byte[] { 6, 97, 98, 99, 49, 50, 51 }, data);
      }
   }
}
