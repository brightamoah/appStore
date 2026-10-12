using appStore.Server.Errors;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services.Security
{
	public sealed class PasswordPolicyValidator : IPasswordPolicyValidator
	{
		public void Validate(
			string password,
			PasswordPolicyDefinition policy
		)
		{
			ArgumentNullException.ThrowIfNull( password );

			if (password.Length < policy.MinimumLength)
			{
				throw WeakPassword(
					$"Password must be at least " +
					$"{policy.MinimumLength} characters long."
				);
			}

			if (
				policy.RequireUppercase &&
				!password.Any( char.IsUpper )
			)
			{
				throw WeakPassword(
					"Password must contain at least one uppercase letter."
				);
			}

			if (
				policy.RequireLowercase &&
				!password.Any( char.IsLower )
			)
			{
				throw WeakPassword(
					"Password must contain at least one lowercase letter."
				);
			}

			if (
				policy.RequireDigit &&
				!password.Any( char.IsDigit )
			)
			{
				throw WeakPassword(
					"Password must contain at least one number."
				);
			}

			if (
				policy.RequireSpecialCharacter &&
				!password.Any( IsSpecialCharacter )
			)
			{
				throw WeakPassword(
					"Password must contain at least one special character."
				);
			}
		}

		private static bool IsSpecialCharacter( char character )
		{
			return !char.IsLetterOrDigit( character );
		}

		private static AppException WeakPassword( string message )
		{
			return new AppException(
				StatusCodes.Status400BadRequest,
				AuthErrorCodes.WEAK_PASSWORD,
				message
			);
		}
	}
}