using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace KellySelden.Libraries.Sql
{
	public class SmartSqlBulkCopy
	{
		readonly SqlConnectionWrapper _connectionWrapper;
		readonly string _tableName;
		readonly IDictionary<string, Type> _columns = new Dictionary<string, Type>();
		readonly List<IDictionary<string, object>> _rows = new List<IDictionary<string, object>>();

		public SmartSqlBulkCopy(string connectionString, string tableName) : this(new SqlConnectionWrapper(connectionString), tableName) { }
		public SmartSqlBulkCopy(SqlConnection connection, string tableName) : this(new SqlConnectionWrapper(connection), tableName) { }
		SmartSqlBulkCopy(SqlConnectionWrapper connectionWrapper, string tableName)
		{
			_connectionWrapper = connectionWrapper;
			_tableName = tableName;
		}

		public void AddRow(object row)
		{
			int i = _rows.Count;
			AddEmptyRows(i);
			foreach (PropertyInfo propertyInfo in row.GetType().GetProperties())
				AddRowValuePrivate(i, propertyInfo.Name, propertyInfo.GetValue(row, null));
		}
		public void AddRow(IDictionary<string, object> row)
		{
			int i = _rows.Count;
			AddEmptyRows(i);
			foreach (KeyValuePair<string, object> column in row)
				AddRowValuePrivate(i, column.Key, column.Value);
		}

		public void AddRowValue(int row, string column, object value)
		{
			AddEmptyRows(row);
			AddRowValuePrivate(row, column, value);
		}
		void AddRowValuePrivate(int row, string column, object value)
		{
			if (value != null && !_columns.ContainsKey(column))
				_columns.Add(column, value.GetType());
			_rows[row].Add(column, value);
		}

		void AddEmptyRows(int row)
		{
			while (_rows.Count <= row)
				_rows.Add(new Dictionary<string, object>());
		}

		public void Insert(int? timeout = null, int batchSize = 20000)
		{
			if (_rows.Count == 0) return;
			
			var table = new DataTable();
			foreach (KeyValuePair<string, Type> kvp in _columns)
				table.Columns.Add(kvp.Key, kvp.Value);

			foreach (IDictionary<string, object> row in _rows)
			{
				DataRow r = table.NewRow();
				foreach (KeyValuePair<string, object> column in row)
					if (column.Value != null && _columns.ContainsKey(column.Key))
						r[column.Key] = column.Value;
				table.Rows.Add(r);
			}

			using (_connectionWrapper)
			using (var bulkCopy = new SqlBulkCopy(_connectionWrapper.Connection)
			{
				BatchSize = batchSize,
				DestinationTableName = _tableName
			})
			{
				if (timeout.HasValue)
					bulkCopy.BulkCopyTimeout = timeout.Value;
				foreach (string column in _columns.Keys)
					bulkCopy.ColumnMappings.Add(column, column);
				_connectionWrapper.Connection.Open();
				bulkCopy.WriteToServer(table);
				_connectionWrapper.Connection.Close();
			}

			_columns.Clear();
			_rows.Clear();
		}
	}
}