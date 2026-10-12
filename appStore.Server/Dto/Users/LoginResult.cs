namespace appStore.Server.Dto.Users
{
	public sealed record LoginResult(
		UserDto User,
		IReadOnlyCollection<string> Roles,
		IReadOnlyCollection<string> Permissions,
		string AccessToken,
		string RefreshToken,
		DateTime RefreshTokenExpiresAt
	);
}