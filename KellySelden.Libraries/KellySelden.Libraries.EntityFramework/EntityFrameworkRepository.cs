using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using KellySelden.Libraries.Domain.Abstract;

namespace KellySelden.Libraries.EntityFramework
{
	public class EntityFrameworkRepository : IRepository
	{
		protected readonly DbContext Context;

		public EntityFrameworkRepository(DbContext context)
		{
			Context = context;
		}

		public virtual T GetEntity<T>(int id) where T : class, IEntity
		{
			return GetEntities<T>().Single(p => p.Id == id);
		}

		public virtual IQueryable<T> GetEntities<T>() where T : class
		{
			return Context.Set<T>();
		}

		public virtual void SaveEntity<T>(T entity) where T : class, IEntity
		{
			SaveEntities(new[] { entity });
		}

		public virtual void SaveEntities<T>(IEnumerable<T> entities) where T : class, IEntity
		{
			foreach (T entity in entities)
				Context.Entry(entity).State = entity.Id == 0 ? EntityState.Added : EntityState.Modified;
			Context.SaveChanges();
		}

		public virtual void DeleteEntity<T>(T entity) where T : class
		{
			DeleteEntities(new[] { entity });
		}

		public virtual void DeleteEntities<T>(IEnumerable<T> entities) where T : class
		{
			foreach (T entity in entities)
				Context.Entry(entity).State = EntityState.Deleted;
			Context.SaveChanges();
		}
	}
}