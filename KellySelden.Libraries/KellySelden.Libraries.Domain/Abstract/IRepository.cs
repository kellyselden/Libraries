using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Domain.Abstract
{
	public interface IRepository
	{
		T GetEntity<T>(int id) where T : class;
		T GetEntity<T>(Func<T, bool> single) where T : class;
		T GetEntityOrDefault<T>(int id) where T : class;
		T GetEntityOrDefault<T>(Func<T, bool> single) where T : class;
		IQueryable<T> GetEntities<T>() where T : class;
		IQueryable<T> GetEntities<T>(Func<T, bool> where) where T : class;
		void SaveEntity<T>(T entity) where T : class, IEntity;
		void SaveEntities<T>(IEnumerable<T> entities) where T : class, IEntity;
		void SaveEntity<T>(T entity, Func<T, int> keySelector) where T : class;
		void SaveEntities<T>(IEnumerable<T> entities, Func<T, int> keySelector) where T : class;
		void DeleteEntity<T>(T entity) where T : class;
		void DeleteEntities<T>(IEnumerable<T> entities) where T : class;
	}
}