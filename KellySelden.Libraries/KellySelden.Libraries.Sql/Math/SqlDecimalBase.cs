using System.Data.SqlTypes;
using System.Diagnostics;

namespace KellySelden.Libraries.Sql.Math
{
	[DebuggerDisplay("{_d}")]
	public abstract class SqlDecimalBase
	{
		readonly SqlDecimal _d;

		public decimal? Value { get { return _d.IsNull ? (decimal?)null : _d.Value; } }

		protected SqlDecimalBase(SqlDecimal d)
		{
			_d = d;
		}

		protected SqlDecimalBase(SqlDecimalAny d, byte precision, byte scale)
		{
			if (d != null)
			{
				_d = SqlDecimal.ConvertToPrecScale(d._d, precision, scale);
			}
		}

		internal static readonly SqlDecimalAny Null = new SqlDecimalAny(SqlDecimal.Null);

		public override bool Equals(object obj)
		{
			return obj as SqlDecimalBase == this;
		}

		public override int GetHashCode()
		{
			return _d.GetHashCode();
		}

		static SqlDecimal Get(SqlDecimalBase d)
		{
			return d == null ? new SqlDecimal() : d._d;
		}

		public static SqlDecimalAny operator *(SqlDecimalBase d1, SqlDecimalBase d2) { return new SqlDecimalAny(Get(d1) * Get(d2)); }
		public static SqlDecimalAny operator /(SqlDecimalBase d1, SqlDecimalBase d2) { return new SqlDecimalAny(Get(d1) / Get(d2)); }
		public static SqlDecimalAny operator +(SqlDecimalBase d1, SqlDecimalBase d2) { return new SqlDecimalAny(Get(d1) + Get(d2)); }
		public static SqlDecimalAny operator -(SqlDecimalBase d1, SqlDecimalBase d2) { return new SqlDecimalAny(Get(d1) - Get(d2)); }
		public static SqlDecimalAny operator *(SqlDecimalBase d1, decimal d2) { return d1 * new SqlDecimalAny(d2); }
		public static SqlDecimalAny operator /(SqlDecimalBase d1, decimal d2) { return d1 / new SqlDecimalAny(d2); }
		public static SqlDecimalAny operator +(SqlDecimalBase d1, decimal d2) { return d1 + new SqlDecimalAny(d2); }
		public static SqlDecimalAny operator -(SqlDecimalBase d1, decimal d2) { return d1 - new SqlDecimalAny(d2); }

		public static bool IsNull(SqlDecimalBase d)
		{
			return ReferenceEquals(d, null) || d._d.IsNull;
		}

		public static bool operator ==(SqlDecimalBase d1, SqlDecimalBase d2)
		{
			if (ReferenceEquals(d1, d2))
			{
				return true;
			}
			if (IsNull(d1) || IsNull(d2))
			{
				return false;
			}
			return (d1._d == d2._d).Value;
		}

		public static bool operator !=(SqlDecimalBase d1, SqlDecimalBase d2) { return !(d1 == d2); }

		public static bool operator >(SqlDecimalBase d1, SqlDecimalBase d2)
		{
			if (IsNull(d1) || IsNull(d2))
			{
				return false;
			}
			return (d1._d > d2._d).Value;
		}

		public static bool operator <(SqlDecimalBase d1, SqlDecimalBase d2)
		{
			if (IsNull(d1) || IsNull(d2))
			{
				return false;
			}
			return (d1._d < d2._d).Value;
		}

		public static bool operator >=(SqlDecimalBase d1, SqlDecimalBase d2) { return d1 > d2 || d1 == d2; }
		public static bool operator <=(SqlDecimalBase d1, SqlDecimalBase d2) { return d1 < d2 || d1 == d2; }
	}
}