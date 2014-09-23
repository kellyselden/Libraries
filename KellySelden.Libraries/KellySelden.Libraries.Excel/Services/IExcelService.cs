using System;
using System.IO;
using KellySelden.Libraries.Excel.Entities;

namespace KellySelden.Libraries.Excel.Services
{
	public interface IExcelService
	{
		IFormatProvider Format { get; set; }
		Workbook ReadWorkbook(string path);
		Workbook ReadWorkbook(Stream stream);
		void WriteWorkbook(Workbook workbook, ExcelVersion version, string path);
		void WriteWorkbook(Workbook workbook, ExcelVersion version, Stream stream);
	}
}