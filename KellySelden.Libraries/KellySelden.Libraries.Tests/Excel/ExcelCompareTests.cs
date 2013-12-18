using System.Linq;
using KellySelden.Libraries.Excel.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests.Excel
{
	public abstract class ExcelCompareTests
	{
		readonly string _fileName;
		readonly IExcelService[] _excelServices;

		protected ExcelCompareTests(string fileName, params IExcelService[] excelServices)
		{
			_fileName = fileName;
			_excelServices = excelServices;
		}

		public virtual void ExcelService_Compare()
		{
			for (int i = 0; i < _excelServices.Length - 1; i++)
			{
				var worksheets1 = _excelServices[i].ReadWorkbook(_fileName).Worksheets.ToArray();
				var worksheets2 = _excelServices[i + 1].ReadWorkbook(_fileName).Worksheets.ToArray();

				Assert.AreEqual(worksheets1.Length, worksheets2.Length);

				for (int j = 0; j < worksheets1.Length; j++)
				{
					var worksheet1 = worksheets1[j];
					var worksheet2 = worksheets2[j];

					Assert.AreEqual(worksheet1.Name, worksheet2.Name);

					var rows1 = worksheet1.Rows.ToArray();
					var rows2 = worksheet2.Rows.ToArray();

					Assert.AreEqual(rows1.Length, rows2.Length);

					for (int k = 0; k < rows1.Length; k++)
					{
						var cells1 = rows1[k].Cells.ToArray();
						var cells2 = rows2[k].Cells.ToArray();

						Assert.AreEqual(cells1.Length, cells2.Length);

						for (int l = 0; l < cells1.Length; l++)
						{
							var cell1 = cells1[l];
							var cell2 = cells2[l];

							Assert.AreEqual(cell1.RowIndex, cell2.RowIndex);
							Assert.AreEqual(cell1.ColumnIndex, cell2.ColumnIndex);
							Assert.AreEqual(cell1.Value, cell2.Value);
						}
					}
				}
			}
		}
	}
}