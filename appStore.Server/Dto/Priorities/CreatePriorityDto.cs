using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Priorities
{
    public record CreatePriorityDto(
        [Required]
        [StringLength(50)]
        string Name
    );
}