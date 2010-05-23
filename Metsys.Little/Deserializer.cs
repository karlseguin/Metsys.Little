using System;
using System.Collections.Generic;
using System.IO;

namespace Metsys.Little
{
   public class Deserializer
   {
      private static readonly IDictionary<Type, Func<BinaryReader, object>> _readerLookup = new Dictionary<Type, Func<BinaryReader, object>>
         {
            {typeof(bool), r => r.ReadBoolean()},
            {typeof(int), r => r.ReadInt32()},
            {typeof(short), r => r.ReadInt16()},
            {typeof(long), r => r.ReadInt64()},
            {typeof(float), r => r.ReadSingle()},
            {typeof(double), r => r.ReadDouble()},
            {typeof(decimal), r => r.ReadDecimal()},
            {typeof(byte), r => r.ReadByte()},
            {typeof(string), r => r.ReadString()},
            {typeof(char), r => r.ReadChar()},
            {typeof(DateTime), r => DateTime.FromBinary(r.ReadInt64())}
         };
      private readonly BinaryReader _reader;
      private readonly Stream _stream;
      private readonly long _length;

      private Deserializer(BinaryReader reader)
      {
         _reader = reader;
         _stream = reader.BaseStream;
         _length = _stream.Length;
      }

      public static T Deserialize<T>(byte[] objectData) where T : class
      {
         using (var ms = new MemoryStream())
         {
            ms.Write(objectData, 0, objectData.Length);
            ms.Position = 0;
            return Deserialize<T>(new BinaryReader(ms));
         }
      }
      public static T Deserialize<T>(BinaryReader stream)
      {
         return new Deserializer(stream).Read<T>();
      }

      private T Read<T>()
      {
         return (T)DeserializeValue(typeof(T), false);
      }
      private object DeserializeValue(Type type, bool nullable)
      {
         Func<BinaryReader, object> r;
         if (_readerLookup.TryGetValue(type, out r))
         {
            return r(_reader);
         }
         return ReadObject(type);
      }

      private object ReadObject(Type type)
      {
         var instance = Activator.CreateInstance(type, true);
         var helper = TypeHelper.GetHelperForType(type);
         foreach(var property in helper.Properties)
         {
            var value = property.Nullable && IsNull() ? null : DeserializeValue(property.Type, property.Nullable);
            property.Setter(instance, value);
         }
         return instance;
      }
      private bool IsNull()
      {
         return _reader.ReadByte() == 0;
      }
   }
}