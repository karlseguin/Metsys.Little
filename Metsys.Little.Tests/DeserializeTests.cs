using System;
using System.IO;
using System.Text;
using Xunit;

namespace Metsys.Little.Tests
{
   public class DeserializerTests
   {
      [Fact]
      public void NonNullNullableGetsDeserialized()
      {
         var o = Deserializer.Deserialize<SimpleClass<int?>>(new byte[]{ 1, 1, 2, 3, 4});
         Assert.Equal(67305985, o.Value);
      }
      [Fact]
      public void NullNullableGetsDeserialized()
      {
         var o = Deserializer.Deserialize<SimpleClass<int?>>(new byte[] { 0 });
         Assert.Equal(null, o.Value);
      }
      [Fact]
      public void NullGetsDeserialized()
      {
         var o = Deserializer.Deserialize<SimpleNullClass>(new byte[2]);
         Assert.Equal(null, o.NullInt);
         Assert.Equal(null, o.NullString);
      }
      [Fact]
      public void TrueGetsDeserialized()
      {
         var o = Deserializer.Deserialize<SimpleClass<bool>>(new byte[] { 1 });
         Assert.Equal(true, o.Value);
      }
      [Fact]
      public void FalseGetsDeserialized()
      {
         var o = Deserializer.Deserialize<SimpleClass<bool>>(new byte[] { 0 });
         Assert.Equal(false, o.Value);
      }
      [Fact]
      public void IntegerGetsDeserialized()
      {
         var data = BitConverter.GetBytes(int.MinValue);
         var o = Deserializer.Deserialize<SimpleClass<int>>(data);
         Assert.Equal(int.MinValue, o.Value);
      }
      [Fact]
      public void ShortGetsDeserialized()
      {
         var data = BitConverter.GetBytes(short.MinValue);
         var o = Deserializer.Deserialize<SimpleClass<short>>(data);
         Assert.Equal(short.MinValue, o.Value);
      }
      [Fact]
      public void LongGetsDeserialized()
      {
         var data = BitConverter.GetBytes(long.MinValue);
         var o = Deserializer.Deserialize<SimpleClass<long>>(data);
         Assert.Equal(long.MinValue, o.Value);
      }
      [Fact]
      public void FloatGetsDeserialized()
      {
         var data = BitConverter.GetBytes(float.MinValue);
         var o = Deserializer.Deserialize<SimpleClass<float>>(data);
         Assert.Equal(float.MinValue, o.Value);
      }
      [Fact]
      public void DoubleGetsDeserialized()
      {
         var data = BitConverter.GetBytes(double.MinValue);
         var o = Deserializer.Deserialize<SimpleClass<double>>(data);
         Assert.Equal(double.MinValue, o.Value);
      }
      [Fact]
      public void DecimalGetsDeserialized()
      {
         var bits = decimal.GetBits(decimal.MinValue);
         var data = new byte[16];
         Buffer.BlockCopy(bits, 0, data, 0, 16);
         var o = Deserializer.Deserialize<SimpleClass<decimal>>(data);
         Assert.Equal(decimal.MinValue, o.Value);
      }
      [Fact]
      public void ByteGetsDeserialized()
      {
         var data = BitConverter.GetBytes(byte.MinValue);
         var o = Deserializer.Deserialize<SimpleClass<byte>>(data);
         Assert.Equal(byte.MinValue, o.Value);
      }
      [Fact]
      public void EnumGetsDeserialized()
      {
         var data = BitConverter.GetBytes((int)PowerLevels.Two);
         var o = Deserializer.Deserialize<SimpleClass<PowerLevels>>(data);
         Assert.Equal(PowerLevels.Two, o.Value); 
      }
      [Fact]
      public void NullStringGetsDeserialized()
      {
         var data = new byte[1];
         var o = Deserializer.Deserialize<SimpleClass<string>>(data);
         Assert.Equal(null, o.Value); 
      }
      [Fact]
      public void StringGetsDeserialized()
      {
         var data = Encoding.Default.GetBytes("ovr9k").Prefix(5).Prefix(1);
         var o = Deserializer.Deserialize<SimpleClass<string>>(data);
         Assert.Equal("ovr9k", o.Value);
      }
      [Fact]
      public void CharGetsDeserialized()
      {
         var data = BitConverter.GetBytes('y');
         var o = Deserializer.Deserialize<SimpleClass<char>>(data);
         Assert.Equal('y', o.Value);
      }
      [Fact]
      public void DateTimeGetsDeserialized()
      {
         var now = DateTime.Now;
         var data = BitConverter.GetBytes(now.ToBinary());
         var o = Deserializer.Deserialize<SimpleClass<DateTime>>(data);
         Assert.Equal(now, o.Value);
      }
   }

   internal static class Extensions
   {
      internal static byte[] Prefix(this byte[] data, byte b)
      {
         var n = new byte[data.Length + 1];
         n[0] = b;
         Buffer.BlockCopy(data, 0, n, 1, data.Length);
         return n;
      }
   }
}