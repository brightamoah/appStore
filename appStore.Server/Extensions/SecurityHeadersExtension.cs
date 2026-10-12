namespace appStore.Server.Extensions
{
	public static class SecurityHeadersExtension
	{
		public static IApplicationBuilder UserSecurityHeaders(this IApplicationBuilder app)
		{
			return app.Use(async (context, next) =>
			{
				var headers = context.Response.Headers;
				headers.XContentTypeOptions="nosniff";
				headers.XFrameOptions="DENY";
				headers["Referrer-Policy"] ="strict-origin-when-cross-origin";
				headers["X-Permitted-Cross-Domain-Policies"]="none";
				headers["Permissions-Policy"]="camera=(), microphone=(), geolocation=()";

				if(context.Request.IsHttps)
				{
					headers.StrictTransportSecurity="max-age=31536000; includeSubDomains";
				}
				await next();
			});
		}
	}
}