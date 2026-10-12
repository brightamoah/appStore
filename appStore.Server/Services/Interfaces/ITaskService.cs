using appStore.Server.Dto.Common;
using appStore.Server.Dto.Tasks;
using appStore.Server.Models;

namespace appStore.Server.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(
	        CreateTaskDto dto, 
	        Guid currentUserId, 
	        CancellationToken cancellationToken = default
        );

        Task<TaskDto> MoveTaskAsync(
	        Guid taskId, 
			MoveTaskDto dto, 
			Guid currentUserId, 
			CancellationToken cancellationToken = default
		);

        Task<TaskDto> TransitionStatusAsync(
	        Guid taskId, 
			Status targetStatus, 
			Guid currentUserId, 
			CancellationToken cancellationToken = default
		);

        Task<TaskDto> UpdateTaskAsync(
	        Guid taskId,
	        UpdateTaskDto updateTaskDto,
	        Guid currentUserId,
	        CancellationToken cancellationToken = default
        );

        Task<TaskDto> GetTaskByIdAsync(
			Guid taskId, 
			CancellationToken cancellationToken = default
		);

        Task<PagedResponse<TasksListDto>> GetTasksAsync(
			Guid? projectId, 
			PaginationParams pagination, 
			CancellationToken cancellationToken = default
		);

        Task DeleteTaskAsync(
			Guid taskId, 
			CancellationToken cancellationToken = default
		);
    }
}