using appStore.Server.Dto.Boards;

namespace appStore.Server.Services.Interfaces
{
    public interface IBoardService
    {
        Task<BoardDto> GetBoardAsync(
            Guid projectId,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<BoardColumnDto>> GetColumnsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default
        );

        Task<BoardColumnDto> CreateColumnAsync(
            Guid projectId,
            CreateColumnDto dto,
            CancellationToken cancellationToken = default
        );

        Task<BoardColumnDto> UpdateColumnAsync(
            Guid projectId,
            Guid columnId,
            UpdateColumnDto dto,
            CancellationToken cancellationToken = default
        );

        Task ReorderColumnsAsync(
            Guid projectId,
            ReorderColumnsDto dto,
            CancellationToken cancellationToken = default
        );

        Task DeleteColumnAsync(
            Guid projectId,
            Guid columnId,
            Guid? fallbackColumnId = null,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<BoardColumnDto>> ProvisionDefaultColumnsAsync(
            Guid projectId, 
            CancellationToken cancellationToken = default
        );
    }
}