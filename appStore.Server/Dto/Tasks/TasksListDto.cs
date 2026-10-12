using appStore.Server.Models;

namespace appStore.Server.Dto.Tasks
{
	public record TasksListDto(
		Guid Id,
		string? TaskKey,
		string Name,
		string Description,
		string Priority,
		int PriorityId,
		Status Status,
		IssueType IssueType,
		bool IsCompleted,
		Guid? ProjectId,
		Guid? ColumnId,
		int OrderIndex,
		Guid? AssigneeId,
		IReadOnlyList<Guid> AssigneeIds,
		DateTime? DueDate,
		DateTime CreatedAt,
		DateTime UpdatedAt,
		DateTime? CompletedAt
	)
	{
		public static TasksListDto FromModel(TaskItem task)
		{
			return new TasksListDto(
				task.Id,
				task.TaskKey,
				task.Name,
				task.Description ?? string.Empty,
				task.Priority?.Name ?? string.Empty,
				task.PriorityId,
				task.Status,
				task.IssueType,
				task.IsCompleted,
				task.ProjectId,
				task.ColumnId,
				task.OrderIndex,
				task.AssigneeId,
				task.AssigneeIds,
				task.DueDate,
				task.CreatedAt,
				task.UpdatedAt,
				task.CompletedAt
			);
		}
	};
}