using LinguacApi.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LinguacApi.Services.Authentication.TokenHandler
{
	public class JwtTokenHandler : ITokenHandler
	{
		private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

		private readonly AuthTokenConfiguration tokenConfiguration;

		private readonly TokenValidationParameters accessTokenValidationParameters;

		private readonly TokenValidationParameters refreshTokenValidationParameters;

		public JwtTokenHandler(IOptions<AuthTokenConfiguration> tokenConfiguration)
		{
			this.tokenConfiguration = tokenConfiguration.Value;

			accessTokenValidationParameters = new()
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = this.tokenConfiguration.Issuer,
				ValidAudience = this.tokenConfiguration.Audience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.tokenConfiguration.AccessSecret))
			};

			refreshTokenValidationParameters = new()
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = this.tokenConfiguration.Issuer,
				ValidAudience = this.tokenConfiguration.Audience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.tokenConfiguration.RefreshSecret))
			};
		}

		public TokenResult GenerateAccessToken(Guid userId, IEnumerable<string> roles)
		{
			IEnumerable<Claim> claims = [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())];

			claims = claims.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			SecurityTokenDescriptor tokenDescriptor = new()
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.Add(tokenConfiguration.AccessExpirationTime),
				SigningCredentials = new(accessTokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature),
				Issuer = tokenConfiguration.Issuer,
				Audience = tokenConfiguration.Audience
			};

			JwtSecurityToken jwt = jwtSecurityTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

			return new TokenResult(jwtSecurityTokenHandler.WriteToken(jwt), jwt.ValidTo);
		}

		public TokenResult GenerateRefreshToken(Guid userId)
		{
			SecurityTokenDescriptor tokenDescriptor = new()
			{
				Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]),
				Expires = DateTime.UtcNow.Add(tokenConfiguration.RefreshExpirationTime),
				SigningCredentials = new(refreshTokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature),
				Issuer = tokenConfiguration.Issuer,
				Audience = tokenConfiguration.Audience
			};

			JwtSecurityToken jwt = jwtSecurityTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

			return new TokenResult(jwtSecurityTokenHandler.WriteToken(jwt), jwt.ValidTo);
		}

		public ClaimsPrincipal? ValidateAccessToken(string accessToken)
		{
			try
			{
				return jwtSecurityTokenHandler.ValidateToken(accessToken, accessTokenValidationParameters, out SecurityToken _);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
		{
			try
			{
				return jwtSecurityTokenHandler.ValidateToken(refreshToken, refreshTokenValidationParameters, out SecurityToken _);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
