using System;
using System.Data.SqlClient;

namespace KellySelden.Libraries.Sql
{
	class SqlConnectionWrapper : IDisposable
	{
		readonly string _connectionString;
		readonly SqlConnection _connection;
		SqlConnection _myConnection;

		public SqlConnectionWrapper(string connectionString)
		{
			_connectionString = connectionString;
		}
		public SqlConnectionWrapper(SqlConnection connection)
		{
			_connection = connection;
		}

		public SqlConnection Connection { get { return _connection ?? _myConnection ?? (_myConnection = new SqlConnection(_connectionString)); } }

		public void Dispose()
		{
			if (_myConnection != null)
			{
				_myConnection.Dispose();
				_myConnection = null;
			}
		}
	}
}