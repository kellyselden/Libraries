using System.Collections.Generic;

namespace KellySelden.Libraries.Excel.Domain.Entities
{
	public class Worksheet
	{
		public string Name { get; private set; }
		public int ColumnCount { get; private set; }
		public IEnumerable<Row> Rows { get; private set; }

		public Worksheet(string name, int columnCount, IEnumerable<Row> rows)
		{
			Name = name;
			ColumnCount = columnCount;
			Rows = rows;
		}
	}
}