using EntityEase.Models.Extensions;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Pagination;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Models.SecretFriend;

namespace Utilidades.Api.Controllers;

public class UsersController(UtilDbContext dbContext) : ApiControllerBase(dbContext) {
    private UtilDbContext DbContext { get; } = dbContext;

    [HttpGet("List")]
    [Authorize(Roles = $"{nameof(RoleType.Master)},{nameof(RoleType.ListAllUsers)}")]
    public async Task<Response<UserResponse[]>> List(Pagination pagination) {
        return new(await DbContext.Users.PaginateAsync<UserResponse>(pagination)) {
            StatusCode = 200
        };
    }

    [HttpGet("{id}")]
    public async Task<Response<UserResponse>> Get(int id) {
        return new(await DbContext.Users.WhereId(id).FirstOrDefaultAsync()) {
            StatusCode = 200
        };
    }

    [HttpPost("{id}/addRole")]
    [Authorize(Roles = nameof(RoleType.Master))]
    public async Task<Response<UserResponse>> AddRole(int id, [FromBody] string role) {
        var user = await DbContext.Users.WhereId(id).Include(x => x.Roles).FirstOrDefaultAsync();

        if (user is null)
            return new() {
                StatusCode = StatusCodes.Status404NotFound,
                Messages = {
                    new() {
                        Message = "Usuario não encontrado",
                        Type = MessageType.Warning
                    }
                }
            };

        var roleParsed = Enum.Parse<RoleType>(role);

        if (user.Roles.Any(x => x.Role == roleParsed)) {
            return new() {
                StatusCode = StatusCodes.Status200OK,
                Messages = {
                    new() {
                        Message = "O usuario já faz parte da role",
                        Type = MessageType.Info
                    }
                }
            };
        }

        user.Roles.Add(new() {
            Role = roleParsed
        });

        await DbContext.SaveChangesAsync();

        return new(user);
    }

    [HttpGet("{id}/SecretFriends")]
    [Authorize(Roles = nameof(RoleType.Master))]
    public async Task<Response<SecretFriend[]>> GetSecretFriends(int id, Pagination pagination) {
        return new(await DbContext.SecretFriends
            .Include(s => s.Members.Where(m => m.UserId == id))
            .ThenInclude(x => x.UserId)
            .Where(s => s.Members.Any(m => m.UserId == id))
            .PaginateAsync(pagination));
    }
}