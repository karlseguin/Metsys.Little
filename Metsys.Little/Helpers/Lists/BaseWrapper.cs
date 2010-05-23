using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metsys.Little
{
    internal abstract class BaseWrapper
    {
        public static BaseWrapper Create(Type type, Type itemType, object existingContainer)
        {            
            var instance = CreateWrapperFromType(existingContainer == null ? type : existingContainer.GetType(), itemType);
            instance.SetContainer(existingContainer ?? instance.CreateContainer(type, itemType));
            return instance;            
        }

        private static BaseWrapper CreateWrapperFromType(Type type, Type itemType)
        {
            if (type.IsArray)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(ArrayWrapper<>).MakeGenericType(itemType));
            }

            var isCollection = false;
            var interfaces = type.GetInterfaces();
            var count = interfaces.Length + 1;
            var types = new List<Type>(count) {type.IsGenericType ? type.GetGenericTypeDefinition() : type};
            types.AddRange(interfaces);
            
            for(var i = 0; i < count; ++i)
            {
                var @interface = types[i];
                if (@interface.IsGenericType)
                {
                    @interface = @interface.GetGenericTypeDefinition();
                    types[i] = @interface;
                }
                if (typeof(IList<>).IsAssignableFrom(@interface) || typeof(IList).IsAssignableFrom(@interface))
                {
                    return new ListWrapper();
                }
                if (typeof(ICollection<>).IsAssignableFrom(@interface))
                {
                    isCollection = true;
                }
            }
            if (isCollection)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(itemType));
            }

            //a last-ditch pass
            for(var i = 0; i < count; ++i)
            {
                var @interface = types[i];
                if (typeof(IEnumerable<>).IsAssignableFrom(@interface) || typeof(IEnumerable).IsAssignableFrom(@interface))
                {
                    return new ListWrapper();
                }
            }
            throw new NotSupportedException(string.Format("Collection of type {0} cannot be deserialized", type.FullName));
        }

        public abstract void Add(object value);
        public abstract object Collection { get; }

        protected abstract object CreateContainer(Type type, Type itemType);
        protected abstract void SetContainer(object container);        
    }      
}