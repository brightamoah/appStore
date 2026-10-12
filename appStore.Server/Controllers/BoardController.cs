using appStore.Server.Authorization;
using appStore.Server.Dto.Boards;
using appStore.Server.Dto.Common;
using appStore.Server.Extensions;
using appStore.Server.Models;
using appStore.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/projects/{projectId:guid}/board")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class BoardController(
		IBoardService boardService,
		IProjectAuthorizationService projectAuthorizationService
	) : ControllerBase
	{
		[HttpGet]
		[RequirePermission(AppPermissions.Tasks.Read)]
		public async Task<ActionResult<ApiResponse<BoardDto>>> GetBoard(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
            
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Viewer,
				cancellationToken
			);

			var board = await boardService.GetBoardAsync(projectId, cancellationToken);

			return Ok(
				ApiResponse<BoardDto>.Success(board, 
					"Board retrieved successfully."
				)
			);
		}
        
        
        [HttpGet("columns")]
        [RequirePermission(AppPermissions.Tasks.Read)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<BoardColumnDto>>>> GetColumns(
	        Guid projectId,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();
            
	        await projectAuthorizationService.EnsureProjectRoleAsync(
		        projectId,
		        currentUserId,
		        ProjectRole.Viewer,
		        cancellationToken
	        );

	        var columns = await boardService.GetColumnsAsync(projectId, cancellationToken);

	        return Ok(
				ApiResponse<IReadOnlyList<BoardColumnDto>>.Success(
					columns, 
					"Columns retrieved successfully."
				)
			);
        }

		[HttpPost("columns")]
		[RequirePermission(AppPermissions.Tasks.Update)]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		public async Task<ActionResult<ApiResponse<BoardColumnDto>>> CreateColumn(
			Guid projectId,
			[FromBody] CreateColumnDto dto,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
            
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Member,
				cancellationToken
			);
            
			var createdColumn = await boardService.CreateColumnAsync(projectId, dto, cancellationToken);
            
			return CreatedAtAction(
				nameof(GetColumns),
				new { projectId },
				ApiResponse<BoardColumnDto>.Success(
					createdColumn, 
					"Column created successfully."
				)
			);
		}

		[HttpPut("columns/{columnId:guid}")]
		[RequirePermission(AppPermissions.Tasks.Update)]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		public async Task<ActionResult<ApiResponse<BoardColumnDto>>> UpdateColumn(
			Guid projectId,
			Guid columnId,
			[FromBody] UpdateColumnDto dto,
			CancellationToken cancellationToken = default
		)
		{
			var currentUserId = User.GetRequiredUserId();
            
			await projectAuthorizationService.EnsureProjectRoleAsync(
				projectId,
				currentUserId,
				ProjectRole.Member,
				cancellationToken
			);
            
			var updatedColumn = await boardService.UpdateColumnAsync(
				projectId, 
				columnId, 
				dto, 
				cancellationToken
			);
            
			return Ok(
				ApiResponse<BoardColumnDto>.Success(
					updatedColumn, 
					"Column updated successfully."
				)
			);
		}
        
        [HttpPost("columns/reorder")]
        [RequirePermission(AppPermissions.Tasks.Update)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<object?>>> ReorderColumns(
	        Guid projectId,
	        [FromBody] ReorderColumnsDto dto,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();
            
	        await projectAuthorizationService.EnsureProjectRoleAsync(
		        projectId,
		        currentUserId,
		        ProjectRole.Member,
		        cancellationToken
	        );
            
	        await boardService.ReorderColumnsAsync(
		        projectId, 
		        dto, 
		        cancellationToken
		    );
            
	        return Ok(
				ApiResponse<object?>.Success(
					null, 
					"Columns reordered successfully."
				)
			);
        }

        [HttpDelete("columns/{columnId:guid}")]
        [RequirePermission(AppPermissions.Tasks.Delete)]
        [EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteColumn(
	        Guid projectId,
	        Guid columnId,
	        [FromQuery] Guid? fallbackColumnId = null,
	        CancellationToken cancellationToken = default
        )
        {
	        var currentUserId = User.GetRequiredUserId();

	        await projectAuthorizationService.EnsureProjectRoleAsync(
		        projectId,
		        currentUserId,
		        ProjectRole.Member,
		        cancellationToken
	        );

	        await boardService.DeleteColumnAsync(
		        projectId,
		        columnId,
		        fallbackColumnId,
		        cancellationToken
	        );

	        return Ok(
		        ApiResponse<object?>.Success(
			        null,
			        "Column deleted successfully."
		        )
	        );
        }
	}
}