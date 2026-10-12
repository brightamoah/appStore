using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure( EntityTypeBuilder<User> builder )
		{
			builder.Property( user => user.Email )
			.IsRequired()
			.HasMaxLength( 255 );

			builder.HasIndex( user => user.Email )
			.IsUnique();

			builder.Property( user => user.PasswordHash )
			.IsRequired();

			builder.Property( user => user.Name )
				.IsRequired()
				.HasMaxLength( 255 );

			builder.HasMany( user => user.UserRoles )
			.WithOne( userRole => userRole.User )
			.HasForeignKey( userRole => userRole.UserId )
			.OnDelete( DeleteBehavior.Cascade );

			builder.HasMany( user => user.RefreshTokens )
				.WithOne( refreshToken => refreshToken.User )
				.HasForeignKey( refreshToken => refreshToken.UserId )
				.OnDelete( DeleteBehavior.Cascade );
		}
	}
}
