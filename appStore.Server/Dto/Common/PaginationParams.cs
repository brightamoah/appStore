using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Common
{
	public record PaginationParams
	{
		private const int MaxPageSize = 100;
		private int _pageSize = 20;
		[Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
		public int PageNumber { get; init; } = 1;
		[Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100.")]
		public int PageSize
		{
			get => _pageSize;
			init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
		}
		public string? Search { get; init; }
		public string? SortBy { get; init; }
		public bool IsDescending { get; init; } = false;
	}
}