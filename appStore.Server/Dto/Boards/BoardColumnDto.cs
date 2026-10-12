using appStore.Server.Models;

namespace appStore.Server.Dto.Boards
{
    public record BoardColumnDto(
        Guid Id,
        Guid ProjectId,
        string Name,
        int OrderIndex,
        Status StatusMapping,
        int WipLimit,
        NuxtUiColor? Color,
        bool IsDefault,
        int TaskCount,
        DateTime CreatedAt
    )
    {
        public static BoardColumnDto FromModel(BoardColumn column, int taskCount = 0) =>
            new(
                column.Id,
                column.ProjectId,
                column.Name,
                column.OrderIndex,
                column.StatusMapping,
                column.WipLimit,
                column.Color,
                column.IsDefault,
                taskCount,
                column.CreatedAt
            );
    }
}