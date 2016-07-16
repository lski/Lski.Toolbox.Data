using System;
using System.Data;
using System.Data.Common;

namespace Lski.Toolbox.Data.Connections {

	/// <summary>
	/// A wrapper for a DbTransaction object that implements the IDisposible interface so the transaction can be used with a using statement and commit prior to the end of the using
	/// </summary>
	/// <remarks>
	/// Usage:
	/// using(var tran = new Transaction(meConnection)) {
	///
	///		// Do Stuff
	///
	///		tran.Commit();
	///
	/// } // If reaches here without committing, then the transaction is rolled back automatically
	/// </remarks>
	public class Transaction : IDisposable {

		private readonly DbConnection _connection;
		private readonly DbTransaction _transaction;

		private Boolean _connectionOpened = false;
		private Boolean _completed = false;
		private Boolean _rolledBack = false;

		/// <summary>
		/// Creates a wrapper for a transaction object that can be used in a using
		/// </summary>
		/// <param name="conn">A db connection object, if connection is closed then it is opened and then closed on dispose implicitly</param>
		public Transaction(DbConnection conn) {

			_connection = conn;

			if (_connection.State == ConnectionState.Closed) {
				_connection.Open();
				_connectionOpened = true;
			}

			_transaction = _connection.BeginTransaction();
		}

		/// <summary>
		/// Gives access to the underlying transaction object
		/// </summary>
		public DbTransaction Base {
			get {
				return _transaction;
			}
		}

		/// <summary>
		/// A reference to the connection associated with the transaction
		/// </summary>
		public DbConnection Conn {
			get {
				return _connection;
			}
		}

		/// <summary>
		/// Mark the transactions as completed successfully, if not run prior to Dispose being called the transaction is rolled back.
		/// </summary>
		public void Commit() {

			// Makes sure if the user has already called rollback we dont run commit by accident
			if (_rolledBack == true)
				return;

			_completed = true;
			_transaction.Commit();

			// Close the connection if openeded implicitly
			if (_connectionOpened && _connection.State == ConnectionState.Open) {
				_connection.Close();
			}
		}

		/// <summary>
		/// Rollsback the transaction explicity, so that the transaction is not completed when
		/// </summary>
		public void Rollback() {

			// Ensure this is not run twice
			_rolledBack = true;

			_transaction.Rollback();
			_transaction.Dispose();

			// Close the connection if openeded implicitly
			if (_connectionOpened && _connection.State == ConnectionState.Open) {
				_connection.Close();
			}
		}

		/// <summary>
		/// If when the object is disposed the transaction has NOT been completed then the transaction will be rolled back, a'la TransactionScope
		/// </summary>
		public void Dispose() {

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// If when the object is disposed the transaction has NOT been completed then the transaction will be rolled back, a'la TransactionScope
		/// </summary>
		public void Dispose(Boolean disposing) {

			// If disposing equals true, dispose all managed and unmanaged resources.
			if (disposing) {

				// When reaching here check the if this level has been completed, if not, then rollback
				if (!_completed && !_rolledBack) {
					Rollback();
				}
			}
		}
	}
}