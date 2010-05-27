using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metsys.Little
{
    internal class TypeHelper
    {
        private static readonly IDictionary<Type, TypeHelper> _cachedTypeLookup = new Dictionary<Type, TypeHelper>();
        private static readonly LittleConfiguration _configuration = LittleConfiguration.Instance;

        private readonly IList<MagicProperty> _properties;
        private readonly Func<object> _createHandler;
        private readonly Type _type;
        private IList<MagicField> _fields;

        public object Create()
        {
            return _createHandler == null ? Activator.CreateInstance(_type, true) : _createHandler();
        }

        private TypeHelper(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            _properties = LoadMagicProperties(type, properties);
            _fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Select(field => new MagicField(field)).ToList();
            _type = type;
            var constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (constructorInfo != null)
            {
                _createHandler = constructorInfo.CreateDelegate(typeof (Func<object>)) as Func<object>;
            }
        }

        public IEnumerable<MagicProperty> Properties
        {
            get { return _properties; }
        }

        public IEnumerable<MagicField> Fields
        {
            get { return _fields; }
        }

        public static TypeHelper GetHelperForType(Type type)
        {
            TypeHelper helper;
            if (!_cachedTypeLookup.TryGetValue(type, out helper))
            {
                helper = new TypeHelper(type);
                _cachedTypeLookup[type] = helper;
            }
            return helper;
        }

        private static IList<MagicProperty> LoadMagicProperties(Type type, ICollection<PropertyInfo> properties)
        {
            var magic = new List<MagicProperty>(properties.Count);
            foreach (var property in properties)
            {
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                if (_configuration.IsIgnored(type, property.Name))
                {
                    continue;
                }
                magic.Add(new MagicProperty(property));
            }
            magic.Sort((a, b) => string.Compare(a.Name, b.Name));
            return magic;
        }
    }
}