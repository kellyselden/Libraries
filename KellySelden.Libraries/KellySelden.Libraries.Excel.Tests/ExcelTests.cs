using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using KellySelden.Libraries.Excel.Entities;
using KellySelden.Libraries.Excel.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Excel.Tests
{
	public abstract class ExcelTests
	{
		protected readonly IExcelService ExcelService;

		protected Workbook Workbook;

		readonly string _fileName, _newFileName = Guid.NewGuid() + ".xls";

		protected ExcelTests(IExcelService excelService, string fileName)
		{
			ExcelService = excelService;
			_fileName = fileName;
		}

		public virtual void TestInitialize()
		{
			Workbook = ExcelService.ReadWorkbook(_fileName);
		}

		public virtual void TestCleanup()
		{
			File.Delete(_newFileName);
		}

		public virtual void ExcelService_OpenWorkbook_FileNotFound_ReturnsNull()
		{
			Assert.IsNull(ExcelService.ReadWorkbook(Guid.NewGuid().ToString()));
		}

		public virtual void ExcelService_CheckGrid()
		{
			foreach (var worksheet in Workbook.Worksheets)
			{
				var rows = worksheet.Rows.ToArray();

				for (int i = 0; i < rows.Length; i++)
				{
					var row = rows[i];

					Assert.AreEqual(i, row.Index);

					var cells = rows[i].Cells.ToArray();

					Assert.AreEqual(worksheet.ColumnCount, cells.Length);

					for (int j = 0; j < cells.Length; j++)
					{
						var cell = cells[j];
						if (cell == null) continue;

						Assert.AreEqual(i, cell.RowIndex);
						Assert.AreEqual(j, cell.ColumnIndex);
					}
				}
			}
		}

		public virtual void ExcelService_UsesCulture()
		{
			ExcelService.Format = new CultureInfo("en-US");
			var before = ExcelService.ReadWorkbook(_fileName);
			ExcelService.Format = new CultureInfo("de-DE");
			var after = ExcelService.ReadWorkbook(_fileName);
			Assert.IsFalse(ExcelCompare.Compare(before, after));
		}

		public virtual void ExcelService_IgnoresThreadCulture()
		{
			ExcelService.Format = new CultureInfo("en-US");
			var before = ExcelService.ReadWorkbook(_fileName);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			var after = ExcelService.ReadWorkbook(_fileName);
			Assert.IsTrue(ExcelCompare.Compare(before, after));
		}

		public virtual void ExcelService_WriteWorkbook_Xls()
		{
			ExcelService.WriteWorkbook(Workbook, ExcelVersion.Xls, _newFileName);
			var newWorkbook = ExcelService.ReadWorkbook(_newFileName);
			Assert.IsTrue(ExcelCompare.Compare(Workbook, newWorkbook));
		}

		public virtual void ExcelService_WriteWorkbook_Xlsx()
		{
			ExcelService.WriteWorkbook(Workbook, ExcelVersion.Xlsx, _newFileName);
			var newWorkbook = ExcelService.ReadWorkbook(_newFileName);
			Assert.IsTrue(ExcelCompare.Compare(Workbook, newWorkbook));
		}
	}
}