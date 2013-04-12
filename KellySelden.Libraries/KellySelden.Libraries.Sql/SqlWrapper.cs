using System;
using System.Data;
using System.Data.SqlClient;

namespace KellySelden.Libraries.Sql
{
	public class SqlWrapper : IDisposable
	{
		readonly SqlConnection _connection;

		public SqlWrapper(string connectionString)
		{
			_connection = new SqlConnection(connectionString);
		}

		public DataSet ExecuteDataset(string sql)
		{
			var ds = new DataSet();
			using (var da = new SqlDataAdapter(sql, _connection))
			{
				_connection.Open();
				da.Fill(ds);
				_connection.Close();
			}
			return ds;
		}

		public void Dispose()
		{
			_connection.Dispose();
		}

		public static DataSet ExecuteDataset(string connectionString, string sql)
		{
			using (var wrapper = new SqlWrapper(connectionString))
			{
				return wrapper.ExecuteDataset(sql);
			}
		}
	}
}