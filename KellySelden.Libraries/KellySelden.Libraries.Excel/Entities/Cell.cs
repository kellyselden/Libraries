using System.Diagnostics;

namespace KellySelden.Libraries.Excel.Entities
{
	[DebuggerDisplay("{Value}")]
	public class Cell
	{
		public int RowIndex { get; private set; }
		public int ColumnIndex { get; private set; }
		public string Value { get; private set; }

		public Cell(int rowIndex, int columnIndex, string value)
		{
			RowIndex = rowIndex;
			ColumnIndex = columnIndex;
			Value = value;
		}
	}
}