namespace appStore.Server.Models
{
	public class PasswordPolicy
	{
		public int Id { get; set; }

		public int MinimumLength { get; set; }

		public bool RequireUppercase { get; set; }

		public bool RequireLowercase { get; set; }

		public bool RequireDigit { get; set; }

		public bool RequireSpecialCharacter { get; set; }

		public bool IsEnabled { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }
	}
}