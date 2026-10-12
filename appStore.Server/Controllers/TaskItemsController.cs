using appStore.Server.Authorization;
using appStore.Server.Dto.Common;
using appStore.Server.Dto.Priorities;
using appStore.Server.Dto.Tasks;
using appStore.Server.Extensions;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/tasks")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class TaskItemsController(
		ITaskService taskService,
        IProjectAuthorizationService projectAuthorizationService
	) : ControllerBase
	{
		[Authorize]
		[HttpGet("metadata")]
		public async Task<ActionResult<ApiResponse<TaskMetadataDto>>> GetMetadata(
			[FromServices] IPriorityRepository priorityRepository,
			CancellationToken cancellationToken = default
		)
		{
			var allPriorities = await priorityRepository.GetAllAsync(cancellationToken);
			var priorities = allPriorities
				.OrderBy(priority => priority.Id)
				.Select(PriorityDto.FromModel)
				.ToList();

			var statuses = Enum.GetValues<Status>()
				.Select(status => new StatusOptionDto(
					status.ToString(),
					FormatStatusLabel(status)
				))
				.ToList();

			var issueTypes = Enum.GetValues<IssueType>()
				.Select(issueType => new IssueTypeOptionDto(
					issueType.ToString(),
					issueType.ToString()
				))
				.ToList();

			return Ok(
				ApiResponse<TaskMetadataDto>.Success(
					new TaskMetadataDto(statuses, issueTypes, priorities),
					"Task metadata retrieved successfully."
				)
			);
		}

		private static string FormatStatusLabel(Status status) => status switch
		{
			Status.NotStarted => "Not Started",
			Status.InProgress => "In Progress",
			Status.UnderReview => "Under Review",
			Status.ReadyForTest => "Ready For Test",
			Status.Completed => "Completed",
			Status.OnHold => "On Hold",
			Status.Cancelled => "Cancelled",
			Status.Closed => "Closed",
			Status.Archived => "Archived",
			Status.Pending => "Pending",
			_ => status.ToString()
		};

		[HttpGet]
		[RequirePermission(AppPermissions.Tasks.Read)]
		public async Task<ActionResult<ApiResponse<PagedResponse<TasksListDto>>>> GetTasks(
			[FromQuery] Guid? projectId = null,
			[FromQuery] PaginationParams? pagination = null,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
            
			var paginationParameters = pagination ?? new PaginationParams();
            
			if (projectId.HasValue)
			{
				await projectAuthorizationService.EnsureProjectRoleAsync(
					projectId.Value,
					currentUserId,
					ProjectRole.Viewer,
					cancellationToken
				);
			}

			var tasks = await taskService.GetTasksAsync(
				projectId, 
				paginationParameters, 
				cancellationToken
			);
            
			return Ok(
				ApiResponse<PagedResponse<TasksListDto>>.Success(
					tasks, 
					"Tasks retrieved successfully."
				)
			);
		}

		[RequirePermission(AppPermissions.Tasks.Read)]
		[HttpGet("{id:guid}")]
		public async Task<ActionResult<ApiResponse<TaskDto>>> GetTaskById(
			Guid id, 
            CancellationToken cancellationToken
		)
		{
			var task = await taskService.GetTaskByIdAsync(id, cancellationToken);

			if (!task.ProjectId.HasValue)
			{
				return Ok(
					ApiResponse<TaskDto>.Success(
						task,
						"Task retrieved successfully."
					)
				);
			}

			var currentUserId = User.GetRequiredUserId();

			await projectAuthorizationService.EnsureProjectRoleAsync(
				task.ProjectId.Value,
				currentUserId,
				ProjectRole.Viewer,
				cancellationToken
			);


			return Ok(
				ApiResponse<TaskDto>.Success(
					task, 
					"Task retrieved successfully."
				)
			);
		}
        
        [HttpPost]
        [RequirePermission(AppPermissions.Tasks.Create)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(
	        [FromBody] CreateTaskDto dto,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();
            
	        await projectAuthorizationService.EnsureProjectRoleAsync(
		        dto.ProjectId,
		        currentUserId,
		        ProjectRole.Member,
		        cancellationToken
	        );
            
	        var createdTask = await taskService.CreateTaskAsync(
				dto, 
				currentUserId, 
				cancellationToken
			);
            
	        return CreatedAtAction(
		        nameof(GetTaskById),
		        new { id = createdTask.Id },
		        ApiResponse<TaskDto>.Success(
			        createdTask, 
			        "Task created successfully."
			    )
	        );
        }
        
        [HttpPost("{id:guid}/move")]
        [RequirePermission(AppPermissions.Tasks.Update)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<TaskDto>>> MoveTask(
	        Guid id,
	        [FromBody] MoveTaskDto dto,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();
            
	        var existingTask = await taskService.GetTaskByIdAsync(id, cancellationToken);
            
	        if (existingTask.ProjectId.HasValue)
	        {
		        await projectAuthorizationService.EnsureProjectRoleAsync(
			        existingTask.ProjectId.Value,
			        currentUserId,
			        ProjectRole.Member,
			        cancellationToken
		        );
	        }

	        var movedTask = await taskService.MoveTaskAsync(
		        id, 
		        dto, 
		        currentUserId, 
		        cancellationToken
		    );
            
	        return Ok(
		        ApiResponse<TaskDto>.Success(
			        movedTask, 
			        "Task moved successfully."
			    )
		    );
        }
        
        [HttpPost("{id:guid}/status")]
        [RequirePermission(AppPermissions.Tasks.Update)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<TaskDto>>> TransitionStatus(
	        Guid id,
	        [FromBody] StatusTransitionDto dto,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();
            
	        var existingTask = await taskService.GetTaskByIdAsync(id, cancellationToken);
            
	        if (existingTask.ProjectId.HasValue)
	        {
		        await projectAuthorizationService.EnsureProjectRoleAsync(
			        existingTask.ProjectId.Value,
			        currentUserId,
			        ProjectRole.Member,
			        cancellationToken
		        );
	        }

	        var transitionedTask = await taskService.TransitionStatusAsync(
		        id,
		        dto.TargetStatus,
		        currentUserId,
		        cancellationToken
	        );
            
	        return Ok(
		        ApiResponse<TaskDto>.Success(
			        transitionedTask, 
			        "Task status transitioned successfully."
			    )
		    );
        }

        [HttpPatch("{id:guid}")]
        [RequirePermission(AppPermissions.Tasks.Update)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(
            Guid id,
            [FromBody] UpdateTaskDto dto,
            CancellationToken cancellationToken = default
        )
        {
            var currentUserId = User.GetRequiredUserId();

            var existingTask = await taskService.GetTaskByIdAsync(id, cancellationToken);

            if (existingTask.ProjectId.HasValue)
            {
                await projectAuthorizationService.EnsureProjectRoleAsync(
                    existingTask.ProjectId.Value,
                    currentUserId,
                    ProjectRole.Member,
                    cancellationToken
                );
            }

            var updatedTask = await taskService.UpdateTaskAsync(
                id,
                dto,
                currentUserId,
                cancellationToken
            );

            return Ok(
                ApiResponse<TaskDto>.Success(
                    updatedTask,
                    "Task updated successfully."
                )
            );
        }
        
        [HttpDelete("{id:guid}")]
        [RequirePermission(AppPermissions.Tasks.Delete)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteTask(
	        Guid id,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();
            
	        var existingTask = await taskService.GetTaskByIdAsync(id, cancellationToken);
            
	        if (existingTask.ProjectId.HasValue)
	        {
		        await projectAuthorizationService.EnsureProjectRoleAsync(
			        existingTask.ProjectId.Value,
			        currentUserId,
			        ProjectRole.Admin,
			        cancellationToken
		        );
	        }

	        await taskService.DeleteTaskAsync(id, cancellationToken);
            
	        return Ok(
		        ApiResponse<object?>.Success(
					null, 
					"Task deleted successfully."
		        )
	        );
        }
	}
}