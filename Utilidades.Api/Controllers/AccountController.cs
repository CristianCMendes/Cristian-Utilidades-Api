using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Identity.Cache;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Identity.Interface;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Services;

namespace Utilidades.Api.Controllers;

[AllowAnonymous]
public class AccountController(
    UtilDbContext dbContext,
    IRedisService redisService,
    IAuthenticationService authenticationService,
    IMailService mailService)
    : ApiControllerBase {
    private IAuthenticationService AuthenticationService { get; } = authenticationService;

    [HttpPost(nameof(Login))]
    [ProducesResponseType<IUserLogin>(200)]
    public async Task<IResponse> Login(
        [Bind(nameof(user.Email), nameof(user.Password))] [FromBody]
        UserLoginDto user) {
        return await AuthenticationService.AuthenticateAndGenerateTokenAsync(user);
    }

    [HttpPost(nameof(Register))]
    [ProducesResponseType<IUser>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResponse> Register(
        [Bind(nameof(userCreate.Name), nameof(userCreate.Email), nameof(userCreate.Password))] [FromBody]
        UserCreateDto userCreate) {
        if (await dbContext.Users.FirstOrDefaultAsync(x => x.Email == userCreate.Email) is UserResponse userFound) {
            // 1) Se já confirmou e-mail, sempre conflita (independe do tempo)
            if (userFound.IsEmailConfirmed) {
                return new Response(userFound) {
                    Messages = {
                        new() {
                            Message = "Já existe um usuario com esse e-mail",
                            Type = MessageType.warning
                        }
                    },
                    Links = {
                        new() {
                            Href = "/"
                        }
                    },
                    StatusCode = StatusCodes.Status409Conflict,
                };
            }

            // 2) Usuário não confirmado: respeita janela de 10 minutos (use UTC para evitar problemas de fuso/servidor)
            var withinWindow = userFound.CreatedAt.AddMinutes(10) >= DateTime.Now;
            if (withinWindow) {
                return new Response() {
                    Messages = {
                        new() {
                            Message =
                                "Usuário já criado e aguardando confirmação. Verifique seu e-mail ou aguarde alguns minutos para reenviar o código.",
                            Type = MessageType.info
                        }
                    },
                    StatusCode = StatusCodes.Status409Conflict,
                };
            }

            // 3) Fora da janela: remove o registro pendente e permite recriar
            dbContext.Users.Remove((userFound as User)!);
        }

        var created = dbContext.Users.Add(userCreate);
        await dbContext.SaveChangesAsync();

        if (!UtilDbContext.HasUsers) {
            // Caso não tenha usuarios na hora que o banco foi inicializado, o proximo será administrador.
            created.Entity.Roles.Add(new() {
                UserId = created.Entity.Id, // Corrige: usar o Id do usuário criado
                Role = RoleType.Master
            });
            created.Entity.IsEmailConfirmed = true;
            await dbContext.SaveChangesAsync();
            UtilDbContext.HasUsers = true;
            var login = await AuthenticationService.AuthenticateAndGenerateTokenAsync(userCreate);

            return new Response(login.Data) {
                StatusCode = 201,
            };
        }

        UserOtp otp = new() {
            UserId = created.Entity.Id,
        };
        redisService.Set(userCreate.Email, otp, otp.Expires);

        await mailService.SendMailAsync("Sua senha de uso unico", new($"<div>Sua senha de uso unico é: {otp.Otp}</div>"),
            userCreate.Email);

        return new Response(new UserLoginResponse(created.Entity)) {
            StatusCode = 201,
            Messages = {
                new() {
                    Message = "Usuario criado, verifique seu e-mail para obter sua senha",
                    Type = MessageType.info
                }
            }
        };
    }

    [HttpPost(nameof(ConfirmMail))]
    public async Task<IResponse> ConfirmMail(UserConfirmMail data) {
        if (await dbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Trim() == data.Email.AsInsensitive()) is
            { } user) {
            return await ConfirmMail(user.Id, data.Token);
        }

        return new Response() {
            StatusCode = StatusCodes.Status404NotFound,
            Messages = {
                new() {
                    Message = "E-mail não encontrado",
                    Type = MessageType.warning
                }
            }
        };
    }


    /// <summary>
    /// Confirma o email do usuario
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("{id}/" + nameof(ConfirmMail))]
    public async Task<IResponse> ConfirmMail(int id, int token) {
        if (await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id) is not UserResponse user) {
            return new Response() {
                StatusCode = StatusCodes.Status404NotFound,
                Messages = {
                    new() {
                        Message = "Usuario não encontrado",
                        Type = MessageType.warning
                    }
                }
            };
        }

        var otp = redisService.Get<UserOtp>(user.Email);

        if (otp is null || otp.Otp != token.ToString()) {
            return new Response() {
                StatusCode = StatusCodes.Status400BadRequest,
                Messages = {
                    new() {
                        Message = "Token inválido",
                        Type = MessageType.warning
                    }
                }
            };
        }

        user.IsEmailConfirmed = true;
        await dbContext.SaveChangesAsync();

        redisService.Remove(user.Email);

        return new Response(user) {
            Messages = {
                new() {
                    Message = "E-mail confirmado com sucesso",
                    Type = MessageType.success
                }
            }
        };
    }
}