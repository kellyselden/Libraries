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
			return ExecuteDataset(sql, type, null, @params);
		}
		public DataSet ExecuteDataset(string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			var ds = new DataSet();
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			using (var da = new SqlDataAdapter(cmd))
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				da.Fill(ds);
				_connectionWrapper.Connection.Close();
			}
			return ds;
		}

		public void ExecuteNonQuery(string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			ExecuteNonQuery(sql, type, null, @params);
		}
		public void ExecuteNonQuery(string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				cmd.ExecuteNonQuery();
				_connectionWrapper.Connection.Close();
			}
		}

		public SqlDataReader ExecuteReader(string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteReader(sql, type, null, @params);
		}
		public SqlDataReader ExecuteReader(string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			SqlDataReader reader;
			//using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			}
			return reader;
		}

		public T ExecuteScalar<T>(string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteScalar<T>(sql, type, null, @params);
		}
		public T ExecuteScalar<T>(string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			object retVal;
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = type;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				retVal = cmd.ExecuteScalar();
				_connectionWrapper.Connection.Close();
			}
			return retVal == DBNull.Value ? default(T) : (T)retVal;
		}

		public static DataSet ExecuteDataset(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataset(connectionString, sql, type, null, @params);
		}
		public static DataSet ExecuteDataset(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataset(connection, sql, type, null, @params);
		}
		public static DataSet ExecuteDataset(string connectionString, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteDataset(sql, type, commandTimeout, @params);
		}
		public static DataSet ExecuteDataset(SqlConnection connection, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteDataset(sql, type, commandTimeout, @params);
		}

		public static void ExecuteNonQuery(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			ExecuteNonQuery(connectionString, sql, type, null, @params);
		}
		public static void ExecuteNonQuery(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			ExecuteNonQuery(connection, sql, type, null, @params);
		}
		public static void ExecuteNonQuery(string connectionString, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			new SqlWrapper(connectionString).ExecuteNonQuery(sql, type, commandTimeout, @params);
		}
		public static void ExecuteNonQuery(SqlConnection connection, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			new SqlWrapper(connection).ExecuteNonQuery(sql, type, commandTimeout, @params);
		}

		public static SqlDataReader ExecuteReader(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteReader(connectionString, sql, type, null, @params);
		}
		public static SqlDataReader ExecuteReader(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteReader(connection, sql, type, null, @params);
		}
		public static SqlDataReader ExecuteReader(string connectionString, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteReader(sql, type, commandTimeout, @params);
		}
		public static SqlDataReader ExecuteReader(SqlConnection connection, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteReader(sql, type, commandTimeout, @params);
		}

		public static T ExecuteScalar<T>(string connectionString, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteScalar<T>(connectionString, sql, type, null, @params);
		}
		public static T ExecuteScalar<T>(SqlConnection connection, string sql, CommandType type = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteScalar<T>(connection, sql, type, null, @params);
		}
		public static T ExecuteScalar<T>(string connectionString, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteScalar<T>(sql, type, commandTimeout, @params);
		}
		public static T ExecuteScalar<T>(SqlConnection connection, string sql, CommandType type, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteScalar<T>(sql, type, commandTimeout, @params);
		}
	}
}