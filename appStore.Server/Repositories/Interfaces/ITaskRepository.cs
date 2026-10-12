using appStore.Server.Dto.Common;
using appStore.Server.Dto.Tasks;
using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface ITaskRepository : IRepository<TaskItem, Guid>
	{
		Task<TaskItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
		Task<PagedResponse<TasksListDto>> GetPagedTasksAsync(Guid? projectId, PaginationParams pagination, CancellationToken cancellationToken = default);
		Task<int> GetMaxOrderIndexInColumnAsync(Guid columnId, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<TaskItem>> GetTasksInColumnAsync(Guid columnId, CancellationToken cancellationToken = default);
		Task ShiftTaskOrdersAsync(Guid columnId, int startIndex, int? endIndex, int delta, Guid? excludeTaskId, CancellationToken cancellationToken = default);
		Task<bool> AnyWithPriorityIdAsync(int priorityId, CancellationToken cancellationToken = default);
	}
}
