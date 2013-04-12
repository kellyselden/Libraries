using System;
using System.Data;
using System.Data.SqlClient;

namespace KellySelden.Libraries.Sql
{
	public class SqlWrapper
	{
		readonly SqlConnectionWrapper _connectionWrapper;

		public SqlWrapper(string connectionString)
		{
			_connectionWrapper = new SqlConnectionWrapper(connectionString);
		}
		public SqlWrapper(SqlConnection connection)
		{
			_connectionWrapper = new SqlConnectionWrapper(connection);
		}

		public DataSet ExecuteDataset(string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			var ds = new DataSet();
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			using (var da = new SqlDataAdapter(cmd))
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				da.Fill(ds);
				_connectionWrapper.Connection.Close();
			}
			return ds;
		}

		public void ExecuteNonQuery(string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				cmd.ExecuteNonQuery();
				_connectionWrapper.Connection.Close();
			}
		}

		public T ExecuteScalar<T>(string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			object retVal;
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				retVal = cmd.ExecuteScalar();
				_connectionWrapper.Connection.Close();
			}
			return retVal == DBNull.Value ? default(T) : (T)retVal;
		}

		public static DataSet ExecuteDataset(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteDataset(sql);
		}
		public static DataSet ExecuteDataset(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteDataset(sql, type, @params);
		}

		public static void ExecuteNonQuery(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			new SqlWrapper(connectionString).ExecuteNonQuery(sql, type, @params);
		}
		public static void ExecuteNonQuery(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			new SqlWrapper(connection).ExecuteNonQuery(sql, type, @params);
		}

		public static T ExecuteScalar<T>(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteScalar<T>(sql, type, @params);
		}
		public static T ExecuteScalar<T>(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteScalar<T>(sql, type, @params);
		}

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
}