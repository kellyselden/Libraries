using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using KellySelden.Libraries.Sql;

namespace KellySelden.Libraries.EntityFramework
{
	public class EntityFrameworkBulkCopy<T> : SmartSqlBulkCopy where T : class
	{
		const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

		readonly Type _entityType;
		readonly object _properties;
		PropertyInfo _columnNameProperty;
		readonly MethodInfo _processPropertiesMethod;
		
		public EntityFrameworkBulkCopy(DbContext context, EntityTypeConfiguration<T> map)
			: base(context.Database.Connection.ConnectionString, GetTableName(map))
		{
			_entityType = typeof(T);
			Type configurationType;
			object configuration = GetConfigurationAndType(map, out configurationType);
			_properties = configurationType.GetProperty("PrimitivePropertyConfigurations", Flags).GetValue(configuration, null);
			_processPropertiesMethod = typeof(EntityFrameworkBulkCopy<T>).GetMethod("ProcessProperties", Flags)
				.MakeGenericMethod(_properties.GetType().GetInterfaces().Single(i =>
					i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments());
		}

		static string GetTableName(EntityTypeConfiguration<T> map)
		{
			Type configurationType;
			object configuration = GetConfigurationAndType(map, out configurationType);
			object databaseName = configurationType.GetMethod("GetTableName", Flags).Invoke(configuration, null);
			return (string)databaseName.GetType().GetProperty("Name").GetValue(databaseName, null);
		}

		static object GetConfigurationAndType(EntityTypeConfiguration<T> map, out Type type)
		{
			object configuration = map.GetType().GetProperty("Configuration", Flags).GetValue(map, null);
			type = configuration.GetType();
			return configuration;
		}

		public void AddRow(T entity)
		{
			_processPropertiesMethod.Invoke(this, new[] { entity, _properties });
		}

		void ProcessProperties<TKey, TValue>(T entity, IDictionary<TKey, TValue> mappedProperties)
		{
			var properties = new Dictionary<string, object>();
			foreach (var kvp in mappedProperties)
			{
				if (_columnNameProperty == null)
					_columnNameProperty = kvp.Value.GetType().GetProperty("ColumnName");

				properties.Add((string)_columnNameProperty.GetValue(kvp.Value, null),
					_entityType.GetProperty(kvp.Key.ToString()).GetValue(entity, null));
			}
			AddRow(properties);
		}
	}
}