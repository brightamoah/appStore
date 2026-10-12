using appStore.Server.Data;
using appStore.Server.Dto.Common;
using appStore.Server.Dto.Tasks;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class TaskRepository(DatabaseContext context) : Repository<TaskItem, Guid>(context), ITaskRepository
	{
		public async Task<TaskItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Include(taskItem => taskItem.Checklists)
				.Include(taskItem => taskItem.Comments)
				.Include(taskItem => taskItem.Activities)
				.Include(taskItem => taskItem.Priority)
				.FirstOrDefaultAsync(taskItem => taskItem.Id == id, cancellationToken);
		}

		public async Task<PagedResponse<TasksListDto>> GetPagedTasksAsync(
			Guid? projectId,
			PaginationParams pagination,
			CancellationToken cancellationToken = default
		)
		{
			var query = DbSet
				.Include(taskItem => taskItem.Priority)
				.AsNoTracking();

			if (projectId.HasValue)
			{
				query = query.Where(taskItem => taskItem.ProjectId == projectId.Value);
			}

			if (!string.IsNullOrWhiteSpace(pagination.Search))
			{
				var search = pagination.Search.Trim().ToLower();

				#pragma warning disable CA1862
				query = query.Where(taskItem =>
					taskItem.Name.ToLower().Contains(search) || (
					taskItem.TaskKey != null &&
					taskItem.TaskKey.ToLower().Contains(search))
				);
			}

			var totalCount = await query.CountAsync(cancellationToken);

			var items = await query
				.OrderBy(taskItem => taskItem.OrderIndex)
				.Skip((pagination.PageNumber - 1) * pagination.PageSize)
				.Take(pagination.PageSize)
				.Select(taskItem => TasksListDto.FromModel(taskItem))
				.ToListAsync(cancellationToken);

			return PagedResponse<TasksListDto>.Create(
				items,
				totalCount,
				pagination.PageNumber,
				pagination.PageSize
			);
		}

		public async Task<int> GetMaxOrderIndexInColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Where(taskItem => taskItem.ColumnId == columnId)
				.Select(taskItem => (int?)taskItem.OrderIndex)
				.MaxAsync(cancellationToken) ?? -1;
		}

		public async Task<IReadOnlyList<TaskItem>> GetTasksInColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Where(taskItem => taskItem.ColumnId == columnId)
				.OrderBy(taskItem => taskItem.OrderIndex)
				.ToListAsync(cancellationToken);
		}

		public async Task ShiftTaskOrdersAsync(
			Guid columnId,
			int startIndex,
			int? endIndex,
			int delta,
			Guid? excludeTaskId,
			CancellationToken cancellationToken = default
		)
		{
			var query = DbSet.Where(taskItem => taskItem.ColumnId == columnId && taskItem.OrderIndex >= startIndex);

			if (endIndex.HasValue)
			{
				query = query.Where(taskItem => taskItem.OrderIndex <= endIndex.Value);
			}

			if (excludeTaskId.HasValue)
			{
				query = query.Where(taskItem => taskItem.Id != excludeTaskId.Value);
			}

			var shiftingTasks = await query.ToListAsync(cancellationToken);

			foreach (var shiftingTask in shiftingTasks)
			{
				shiftingTask.OrderIndex += delta;
			}
		}

		public async Task<bool> AnyWithPriorityIdAsync(int priorityId, CancellationToken cancellationToken = default)
		{
			return await DbSet.AnyAsync(taskItem => taskItem.PriorityId == priorityId, cancellationToken);
		}
	}
}
