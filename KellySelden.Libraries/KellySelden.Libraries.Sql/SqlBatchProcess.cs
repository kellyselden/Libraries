using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KellySelden.Libraries.Extensions;

namespace KellySelden.Libraries.Sql
{
	public class SqlBatchProcess : IDisposable
	{
		readonly SqlConnectionWrapper _connectionWrapper;
		readonly string _tableName;
		readonly SqlDataAdapter _sqlDataAdapter;
		readonly List<IDictionary<string, object>> _insertRows = new List<IDictionary<string, object>>();
		readonly List<KeyValuePair<IDictionary<string, object>, IDictionary<string, object>>> _updateRows = new List<KeyValuePair<IDictionary<string, object>, IDictionary<string, object>>>();
		readonly List<IDictionary<string, object>> _deleteRows = new List<IDictionary<string, object>>();
		
		/// <param name="tableName">don't ever accept user input as the table name (SQL injection)</param>
		public SqlBatchProcess(string connectionString, string tableName) : this(new SqlConnectionWrapper(connectionString), tableName) { }
		/// <param name="tableName">don't ever accept user input as the table name (SQL injection)</param>
		public SqlBatchProcess(SqlConnection connection, string tableName) : this(new SqlConnectionWrapper(connection), tableName) { }
		SqlBatchProcess(SqlConnectionWrapper connectionWrapper, string tableName)
		{
			_connectionWrapper = connectionWrapper;
			_tableName = tableName;
			_sqlDataAdapter = new SqlDataAdapter();
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
		public void AddRowToDelete(IDictionary<string, object> keys)
		{
			int row = _deleteRows.Count;
			AddEmptyRowsToDelete(row);
			AddRow(_deleteRows[row], keys);
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
			AddColumn(_deleteRows[row], column, value);
		}
		public void AddKeyForDelete(int row, string column, object value)
		{
			AddEmptyRowsToDelete(row);
			AddColumn(_deleteRows[row], column, value);
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
		void AddEmptyRowsToDelete(int row)
		{
			while (_deleteRows.Count <= row)
				_deleteRows.Add(new Dictionary<string, object>());
		}

		public void Process(int? timeout = null, int batchSize = 20000)
		{
			IDictionary<string, object> insertRow = _insertRows.FirstOrDefault();
			KeyValuePair<IDictionary<string, object>, IDictionary<string, object>> updateRow = _updateRows.FirstOrDefault();
			IDictionary<string, object> deleteRow = _deleteRows.FirstOrDefault();
			if (insertRow == null && updateRow.Key == null && deleteRow == null)
				return;

			var table = new DataTable(_tableName);
			table.Columns.AddRange(GetColumnsForUnion(insertRow)
				.Union(GetColumnsForUnion(updateRow.Key)
				.Union(GetColumnsForUnion(updateRow.Value))
				.Union(GetColumnsForUnion(deleteRow)))
				.Select(c => new DataColumn(c)).ToArray());

			if (insertRow != null)
			{
				AddRows(table, _insertRows);
				if (_sqlDataAdapter.InsertCommand == null)
				{
					_sqlDataAdapter.InsertCommand = PrepareCommand(
						string.Format("INSERT INTO {0} ({1}) VALUES ({2})", _tableName,
							string.Join(", ", insertRow.Keys),
							string.Join(", ", insertRow.Keys.Select(k => "@" + k))),
						timeout, insertRow);
				}
			}

			if (updateRow.Key != null)
			{
				AddRows(table, _updateRows.Select(r => r.Key.Union(r.Value)), row =>
				{
					row.AcceptChanges();
					row.SetModified();
				});
				if (_sqlDataAdapter.UpdateCommand == null)
				{
					_sqlDataAdapter.UpdateCommand = PrepareCommand(
						string.Format("UPDATE {0} SET {1} WHERE {2}", _tableName,
							string.Join(", ", updateRow.Value.Keys.Select(k => k + " = @" + k)),
							string.Join(", ", updateRow.Key.Keys.Select(k => k + " = @" + k))),
						timeout, updateRow.Key.Union(updateRow.Value));
				}
			}

			if (deleteRow != null)
			{
				AddRows(table, _deleteRows, row =>
				{
					row.AcceptChanges();
					row.Delete();
				});
				if (_sqlDataAdapter.DeleteCommand == null)
				{
					_sqlDataAdapter.DeleteCommand = PrepareCommand(
						string.Format("DELETE FROM {0} WHERE {1}", _tableName,
							string.Join(", ", deleteRow.Keys.Select(k => k + " = @" + k))),
						timeout, deleteRow);
				}
			}

			_sqlDataAdapter.UpdateBatchSize = batchSize;
			_sqlDataAdapter.Update(table);

			_insertRows.Clear();
			_updateRows.Clear();
			_deleteRows.Clear();
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
			var command = new SqlCommand(sql, _connectionWrapper.Connection) { UpdatedRowSource = UpdateRowSource.None };
			if (timeout.HasValue)
				command.CommandTimeout = timeout.Value;
			foreach (var column in row)
				command.Parameters.Add('@' + column.Key, Helpers.ConvertToSqlDbType(column.Value.GetType())).SourceColumn = column.Key;
			return command;
		}

		public void Dispose()
		{
			if (_sqlDataAdapter.InsertCommand != null)
				_sqlDataAdapter.InsertCommand.Dispose();
			if (_sqlDataAdapter.UpdateCommand != null)
				_sqlDataAdapter.UpdateCommand.Dispose();
			if (_sqlDataAdapter.DeleteCommand != null)
				_sqlDataAdapter.DeleteCommand.Dispose();
			_connectionWrapper.Dispose();
		}
	}
}