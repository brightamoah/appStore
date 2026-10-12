using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;

namespace appStore.Server.Dto.Boards
{
    public record UpdateColumnDto(
        [Required]
        [StringLength(100, MinimumLength = 1)]
        string Name,

        [Required]
        Status StatusMapping,

        [Range(0, 100)]
        int WipLimit,

        NuxtUiColor? Color
    );
}