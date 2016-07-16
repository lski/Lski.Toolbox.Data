using System;
using System.Data;
using System.Data.Common;

namespace Lski.Toolbox.Data.Connections {

	/// <summary>
	/// A simple disposible database connection wrapper, that creates the DbConnection object from a connection string name.
	/// Where the type is derived from the provider name e.g. System.Data.SqlClient results in an SqlConnection object.
	/// </summary>
	/// <remarks>
	/// A simple disposible database connection wrapper, that creates the DbConnection object from a connection string name.
	/// Where the type is derived from the provider name e.g. System.Data.SqlClient results in an SqlConnection object.
	///
	/// Alternatively you can supply a raw connection string, with its provider name.
	///
	/// You can use BasicContext however you want, but the simpliest way is:
	///
	/// <code>
	/// using(var context = new BasicContext("myConnString")) {
	///		context.Connection.Open();
	///		// do something
	/// }
	/// </code>
	///
	///
	/// There are a couple of recommended ways of using the class though.
	///
	/// The first way is to create a sub class, that has a parameterless constructor that calls the parent constructor in this class with the correct connection string name.
	/// This means that the context can be type safe in the code and not have Magic Strings throughout the solution.
	///
	/// E.g.
	///
	/// <code>
	/// public class MyContext : BasicConext {
	///		public MyContext() : base("MyContext") {}
	/// }
	/// </code>
	///
	/// The second is to use it along with IBasicContext, with a Dependancy Injection container like Autofac.
	/// This means it is created from a central point, meaning no magic strings throughout.
	///
	/// <code>
	/// builder.Register(c => new BasicContext("myConnString")).As&lt;IBasicContext&gt;();
	/// </code>
	/// </remarks>
	public class BasicContext : IBasicContext {

		private readonly DbConnection _conn;

		public BasicContext(string name) {
			_conn = Connections.Connection.Get(name);
		}

		public BasicContext(string connentionString, string providerName) {
			_conn = Connections.Connection.Get(connentionString, providerName);
		}

		public BasicContext(DbConnection conn) {

			if (conn == null) {
				throw new ArgumentNullException(nameof(conn));
			}

			if (String.IsNullOrEmpty(conn.ConnectionString)) {
				throw new ArgumentException("A connection with a connection string is required");
			}

			_conn = conn;
		}

		/// <summary>
		/// The underlying connection object to use
		/// </summary>
		public DbConnection Connection {
			get {
				return _conn;
			}
		}

		/// <summary>
		/// Disposes of the connection by closing it if the connection is open.
		/// </summary>
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