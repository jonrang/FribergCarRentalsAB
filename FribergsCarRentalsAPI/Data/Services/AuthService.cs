using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FribergCarRentalsAPI.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApiUser> userManager;
        private readonly IConfiguration configuration;

        public AuthService(UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public async Task<AuthResponse?> LoginUserAsync(LoginUserDto userDto)
        {
            var user = await userManager.FindByEmailAsync(userDto.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, userDto.Password))
            {
                // Do NOT throw an exception here; return null to signal failure.
                return null;
            }

            var (accessToken, accessTokenExpiry) = await GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            var expiresInSeconds = (int)(accessTokenExpiry - DateTime.UtcNow).TotalSeconds;
            var refreshDurationHours = Convert.ToInt32(configuration["JwtSettings:RefreshDurationInHours"]);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(refreshDurationHours);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // _logger.LogError("Failed to update user {UserId} with new refresh token.", user.Id);
                throw new InvalidOperationException("Failed to persist token data during login.");
            }

            return new AuthResponse
            {
                Email = userDto.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                ExpiresIn = expiresInSeconds,
                RefreshTokenExpiry = user.RefreshTokenExpiryTime
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            ClaimsPrincipal principal;

            try
            {
                principal = GetClaimsPrincipalFromExpiredToken(accessToken);
            }
            catch (SecurityTokenException)
            {
                // Token is malformed or signature is invalid (tampered with)
                return null;
            }

            var userName = principal.Identity?.Name;
            if (userName == null)
            {
                return null;
            }

            var user = await userManager.FindByNameAsync(userName);

            if (user == null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryTime == null ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                // Returning null forces the client to perform a full re-login.
                return null;
            }

            var (newAccessToken, newAccessTokenExpiry) = await GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var expiresInSeconds = (int)(newAccessTokenExpiry - DateTime.UtcNow).TotalSeconds;

            var refreshDurationHours = Convert.ToInt32(configuration["JwtSettings:RefreshDurationInHours"]);
            var newRefreshTokenExpiryTime = DateTime.UtcNow.AddHours(refreshDurationHours);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = newRefreshTokenExpiryTime;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Log this error: failed to persist the rotated token.
                throw new InvalidOperationException("Failed to persist rotated token data.");
            }

            return new AuthResponse
            {
                Email = user.Email,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = user.Id,
                ExpiresIn = expiresInSeconds,
                RefreshTokenExpiry = newRefreshTokenExpiryTime
            };
        }

        public async Task<(bool Success, IDictionary<string, string[]> Errors,string? userID, string? token)> RegisterUserAsync(RegisterUserDto userDto, string defaultRole)
        {
            var existingUser = await userManager.FindByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                return (false, new Dictionary<string, string[]>
            {
                { "Email", new[] { "A user with this email address already exists." } }
            }, null, null);
            }

            ApiUser user = new ApiUser()
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                DateOfBirth = userDto.DateOfBirth, 
                DriverLicenseNumber = userDto.DriverLicenseNumber,
                PhoneNumber = userDto.PhoneNumber,
            };

            var result = await userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );
                return (false, errors, null, null);
            }

            var roleResult = await userManager.AddToRoleAsync(user, defaultRole);

            if (!roleResult.Succeeded)
            {
                return (false, new Dictionary<string, string[]>
            {
                { "RoleAssignment", new[] { "User was created but failed to assign the default role." } }
            }, null, null);
            }
            var token =  await GenerateEmailConfirmationTokenAsync(user.Id);

            return (true, new Dictionary<string, string[]>(), user.Id, token);
        }

        public async Task<string?> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        public async Task<(bool Success, string? ErrorMessage)> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "Confirmation failed. The link is invalid or expired.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return (true, null);
            }

            return (false, "Confirmation failed. The link is invalid or expired.");
        }

        private async Task<(string tokenString, DateTime expiryTime)> GenerateToken(ApiUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDurationMinutes = Convert.ToInt32(configuration["JwtSettings:DurationInMinutes"]);
            var tokenExpiryTime = DateTime.UtcNow.AddMinutes(tokenDurationMinutes);

            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(q => new Claim(ClaimTypes.Role, q)).ToList();


            var isOfAge = IsUserOfAge(user);
            var hasLicense = !string.IsNullOrWhiteSpace(user.DriverLicenseNumber);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("IsOfAge", isOfAge.ToString()),
                new Claim("HasDriverLicense", hasLicense.ToString())
            }
            .Union(roleClaims)
            .Union(await userManager.GetClaimsAsync(user));

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: tokenExpiryTime,
                signingCredentials: credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, tokenExpiryTime);
        }

        private bool IsUserOfAge(ApiUser user)
        {
            var minAge = configuration.GetValue<int>("RentalSettings:MinimumDrivingAge");

            var requiredDateOfBirth = DateTime.Today.AddYears(-minAge);

            return user.DateOfBirth <= requiredDateOfBirth;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
    }
}
