﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace KellySelden.Libraries.Sql
{
	public class SmartSqlBulkCopy
	{
		readonly SqlConnection _connection;
		readonly DataTable _tableDefinition;
		readonly List<IDictionary<string, object>> _rows = new List<IDictionary<string, object>>();

		/// <param name="tableName">don't ever accept user input as the table name (SQL injection)</param>
		public SmartSqlBulkCopy(string connectionString, string tableName) : this(new SqlConnection(connectionString), tableName) { }
		/// <param name="tableName">don't ever accept user input as the table name (SQL injection)</param>
		public SmartSqlBulkCopy(SqlConnection connection, string tableName)
		{
			_connection = connection;

			_tableDefinition = SqlWrapper.ExecuteDataset(_connection.ConnectionString, "SELECT TOP 0 * FROM " + tableName).Tables[0];
			_tableDefinition.TableName = tableName;
		}

		public void AddRow(object row)
		{
			int i = _rows.Count;
			AddEmptyRows(i);
			foreach (PropertyInfo propertyInfo in row.GetType().GetProperties())
				AddColumnPrivate(i, propertyInfo.Name, propertyInfo.GetValue(row, null));
		}
		public void AddRow(IDictionary<string, object> row)
		{
			int i = _rows.Count;
			AddEmptyRows(i);
			foreach (KeyValuePair<string, object> column in row)
				AddColumnPrivate(i, column.Key, column.Value);
		}

		public void AddColumn(int row, string column, object value)
		{
			AddEmptyRows(row);
			AddColumnPrivate(row, column, value);
		}
		void AddColumnPrivate(int row, string column, object value)
		{
			_rows[row].Add(column, value);
		}

		void AddEmptyRows(int row)
		{
			while (_rows.Count <= row)
				_rows.Add(new Dictionary<string, object>());
		}

		public void Insert(int? timeout = null, int batchSize = 20000)
		{
			foreach (IDictionary<string, object> row in _rows)
			{
				var values = new object[_tableDefinition.Columns.Count];
				foreach (KeyValuePair<string, object> column in row)
				{
					values[_tableDefinition.Columns.IndexOf(column.Key)] = column.Value;
				}
				_tableDefinition.Rows.Add(values);
			}

			using (var bulkCopy = new SqlBulkCopy(_connection)
			{
				BatchSize = batchSize,
				DestinationTableName = _tableDefinition.TableName
			})
			{
				if (timeout.HasValue)
				{
					bulkCopy.BulkCopyTimeout = timeout.Value;
				}
				_connection.Open();
				bulkCopy.WriteToServer(_tableDefinition);
				_connection.Close();
			}

			_rows.Clear();
			_tableDefinition.Rows.Clear();
		}
	}
}