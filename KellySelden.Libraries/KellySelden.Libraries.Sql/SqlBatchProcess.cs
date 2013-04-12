using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KellySelden.Libraries.Extensions;

namespace KellySelden.Libraries.Sql
{
	public class SqlBatchProcess
	{
		readonly SqlConnection _connection;
		readonly string _tableName;
		readonly List<IDictionary<string, object>> _insertRows = new List<IDictionary<string, object>>();
		readonly List<KeyValuePair<IDictionary<string, object>, IDictionary<string, object>>> _updateRows = new List<KeyValuePair<IDictionary<string, object>, IDictionary<string, object>>>();
		
		/// <param name="tableName">don't ever accept user input as the table name (SQL injection)</param>
		public SqlBatchProcess(string connectionString, string tableName) : this(new SqlConnection(connectionString), tableName) { }
		/// <param name="tableName">don't ever accept user input as the table name (SQL injection)</param>
		public SqlBatchProcess(SqlConnection connection, string tableName)
		{
			_connection = connection;
			_tableName = tableName;
		}

		public void AddRowToInsert(IDictionary<string, object> columns)
		{
			int row = _insertRows.Count;
			AddEmptyRowsToInsert(row);
			AddRow(_insertRows[row], columns);
		}
		public void AddRowToUpdate(IDictionary<string, object> keys, IDictionary<string, object> columns)
		{
			int row = _updateRows.Count;
			AddEmptyRowsToUpdate(row);
			AddRow(_updateRows[row].Key, keys);
			AddRow(_updateRows[row].Value, columns);
		}
		void AddRow(IDictionary<string, object> row, IEnumerable<KeyValuePair<string, object>> columns)
		{
			foreach (KeyValuePair<string, object> column in columns)
				AddColumn(row, column.Key, column.Value);
		}

		public void AddColumnToInsert(int row, string column, object value)
		{
			AddEmptyRowsToInsert(row);
			AddColumn(_insertRows[row], column, value);
		}
		public void AddKeyForUpdate(int row, string column, object value)
		{
			AddEmptyRowsToUpdate(row);
			AddColumn(_updateRows[row].Key, column, value);
		}
		public void AddColumnToUpdate(int row, string column, object value)
		{
			AddEmptyRowsToUpdate(row);
			AddColumn(_updateRows[row].Value, column, value);
		}
		void AddColumn(IDictionary<string, object> row, string column, object value)
		{
			row.Add(column, value);
		}

		void AddEmptyRowsToInsert(int row)
		{
			while (_insertRows.Count <= row)
				_insertRows.Add(new Dictionary<string, object>());
		}
		void AddEmptyRowsToUpdate(int row)
		{
			while (_updateRows.Count <= row)
				_updateRows.Add(new KeyValuePair<IDictionary<string, object>, IDictionary<string, object>>(new Dictionary<string, object>(), new Dictionary<string, object>()));
		}

		public void Process(int? timeout = null, int batchSize = 20000)
		{
			IDictionary<string, object> insertRow = _insertRows.FirstOrDefault();
			KeyValuePair<IDictionary<string, object>, IDictionary<string, object>> updateRow = _updateRows.FirstOrDefault();
			if (insertRow == null && updateRow.Key == null)
				return;

			var table = new DataTable(_tableName);
			table.Columns.AddRange(GetColumnsForUnion(insertRow)
				.Union(GetColumnsForUnion(updateRow.Key)
				.Union(GetColumnsForUnion(updateRow.Value)))
				.Select(c => new DataColumn(c)).ToArray());
			using (var sqlDataAdapter = new SqlDataAdapter { UpdateBatchSize = batchSize })
			{
				if (insertRow != null)
				{
					AddRows(table, _insertRows);
					sqlDataAdapter.InsertCommand = PrepareCommand(
						string.Format("INSERT INTO {0} ({1}) VALUES ({2})", _tableName,
						string.Join(", ", insertRow.Keys),
						string.Join(", ", insertRow.Keys.Select(k => "@" + k))),
						timeout, insertRow);
				}
				if (updateRow.Key != null)
				{
					AddRows(table, _updateRows.Select(r => r.Key.Union(r.Value)), row =>
					{
						row.AcceptChanges();
						row.SetModified();
					});
					sqlDataAdapter.UpdateCommand = PrepareCommand(
						string.Format("UPDATE {0} SET {1} WHERE {2}", _tableName,
						string.Join(", ", updateRow.Value.Keys.Select(k => k + " = @" + k)),
						string.Join(", ", updateRow.Key.Keys.Select(k => k + " = @" + k))),
						timeout, updateRow.Key.Union(updateRow.Value));
				}
				sqlDataAdapter.Update(table);
			}
		}

		IEnumerable<string> GetColumnsForUnion(IDictionary<string, object> columns)
		{
			return (columns ?? new Dictionary<string, object>()).Select(c => c.Key);
		}

		void AddRows(DataTable table, IEnumerable<IDictionary<string, object>> rows, Action<DataRow> rowAction = null)
		{
			foreach (IDictionary<string, object> columns in rows)
			{
				DataRow row = table.NewRow();
				foreach (KeyValuePair<string, object> kvp in columns)
					row[kvp.Key] = kvp.Value;
				table.Rows.Add(row);
				if (rowAction != null) rowAction(row);
			}
		}

		SqlCommand PrepareCommand(string sql, int? timeout, IEnumerable<KeyValuePair<string, object>> row)
		{
			var command = new SqlCommand(sql, _connection) { UpdatedRowSource = UpdateRowSource.None };
			if (timeout.HasValue)
				command.CommandTimeout = timeout.Value;
			foreach (var column in row)
				command.Parameters.Add('@' + column.Key, Helpers.ConvertToSqlDbType(column.Value.GetType())).SourceColumn = column.Key;
			return command;
		}
	}
}