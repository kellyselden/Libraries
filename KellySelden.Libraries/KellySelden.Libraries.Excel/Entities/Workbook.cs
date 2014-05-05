using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace KellySelden.Libraries.Excel.Entities
{
	public class Workbook
	{
		public IEnumerable<Worksheet> Worksheets { get; private set; }

		public Workbook(IEnumerable<Worksheet> worksheets)
		{
			Worksheets = worksheets;
		}

		public DataSet AsDataSet(bool isFirstRowHeader)
		{
			var dataSet = new DataSet();
			foreach (Worksheet worksheet in Worksheets)
			{
				var dataTable = dataSet.Tables.Add(worksheet.Name);
				if (isFirstRowHeader)
				{
					Row header = worksheet.Rows.FirstOrDefault();
					if (header != null)
						dataTable.Columns.AddRange(header.Cells.Select(cell => new DataColumn(cell.Value)).ToArray());
				}
				else
					for (int i = 0; i < worksheet.ColumnCount; i++)
						dataTable.Columns.Add();
				foreach (Row row in worksheet.Rows.Skip(isFirstRowHeader ? 1 : 0))
					dataTable.Rows.Add(row.Cells.Select(cell => cell == null ? DBNull.Value : (object)cell.Value).ToArray());
			}
			return dataSet;
		}

		public static Workbook FromDataSet(DataSet dataSet, IFormatProvider format, bool columnsAsFirstRow)
		{
			var worksheets = new List<Worksheet>();
			foreach (DataTable dataTable in dataSet.Tables)
			{
				var rows = new List<Row>();
				if (columnsAsFirstRow)
				{
					var cells = new List<Cell>();
					foreach (DataColumn dataColumn in dataTable.Columns)
						cells.Add(new Cell(0, cells.Count, dataColumn.ColumnName));
					rows.Add(new Row(0, cells));
				}
				foreach (DataRow dataRow in dataTable.Rows)
				{
					var cells = new List<Cell>();
					foreach (object value in dataRow.ItemArray)
					{
						if (value == DBNull.Value)
							cells.Add(null);
						else
						{
							var formattable = value as IFormattable;
							cells.Add(new Cell(rows.Count, cells.Count,
								formattable == null ? value.ToString() : formattable.ToString(null, format)));
						}
					}
					rows.Add(new Row(rows.Count, cells));
				}
				worksheets.Add(new Worksheet(dataTable.TableName, worksheets.Count, dataTable.Columns.Count, rows));
			}
			return new Workbook(worksheets);
		}
	}
}