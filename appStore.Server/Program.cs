
using System.Text.Json.Serialization;
using appStore.Server.Errors;
using appStore.Server.Extensions;


namespace appStore.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);


			builder.AddDatabaseConnection();
			builder.AddApplicationServices();
			builder.AddApiBehaviour();
			builder.AddJwtAuthentication();
			builder.AddAuthorizationServices();
			builder.Services.AddRateLimiting();

			builder.Services.AddControllers()
				.AddJsonOptions(jsonOptions =>
				{
					jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				});
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			app.UseGlobalExceptionHandling();
			app.UserSecurityHeaders();

			app.UseDefaultFiles();
			app.MapStaticAssets();

			app.MigrateDatabase();

			if(app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			app.UseRateLimiter();

			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}