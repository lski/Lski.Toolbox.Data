using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Lski.Toolbox.Data.Loader {

    public static class DataLoaderExtensions {

        public static DataLoader<T> CreateLoader<T>(this IDataReader reader, IEnumerable<PropertyFieldMapping> mappings = null) where T : new() {
            return new DataLoader<T>(reader, mappings);
        }
    }

    /// <summary>
    /// Loads data from a data reader into an object or list of objects
    /// </summary>
    public class DataLoader<T> where T : new() {

        /// <summary>
        /// Cached properties so they dont have to be re-calculated each time
        /// </summary>
        private static ConcurrentDictionary<Type, HashSet<PropertyInfo>> _cachedProperties = new ConcurrentDictionary<Type, HashSet<PropertyInfo>>();

        private readonly IEnumerable<PropertyFieldLink> _propertyFieldLinks;

        private readonly IDataReader _reader;

        public DataLoader(IDataReader reader, IEnumerable<PropertyFieldMapping> mappings = null) {

            _propertyFieldLinks = GeneratePropertyFieldLinks(reader, mappings);
            _reader = reader;
        }

        /// <summary>
        /// Loads the next record from the IDataReader into a new object of type T using the mapping settings for this Loader.
        /// </summary>
        /// <returns></returns>
        public T Load() {
            return Load(new T());
        }

        /// <summary>
        /// Loads each record into the passed collection until the reader is complete. It is then returned to make creating a new Collection a single line expression.
        /// </summary>
        public ICollection<T> Load(ICollection<T> items) {

            while (_reader.Read()) {
                items.Add(Load());
            }

            return items;
        }

        /// <summary>
        /// Loads the next record from the IDataReader into the object passed in using the mapping settings for this Loader. It is then returned to make creating a new object a single line expression.
        /// </summary>
        public T Load(T item) {

            foreach (var p in _propertyFieldLinks) {

                var pos = p.Field.Position;
                var valueType = p.Field.Type;
                var value = _reader.GetValue(pos);

                // Only attempt to update property if not DBNull
                if (!DBNull.Value.Equals(value)) {

                    if (valueType == typeof(Boolean)) {

                        p.Property.SetValue(item, _reader.GetBoolean(pos), null);

                    } else if (p.Property.PropertyType == typeof(Guid)) {

                        p.Property.SetValue(item, _reader.GetGuid(pos), null);

                    } else {

                        try {
                            p.Property.SetValue(item, value, null);
                        } catch {
                            // Stub, in case of the very unlikely event theres an error setting the value
                        }
                    }
                }
            }

            return item;
        }

        private class PropertyFieldLink {

            public PropertyInfo Property { get; set; }

            public DataFieldInfo Field { get; set; }
        }

        /// <summary>
        /// Generates a list of objects that joins properties in a class and fields within a data reader. Optionally accepts a list of field/property name mappings in case you need to
        /// match different names
        /// </summary>
        private IEnumerable<PropertyFieldLink> GeneratePropertyFieldLinks(IDataReader reader, IEnumerable<PropertyFieldMapping> mappings = null) {

            var fields = reader.FieldInfo().ToList();
            var props = GetProperties(typeof(T));

            if (_propertyFieldLinks == null) {

                return (from f in fields
                        join p in props on f.Name equals p.Name.ToLowerInvariant()
                        select new PropertyFieldLink {
                            Property = p,
                            Field = f
                        });
            }

            return (from f in fields
                    join map in mappings on f.Name equals map.Field.ToLowerInvariant()
                    join p in props on map.Property.ToLowerInvariant() equals p.Name.ToLowerInvariant()
                    select new PropertyFieldLink {
                        Property = p,
                        Field = f
                    });
        }

        /// <summary>
        /// Returns the property list for this object, either from cache or calculated if this is the first time its called
        /// </summary>
        private static HashSet<PropertyInfo> GetProperties(Type t) {

            HashSet<PropertyInfo> props;

            if (_cachedProperties.TryGetValue(t, out props)) {
                return props;
            }

            props = new HashSet<PropertyInfo>(t.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanWrite));

            _cachedProperties.TryAdd(t, props);

            return props;
        }
    }
}