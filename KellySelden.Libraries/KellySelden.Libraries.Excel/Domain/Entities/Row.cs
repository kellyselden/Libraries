using System.Collections.Generic;

namespace KellySelden.Libraries.Excel.Domain.Entities
{
	public class Row
	{
		public IEnumerable<Cell> Cells { get; private set; }

		public Row(IEnumerable<Cell> cells)
		{
			Cells = cells;
		}
	}
}