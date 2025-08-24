using EntityEase.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Pagination;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Models.SecretFriend;
using Utilidades.Api.Models.SecretFriend.Dto;

namespace Utilidades.Api.Controllers;

public class SecretFriendController(UtilDbContext dbContext) : ApiControllerBase(dbContext) {
    private UtilDbContext DbContext { get; } = dbContext;

    [HttpGet(nameof(List))]
    public async Task<Response<SecretFriend[]>> List(Pagination pagination) {
        var query = DbContext.SecretFriends.AsQueryable();
        query = query.Where(x => x.Members.Any(m => m.UserId == UserId));

        return new(await query.PaginateAsync(pagination));
    }

    [HttpGet("{id}")]
    public async Task<Response<SecretFriend>> Get(int id) {
        var query = DbContext.SecretFriends.AsQueryable();
        if (!User.IsInRole(nameof(RoleType.Master))) {
            query = query.Where(x => x.Members.Any(m => m.UserId == UserId));
        }

        var sf = await query.WhereId(id).FirstOrDefaultAsync();

        return new(sf) {
            StatusCode = sf is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
        };
    }

    [HttpGet("{id}/membros")]
    public async Task<Response<SecretFriendMember[]>> GetMembers(int id, Pagination pagination) {
        var query = DbContext.SecretFriendMembers.Where(x => x.SecretFriendId == id);
        if (!User.IsInRole(nameof(RoleType.Master))) {
            query = query.Where(x => x.UserId == UserId);
        }
        
        return new(await query.PaginateAsync(pagination));
    }

    [HttpPost(nameof(Create))]
    public async Task<Response<SecretFriend>> Create([Bind(nameof(data.Name), nameof(data.Date),
            nameof(data.Description), nameof(data.MinimumPrice), nameof(data.MaximumPrice))]
        CreateSecretFriendDto data) {
        var created = DbContext.SecretFriends.Add(new(data) {
            CreatedById = UserId,
            Members = [
                new() {
                    UserId = UserId,
                    IsAdmin = true,
                }
            ]
        });

        await DbContext.SaveChangesAsync();

        return new(created.Entity) {
            StatusCode = 201
        };
    }

    [HttpPost("{id}/addMember")]
    public async Task<Response<SecretFriend>> AddMember(int id, [FromBody] (int userId, bool? admin) data) {
        var sf = await DbContext.SecretFriends.Include(x => x.Members).WhereId(id).FirstOrDefaultAsync();

        if (sf is null) {
            return new() {
                Messages = {
                    new() {
                        Message = "Amigo secreto não encontrado",
                        Type = MessageType.Warning
                    }
                },
                StatusCode = StatusCodes.Status404NotFound,
            };
        }

        // Apenas o criador ou um admin do grupo pode adicionar membros
        if (sf.CreatedById != UserId && !sf.Members.Any(x => x.IsAdmin && x.UserId == UserId)) {
            return new() {
                Messages = {
                    new() {
                        Message = "Apenas um administrador pode adicionar membros",
                        Type = MessageType.Warning
                    }
                }
            };
        }

        if (sf.Members.Any(x => x.UserId == data.userId)) {
            return new() {
                Messages = {
                    new() {
                        Message = "Usuario já é membro do amigo secreto",
                        Type = MessageType.Warning
                    }
                }
            };
        }

        sf.Members.Add(new() {
            UserId = data.userId,
            IsAdmin = data.admin ?? false
        });

        await DbContext.SaveChangesAsync();

        return new(sf);
    }
}