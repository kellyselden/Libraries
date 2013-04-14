using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using KellySelden.Libraries.Sql;

namespace KellySelden.Libraries.EntityFramework
{
	public class EntityFrameworkBatchProcess<T> where T : class
	{
		const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

		readonly Type _entityType;
		readonly IEnumerable<string> _keyProperties, _ignoredProperties;
		readonly object _properties;
		readonly MethodInfo _processPropertiesMethod;
		Type _primitivePropertyConfigurationType;
		PropertyInfo _columnNameProperty, _databaseGeneratedOptionProperty;
		readonly SqlBatchProcess _sqlBatchProcess;

		public EntityFrameworkBatchProcess(DbContext context, EntityTypeConfiguration<T> map) //make map nullable
		{
			_entityType = typeof(T);

			object configuration = map.GetType().GetProperty("Configuration", Flags).GetValue(map, null);
			Type configurationType = configuration.GetType();

			_keyProperties = ((IEnumerable<PropertyInfo>)configurationType.GetField("_keyProperties", Flags).GetValue(configuration)).Select(p => p.Name);
			_ignoredProperties = ((IEnumerable<PropertyInfo>)configurationType.GetProperty("IgnoredProperties", Flags).GetValue(configuration, null)).Select(p => p.Name);

			_properties = configurationType.GetProperty("PrimitivePropertyConfigurations", Flags).GetValue(configuration, null);
			_processPropertiesMethod = typeof(EntityFrameworkBatchProcess<T>).GetMethod("ProcessProperties", Flags)
				.MakeGenericMethod(_properties.GetType().GetInterfaces().Single(i =>
					i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments());
			
			object databaseName = configurationType.GetMethod("GetTableName", Flags).Invoke(configuration, null);
			var tableName = databaseName == null ? _entityType.Name : (string)databaseName.GetType().GetProperty("Name").GetValue(databaseName, null);
			_sqlBatchProcess = new SqlBatchProcess((SqlConnection)context.Database.Connection, tableName);
		}
		
		public void AddEntitiesToInsert(IEnumerable<T> entities)
		{
			foreach (T entity in entities)
				AddEntityToInsert(entity);
		}
		public void AddEntitiesToUpdate(IEnumerable<T> entities)
		{
			foreach (T entity in entities)
				AddEntityToUpdate(entity);
		}
		public void AddEntitiesToDelete(IEnumerable<T> entities)
		{
			foreach (T entity in entities)
				AddEntityToDelete(entity);
		}
		public void AddEntityToInsert(T entity)
		{
			_processPropertiesMethod.Invoke(this, new[] { entity, _properties, DbOperation.Insert });
		}
		public void AddEntityToUpdate(T entity)
		{
			_processPropertiesMethod.Invoke(this, new[] { entity, _properties, DbOperation.Update });
		}
		public void AddEntityToDelete(T entity)
		{
			_processPropertiesMethod.Invoke(this, new[] { entity, _properties, DbOperation.Delete });
		}

		enum DbOperation { Insert, Update, Delete }

		void ProcessProperties<TKey, TValue>(T entity, IDictionary<TKey, TValue> mappedProperties, DbOperation operation)
		{
			var mappedPropertyNames = new List<string>();
			var keyColumns = new Dictionary<string, object>();
			var columns = new Dictionary<string, object>();
			foreach (var kvp in mappedProperties)
			{
				if (_primitivePropertyConfigurationType == null)
				{
					_primitivePropertyConfigurationType = kvp.Value.GetType();
					_columnNameProperty = _primitivePropertyConfigurationType.GetProperty("ColumnName");
				}

				string propertyName = kvp.Key.ToString();
				mappedPropertyNames.Add(propertyName);

				var columnName = (string)_columnNameProperty.GetValue(kvp.Value, null) ?? propertyName; //property was mapped to key, but not as property
				object columnValue = _entityType.GetProperty(propertyName).GetValue(entity, null);
				
				if (_keyProperties.Contains(propertyName))
				{
					if (operation != DbOperation.Insert)
					{
						keyColumns.Add(columnName, columnValue);
						continue;
					}

					if (_databaseGeneratedOptionProperty == null)
						_databaseGeneratedOptionProperty = _primitivePropertyConfigurationType.GetProperty("DatabaseGeneratedOption");

					var databaseGeneratedOption = (DatabaseGeneratedOption?)_databaseGeneratedOptionProperty.GetValue(kvp.Value, null);
					if (!databaseGeneratedOption.HasValue || databaseGeneratedOption.Value == DatabaseGeneratedOption.Identity)
						continue;
				}

				if (operation != DbOperation.Delete)
					columns.Add(columnName, columnValue);
			}
			foreach (PropertyInfo property in _entityType.GetProperties())
			{
				if (!mappedPropertyNames.Contains(property.Name)
					&& !_ignoredProperties.Contains(property.Name)
					&& property.PropertyType.Assembly != _entityType.Assembly) //terrible way to guess property is a table map, fix later
					columns.Add(property.Name, property.GetValue(entity, null));
			}
			switch (operation)
			{
				case DbOperation.Insert:
					_sqlBatchProcess.AddRowToInsert(columns);
					break;
				case DbOperation.Update:
					_sqlBatchProcess.AddRowToUpdate(keyColumns, columns);
					break;
				case DbOperation.Delete:
					_sqlBatchProcess.AddRowToDelete(keyColumns);
					break;
			}
		}

		public void Process(int? timeout = null, int batchSize = 20000)
		{
			_sqlBatchProcess.Process(timeout, batchSize);
		}
	}
}