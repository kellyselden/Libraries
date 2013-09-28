using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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

		public T[] ExecuteArray<T>(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteArray<T>(sql, commandType, null, @params);
		}
		public T[] ExecuteArray<T>(string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return ExecuteDataRows(sql, commandType, commandTimeout, @params).Select(r => (T)r[0]).ToArray();
		}

		public DataRow[] ExecuteDataRows(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataRows(sql, commandType, null, @params);
		}
		public DataRow[] ExecuteDataRows(string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return (from DataRow row in ExecuteDataset(sql, commandType, commandTimeout, @params).Tables[0].Rows select row).ToArray();
		}

		public DataSet ExecuteDataset(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataset(sql, commandType, null, @params);
		}
		public DataSet ExecuteDataset(string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			var ds = new DataSet();
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			using (var da = new SqlDataAdapter(cmd))
			{
				cmd.CommandText = sql;
				cmd.CommandType = commandType;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				da.Fill(ds);
				_connectionWrapper.Connection.Close();
			}
			return ds;
		}

		public void ExecuteNonQuery(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			ExecuteNonQuery(sql, commandType, null, @params);
		}
		public void ExecuteNonQuery(string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = commandType;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				cmd.ExecuteNonQuery();
				_connectionWrapper.Connection.Close();
			}
		}

		public SqlDataReader ExecuteReader(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteReader(sql, commandType, null, @params);
		}
		public SqlDataReader ExecuteReader(string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			SqlDataReader reader;
			//using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = commandType;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			}
			return reader;
		}

		public T ExecuteScalar<T>(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteScalar<T>(sql, commandType, null, @params);
		}
		public T ExecuteScalar<T>(string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			object retVal;
			using (_connectionWrapper)
			using (var cmd = _connectionWrapper.Connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = commandType;
				if (commandTimeout.HasValue)
					cmd.CommandTimeout = commandTimeout.Value;
				cmd.Parameters.AddRange(@params);

				_connectionWrapper.Connection.Open();
				retVal = cmd.ExecuteScalar();
				_connectionWrapper.Connection.Close();
			}
			if (retVal == null || //no result
			    retVal == DBNull.Value) //null result
				return default(T);
			var type = typeof(T);
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				type = Nullable.GetUnderlyingType(type); //converts T? to T for conversion
			return (T)Convert.ChangeType(retVal, type); //converts decimal to int for @@scope_identity
		}

		public static T[] ExecuteArray<T>(string connectionString, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteArray<T>(connectionString, sql, commandType, null, @params);
		}
		public static T[] ExecuteArray<T>(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteArray<T>(connection, sql, commandType, null, @params);
		}
		public static T[] ExecuteArray<T>(string connectionString, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteArray<T>(sql, commandType, commandTimeout, @params);
		}
		public static T[] ExecuteArray<T>(SqlConnection connection, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteArray<T>(sql, commandType, commandTimeout, @params);
		}

		public static DataRow[] ExecuteDataRows(string connectionString, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataRows(connectionString, sql, commandType, null, @params);
		}
		public static DataRow[] ExecuteDataRows(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataRows(connection, sql, commandType, null, @params);
		}
		public static DataRow[] ExecuteDataRows(string connectionString, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteDataRows(sql, commandType, commandTimeout, @params);
		}
		public static DataRow[] ExecuteDataRows(SqlConnection connection, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteDataRows(sql, commandType, commandTimeout, @params);
		}

		public static DataSet ExecuteDataset(string connectionString, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataset(connectionString, sql, commandType, null, @params);
		}
		public static DataSet ExecuteDataset(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteDataset(connection, sql, commandType, null, @params);
		}
		public static DataSet ExecuteDataset(string connectionString, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteDataset(sql, commandType, commandTimeout, @params);
		}
		public static DataSet ExecuteDataset(SqlConnection connection, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteDataset(sql, commandType, commandTimeout, @params);
		}

		public static void ExecuteNonQuery(string connectionString, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			ExecuteNonQuery(connectionString, sql, commandType, null, @params);
		}
		public static void ExecuteNonQuery(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			ExecuteNonQuery(connection, sql, commandType, null, @params);
		}
		public static void ExecuteNonQuery(string connectionString, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			new SqlWrapper(connectionString).ExecuteNonQuery(sql, commandType, commandTimeout, @params);
		}
		public static void ExecuteNonQuery(SqlConnection connection, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			new SqlWrapper(connection).ExecuteNonQuery(sql, commandType, commandTimeout, @params);
		}

		public static SqlDataReader ExecuteReader(string connectionString, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteReader(connectionString, sql, commandType, null, @params);
		}
		public static SqlDataReader ExecuteReader(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteReader(connection, sql, commandType, null, @params);
		}
		public static SqlDataReader ExecuteReader(string connectionString, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteReader(sql, commandType, commandTimeout, @params);
		}
		public static SqlDataReader ExecuteReader(SqlConnection connection, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteReader(sql, commandType, commandTimeout, @params);
		}

		public static T ExecuteScalar<T>(string connectionString, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteScalar<T>(connectionString, sql, commandType, null, @params);
		}
		public static T ExecuteScalar<T>(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] @params)
		{
			return ExecuteScalar<T>(connection, sql, commandType, null, @params);
		}
		public static T ExecuteScalar<T>(string connectionString, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connectionString).ExecuteScalar<T>(sql, commandType, commandTimeout, @params);
		}
		public static T ExecuteScalar<T>(SqlConnection connection, string sql, CommandType commandType, int? commandTimeout, params SqlParameter[] @params)
		{
			return new SqlWrapper(connection).ExecuteScalar<T>(sql, commandType, commandTimeout, @params);
		}
	}
}