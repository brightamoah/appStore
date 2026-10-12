namespace appStore.Server.Errors
{
	public static class ErrorHandlingExtensions
	{
		public static IApplicationBuilder UseGlobalExceptionHandling( this IApplicationBuilder app )
		{
			return app.UseMiddleware<GlobalExceptionMiddleware>();
		}
	}
}