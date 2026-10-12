namespace appStore.Server.Dto.Boards
{
    public record BoardDto(
        Guid ProjectId,
        string ProjectName,
        string ProjectKey,
        IReadOnlyList<BoardColumnDto> Columns
    );
}