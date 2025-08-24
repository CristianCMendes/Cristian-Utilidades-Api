using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Utilidades.Api.Context;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Services;

public class AuthenticationService(UtilDbContext dbContext, IConfiguration configuration) : IAuthenticationService {
    private UtilDbContext DbContext { get; } = dbContext;
    private IConfiguration Configuration { get; } = configuration;

    public Response<UserLoginResponse> AuthenticateAndGenerateToken(UserLoginDto loginDto) {
        var user = GetUser(loginDto);

        return ValidateUser(loginDto, user);
    }

    private Response<UserLoginResponse> ValidateUser(UserLoginDto loginDto, User? user) {
        if (user is null)
            return new() {
                StatusCode = 401,
                Messages = {
                    new() {
                        Message = "Usuario não encontrado",
                        Type = MessageType.Warning
                    }
                }
            };

        if (!user.IsActive) {
            return new() {
                StatusCode = 401,
                Messages = {
                    new() {
                        Message = "Usuario inativo!",
                        Type = MessageType.Warning
                    }
                }
            };
        }

        if (!user.IsEmailConfirmed) {
            return new() {
                StatusCode = 401,
                Messages = {
                    new() {
                        Message = "Usuario não confirmou o e-mail!",
                        Type = MessageType.Warning
                    }
                }
            };
        }

        if (user.Password != loginDto.Password)
            return new() {
                StatusCode = 401,
                Messages = {
                    new() {
                        Message = "Senha incorreta",
                        Type = MessageType.Warning
                    }
                }
            };

        var (token, expiresAt) = GenerateJwt(user);
        var response = new UserLoginResponse(user) {
            Token = token,
            Expires = expiresAt
        };

        return new(response);
    }

    private IQueryable<User> GetUserQuery => DbContext.Users.AsNoTracking().Include(x => x.Roles);

    private User? GetUser(UserLoginDto loginDto) {
        var user = GetUserQuery.FirstOrDefault(u => u.Email == loginDto.Email);

        return user;
    }

    private async Task<User?> GetUserAsync(UserLoginDto email) {
        var user = await GetUserQuery.FirstOrDefaultAsync(u => u.Email == email.Email);

        return user;
    }


    private (string token, DateTime expiresAt) GenerateJwt(User user) {
        // Configurações
        var key = Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key)) {
            throw new InvalidOperationException("Configuração Jwt:Key não definida.");
        }

        var issuer = Configuration["Jwt:Issuer"] ?? "Utilidades.Api";
        var audience = Configuration["Jwt:Audience"] ?? "Utilidades.Api.Client";
        var expiresMinutes = int.TryParse(Configuration["Jwt:ExpiresMinutes"], out var m) ? m : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Claims básicas (ajuste conforme necessidade)
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.Name),
        };

        var roles = user.Roles.Select(x => x.Role.ToString()).Distinct().ToArray();

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));


        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }

    /// <inheritdoc />
    public async Task<Response<UserLoginResponse>> AuthenticateAndGenerateTokenAsync(UserLoginDto loginDto) {
        var user = await GetUserAsync(loginDto);

        return ValidateUser(loginDto, user);
    }
}