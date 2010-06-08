using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Metsys.Little
{
   public class Deserializer
   {
      private static readonly LittleConfiguration _configuration = LittleConfiguration.Instance;

      private static readonly IDictionary<Type, Func<Deserializer, object>> _readerLookup = new Dictionary<Type, Func<Deserializer, object>>
        {
           {typeof (bool), d => d._reader.ReadBoolean()},
           {typeof (int), d => d._reader.ReadInt32()},
           {typeof (short), d => d._reader.ReadInt16()},
           {typeof (long), d => d._reader.ReadInt64()},
           {typeof (float), d => d._reader.ReadSingle()},
           {typeof (double), d => d._reader.ReadDouble()},
           {typeof (decimal), d => d._reader.ReadDecimal()},
           {typeof (byte), d => d._reader.ReadByte()},
           {typeof (string), d => d._reader.ReadString()},
           {typeof (char), d => d._reader.ReadChar()},
           {typeof (DateTime), d => d.ReadDateTime()},
           {typeof (Guid), d => new Guid(d._reader.ReadBytes(16))},
        };

      private readonly BinaryReader _reader;

      private Deserializer(BinaryReader reader)
      {
         _reader = reader;
      }

      public static T Deserialize<T>(byte[] objectData)
      {
         using (var ms = new MemoryStream())
         {
            ms.Write(objectData, 0, objectData.Length);
            ms.Position = 0;
            return Deserialize<T>(ms);
         }
      }

      public static T Deserialize<T>(BinaryReader stream)
      {
         return new Deserializer(stream).Read<T>();
      }

      public static T Deserialize<T>(Stream stream)
      {
         if (stream.CanSeek && stream.Length == stream.Position)
         {
            return default(T);
         }
         try
         {
            return Deserialize<T>(new BinaryReader(stream));
         }
         catch (EndOfStreamException)
         {
            return default(T);
         }
      }

      private T Read<T>()
      {
         return (T) DeserializeValue(typeof (T), null);
      }

      private object DeserializeValue(Type type, object container)
      {
         Func<Deserializer, object> r;
         if (_readerLookup.TryGetValue(type, out r))
         {
            return r(this);
         }
         if (typeof (IEnumerable).IsAssignableFrom(type))
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
         var isAmbiguous = Helper.IsAmbiguous(itemType);
         while (count++ < totalItems)
         {
            var type = itemType;
            if(isAmbiguous)
            {
               ReaderHeader();
               type = Type.GetType((string)_readerLookup[typeof(string)](this));
            }
            var value = DeserializeValue(type, null);
            wrapper.Add(value);
         }
         return wrapper.Collection;
      }

      private DateTime ReadDateTime()
      {
         if (_configuration.DateTimeMode == DateTimeMode.Detailed)
         {
            return DateTime.FromBinary(_reader.ReadInt64());
         }
         return Helper.Epoch.AddSeconds(_reader.ReadInt32());
      }

      private object ReadObject(Type type)
      {
         var helper = TypeHelper.GetHelperForType(type);
         var instance = helper.Create();
         foreach (var property in helper.Properties)
         {
            object container = null;
            if (property.Setter == null)
            {
               container = property.Getter(instance);
            }
            var header = property.HasHeader ? ReaderHeader() : DataHeader.Default;
            object value = null;
            if (!header.IsNull)
            {
               var propertyType = !header.IsAmbiguous ? property.Type : Type.GetType((string) _readerLookup[typeof (string)](this));
               value = DeserializeValue(propertyType, container);
            }
            if (container == null)
            {
               property.Setter(instance, value);
            }
         }
         foreach(var field in helper.Fields) {
             object container = null;
             if(field.Setter == null) {
                 container = field.Getter(instance);
             }
             var header = field.HasHeader ? ReaderHeader() : DataHeader.Default;
            object value = null;
            if (!header.IsNull)
            {
                var propertyType = !header.IsAmbiguous ? field.Type : Type.GetType((string)_readerLookup[typeof(string)](this));
               value = DeserializeValue(propertyType, container);
            }
            if (container == null)
            {
                field.Setter(instance, value);
            }
         }
         return instance;
      }

      private DataHeader ReaderHeader()
      {
         var data = _reader.ReadByte();
         var header = new DataHeader();
         
         if ((data & 128) == 128) { header.IsNull = true; }
         if ((data & 64) == 64) { header.IsAmbiguous = true; }
         
         return header;
      }
   }
}
