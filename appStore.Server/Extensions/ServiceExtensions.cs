using appStore.Server.Repositories;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services;
using appStore.Server.Services.Interfaces;
using appStore.Server.Services.Security;

namespace appStore.Server.Extensions
{
	public static class ServiceExtensions
	{
		public static WebApplicationBuilder AddApplicationServices( this WebApplicationBuilder builder )
		{
			// Repositories
			builder.Services.AddScoped<ITaskRepository, TaskRepository>();
			builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
			builder.Services.AddScoped<IBoardRepository, BoardRepository>();
			builder.Services.AddScoped<IPriorityRepository, PriorityRepository>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
			builder.Services.AddScoped<IRoleRepository, RoleRepository>();
			builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
			builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
			builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
			builder.Services.AddScoped<IPasswordPolicyRepository, PasswordPolicyRepository>();
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

			// Services
			builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
			builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
			builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
			builder.Services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
			builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
			builder.Services.AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>();
			builder.Services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
			builder.Services.AddSingleton<IPasswordPolicyValidator, PasswordPolicyValidator>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddSingleton<ITaskStateMachine, TaskStateMachine>();
			builder.Services.AddScoped<IBoardService, BoardService>();
			builder.Services.AddScoped<ITaskService, TaskService>();
			builder.Services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

			return builder;
		}
	}
}