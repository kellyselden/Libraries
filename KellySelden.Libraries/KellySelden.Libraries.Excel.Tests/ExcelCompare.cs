using System.Linq;
using KellySelden.Libraries.Excel.Entities;
using KellySelden.Libraries.Excel.Services;

namespace KellySelden.Libraries.Excel.Tests
{
	public static class ExcelCompare
	{
		public static bool Compare(string fileName, params IExcelService[] excelServices)
		{
			return Compare(excelServices.Select(s => s.ReadWorkbook(fileName)).ToArray());
		}
		public static bool Compare(params Workbook[] workbooks)
		{
			for (int i = 0; i < workbooks.Length - 1; i++)
			{
				var worksheets1 = workbooks[i].Worksheets.ToArray();
				var worksheets2 = workbooks[i + 1].Worksheets.ToArray();

				if (worksheets1.Length != worksheets2.Length)
					return false;

				for (int j = 0; j < worksheets1.Length; j++)
				{
					var worksheet1 = worksheets1[j];
					var worksheet2 = worksheets2[j];

					if (worksheet1.Name != worksheet2.Name)
						return false;

					var rows1 = worksheet1.Rows.ToArray();
					var rows2 = worksheet2.Rows.ToArray();

					if (rows1.Length != rows2.Length)
						return false;

					for (int k = 0; k < rows1.Length; k++)
					{
						var row1 = rows1[k];
						var row2 = rows2[k];

						if (row1.Index != row2.Index)
							return false;

						var cells1 = row1.Cells.ToArray();
						var cells2 = row2.Cells.ToArray();

						if (cells1.Length != cells2.Length)
							return false;

						for (int l = 0; l < cells1.Length; l++)
						{
							var cell1 = cells1[l];
							var cell2 = cells2[l];

							if (cell1 == null && cell2 == null)
								continue;
							if (cell1 == null || cell2 == null)
								return false;
							if (cell1.RowIndex != cell2.RowIndex)
								return false;
							if (cell1.ColumnIndex != cell2.ColumnIndex)
								return false;
							if (cell1.Value != cell2.Value)
								return false;
						}
					}
				}
			}
			return true;
		}
	}
}