using System;
using System.Linq;
using KellySelden.Libraries.Excel.Domain.Entities;
using KellySelden.Libraries.Excel.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests.Excel
{
	public abstract class ExcelTests
	{
		readonly string _fileName;

		protected readonly IExcelService ExcelService;

		protected ExcelTests(string fileName, IExcelService excelService)
		{
			_fileName = fileName;
			ExcelService = excelService;
		}

		protected Workbook Workbook;

		public virtual void TestInitialize()
		{
			Workbook = ExcelService.ReadWorkbook(_fileName);
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
					var cells = rows[i].Cells.ToArray();

					for (int j = 0; j < cells.Length; j++)
					{
						var cell = cells[j];

						Assert.AreEqual(i, cell.RowIndex);
						Assert.AreEqual(j, cell.ColumnIndex);
					}

					Assert.AreEqual(worksheet.ColumnCount, cells.Length);
				}
			}
		}
	}
}