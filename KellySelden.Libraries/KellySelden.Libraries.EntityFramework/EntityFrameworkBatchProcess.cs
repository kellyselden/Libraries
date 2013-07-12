using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using KellySelden.Libraries.Sql;

namespace KellySelden.Libraries.EntityFramework
{
	public class EntityFrameworkBatchProcess<T> : IDisposable where T : class
	{
		const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

		readonly Type _entityType;
		readonly IDictionary<string, PropertyInfo> _propertyLookup;
		readonly IEnumerable<string> _keyProperties, _ignoredProperties;
		readonly object _mappedProperties;
		readonly MethodInfo _processPropertiesMethod;
		readonly SqlBatchProcess _sqlBatchProcess;

		Type _primitivePropertyConfigurationType;
		PropertyInfo _columnNameProperty, _databaseGeneratedOptionProperty;
		T[] _inserts;

		public EntityFrameworkBatchProcess(string connectionString, EntityTypeConfiguration<T> map) //make map nullable
		{
			_entityType = typeof(T);
			_propertyLookup = _entityType.GetProperties().ToDictionary(p => p.Name);

			object configuration = map.GetType().GetProperty("Configuration", Flags).GetValue(map, null);
			Type configurationType = configuration.GetType();

			_keyProperties = ((IEnumerable<PropertyInfo>)configurationType.GetField("_keyProperties", Flags).GetValue(configuration)).Select(p => p.Name);
			_ignoredProperties = ((IEnumerable<PropertyInfo>)configurationType.GetProperty("IgnoredProperties", Flags).GetValue(configuration, null)).Select(p => p.Name);

			_mappedProperties = configurationType.GetProperty("PrimitivePropertyConfigurations", Flags).GetValue(configuration, null);
			_processPropertiesMethod = typeof(EntityFrameworkBatchProcess<T>).GetMethod("ProcessProperties", Flags)
				.MakeGenericMethod(_mappedProperties.GetType().GetInterfaces().Single(i =>
					i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments());
			
			object databaseName = configurationType.GetMethod("GetTableName", Flags).Invoke(configuration, null);
			var tableName = databaseName == null ? _entityType.Name : (string)databaseName.GetType().GetProperty("Name").GetValue(databaseName, null);
			_sqlBatchProcess = new SqlBatchProcess(connectionString, tableName);
		}

		public void AddEntitiesToInsert(IEnumerable<T> entities)
		{
			_inserts = entities.ToArray();
			AddEntities(_inserts, DbOperation.Insert);
		}
		public void AddEntitiesToUpdate(IEnumerable<T> entities)
		{
			AddEntities(entities, DbOperation.Update);
		}
		public void AddEntitiesToDelete(IEnumerable<T> entities)
		{
			AddEntities(entities, DbOperation.Delete);
		}
		void AddEntities(IEnumerable<T> entities, DbOperation operation)
		{
			foreach (T entity in entities)
				_processPropertiesMethod.Invoke(this, new[] { entity, _mappedProperties, operation });
		}

		enum DbOperation { Insert, Update, Delete }

		KeyValuePair<string, Type>? _identity;
		readonly IDictionary<string, string> _columnNameLookup = new Dictionary<string, string>();
		readonly IDictionary<string, DatabaseGeneratedOption?> _databaseGeneratedOptionLookup = new Dictionary<string, DatabaseGeneratedOption?>();
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

				string columnName = (_columnNameLookup.ContainsKey(propertyName)
					? _columnNameLookup[propertyName]
					: _columnNameLookup[propertyName] = (string)_columnNameProperty.GetValue(kvp.Value, null))
					?? propertyName; //property was mapped to key, but not as property
				object columnValue = _propertyLookup[propertyName].GetValue(entity, null);

				if (_keyProperties.Contains(propertyName))
				{
					if (operation != DbOperation.Insert)
					{
						keyColumns.Add(columnName, columnValue);
						continue;
					}

					if (_databaseGeneratedOptionProperty == null)
						_databaseGeneratedOptionProperty = _primitivePropertyConfigurationType.GetProperty("DatabaseGeneratedOption");

					DatabaseGeneratedOption? databaseGeneratedOption = _databaseGeneratedOptionLookup.ContainsKey(propertyName)
						? _databaseGeneratedOptionLookup[propertyName]
						: _databaseGeneratedOptionLookup[propertyName] = (DatabaseGeneratedOption?)_databaseGeneratedOptionProperty.GetValue(kvp.Value, null);
					if (!databaseGeneratedOption.HasValue || databaseGeneratedOption.Value == DatabaseGeneratedOption.Identity)
					{
						if (_identity == null)
							_identity = new KeyValuePair<string, Type>(propertyName, columnValue.GetType());
						continue;
					}
				}

				if (operation != DbOperation.Delete)
					columns.Add(columnName, columnValue);
			}
			foreach (PropertyInfo property in _propertyLookup.Values)
			{
				if (!mappedPropertyNames.Contains(property.Name)
					&& !_ignoredProperties.Contains(property.Name)
					&& property.PropertyType.Assembly != _entityType.Assembly) //terrible way to guess property is a table map, fix later
					columns.Add(property.Name, property.GetValue(entity, null));
			}
			switch (operation)
			{
				case DbOperation.Insert:
					_sqlBatchProcess.AddRowToInsert(columns, _identity);
					break;
				case DbOperation.Update:
					_sqlBatchProcess.AddRowToUpdate(keyColumns, columns);
					break;
				case DbOperation.Delete:
					_sqlBatchProcess.AddRowToDelete(keyColumns);
					break;
			}
		}

		public void Process(int? timeout = null, int batchSize = SqlBatchProcess.DefaultBatchSize)
		{
			object[] identityKeys = _sqlBatchProcess.Process(timeout, batchSize).ToArray();
			for (int i = 0; i < identityKeys.Length; i++)
			{
				_propertyLookup[_identity.Value.Key].SetValue(_inserts[i], identityKeys[i], null);
			}
			_inserts = null;
		}

		public void Dispose()
		{
			_sqlBatchProcess.Dispose();
		}
	}
}