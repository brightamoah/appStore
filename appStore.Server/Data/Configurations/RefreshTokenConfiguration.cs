using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure( EntityTypeBuilder<RefreshToken> builder )
		{
			builder.HasKey( refreshToken => refreshToken.Id );

			builder.Property( refreshToken => refreshToken.TokenHash )
				.IsRequired();

			builder.Property( refreshToken => refreshToken.TokenFamilyId )
				.IsRequired();

			builder.Property( refreshToken => refreshToken.RememberMe )
				.IsRequired();

			builder.Property( refreshToken => refreshToken.CreatedAt )
				.IsRequired();

			builder.Property( refreshToken => refreshToken.ExpiresAt )
				.IsRequired();

			builder.Property( refreshToken => refreshToken.FamilyExpiresAt )
				.IsRequired();

			builder.HasIndex( refreshToken => refreshToken.TokenHash )
				.IsUnique();

			builder.HasIndex( refreshToken => refreshToken.TokenFamilyId );

			builder.HasIndex( refreshToken => refreshToken.UserId );

			builder.HasOne( refreshToken => refreshToken.User )
			   .WithMany( user => user.RefreshTokens )
			   .HasForeignKey( refreshToken => refreshToken.UserId )
			   .OnDelete( DeleteBehavior.Cascade );
		}
	}
}