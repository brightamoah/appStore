using System.ComponentModel.DataAnnotations;
using appStore.Server.Models;

namespace appStore.Server.Dto.Projects
{
	public record ProjectMemberUserDto(
		Guid Id,
		string Name,
		string Email,
		string? ProfilePictureUrl,
		ProjectRole Role = ProjectRole.Member
	);

	public record ProjectMemberDto(
		Guid UserId,
		string Name,
		string Email,
		string? ProfilePictureUrl,
		ProjectRole Role,
		DateTime JoinedAt
	);

	public record AddProjectMemberDto(
		[Required] Guid UserId,
		ProjectRole Role = ProjectRole.Member
	);

	public record UpdateProjectMemberRoleDto(
		[Required] ProjectRole Role
	);

	public record ProjectDto(
		Guid Id,
		string Name,
		string Key,
		string? Description,
		string? Icon,
		Guid OwnerId,
		DateTime CreatedAt,
		DateTime UpdatedAt,
		IReadOnlyList<ProjectMemberUserDto> Members
	);

	public record CreateProjectDto(
		[Required]
		[StringLength(100, MinimumLength = 2)]
		string Name,

		[Required]
		[StringLength(10, MinimumLength = 2)]
		string Key,

		[StringLength(1000)]
		string? Description = null,

		[StringLength(50)]
		string? Icon = null
	);
}