using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Boards
{
    public record ReorderColumnsDto(
        [Required]
        List<Guid> OrderedColumnIds
    );
}