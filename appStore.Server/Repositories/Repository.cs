using System.Linq.Expressions;
using appStore.Server.Data;
using appStore.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class Repository<TEntity, TId>(DatabaseContext context) : IRepository<TEntity, TId> where TEntity : class
	{
		protected readonly DatabaseContext Context = context;
		protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

		public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
		{
			return await DbSet.FindAsync([id], cancellationToken);
		}

		public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
		}

		public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
			Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default
		)
		{
			return await DbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
		}

		public virtual async Task<bool> AnyAsync(
			Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default
		)
		{
			return await DbSet.AnyAsync(predicate, cancellationToken);
		}

		public virtual async Task<TEntity> AddAsync(
			TEntity entity,
			CancellationToken cancellationToken = default
		)
		{
			await DbSet.AddAsync(entity, cancellationToken);
			return entity;
		}

		public virtual async Task AddRangeAsync(
			IEnumerable<TEntity> entities,
			CancellationToken cancellationToken = default
		)
		{
			await DbSet.AddRangeAsync(entities, cancellationToken);
		}

		public virtual void Update(TEntity entity)
		{
			DbSet.Update(entity);
		}

		public virtual void Remove(TEntity entity)
		{
			DbSet.Remove(entity);
		}

		public virtual void RemoveRange(IEnumerable<TEntity> entities)
		{
			DbSet.RemoveRange(entities);
		}

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await Context.SaveChangesAsync(cancellationToken);
		}
	}
}