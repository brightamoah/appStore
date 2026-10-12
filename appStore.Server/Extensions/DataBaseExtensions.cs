using appStore.Server.Authorization;
using appStore.Server.Data;
using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Extensions
{
	public static class DataBaseExtensions
	{
		public static void MigrateDatabase(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();

			var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

			dbContext.Database.Migrate();
		}


		public static void AddDatabaseConnection(this WebApplicationBuilder builder)
		{

			var connectionString = builder.Configuration.GetConnectionString("TasksDB");

			builder.Services.AddSqlite<DatabaseContext>(
				connectionString,
				optionsAction: options => options.UseSeeding((context, _) =>
				{
					if (!context.Set<Priority>().Any())
					{
						context.Set<Priority>().AddRange(
							new Priority { Name = "Low" },
							new Priority { Name = "Medium" },
							new Priority { Name = "High" },
							new Priority { Name = "Emergency" }
						);

						context.SaveChanges();
					}

					if (!context.Set<Role>().Any())
					{
						context.Set<Role>().AddRange(
								new Role
								{
									Name = "user",
									Description = "Default role for newly registered users"
								},
								new Role
								{
									Name = "admin",
									Description = "Administrator role with full permissions"
								},
								new Role
								{
									Name = "manager",
									Description = "Manager role with elevated permissions"
								}
						);

						context.SaveChanges();
					}

					var existingPermissions = context.Set<Permission>()
						.Select(permission => permission.Name)
						.ToHashSet(StringComparer.OrdinalIgnoreCase);

					var missingPermissions = AppPermissions.All
						.Where(permission => !existingPermissions.Contains(permission.Name))
						.Select(permission => new Permission
						{
							Name = permission.Name,
							Description = permission.Description
						})
						.ToList();

					if (missingPermissions.Count > 0)
					{
						context.Set<Permission>().AddRange(missingPermissions);
						context.SaveChanges();
					}
				})
				);
		}
	}
}