using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using KellySelden.Libraries.Domain.Abstract;

namespace KellySelden.Libraries.EntityFramework
{
	public class EntityFrameworkRepository : IRepository
	{
		readonly DbContext _context;

		public EntityFrameworkRepository(DbContext context)
		{
			_context = context;
		}

		public virtual T GetEntity<T>(int id) where T : class, IEntity
		{
			return GetEntities<T>().Single(p => p.Id == id);
		}

		public virtual IQueryable<T> GetEntities<T>() where T : class
		{
			return _context.Set<T>();
		}

		public virtual void SaveEntity<T>(T entity) where T : class, IEntity
		{
			SaveEntities(new[] { entity });
		}

		public virtual void SaveEntities<T>(IEnumerable<T> entities) where T : class, IEntity
		{
			foreach (T entity in entities)
				_context.Entry(entity).State = entity.Id == 0 ? EntityState.Added : EntityState.Modified;
			_context.SaveChanges();
		}

		public virtual void DeleteEntity<T>(T entity) where T : class
		{
			DeleteEntities(new[] { entity });
		}

		public virtual void DeleteEntities<T>(IEnumerable<T> entities) where T : class
		{
			foreach (T entity in entities)
				_context.Entry(entity).State = EntityState.Deleted;
			_context.SaveChanges();
		}
	}
}