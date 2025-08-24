using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Identity.Cache;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Services;

namespace Utilidades.Api.Controllers;

[AllowAnonymous]
public class AccountController(
    UtilDbContext dbContext,
    IRedisService redisService,
    IAuthenticationService authenticationService,
    IMailService mailService)
    : ApiControllerBase(dbContext) {
    private UtilDbContext DbContext { get; } = dbContext;
    private IAuthenticationService AuthenticationService { get; } = authenticationService;

    [HttpPost("Login")]
    public async Task<Response<UserLoginResponse>> Login(
        [Bind(nameof(user.Email), nameof(user.Password))] [FromBody]
        UserLoginDto user) {
        var t = HttpContext.Request.Headers;

        return await AuthenticationService.AuthenticateAndGenerateTokenAsync(user);
    }

    [HttpPost("Create")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Response<UserResponse>> Create(
        [Bind(nameof(userCreate.Name), nameof(userCreate.Email), nameof(userCreate.Password))] [FromBody]
        UserCreateDto userCreate) {
        var connectionId = HttpContext.Connection.LocalIpAddress?.ToString() ?? HttpContext.Connection.Id;
        var t = redisService.Get<UserCreatedCache>(connectionId);

        if (await DbContext.Users.FirstOrDefaultAsync(x => x.Email == userCreate.Email) is UserResponse userFound) {
            return new(userFound) {
                Messages = {
                    new() {
                        Message = "Já existe um usuario com esse e-mail",
                        Type = MessageType.Warning
                    }
                },
                Links = {
                    new() {
                        Reference = "/"
                    }
                },
                StatusCode = StatusCodes.Status409Conflict,
            };
        }

        var created = DbContext.Users.Add(userCreate);
        await DbContext.SaveChangesAsync();

        UserOtp otp = new() {
            UserId = created.Entity.Id,
        };
        redisService.Set(userCreate.Email, otp);

        await mailService.SendMailAsync("Sua senha de uso unico", new($"<div>Sua senha de uso unico é: {otp.Otp}</div>"),
            userCreate.Email);


        if (t is not null) {
            t.UserIds = t.UserIds.Append(created.Entity.Id).ToArray();
            redisService.Set(connectionId, t);
        }
        else {
            redisService.Set(connectionId, new UserCreatedCache {
                UserIds = [created.Entity.Id],
                ConnectionId = connectionId,
                InPort = HttpContext.Connection.LocalPort,
                OutPort = HttpContext.Connection.RemotePort,
            });
        }

        if (!UtilDbContext.HasUsers) {
            // Caso não tenha usuarios na hora que o banco foi inicializado, o proximo será administrador.
            created.Entity.Roles.Add(new() {
                UserId = 1,
                Role = RoleType.Master
            });
            created.Entity.IsEmailConfirmed = true;
            await DbContext.SaveChangesAsync();
            UtilDbContext.HasUsers = true;
        }

        return new(created.Entity) {
            StatusCode = 201
        };
    }

    [HttpPost("{id}/ConfirmMail")]
    public async Task<Response<UserResponse>> ConfirmMail(int id, int token) {
        if (await DbContext.Users.FirstOrDefaultAsync(x => x.Id == id) is not UserResponse user) {
            return new Response<UserResponse>() {
                StatusCode = StatusCodes.Status404NotFound,
                Messages = {
                    new() {
                        Message = "Usuario não encontrado",
                        Type = MessageType.Warning
                    }
                }
            };
        }

        var otp = redisService.Get<UserOtp>(user.Email);

        if (otp is null || otp.Otp != token.ToString()) {
            return new() {
                StatusCode = StatusCodes.Status400BadRequest,
                Messages = {
                    new() {
                        Message = "Token inválido",
                        Type = MessageType.Warning
                    }
                }
            };
        }

        user.IsEmailConfirmed = true;
        await DbContext.SaveChangesAsync();

        return new(user) {
            Messages = {
                new() {
                    Message = "E-mail confirmado com sucesso",
                    Type = MessageType.Success
                }
            }
        };
    }
}