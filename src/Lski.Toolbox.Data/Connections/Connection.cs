using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Lski.Toolbox.Data.Connections {

	/// <summary>
	/// A basic selection of static methods associated with connected to a database and the DbConnection object and their providers
	/// </summary>
	public static class Connection {

		static Connection() {
			ConnectionStringSettings = ConfigurationManager.ConnectionStrings;
		}

		private static ConnectionStringSettingsCollection ConnectionStringSettings { get; set; }

		private static DbProviderFactory GetFactory(string providerName) {
			return DbProviderFactories.GetFactory(providerName);
		}

		/// <summary>
		/// Creates a DbConnection object from a connection string stored in the connection string settings
		/// </summary>
		/// <param name="connectionStringName">The name of the connection string in the </param>
		public static DbConnection Get(String connectionStringName) {

			if (String.IsNullOrEmpty(connectionStringName)) {
				throw new ArgumentException(nameof(connectionStringName));
			}

			var css = ConnectionStringSettings[connectionStringName];

			if (css == null) {
				throw new ArgumentException($"The connection string with the name '{connectionStringName}' could not be found");
			}

			var conn = GetFactory(css.ProviderName).CreateConnection();

			conn.ConnectionString = css.ConnectionString;

			return conn;
		}

		/// <summary>
		/// Creates a DbConnection object from a raw connection string and a provider name
		/// </summary>
		/// <param name="connectionString">The raw connection string used to connect to the database</param>
		/// <param name="providerName">The provider name, is the name of the type of connection &amp; e.g. System.Data.SqlClient results in an SqlConnection object</param>
		public static DbConnection Get(String connectionString, string providerName) {

			if (String.IsNullOrEmpty(connectionString)) {
				throw new ArgumentException(nameof(connectionString));
			}

			var conn = GetFactory(providerName).CreateConnection();

			conn.ConnectionString = connectionString;

			return conn;
		}

		/// <summary>
		/// Tries to open the connection object that has been passed, returns true if it implicitily opened the connection or false if it was already open
		/// </summary>
		/// <param name="conn">The DbConnection object to open</param>
		public static bool Open(ref DbConnection conn) {

			var connState = conn.State;

			if (connState == ConnectionState.Broken || connState == ConnectionState.Closed) {

				conn.Open();

				// If the connection was mearly broken and not closed, dont state this is implicitly opened
				if (connState == ConnectionState.Closed) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Simple close connection method, only attempts to close if currently open
		/// </summary>
		/// <param name="conn">The DbConnection object to close</param>
		public static void Close(DbConnection conn) {

			if (conn != null && conn.State == ConnectionState.Open) {
				conn.Close();
			}
		}

		/// <summary>
		/// Close the connection object passed, but only if it is open. Any error messages can be suppressed by setting suppressError to True (there are recorded however)
		/// </summary>
		/// <param name="conn">The connection to close</param>
		/// <param name="suppressError">If true will catch and disgard any errors produced by the </param>
		public static void Close(DbConnection conn, bool suppressError) {

			if (conn != null && conn.State == ConnectionState.Open) {

				if (suppressError) {
					try {
						conn.Close();
					}
					catch (Exception) {
					}
				}
				else {
					conn.Close();
				}
			}
		}
	}
}
