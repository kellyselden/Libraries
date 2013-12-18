using KellySelden.Libraries.Excel.Domain.Entities;

namespace KellySelden.Libraries.Excel.Domain.Services
{
	public interface IExcelService
	{
		Workbook ReadWorkbook(string path);
	}
}