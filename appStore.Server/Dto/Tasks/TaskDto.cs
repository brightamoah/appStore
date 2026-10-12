using appStore.Server.Models;

namespace appStore.Server.Dto.Tasks
{
	public record TaskDto(
		Guid Id,
        string? TaskKey,
		string Name,
		string Description,
		int PriorityId,
		Status Status,
		IssueType IssueType,
		bool IsCompleted,
		Guid? ProjectId,
		Guid? ColumnId,
		int OrderIndex,
		Guid? AssigneeId,
		IReadOnlyList<Guid> AssigneeIds,
		Guid? ReporterId,
		DateTime? DueDate,
		int? EstimatedMinutes,
		DateTime CreatedAt,
		DateTime UpdatedAt,
		DateTime? CompletedAt
	)
	{
		public static TaskDto FromModel( TaskItem task )
		{
			return new TaskDto(
				task.Id,
				task.TaskKey,
				task.Name,
				task.Description ?? string.Empty,
				task.PriorityId,
				task.Status,
				task.IssueType,
				task.IsCompleted,
				task.ProjectId,
				task.ColumnId,
				task.OrderIndex,
				task.AssigneeId,
				task.AssigneeIds,
				task.ReporterId,
				task.DueDate,
				task.EstimatedMinutes,
				task.CreatedAt,
				task.UpdatedAt,
				task.CompletedAt
			);
		}
	}
}