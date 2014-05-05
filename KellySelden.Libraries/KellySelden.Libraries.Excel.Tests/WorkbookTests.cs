using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using KellySelden.Libraries.Excel.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Excel.Tests
{
	[TestClass]
	public class WorkbookTests
	{
		readonly DataSet _dataSet;

		public WorkbookTests()
		{
			_dataSet = CreateDataSet();
		}

		[TestMethod]
		public void Workbook_FromDataSet_WithoutHeader()
		{
			CompareDataSetAndWorkbook(false);
		}

		[TestMethod]
		public void Workbook_FromDataSet_WithHeader()
		{
			CompareDataSetAndWorkbook(true);
		}

		void CompareDataSetAndWorkbook(bool header)
		{
			int modifier = header ? 1 : 0;
			var workbook = Workbook.FromDataSet(_dataSet.Copy(), new CultureInfo("en-US"), header);
			Assert.AreEqual(_dataSet.Tables.Count, workbook.Worksheets.Count());
			foreach (Worksheet worksheet in workbook.Worksheets)
			{
				var dataTable = _dataSet.Tables[worksheet.Index];
				Assert.AreEqual(dataTable.TableName, worksheet.Name);
				Assert.AreEqual(dataTable.Columns.Count, worksheet.ColumnCount);
				Assert.AreEqual(dataTable.Rows.Count + modifier, worksheet.Rows.Count());
				if (header)
					foreach (Cell cell in worksheet.Rows.First().Cells)
						Assert.AreEqual(dataTable.Columns[cell.ColumnIndex].ColumnName, cell.Value);
				foreach (Row row in worksheet.Rows.Skip(modifier))
				{
					var dataRow = dataTable.Rows[row.Index - modifier];
					Assert.AreEqual(dataRow.ItemArray.Length, row.Cells.Count());
					foreach (Cell cell in row.Cells)
						Assert.AreEqual(dataRow[cell.ColumnIndex], cell.Value);
				}
			}
		}

		static DataSet CreateDataSet()
		{
			var dataSet = new DataSet();
			for (int i = 0; i < 5; i++)
			{
				var dataTable = dataSet.Tables.Add(Guid.NewGuid().ToString().Substring(0, 31));
				for (int j = 0; j < 4; j++)
					dataTable.Columns.Add(Guid.NewGuid().ToString());
				for (int j = 0; j < 6; j++)
				{
					var cells = new List<object>();
					for (int k = 0; k < dataTable.Columns.Count; k++)
						cells.Add(Guid.NewGuid());
					dataTable.Rows.Add(cells.ToArray());
				}
			}
			return dataSet;
		}
	}
}