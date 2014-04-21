using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using KellySelden.Libraries.Domain.Abstract;

namespace KellySelden.Libraries.EntityFramework
{
	/// <summary>
	/// why are these functions virtual?
	/// why isn't this class abstract?
	/// </summary>
	public class EntityFrameworkRepository : IRepository
	{
		protected readonly DbContext Context;

		public EntityFrameworkRepository(DbContext context)
		{
			Context = context;
		}

		public virtual T GetEntity<T>(int id) where T : class
		{
			T entity = GetEntityOrDefault<T>(id);
			if (entity == null) throw new InvalidOperationException("Sequence contains no elements");
			return entity;
		}

		public virtual T GetEntity<T>(Func<T, bool> single) where T : class
		{
			return GetEntities<T>().Single(single);
		}

		public virtual T GetEntityOrDefault<T>(int id) where T : class
		{
			return Context.Set<T>().Find(id);
		}

		public virtual T GetEntityOrDefault<T>(Func<T, bool> single) where T : class
		{
			return GetEntities<T>().SingleOrDefault(single);
		}

		public virtual IQueryable<T> GetEntities<T>() where T : class
		{
			return Context.Set<T>();
		}

		public virtual IQueryable<T> GetEntities<T>(Func<T, bool> where) where T : class
		{
			return Context.Set<T>().Where(where).AsQueryable();
		}

		public virtual void SaveEntity<T>(T entity) where T : class, IEntity
		{
			SaveEntities(new[] { entity });
		}

		public virtual void SaveEntities<T>(IEnumerable<T> entities) where T : class, IEntity
		{
			SaveEntities(entities, entity => entity.Id);
		}

		public virtual void SaveEntity<T>(T entity, Func<T, int> keySelector) where T : class
		{
			SaveEntities(new[] { entity }, keySelector);
		}

		public virtual void SaveEntities<T>(IEnumerable<T> entities, Func<T, int> keySelector) where T : class
		{
			foreach (T entity in entities)
				//Context.Entry(entity).State = keySelector(entity) == 0 ? EntityState.Added : EntityState.Modified;
				Context.Entry(entity).CurrentValues.SetValues(entity);
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