using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;

namespace appStore.Server.Dto.Tasks
{
    public record UpdateTaskDto(
        [StringLength(200, MinimumLength = 1)]
        string? Name = null,

        [StringLength(4000)]
        string? Description = null,

        [Range(1, int.MaxValue, ErrorMessage = "PriorityId must be a positive integer.")]
        int? PriorityId = null,

        Guid? AssigneeId = null,

        List<Guid>? AssigneeIds = null,

        DateTime? DueDate = null,

        int? EstimatedMinutes = null,

        IssueType? IssueType = null
    );
}