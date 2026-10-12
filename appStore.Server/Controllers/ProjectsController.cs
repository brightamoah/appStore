using appStore.Server.Dto.Common;
using appStore.Server.Dto.Projects;
using appStore.Server.Dto.Users;
using appStore.Server.Errors;
using appStore.Server.Extensions;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/projects")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class ProjectsController(
		IProjectRepository projectRepository,
		IUserRepository userRepository,
		IBoardService boardService,
		IProjectAuthorizationService projectAuthorizationService
	) : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectDto>>>> GetUserProjects(
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			var projects = await projectRepository.GetProjectsForUserAsync(currentUserId, cancellationToken);

			var projectDtos = projects.Select(project => new ProjectDto(
				project.Id,
				project.Name,
				project.Key,
				project.Description,
				project.Icon,
				project.OwnerId,
				project.CreatedAt,
				project.UpdatedAt,
				project.Members.Select(member => new ProjectMemberUserDto(
					member.User.Id,
					member.User.Name,
					member.User.Email,
					member.User.ProfilePictureUrl,
					member.Role
				)).ToList()
			)).ToList();

			return Ok(ApiResponse<IReadOnlyList<ProjectDto>>.Success(projectDtos, "Projects retrieved successfully."));
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProjectById(
			Guid id,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			var hasAccess = await projectAuthorizationService.HasProjectRoleAsync(
				id,
				currentUserId,
				ProjectRole.Viewer,
				cancellationToken
			);

			if (!hasAccess)
			{
				throw AppException.Forbidden("You do not have access to view this project.");
			}

			var project = await projectRepository.GetByIdWithMembersAsync(id, cancellationToken)
				?? throw AppException.NotFound($"Project with ID '{id}' was not found.", ErrorCodes.PROJECT_NOT_FOUND);

			var projectDto = new ProjectDto(
				project.Id,
				project.Name,
				project.Key,
				project.Description,
				project.Icon,
				project.OwnerId,
				project.CreatedAt,
				project.UpdatedAt,
				project.Members.Select(member => new ProjectMemberUserDto(
					member.User.Id,
					member.User.Name,
					member.User.Email,
					member.User.ProfilePictureUrl,
					member.Role
				)).ToList()
			);

			return Ok(ApiResponse<ProjectDto>.Success(projectDto, "Project retrieved successfully."));
		}

		[HttpPost]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject(
			[FromBody] CreateProjectDto createProjectDto,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken)
				?? throw AppException.NotFound("User not found.", ErrorCodes.USER_NOT_FOUND);

			var sanitizedKey = createProjectDto.Key.Trim().ToUpperInvariant();
			var keyExists = await projectRepository.ExistsByKeyAsync(sanitizedKey, cancellationToken);
			if (keyExists)
			{
				throw AppException.Conflict($"A project with key '{sanitizedKey}' already exists.", ErrorCodes.CONFLICT);
			}

			var now = DateTime.UtcNow;
			var project = new Project
			{
				Id = Guid.NewGuid(),
				Name = createProjectDto.Name.Trim(),
				Key = sanitizedKey,
				Description = createProjectDto.Description?.Trim(),
				Icon = createProjectDto.Icon?.Trim() ?? "frontend",
				OwnerId = currentUserId,
				TaskCounter = 0,
				CreatedAt = now,
				UpdatedAt = now,
			};

			var ownerMembership = new ProjectMember
			{
				ProjectId = project.Id,
				UserId = currentUserId,
				Role = ProjectRole.Owner,
				JoinedAt = now
			};

			await projectRepository.AddAsync(project, cancellationToken);
			await projectRepository.AddMemberAsync(ownerMembership, cancellationToken);
			await projectRepository.SaveChangesAsync(cancellationToken);

			// Automatically provision default Kanban columns for the new project
			await boardService.ProvisionDefaultColumnsAsync(project.Id, cancellationToken);

			var projectDto = new ProjectDto(
				project.Id,
				project.Name,
				project.Key,
				project.Description,
				project.Icon,
				project.OwnerId,
				project.CreatedAt,
				project.UpdatedAt,
				[new ProjectMemberUserDto(currentUser.Id, currentUser.Name, currentUser.Email, currentUser.ProfilePictureUrl, ProjectRole.Owner)]
			);

			return CreatedAtAction(
				nameof(GetProjectById),
				new { id = project.Id },
				ApiResponse<ProjectDto>.Success(projectDto, "Project created successfully.")
			);
		}

		[HttpGet("{projectId:guid}/members")]
		public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectMemberDto>>>> GetProjectMembers(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			var hasAccess = await projectAuthorizationService.HasProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Viewer,
				cancellationToken
			);

			if (!hasAccess)
			{
				throw AppException.Forbidden("You do not have access to view members of this project.");
			}

			var members = await projectRepository.GetMembersAsync(projectId, cancellationToken);
			var memberDtos = members.Select(member => new ProjectMemberDto(
				member.UserId,
				member.User.Name,
				member.User.Email,
				member.User.ProfilePictureUrl,
				member.Role,
				member.JoinedAt
			)).ToList();

			return Ok(ApiResponse<IReadOnlyList<ProjectMemberDto>>.Success(memberDtos, "Project members retrieved successfully."));
		}

		[HttpGet("{projectId:guid}/available-users")]
		public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAvailableUsers(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Admin,
				cancellationToken
			);

			var existingMembers = await projectRepository.GetMembersAsync(projectId, cancellationToken);
			var existingMemberIds = existingMembers.Select(member => member.UserId).ToHashSet();

			var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
			if (project != null)
			{
				existingMemberIds.Add(project.OwnerId);
			}

			var allUsers = await userRepository.GetAllAsync(cancellationToken);
			var availableUsers = allUsers
				.Where(user => !existingMemberIds.Contains(user.Id))
				.Select(user => UserDto.FromModel(user))
				.ToList();

			return Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(availableUsers, "Available users retrieved successfully."));
		}

		[HttpPost("{projectId:guid}/members")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		public async Task<ActionResult<ApiResponse<ProjectMemberDto>>> AddProjectMember(
			Guid projectId,
			[FromBody] AddProjectMemberDto addProjectMemberDto,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Admin,
				cancellationToken
			);

			var project = await projectRepository.GetByIdAsync(projectId, cancellationToken)
				?? throw AppException.NotFound($"Project with ID '{projectId}' was not found.", ErrorCodes.PROJECT_NOT_FOUND);

			var user = await userRepository.GetByIdAsync(addProjectMemberDto.UserId, cancellationToken)
				?? throw AppException.NotFound($"User with ID '{addProjectMemberDto.UserId}' was not found.", ErrorCodes.USER_NOT_FOUND);

			var existingMember = await projectRepository.GetMemberAsync(projectId, addProjectMemberDto.UserId, cancellationToken);
			if (existingMember != null || project.OwnerId == addProjectMemberDto.UserId)
			{
				throw AppException.Conflict("User is already a member of this project.", ErrorCodes.CONFLICT);
			}

			var now = DateTime.UtcNow;
			var member = new ProjectMember
			{
				ProjectId = projectId,
				UserId = addProjectMemberDto.UserId,
				Role = addProjectMemberDto.Role == ProjectRole.None ? ProjectRole.Member : addProjectMemberDto.Role,
				JoinedAt = now
			};

			await projectRepository.AddMemberAsync(member, cancellationToken);
			await projectRepository.SaveChangesAsync(cancellationToken);

			var memberDto = new ProjectMemberDto(
				user.Id,
				user.Name,
				user.Email,
				user.ProfilePictureUrl,
				member.Role,
				member.JoinedAt
			);

			return CreatedAtAction(
				nameof(GetProjectMembers),
				new { projectId },
				ApiResponse<ProjectMemberDto>.Success(memberDto, "Project member added successfully.")
			);
		}

		[HttpPatch("{projectId:guid}/members/{userId:guid}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		public async Task<ActionResult<ApiResponse<ProjectMemberDto>>> UpdateProjectMemberRole(
			Guid projectId,
			Guid userId,
			[FromBody] UpdateProjectMemberRoleDto updateProjectMemberRoleDto,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Admin,
				cancellationToken
			);

			var project = await projectRepository.GetByIdAsync(projectId, cancellationToken)
				?? throw AppException.NotFound($"Project with ID '{projectId}' was not found.", ErrorCodes.PROJECT_NOT_FOUND);

			if (project.OwnerId == userId)
			{
				throw AppException.BadRequest("Cannot change the role of the project owner.", ErrorCodes.VALIDATION_ERROR);
			}

			var member = await projectRepository.GetMemberAsync(projectId, userId, cancellationToken)
				?? throw AppException.NotFound("User is not a member of this project.", ErrorCodes.NOT_FOUND);

			member.Role = updateProjectMemberRoleDto.Role;
			projectRepository.UpdateMember(member);
			await projectRepository.SaveChangesAsync(cancellationToken);

			var user = await userRepository.GetByIdAsync(userId, cancellationToken);

			var memberDto = new ProjectMemberDto(
				userId,
				user?.Name ?? string.Empty,
				user?.Email ?? string.Empty,
				user?.ProfilePictureUrl,
				member.Role,
				member.JoinedAt
			);

			return Ok(ApiResponse<ProjectMemberDto>.Success(memberDto, "Member role updated successfully."));
		}

		[HttpDelete("{projectId:guid}/members/{userId:guid}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		public async Task<ActionResult<ApiResponse<bool>>> RemoveProjectMember(
			Guid projectId,
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Admin,
				cancellationToken
			);

			var project = await projectRepository.GetByIdAsync(projectId, cancellationToken)
				?? throw AppException.NotFound($"Project with ID '{projectId}' was not found.", ErrorCodes.PROJECT_NOT_FOUND);

			if (project.OwnerId == userId)
			{
				throw AppException.BadRequest("Cannot remove the project owner from the project.", ErrorCodes.VALIDATION_ERROR);
			}

			var member = await projectRepository.GetMemberAsync(projectId, userId, cancellationToken)
				?? throw AppException.NotFound("Member not found in this project.", ErrorCodes.NOT_FOUND);

			projectRepository.RemoveMember(member);
			await projectRepository.SaveChangesAsync(cancellationToken);

			return Ok(ApiResponse<bool>.Success(true, "Member removed from project successfully."));
		}
	}
}
