using appStore.Server.Services;

namespace appStore.Server.Tests
{
	public class UnitTest1
	{
		[Fact]
		public void HashPassword_And_VerifyPassword_Works()
		{
			var hasher = new PasswordHasher();
			var password = "MyPassword";

			// Act
			var passwordHash = hasher.HashPassword(password);

			Console.WriteLine($"Password: {password}");

			var correctPassword = hasher.VerifyPassword(
				password,
				passwordHash
			);

			Console.WriteLine($"Password Hash: {passwordHash}");

			var wrongPassword = hasher.VerifyPassword(
				"WrongPassword",
				passwordHash
			);

			// Assert
			Assert.True(correctPassword);
			Assert.False(wrongPassword);
		}
	}

}