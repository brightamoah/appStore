namespace appStore.Server.Services.Authorization
{
	public sealed record UserAuthorizationSnapshot(
		IReadOnlyCollection<string> Roles,
		IReadOnlyCollection<string> Permissions
	);
}