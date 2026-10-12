using System.Linq.Expressions;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IRepository<TEntity, in TId> where TEntity : class
	{
		Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
		Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
		Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
		Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
		void Update(TEntity entity);
		void Remove(TEntity entity);
		void RemoveRange(IEnumerable<TEntity> entities);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}