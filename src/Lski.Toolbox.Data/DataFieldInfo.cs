using System;

namespace Lski.Toolbox.Data {

    /// <summary>
    /// Meta data about a field in an IDataReader
    /// </summary>
    public class DataFieldInfo {

        public string Name { get; set; }

        public int Position { get; set; }

        public Type Type { get; set; }
    }
}