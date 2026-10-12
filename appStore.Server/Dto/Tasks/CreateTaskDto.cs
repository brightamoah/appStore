using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;

namespace appStore.Server.Dto.Tasks
{
    public record CreateTaskDto(
        [Required]
        [StringLength(200, MinimumLength = 1)]
        string Name,

        [StringLength(4000)]
        string? Description,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PriorityId must be a positive integer.")]
        int PriorityId,

        [Required]
        Guid ProjectId,

        Guid? ColumnId = null,

        IssueType IssueType = IssueType.Task,

        Guid? AssigneeId = null,

        List<Guid>? AssigneeIds = null,

        DateTime? DueDate = null,

        int? EstimatedMinutes = null
    );
}