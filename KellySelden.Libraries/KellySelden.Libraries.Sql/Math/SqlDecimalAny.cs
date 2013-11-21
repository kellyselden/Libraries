using System.Data.SqlTypes;

namespace KellySelden.Libraries.Sql.Math
{
	public sealed class SqlDecimalAny : SqlDecimalBase
	{
		internal SqlDecimalAny(SqlDecimal d) : base(d) { }

		public static implicit operator SqlDecimalAny(decimal d) { return new SqlDecimalAny(d); }
	}
}