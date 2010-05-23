using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Metsys.Little
{
   public class Deserializer
   {
      private static readonly LittleConfiguration _configuration = LittleConfiguration.Instance;
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
            {typeof(DateTime), r => _configuration.DateTimeMode == DateTimeMode.Detailed ? DateTime.FromBinary(r.ReadInt64()) : Helper.Epoch.AddSeconds(r.ReadInt32())},
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
         return (T)DeserializeValue(typeof(T), null);
      }
      private object DeserializeValue(Type type, object container)
      {
         Func<BinaryReader, object> r;
         if (_readerLookup.TryGetValue(type, out r))
         {
            return r(_reader);
         }
         if (typeof(IEnumerable).IsAssignableFrom(type))
         {
            return ReadList(type, container);
         }
         return ReadObject(type);
      }

      private object ReadList(Type listType, object existingContainer)
      {
         var totalItems = _reader.ReadInt32();
         var count = 0;
         var itemType = ListHelper.GetListItemType(listType);
         var wrapper = BaseWrapper.Create(listType, itemType, existingContainer);
         var nullable = MagicProperty.IsNullable(itemType);

         while (count++ < totalItems)
         {
            var value = nullable && IsNull() ? null : DeserializeValue(itemType, null);
            wrapper.Add(value);
         }
         return wrapper.Collection; 
      }

      private object ReadObject(Type type)
      {
         var instance = Activator.CreateInstance(type, true);
         var helper = TypeHelper.GetHelperForType(type);
         foreach(var property in helper.Properties)
         {
            object container = null;
            if (property.Setter == null)
            {
               container = property.Getter(instance);
            }
            var value = property.Nullable && IsNull() ? null : DeserializeValue(property.Type, container);
            if (container == null)
            {
               property.Setter(instance, value);
            }
         }
         return instance;
      }
      private bool IsNull()
      {
         return _reader.ReadByte() == 0;
      }
   }
}