using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KellySelden.Libraries.Excel.Domain.Entities;
using KellySelden.Libraries.Excel.Domain.Services;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace KellySelden.Libraries.Excel.Services.NPOI
{
	public class ExcelService : IExcelService
	{
		public Workbook ReadWorkbook(string path)
		{
			if (!File.Exists(path)) return null;

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
				return CreateWorkbook(new HSSFWorkbook(stream));
		}

		Workbook CreateWorkbook(HSSFWorkbook workbook)
		{
			var worksheets = new List<Worksheet>();
			for (int i = 0; i < workbook.NumberOfSheets; i++)
				worksheets.Add(CreateWorksheet(workbook.GetSheetAt(i)));
			return new Workbook(worksheets);
		}

		Worksheet CreateWorksheet(ISheet sheet)
		{
			var rows = new List<IRow>();
			int maxColumns = 0;
			for (int i = 0; i < sheet.PhysicalNumberOfRows; i++)
			{
				IRow row = sheet.GetRow(i);
				maxColumns = Math.Max(maxColumns, row.LastCellNum);
				rows.Add(row);
			}
			return new Worksheet(sheet.SheetName, maxColumns, rows.Select(row => CreateRow(row, maxColumns)));
		}

		Row CreateRow(IRow row, int columns)
		{
			var cells = new List<Cell>();
			ICell[] iCells = row.Cells.ToArray();
			int skipped = 0;
			for (int i = 0; i < columns; i++)
			{
				Cell cell;
				if (i - skipped >= iCells.Length || i != iCells[i - skipped].ColumnIndex)
				{
					cell = new Cell(row.RowNum, i, null);
					skipped++;
				}
				else cell = CreateCell(iCells[i - skipped]);
				cells.Add(cell);
			}
			return new Row(cells);
		}

		Cell CreateCell(ICell cell)
		{
			string value = null;
			switch (cell.CellType)
			{
				case CellType.STRING:
					value = cell.StringCellValue;
					break;
				case CellType.NUMERIC:
					value = cell.NumericCellValue.ToString();
					break;
				case CellType.FORMULA:
					switch (cell.CachedFormulaResultType)
					{
						case CellType.STRING:
							value = cell.StringCellValue;
							break;
						case CellType.NUMERIC:
							//excel trigger is probably out-of-date
							value = (cell.CellFormula == "TODAY()" ? DateTime.Today.ToOADate() : cell.NumericCellValue).ToString();
							break;
					}
					break;
			}
			return new Cell(cell.RowIndex, cell.ColumnIndex, value);
		}
	}
}