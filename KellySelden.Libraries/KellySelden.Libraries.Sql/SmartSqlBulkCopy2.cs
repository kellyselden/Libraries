using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace KellySelden.Libraries.Sql
{
	public class SmartSqlBulkCopy2
	{
		readonly SqlConnectionWrapper _connectionWrapper;
		readonly DataTable _tableDefinition;
		readonly List<IDictionary<string, object>> _rows = new List<IDictionary<string, object>>();

		public SmartSqlBulkCopy2(string connectionString, string tableName, string schemaName = null, bool tolerateSqlInjection = false) : this(new SqlConnectionWrapper(connectionString), schemaName, tableName, tolerateSqlInjection) { }
		public SmartSqlBulkCopy2(SqlConnection connection, string tableName, string schemaName = null, bool tolerateSqlInjection = false) : this(new SqlConnectionWrapper(connection), schemaName, tableName, tolerateSqlInjection) { }
		SmartSqlBulkCopy2(SqlConnectionWrapper connectionWrapper, string schemaName, string tableName, bool tolerateSqlInjection)
		{
			schemaName = schemaName ?? "dbo";
			_connectionWrapper = connectionWrapper;
			_tableDefinition = SqlWrapper.ExecuteDataset(_connectionWrapper.Connection,
				tolerateSqlInjection ? SqlInjectRisk : NoSqlInjectRisk,
				CommandType.Text,
				new SqlParameter("schemaName", schemaName),
				new SqlParameter("tableName", tableName)).Tables[0];
			_tableDefinition.TableName = schemaName + '.' + tableName;
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
			_rows[row].Add(column, value);
		}

		void AddEmptyRows(int row)
		{
			while (_rows.Count <= row)
				_rows.Add(new Dictionary<string, object>());
		}

		public void Insert(int? timeout = null, int batchSize = 20000)
		{
			IDictionary<string, object> firstRow = _rows.FirstOrDefault();
			if (firstRow == null) return;
			
			foreach (IDictionary<string, object> row in _rows)
			{
				DataRow r = _tableDefinition.NewRow();
				foreach (KeyValuePair<string, object> column in row)
					if (column.Value != null)
						r[column.Key] = column.Value;
				_tableDefinition.Rows.Add(r);
			}

			using (_connectionWrapper)
			using (var bulkCopy = new SqlBulkCopy(_connectionWrapper.Connection)
			{
				BatchSize = batchSize,
				DestinationTableName = _tableDefinition.TableName
			})
			{
				if (timeout.HasValue)
					bulkCopy.BulkCopyTimeout = timeout.Value;
				foreach (string column in firstRow.Keys)
					bulkCopy.ColumnMappings.Add(column, column);
				_connectionWrapper.Connection.Open();
				bulkCopy.WriteToServer(_tableDefinition);
				_connectionWrapper.Connection.Close();
			}

			_rows.Clear();
			_tableDefinition.Rows.Clear();
		}

		const string SqlInjectRisk = @"
declare @sql nvarchar(max) = 'select top 0 * from ' + @schemaName + '.' + @tableName
execute sp_executesql @sql";
		const string NoSqlInjectRisk = @"
declare @name sysname
declare @type sysname
declare @max_length smallint
declare @precision tinyint
declare @scale tinyint

declare @sql nvarchar(max) = 'create table #t ('

declare cursorName cursor local fast_forward for

select
	c.name,
	ty.name type,
	case when ty.name like 'n%char' and c.max_length <> -1 then c.max_length / 2 else c.max_length end max_length,
	c.precision,
	c.scale
from sys.columns c
join sys.tables t on c.object_id = t.object_id
join sys.schemas sch on t.schema_id = sch.schema_id
join sys.types ty on c.user_type_id = ty.user_type_id
where sch.name = @schemaName
	and t.name = @tableName

open cursorName

fetch next from cursorName into @name, @type, @max_length, @precision, @scale

while @@fetch_status = 0
begin
	set @sql = @sql + @name + ' ' + @type

	if @type in ('decimal', 'numeric')
		set @sql = @sql + '(' + cast(@precision as varchar(3)) + ',' + cast(@scale as varchar(3)) + ')'
	if @type like '%binary' or @type like '%char'
		set @sql = @sql + '(' + case when @max_length = -1 then 'max' else cast(@max_length as varchar(5)) end + ')'
	if @type in ('datetime2', 'datetimeoffset', 'time')
		set @sql = @sql + '(' + cast(@scale as varchar(3)) + ')'
			
	fetch next from cursorName into @name, @type, @max_length, @precision, @scale

	if @@fetch_status = 0
		set @sql = @sql + ', '
end

close cursorName
deallocate cursorName

set @sql = @sql + ') select * from #t'

execute sp_executesql @sql";
	}
}