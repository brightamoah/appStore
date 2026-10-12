using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;

namespace appStore.Server.Dto.Boards
{
    public record CreateColumnDto(
        [Required]
        [StringLength(100, MinimumLength = 1)]
        string Name,

        [Required]
        Status StatusMapping,

        [Range(0, 100)]
        int WipLimit = 0,

        NuxtUiColor? Color = NuxtUiColor.Neutral
    );
}