using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Domain.Abstract
{
	public interface IRepository
	{
		T GetEntity<T>(int id) where T : class, IEntity;
		IQueryable<T> GetEntities<T>() where T : class;
		void SaveEntity<T>(T entity) where T : class, IEntity;
		void SaveEntities<T>(IEnumerable<T> entities) where T : class, IEntity;
		void DeleteEntity<T>(T entity) where T : class;
		void DeleteEntities<T>(IEnumerable<T> entities) where T : class;
	}
}