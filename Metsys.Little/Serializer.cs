using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Metsys.Little
{
    public class Serializer
    {
        private static readonly LittleConfiguration _configuration = LittleConfiguration.Instance;

        private static readonly IDictionary<Type, Action<Serializer, object>> _writerLookup = new Dictionary<Type, Action<Serializer, object>>
          {
              {typeof (bool), (s, o) => s._writer.Write((bool) o)},
              {typeof (int), (s, o) => s._writer.Write((int) o)},
              {typeof (short), (s, o) => s._writer.Write((short) o)},
              {typeof (long), (s, o) => s._writer.Write((long) o)},
              {typeof (float), (s, o) => s._writer.Write((float) o)},
              {typeof (double), (s, o) => s._writer.Write((double) o)},
              {typeof (decimal), (s, o) => s._writer.Write((decimal) o)},
              {typeof (byte), (s, o) => s._writer.Write((byte) o)},
              {typeof (string), (s, o) => s._writer.Write((string) o)},
              {typeof (char), (s, o) => s._writer.Write((char) o)},
              {typeof (DateTime), (s, o) => s.Write((DateTime) o)},
              {typeof(Guid), (s, o) => s._writer.Write(((Guid)o).ToByteArray())},
          };

        private readonly Stream _stream;
        private readonly BinaryWriter _writer;

        private Serializer(BinaryWriter writer)
        {
            _writer = writer;
            _stream = writer.BaseStream;
        }

        public static byte[] Serialize<T>(T document)
        {
            using (var ms = new MemoryStream(250))
            {
                Serialize(document, ms);
                return ms.ToArray();
            }
        }

        public static void Serialize<T>(T document, Stream destination)
        {
            var writer = new BinaryWriter(destination);
            new Serializer(writer).Start(document);
        }

        private void Start(object o)
        {
            if (o is IEnumerable)
            {
                Write((IEnumerable) o);
            }
            else
            {
                WriteObject(o);
            }
        }

        private void WriteObject(object o)
        {
            var helper = TypeHelper.GetHelperForType(o.GetType());
            foreach (var property in helper.Properties)
            {
                var value = property.Getter(o);
                if (value == null)
                {
                    WriteNull();
                    continue;
                }
                SerializeMember(property.Getter(o), property.Nullable);
            }
        }

        private void SerializeMember(object value, bool nullable)
        {
            var type = value.GetType();
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }
            if (nullable)
            {
                _writer.Write((byte) 1);
            }
            Action<Serializer, object> w;
            if (_writerLookup.TryGetValue(type, out w))
            {
                w(this, value);
            }
            else if (value is IEnumerable)
            {
                Write((IEnumerable) value);
            }
            else
            {
                WriteObject(value);
            }
        }

        private void Write(IEnumerable enumerable)
        {
            var start = _stream.Position;
            _writer.Write(0); //placeholder for # of elements
            var count = 0;
            //bool? nullable = null;
            foreach (var item in enumerable)
            {
                ++count;
/*               if (item == null)
                {
                    WriteNull();
                    continue;
                }
                if (nullable == null)
                {
                    nullable = MagicProperty.IsNullable(item.GetType());
                }
 */
                SerializeMember(item, false);
            }
            _stream.Seek(start, SeekOrigin.Begin);
            _writer.Write(count);
            _stream.Seek(0, SeekOrigin.End);
        }

        private void Write(DateTime date)
        {
            if (_configuration.DateTimeMode == DateTimeMode.Detailed)
            {
                _writer.Write(date.ToBinary());
            }
            else
            {
                _writer.Write((int)date.Subtract(Helper.Epoch).TotalSeconds);
            }
        }

        private void WriteNull()
        {
            _writer.Write((byte) 0);
        }
    }
}
