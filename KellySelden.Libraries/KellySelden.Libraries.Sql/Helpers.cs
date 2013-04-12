using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace KellySelden.Libraries.Sql
{
	public static class Helpers
	{
		public static SqlDbType ConvertToSqlDbType(Type type)
		{
			SqlDbType sqlDbType;
			TryConvertToSqlDbType(type, out sqlDbType);
			return sqlDbType;
		}

		public static bool TryConvertToSqlDbType(Type type, out SqlDbType sqlDbType)
		{
			var parameter = new SqlParameter();
			TypeConverter converter = TypeDescriptor.GetConverter(parameter.DbType);
			try { parameter.DbType = (DbType)converter.ConvertFrom(type.Name); }
			catch
			{
				sqlDbType = 0;
				return false;
			}
			sqlDbType = parameter.SqlDbType;
			return true;
		}
	}
}