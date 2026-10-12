

using appStore.Server.Services.Interfaces;

using System.Security.Cryptography;

namespace appStore.Server.Services
{
	public class PasswordHasher : IPasswordHasher
	{
		private const string Version = "v1";
		private const string Algorithm = "pbkdf2-sha256";
		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iterations = 600_000;
		public string HashPassword( string password )
		{
			byte[] salt = RandomNumberGenerator.GetBytes( SaltSize );

			byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
				password,
				salt,
				Iterations,
				HashAlgorithmName.SHA256,
				HashSize
			);

			return $"{Version}.{Algorithm}.{Iterations}" +
			$".{Convert.ToBase64String( salt )}" +
			$".{Convert.ToBase64String( hash )}";
		}

		public bool VerifyPassword( string password, string passwordHash )
		{
			string[] parts = passwordHash.Split( '.' );

			if (parts.Length != 5) return false;

			if (parts[0] != Version) return false;

			if (parts[1] != Algorithm) return false;


			if (!int.TryParse( parts[2], out int iterations )) return false;

			byte[] salt;
			byte[] storedHash;

			try
			{
				salt = Convert.FromBase64String( parts[3] );
				storedHash = Convert.FromBase64String( parts[4] );
			}
			catch (FormatException)
			{
				return false;
			}

			byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
				password,
				salt,
				iterations,
				HashAlgorithmName.SHA256,
				storedHash.Length
			);

			return CryptographicOperations.FixedTimeEquals(
				computedHash,
				storedHash
			);
		}
	}
}