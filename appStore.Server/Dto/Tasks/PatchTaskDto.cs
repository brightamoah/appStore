using System.ComponentModel.DataAnnotations;

using appStore.Server.Models;

namespace appStore.Server.Dto.Tasks
{
    public record PatchTaskDto(
        [StringLength(50)]
        string? Name,

        [StringLength(200)]
        string? Description,

        [Range(1, int.MaxValue, ErrorMessage = "PriorityId must be a positive integer.")]
        int? PriorityId,

        Status Status,

        bool? IsCompleted
    )
    {
        public void PatchModel(TaskItem patchTaskItem)
        {
            patchTaskItem.Name = Name ?? patchTaskItem.Name;
            patchTaskItem.Description = Description ?? patchTaskItem.Description;
            patchTaskItem.PriorityId = PriorityId ?? patchTaskItem.PriorityId;

            if (IsCompleted.HasValue)
            {
                patchTaskItem.IsCompleted = IsCompleted.Value;

                if (IsCompleted.Value && patchTaskItem.CompletedAt == null)
                {
                    patchTaskItem.CompletedAt = DateTime.UtcNow;
                }
                else if (!IsCompleted.Value)
                {
                    patchTaskItem.CompletedAt = null;
                }
            }
            patchTaskItem.UpdatedAt = DateTime.UtcNow;
        }
    };
}