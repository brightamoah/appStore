namespace appStore.Server.Dto.Common
{
	public record PagedResponse<T>(
		IReadOnlyList<T> Items,
		int TotalCount,
		int PageNumber,
		int PageSize
	)
	{
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
		public bool HasPreviousPage => PageNumber > 1;
		public bool HasNextPage => PageNumber < TotalPages;

		public static PagedResponse<T> Create(
			IReadOnlyList<T> items,
			int totalCount,
			int pageNumber,
			int pageSize
		)
		{
			return new(
				items,
				totalCount,
				pageNumber,
				pageSize
			);
		}
	}
}