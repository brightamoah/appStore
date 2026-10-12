using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public sealed class PasswordPolicyConfiguration : IEntityTypeConfiguration<PasswordPolicy>
	{
		public void Configure(
		   EntityTypeBuilder<PasswordPolicy> builder)
		{
			builder.HasKey(policy => policy.Id);

			builder.Property(policy => policy.MinimumLength)
				.IsRequired();

			builder.Property(policy => policy.RequireUppercase)
				.IsRequired();

			builder.Property(policy => policy.RequireLowercase)
				.IsRequired();

			builder.Property(policy => policy.RequireDigit)
				.IsRequired();

			builder.Property(policy => policy.RequireSpecialCharacter)
				.IsRequired();

			builder.Property(policy => policy.CreatedAt)
				.IsRequired();

			builder.Property(policy => policy.UpdatedAt)
				.IsRequired();
		}
	}
}
