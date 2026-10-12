using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Tasks
{
    public record MoveTaskDto(
        [Required]
        Guid TargetColumnId,

        [Range(0, 1000)]
        int TargetOrderIndex,

        Guid? AssigneeId = null
    );
}