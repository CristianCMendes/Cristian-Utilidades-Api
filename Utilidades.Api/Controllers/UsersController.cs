using EntityEase.Models.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Identity.Interface;
using Utilidades.Api.Models.Pagination;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Models.SecretFriend;
using Utilidades.Api.Services;

namespace Utilidades.Api.Controllers;

public class UsersController(
    UtilDbContext dbContext,
    IUserService userService,
    IMailService mailService,
    IConfiguration configuration)
    : ApiControllerBase {
    [HttpGet(nameof(List))]
    [Authorize(Roles = $"{nameof(RoleType.Master)},{nameof(RoleType.ListAllUsers)}")]
    public async Task<IResponse> List(Pagination pagination) {
        return new Response(await dbContext.Users.PaginateAsync<UserResponse>(pagination)) {
            StatusCode = 200
        };
    }

    [HttpGet("{id}")]
    public async Task<IResponse> Get(int id) {
        return new Response(await dbContext.Users.WhereId(id).FirstOrDefaultAsync()) {
            StatusCode = 200
        };
    }

    [HttpPost("{id}/addRole")]
    [Authorize(Roles = nameof(RoleType.Master))]
    public async Task<IResponse> AddRole(int id, [FromBody] string role) {
        var user = await dbContext.Users.WhereId(id).Include(x => x.Roles).FirstOrDefaultAsync();

        if (user is null)
            return new Response {
                StatusCode = StatusCodes.Status404NotFound,
                Messages = {
                    new() {
                        Message = "Usuario não encontrado",
                        Type = MessageType.warning
                    }
                }
            };

        var roleParsed = Enum.Parse<RoleType>(role);

        if (user.Roles.Any(x => x.Role == roleParsed)) {
            return new Response {
                StatusCode = StatusCodes.Status200OK,
                Messages = {
                    new() {
                        Message = "O usuario já faz parte da role",
                        Type = MessageType.info
                    }
                }
            };
        }

        user.Roles.Add(new() {
            Role = roleParsed
        });

        await dbContext.SaveChangesAsync();

        return new Response(user);
    }

    [HttpGet("{id}/SecretFriends")]
    [Authorize(Roles = nameof(RoleType.Master))]
    public async Task<IResponse> GetSecretFriends(int id, Pagination pagination) {
        return new Response<SecretFriend[]>(await dbContext.SecretFriends
            .Include(s => s.Members.Where(m => m.UserId == id))
            .ThenInclude(x => x.UserId)
            .Where(s => s.Members.Any(m => m.UserId == id))
            .PaginateAsync(pagination));
    }

    [HttpPost(nameof(InviteUser))]
    [ProducesResponseType<IUser>(200)]
    public async Task<IResponse> InviteUser([FromBody] UserInviteDto data) {
        var response = await userService.Invite(data, HttpContext.GetCurrentUserId());
        await dbContext.SaveChangesAsync();

        if (response.Data is { } user) {
            await mailService.SendMailAsync("Você recebeu um convite",
                // Language=html
                new($"""
                     <div> Olá, você recebeu um convite para criar uma conta na plataforma de utilidades do Korsa,
                     acesse a plataforma para confirmar sua conta
                     <a href="{configuration.GetValue<string>("App:FrontendUrl")}">Clique aqui para ir</a>
                     </div>
                     """), user.Email);
        }

        return response;
    }
}