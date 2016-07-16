using System;
using System.Data.Common;

namespace Lski.Toolbox.Data.Connections {

	/// <summary>
	/// A simple disposible database connection wrapper.
	/// </summary>
	public interface IBasicContext : IDisposable {

		DbConnection Connection { get; }
	}
}