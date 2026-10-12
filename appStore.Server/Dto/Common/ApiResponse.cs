namespace appStore.Server.Dto.Common
{
	public record ApiResponse<T>(
		string Message,
		T Data
	)
	{
		public static ApiResponse<T> Success(T data, string message)
			 => new(message, data);
	}
}