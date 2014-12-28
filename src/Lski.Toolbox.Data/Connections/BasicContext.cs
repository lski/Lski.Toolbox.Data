using System;
using System.Data;
using System.Data.Common;

namespace Lski.Toolbox.Data.Connections {

    /// <summary>
    /// Simple class to create a disposible Connection object from a connection string name rather than a connection string. Uses the provider name in the connection string setting to 
    /// create the appropriate Connection type (e.g. SqlConnection).
    /// </summary>
    /// <remarks>
    /// Simple class to create a disposible Connection object from a connection string name rather than a connection string. Uses the provider name in the connection string setting to 
    /// create the appropriate Connection type (e.g. SqlConnection).
    ///
    /// Recommended usage is to create a sub class, that has a parameterless constructor that calls the parent constructor in this class with the correct connection string name.
    /// This means that the context can be type safe in the code and not have Magic Strings throughout the solution.
    ///
    /// E.g.
    ///
    /// <code>
    /// public class MyContext : BasicConext {
    ///		public MyContext() : base("MyContext") {}
    /// }
    /// </code>
    /// </remarks>
    public class BasicContext : IDisposable {

        private readonly DbConnection _conn;

        public BasicContext(string name) {
            _conn = Connections.Connection.Get(name);
        }

        public BasicContext(DbConnection conn) {

            if (conn == null) {
                throw new ArgumentNullException("conn");
            }

            if (String.IsNullOrEmpty(conn.ConnectionString)) {
                throw new ArgumentException("A connection with a connection string is required");
            }

            _conn = conn;
        }

        public DbConnection Connection {
            get {
                return _conn;
            }
        }

        public void Dispose() {

            try {

                if (_conn != null && _conn.State == ConnectionState.Open) {
                    _conn.Close();
                }
            } 
            catch {
                // Stub
            }
        }
    }
}