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
		public const int DefaultBatchSize = 20000;

		readonly SqlConnectionWrapper _connectionWrapper;
		readonly string _tableName;
		readonly SqlDataAdapter _sqlDataAdapter;

		readonly List<KeyValuePair<KeyValuePair<string, Type>?, IDictionary<string, object>>> _insertRows = new List<KeyValuePair<KeyValuePair<string, Type>?, IDictionary<string, object>>>();
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

		public void AddRowToInsert(IDictionary<string, object> columns, KeyValuePair<string, Type>? output = null)
		{
			int row = _insertRows.Count;
			AddEmptyRowsToInsert(row, output);
			AddRow(_insertRows[row].Value, columns);
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
				row.Add(column.Key, column.Value);
		}

		void AddEmptyRowsToInsert(int row, KeyValuePair<string, Type>? output)
		{
			while (_insertRows.Count <= row)
				_insertRows.Add(new KeyValuePair<KeyValuePair<string, Type>?, IDictionary<string, object>>(output, new Dictionary<string, object>()));
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

		public IEnumerable<object> Process(int? timeout = null, int batchSize = DefaultBatchSize)
		{
			KeyValuePair<KeyValuePair<string, Type>?, IDictionary<string, object>> insertRow = _insertRows.FirstOrDefault();
			KeyValuePair<IDictionary<string, object>, IDictionary<string, object>> updateRow = _updateRows.FirstOrDefault();
			IDictionary<string, object> deleteRow = _deleteRows.FirstOrDefault();
			if (insertRow.Value == null && updateRow.Value == null && deleteRow == null)
				return new object[0];

			var table = new DataTable(_tableName);
			table.Columns.AddRange((insertRow.Key == null ? new string[0] : new[] { insertRow.Key.Value.Key })
				.Union(GetColumnsForUnion(insertRow.Value)
				.Union(GetColumnsForUnion(updateRow.Key)
				.Union(GetColumnsForUnion(updateRow.Value)
				.Union(GetColumnsForUnion(deleteRow)))))
				.Select(c => new DataColumn(c)).ToArray());

			var insertedRows = new List<DataRow>();
			if (insertRow.Value != null)
			{
				AddRows(table, _insertRows.Select(r => r.Value), insertedRows.Add);
				if (_sqlDataAdapter.InsertCommand == null)
				{
					_sqlDataAdapter.InsertCommand = PrepareCommand(
						string.Format("INSERT INTO {0} ({1}) VALUES ({2}){3}", _tableName,
							string.Join(", ", insertRow.Value.Keys),
							string.Join(", ", insertRow.Value.Keys.Select(k => '@' + k)),
							insertRow.Key != null ? string.Format(" SET @{0} = SCOPE_IDENTITY()", insertRow.Key.Value.Key) : ""),
						timeout, insertRow.Value, insertRow.Key);
				}
			}

			if (updateRow.Value != null)
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

			return insertRow.Key == null ? new object[0] : insertedRows.Select(r => Convert.ChangeType(r[insertRow.Key.Value.Key], insertRow.Key.Value.Value));
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

		SqlCommand PrepareCommand(string sql, int? timeout, IEnumerable<KeyValuePair<string, object>> row, KeyValuePair<string, Type>? output = null)
		{
			var command = new SqlCommand(sql, _connectionWrapper.Connection);
			if (timeout.HasValue)
				command.CommandTimeout = timeout.Value;
			foreach (var column in row)
				command.Parameters.Add('@' + column.Key, Helpers.ConvertToSqlDbType(column.Value.GetType())).SourceColumn = column.Key;
			if (output == null)
				command.UpdatedRowSource = UpdateRowSource.None;
			else
			{
				var outputParam = command.Parameters.Add('@' + output.Value.Key, Helpers.ConvertToSqlDbType(output.Value.Value));
				outputParam.SourceColumn = output.Value.Key;
				outputParam.Direction = ParameterDirection.Output;
				command.UpdatedRowSource = UpdateRowSource.OutputParameters;
			}
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