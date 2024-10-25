// Ignore Spelling: Jwt

using Core.DTO.Authentication;
using Core.Identity;
using Core.Settings;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services;

public interface IJwtService
{
    /// <summary>
    /// this makes the service not pure, as in it knows more than it should (a librarian is shaking his head).
    /// at the same time tho, it's easier for an example.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>A ready to send Response with the token</returns>
    AuthenticationResponse CreateJwtToken(ApplicationUser user);

    /// <summary>
    /// decode a token and return it's deep and dark hidden knowledge . . . or the user email >.>
    /// </summary>
    /// <param name="token"></param>
    /// <returns>the user information stored in the token</returns>
    ClaimsPrincipal? GetPrincipleFromJwtToken(string? token);
}
public class JwtService : IJwtService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettings _jwtSettings;
    private readonly RefreshTokenSettings _refreshTokenSettings;

    public JwtService(
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtSettings> jwtOptions,
        IOptions<RefreshTokenSettings> refreshTokenOptions)
    {
        _dateTimeProvider = dateTimeProvider;
        _jwtSettings = jwtOptions.Value;
        _refreshTokenSettings = refreshTokenOptions.Value;
    }

    public AuthenticationResponse CreateJwtToken(ApplicationUser user)
    {
        var tokenIssueDate = _dateTimeProvider.UtcNow;
        // 1. create the claims 
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // user identity
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // token ID
            new Claim(JwtRegisteredClaimNames.Iat, tokenIssueDate.ToString()) ,// when it was issued
            new Claim(JwtRegisteredClaimNames.Email,user.Email!),
            new Claim(JwtRegisteredClaimNames.NameId ,user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Email!) // anything that uniquely identifies the user
        };
        // 2. the signing key
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret) // can be in either: env variable or user secrets
            ),
            SecurityAlgorithms.HmacSha512
        );

        // 3. security token descriptor, cause this is the new thing it seems
        // it . . . . describes the token information (￣﹃￣) 
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = _jwtSettings.Audience,
            Expires = tokenIssueDate.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            SigningCredentials = signingCredentials,
            NotBefore = null,
            IssuedAt = tokenIssueDate

        };
        // 4. return anything, like this object
        return new AuthenticationResponse
        {
            Email = user.Email,
            Expiration = descriptor.Expires.Value,
            DisplayName = user.UserName,
            Token = new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor),
            RefreshToken = GenerateRefershToken(),
            RefreshTokenExpirationDateTime = tokenIssueDate.AddMinutes(_refreshTokenSettings.ExpiryMinutes)
        };
    }

    public ClaimsPrincipal? GetPrincipleFromJwtToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha512],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)
            ),
            ValidateLifetime = false // should be false, cause the token passed can be expired
        };

        JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
        var principle = jwtTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if ( // check if the token is invalid
            securityToken is not JwtSecurityToken jwtSecurityToken
            || !jwtSecurityToken.Header.Alg // or if the algo mismatch, ignoring casing; (HMAC522 =? hmac522) -> true
                .Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase)
           )
        {
            // can be a custom exception or a Result<ClaimsPrincipal?>.Failure("Invalid Token").
            throw new SecurityTokenException("Invalid Token");
        }

        return principle;
    }


    /// <summary>
    /// generates a base64 string from a random secure number using cryptography
    /// </summary>
    /// <returns>base64 string</returns>
    private static string GenerateRefershToken()
    {
        var bytes = new byte[64];
        var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
