using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class BoardRepository(DatabaseContext context) : Repository<BoardColumn, Guid>(context), IBoardRepository
	{
		public async Task<IReadOnlyList<BoardColumn>> GetColumnsWithTasksAsync(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			return await DbSet
				.Include(boardColumn => boardColumn.Tasks)
				.Where(boardColumn => boardColumn.ProjectId == projectId)
				.OrderBy(boardColumn => boardColumn.OrderIndex)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<BoardColumn>> GetColumnsByProjectIdAsync(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			return await DbSet
				.Where(boardColumn => boardColumn.ProjectId == projectId)
				.OrderBy(boardColumn => boardColumn.OrderIndex)
				.ToListAsync(cancellationToken);
		}

		public async Task<BoardColumn?> GetMatchingColumnAsync(
			Guid projectId,
			Status status,
			CancellationToken cancellationToken = default
		)
		{
			return await DbSet
				.FirstOrDefaultAsync(
					boardColumn => boardColumn.ProjectId == projectId && boardColumn.StatusMapping == status,
					cancellationToken
				);
		}

		public async Task<int> GetMaxOrderIndexAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Where(boardColumn => boardColumn.ProjectId == projectId)
				.Select(boardColumn => (int?)boardColumn.OrderIndex)
				.MaxAsync(cancellationToken) ?? -1;
		}

		public async Task<BoardColumn?> GetFirstColumnAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Where(boardColumn => boardColumn.ProjectId == projectId)
				.OrderBy(boardColumn => boardColumn.OrderIndex)
				.FirstOrDefaultAsync(cancellationToken);
		}
	}
}
