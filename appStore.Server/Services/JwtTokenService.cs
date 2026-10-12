using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using appStore.Server.Authorization;
using appStore.Server.Configuration;
using appStore.Server.Services.Authorization;
using appStore.Server.Services.Interfaces;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace appStore.Server.Services
{
	public class JwtTokenService( IOptions<JwtOptions> jwtOptions ) : IJwtTokenService
	{
		private readonly JwtOptions _jwtOptions = jwtOptions.Value;

		public string CreateAccessToken( Guid userId, string email, UserAuthorizationSnapshot authorization )
		{
			var now = DateTime.UtcNow;

			var claims = new List<Claim>
			{
				new (JwtRegisteredClaimNames.Sub, userId.ToString()),
				new(ClaimTypes.NameIdentifier, userId.ToString()),
				new(JwtRegisteredClaimNames.Email, email),
				new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( _jwtOptions.Secret ) );

			var credentials = new SigningCredentials( key, SecurityAlgorithms.HmacSha256 );

			claims.AddRange(
				authorization
				.Roles
				.Select(role => 
					new Claim(ClaimTypes.Role, role)
				)
			);

			claims.AddRange(
				authorization
				.Permissions
				.Select(permission => 
					new Claim(AuthorizationClaimTypes.Permission, permission)
				)
			);

			var token = new JwtSecurityToken(
				issuer: _jwtOptions.Issuer,
				audience: _jwtOptions.Audience,
				claims,
				notBefore: now,
				expires: now.AddMinutes( _jwtOptions.AccessTokenLifetimeMinutes ),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken( token );
		}
	}
}