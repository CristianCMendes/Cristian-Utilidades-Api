using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
    public async Task<IApiResponse> Login(
        [Bind(nameof(user.Email), nameof(user.Password))] [FromBody]
        UserLoginDto user) {
        ApiResponse.Links.AddRange(
            LinkRef(nameof(Register), routeData: new { user.Email, password = "*******" }, method: Method.POST),
            LinkRef(nameof(ConfirmMail), routeData: new { user.Email, token = 111111 }, method: Method.POST));

        return await AuthenticationService.AuthenticateAndGenerateTokenAsync(user);
    }

    [HttpPost(nameof(Register))]
    [ProducesResponseType<IUser>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IApiResponse> Register(
        [Bind(nameof(userCreate.Name), nameof(userCreate.Email), nameof(userCreate.Password))] [FromBody]
        UserCreateDto userCreate) {

        EntityEntry<User> created;

        ApiResponse.Links.AddRange(
            LinkRef(nameof(Login), routeData: new { userCreate.Email, password = "*******" }, method: Method.POST),
            LinkRef(nameof(ConfirmMail), routeData: new { userCreate.Email, token = 111111 }, method: Method.POST));


        if (await dbContext.Users.FirstOrDefaultAsync(x => x.Email == userCreate.Email) is { } userFound) {
            // 1) Se já confirmou e-mail, sempre conflita (independe do tempo)
            if (userFound.IsEmailConfirmed) {
                ApiResponse.Messages.Add(new() {
                    Message = "Já existe um usuario com esse e-mail",
                    Type = MessageType.warning
                });
                ApiResponse.StatusCode = StatusCodes.Status409Conflict;

                return ApiResponse;
            }

            // 2) Usuário não confirmado: respeita janela de 10 minutos (use UTC para evitar problemas de fuso/servidor)
            var withinWindow = userFound.CreatedAt.AddMinutes(10) >= DateTime.Now;
            if (withinWindow) {
                ApiResponse.StatusCode = StatusCodes.Status409Conflict;
                ApiResponse.Messages.Add(
                    new(
                        "Usuário já criado e aguardando confirmação. Verifique seu e-mail ou aguarde alguns minutos para reenviar o código.",
                        MessageType.info)
                );


                return ApiResponse;
            }

            // 3) Fora da janela: atualiza o registro pendente
            created = dbContext.Users.Update(userFound with {
                Password = userCreate.Encrypt(),
                CreatedAt = DateTime.Now,
                Name = userCreate.Name,
            });
        }
        else {
            // If not found, create new user
            created = dbContext.Users.Add(userCreate);
        }


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
            ApiResponse = await AuthenticationService.AuthenticateAndGenerateTokenAsync(userCreate);
            ApiResponse.StatusCode = StatusCodes.Status201Created;

            return ApiResponse;
        }

        UserOtp otp = new() {
            UserId = created.Entity.Id,
        };
        redisService.Set(userCreate.Email, otp, otp.Expires);

        await mailService.SendMailAsync("Sua senha de uso unico", new($"<div>Sua senha de uso unico é: {otp.Otp}</div>"),
            userCreate.Email);
        ApiResponse.SetData(created.Entity);
        ApiResponse.Messages.Add(
            new("Usuario criado verifique seu e-mail para obter sua de uso unico", MessageType.success)
        );

        return ApiResponse;
    }

    [HttpPost(nameof(ConfirmMail))]
    public async Task<IApiResponse> ConfirmMail(UserConfirmMail data) {
        if (await dbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Trim() == data.Email.AsInsensitive()) is
            { } user) {
            ApiResponse = await ConfirmMail(user.Id, data.Token);

            return ApiResponse;
        }

        ApiResponse.StatusCode = StatusCodes.Status404NotFound;
        ApiResponse.Messages.Add(new("Email não encontrado", MessageType.warning));

        return ApiResponse;
    }


    /// <summary>
    /// Confirma o email do usuario
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("{id}/" + nameof(ConfirmMail))]
    public async Task<IApiResponse> ConfirmMail(int id, int token) {
        if (await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id) is not UserResponse user) {
            ApiResponse.Messages.Add(new("Usuário não encontrado", MessageType.warning));
            ApiResponse.StatusCode = StatusCodes.Status404NotFound;

            return ApiResponse;
        }

        var otp = redisService.Get<UserOtp>(user.Email);

        if (otp is null || otp.Otp != token.ToString()) {
            ApiResponse.Messages.Add(new("Token inválido", MessageType.warning));
            ApiResponse.StatusCode = StatusCodes.Status400BadRequest;

            return ApiResponse;
        }

        user.IsEmailConfirmed = true;
        await dbContext.SaveChangesAsync();

        redisService.Remove(user.Email);

        return ApiResponse;
    }
}