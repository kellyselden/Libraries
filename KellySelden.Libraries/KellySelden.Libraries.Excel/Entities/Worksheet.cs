using System.Collections.Generic;

namespace KellySelden.Libraries.Excel.Entities
{
	public class Worksheet
	{
		public string Name { get; private set; }
		public int Index { get; private set; }
		public int ColumnCount { get; private set; }
		public IEnumerable<Row> Rows { get; private set; }

		public Worksheet(string name, int index, int columnCount, IEnumerable<Row> rows)
		{
			Name = name;
			Index = index;
			ColumnCount = columnCount;
			Rows = rows;
		}
	}
}