using appStore.Server.Dto.Boards;
using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public sealed class BoardService(
		IBoardRepository boardRepository,
		IProjectRepository projectRepository
	) : IBoardService
	{
		public async Task<BoardDto> GetBoardAsync(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			var project = await projectRepository.GetByIdAsync(projectId, cancellationToken)
				?? throw AppException.NotFound(
					$"Project with ID '{projectId}' was not found.",
					ErrorCodes.PROJECT_NOT_FOUND
				);

			var columns = await GetColumnsAsync(projectId, cancellationToken);

			return new BoardDto(
				project.Id,
				project.Name,
				project.Key,
				columns
			);
		}

		public async Task<IReadOnlyList<BoardColumnDto>> GetColumnsAsync(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			var columns = await boardRepository.GetColumnsWithTasksAsync(projectId, cancellationToken);

			if(columns.Count == 0)
			{
				return await ProvisionDefaultColumnsAsync(projectId, cancellationToken);
			}

			return
			[
				.. columns.Select(
					boardColumn => BoardColumnDto.FromModel(
						boardColumn,
						boardColumn.Tasks.Count
					)
				)
			];
		}

		public async Task<BoardColumnDto> CreateColumnAsync(
			Guid projectId,
			CreateColumnDto createColumnDto,
			CancellationToken cancellationToken = default
		)
		{
			var projectExists = await projectRepository.AnyAsync(
				project => project.Id == projectId,
				cancellationToken
			);

			if(!projectExists)
			{
				throw AppException.NotFound(
					$"Project with ID '{projectId}' was not found.",
					ErrorCodes.PROJECT_NOT_FOUND
				);
			}

			var maxOrder = await boardRepository.GetMaxOrderIndexAsync(projectId, cancellationToken);

			var column = new BoardColumn
			{
				Id = Guid.NewGuid(),
				ProjectId = projectId,
				Name = createColumnDto.Name.Trim(),
				OrderIndex = maxOrder + 1,
				StatusMapping = createColumnDto.StatusMapping,
				WipLimit = createColumnDto.WipLimit,
				Color = createColumnDto.Color,
				IsDefault = false,
				CreatedAt = DateTime.UtcNow
			};

			await boardRepository.AddAsync(column, cancellationToken);
			await boardRepository.SaveChangesAsync(cancellationToken);

			return BoardColumnDto.FromModel(column, taskCount: 0);
		}

		public async Task<BoardColumnDto> UpdateColumnAsync(
			Guid projectId,
			Guid columnId,
			UpdateColumnDto updateColumnDto,
			CancellationToken cancellationToken = default
		)
		{
			var columns = await boardRepository.GetColumnsWithTasksAsync(projectId, cancellationToken);

			var targetColumn = columns.FirstOrDefault(column => column.Id == columnId)
				?? throw AppException.NotFound(
					$"Column with ID '{columnId}' was not found in project.",
					ErrorCodes.COLUMN_NOT_FOUND
				);

			targetColumn.Name = updateColumnDto.Name.Trim();
			targetColumn.StatusMapping = updateColumnDto.StatusMapping;
			targetColumn.WipLimit = updateColumnDto.WipLimit;
			targetColumn.Color = updateColumnDto.Color;

			boardRepository.Update(targetColumn);

			await boardRepository.SaveChangesAsync(cancellationToken);

			return BoardColumnDto.FromModel(targetColumn, targetColumn.Tasks.Count);
		}

		public async Task ReorderColumnsAsync(
			Guid projectId,
			ReorderColumnsDto reorderColumnsDto,
			CancellationToken cancellationToken = default
		)
		{
			var columns = await boardRepository.GetColumnsByProjectIdAsync(projectId, cancellationToken);

			for(var orderIndex = 0; orderIndex < reorderColumnsDto.OrderedColumnIds.Count; orderIndex++)
			{
				var columnId = reorderColumnsDto.OrderedColumnIds[orderIndex];
				var matchedColumn = columns.FirstOrDefault(column => column.Id == columnId);

				if (matchedColumn == null) continue;

				matchedColumn.OrderIndex = orderIndex;
				boardRepository.Update(matchedColumn);
			}

			await boardRepository.SaveChangesAsync(cancellationToken);
		}

		public async Task DeleteColumnAsync(
			Guid projectId,
			Guid columnId,
			Guid? fallbackColumnId = null,
			CancellationToken cancellationToken = default
		)
		{
			var columns = await boardRepository.GetColumnsWithTasksAsync(projectId, cancellationToken);

			var columnToDelete = columns.FirstOrDefault(column => column.Id == columnId)

				?? throw AppException.NotFound(
					$"Column with ID '{columnId}' was not found.",
					ErrorCodes.COLUMN_NOT_FOUND
				);

			if(columnToDelete.Tasks.Count > 0)
			{
				if(fallbackColumnId is null)
				{
					throw AppException.Conflict(
						$"Cannot delete column '{columnToDelete.Name}' because it contains {columnToDelete.Tasks.Count} task(s). Specify a fallback column to move them to.",
						ErrorCodes.COLUMN_HAS_TASKS
					);
				}

				var fallbackColumn = columns.FirstOrDefault(column => column.Id == fallbackColumnId.Value)
					?? throw AppException.NotFound(
						$"Fallback column with ID '{fallbackColumnId}' was not found.",
						ErrorCodes.COLUMN_NOT_FOUND
					);

				// Move all tasks to fallback column and update their status to match
				foreach(var taskItem in columnToDelete.Tasks)
				{
					taskItem.ColumnId = fallbackColumn.Id;
					taskItem.Status = fallbackColumn.StatusMapping;
					taskItem.UpdatedAt = DateTime.UtcNow;
				}
			}

			boardRepository.Remove(columnToDelete);
			await boardRepository.SaveChangesAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<BoardColumnDto>> ProvisionDefaultColumnsAsync(
			Guid projectId,
			CancellationToken cancellationToken = default
		)
		{
			var defaultColumns = BoardColumn.CreateDefaultColumns(projectId);

			await boardRepository.AddRangeAsync(defaultColumns, cancellationToken);

			await boardRepository.SaveChangesAsync(cancellationToken);

			return [.. defaultColumns
				.Select(
					boardColumn => BoardColumnDto
						.FromModel(
							boardColumn, 
							taskCount: 0
						)
				)
			];
		}
	}
}