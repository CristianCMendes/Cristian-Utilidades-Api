using EntityEase.Models.Extensions;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Identity.Interface;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Services;

public class UserService(UtilDbContext dbContext) : IUserService {
    /// <inheritdoc />
    public async Task<ApiResponse<IUser>> Invite(UserInviteDto data, int invitedBy) {
        if (!data.Email.IsMail()) {
            return new() {
                Messages = {
                    new() {
                        Message = "Email inválido",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var user = dbContext.Users.FirstOrDefault(x => x.Email == data.Email);
        if (user is not null) {
            return new() {
                Messages = {
                    new() {
                        Message = "Usuário já está cadastrado",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var created = dbContext.Users.Add(new() {
            Name = data.Name ?? string.Empty,
            Email = data.Email,
            Password = string.Empty,
            IsEmailConfirmed = false,
            InvitedById = invitedBy,
            IsActive = true
        });

        if (await dbContext.SecretFriends.WhereId(data.SecretFriendId).AnyAsync()) {
            created.Entity.SecretFriendMembers.Add(new() {
                IsAdmin = false
            });
        }

        return new(created.Entity) {
            StatusCode = 201
        };
    }
}