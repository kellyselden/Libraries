using System.Collections.Generic;

namespace KellySelden.Libraries.Excel.Domain.Entities
{
	public class Workbook
	{
		public IEnumerable<Worksheet> Worksheets { get; set; }

		public Workbook(IEnumerable<Worksheet> worksheets)
		{
			Worksheets = worksheets;
		}
	}
}