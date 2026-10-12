using appStore.Server.Dto.Priorities;

namespace appStore.Server.Dto.Tasks
{
	public record StatusOptionDto(string Value, string Label);
	public record IssueTypeOptionDto(string Value, string Label);

	public record TaskMetadataDto(
		IReadOnlyList<StatusOptionDto> Statuses,
		IReadOnlyList<IssueTypeOptionDto> IssueTypes,
		IReadOnlyList<PriorityDto> Priorities
	);
}