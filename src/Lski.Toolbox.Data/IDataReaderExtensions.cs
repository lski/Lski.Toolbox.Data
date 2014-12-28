using System.Collections.Generic;
using System.Data;

namespace Lski.Toolbox.Data {

    /// <summary>
    /// Provides some additional functionality to IDataReader
    /// </summary>
    public static class IDataReaderExtensions {

        /// <summary>
        /// Returns an IEnumerable of field names for this reader
        /// </summary>
        public static IEnumerable<string> FieldNames(this IDataReader rdr) {

            for (int i = 0, n = rdr.FieldCount; i < n; i++) {
                yield return rdr.GetName(i);
            }

        }

        /// <summary>
        /// Returns a list of meta data about the fields for this reader. This includes: name, position on the reader and data type
        /// </summary>
        public static IEnumerable<DataFieldInfo> FieldInfo(this IDataReader rdr) {

            for (int i = 0, n = rdr.FieldCount; i < n; i++) {

                yield return new DataFieldInfo() {
                    Name = rdr.GetName(i),
                    Position = i,
                    Type = rdr.GetFieldType(i)
                };
            }
        }
    }
}