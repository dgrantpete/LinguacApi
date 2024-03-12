using LinguacApi.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LinguacApi.Services.JwtHandler
{
    public class JwtHandler : IJwtHandler
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

        public JwtHandler(IOptions<JwtConfiguration> jwtConfiguration)
        {
            _jwtConfiguration = jwtConfiguration.Value;

            _accessTokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtConfiguration.Issuer,
                ValidAudience = _jwtConfiguration.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.AccessSecret))
            };

            _refreshTokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtConfiguration.Issuer,
                ValidAudience = _jwtConfiguration.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.RefreshSecret))
            };
        }

        private readonly JwtConfiguration _jwtConfiguration;

        private readonly TokenValidationParameters _accessTokenValidationParameters;

        private readonly TokenValidationParameters _refreshTokenValidationParameters;

        public TokenResult GenerateAccessToken(Guid userId, IEnumerable<string> roles)
        {
            IEnumerable<Claim> claims = [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())];

            claims = claims.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(_jwtConfiguration.AccessExpirationSeconds),
                SigningCredentials = new(_accessTokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtConfiguration.Issuer,
                Audience = _jwtConfiguration.Audience
            };

            JwtSecurityToken jwt = _jwtSecurityTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return new TokenResult(_jwtSecurityTokenHandler.WriteToken(jwt), jwt.ValidTo);
        }

        public TokenResult GenerateRefreshToken(Guid userId)
        {
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]),
                Expires = DateTime.UtcNow.AddSeconds(_jwtConfiguration.RefreshExpirationSeconds),
                SigningCredentials = new(_refreshTokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtConfiguration.Issuer,
                Audience = _jwtConfiguration.Audience
            };

            JwtSecurityToken jwt = _jwtSecurityTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return new TokenResult(_jwtSecurityTokenHandler.WriteToken(jwt), jwt.ValidTo);
        }

        public ClaimsPrincipal? ValidateAccessToken(string accessToken)
        {
            try
            {
                return _jwtSecurityTokenHandler.ValidateToken(accessToken, _accessTokenValidationParameters, out SecurityToken _);
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
                return _jwtSecurityTokenHandler.ValidateToken(refreshToken, _refreshTokenValidationParameters, out SecurityToken _);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
