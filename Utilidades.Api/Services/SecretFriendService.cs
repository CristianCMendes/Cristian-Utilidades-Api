using EntityEase.Models.Extensions;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Context;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Models.SecretFriend;
using Utilidades.Api.Models.SecretFriend.Dto;
using Utilidades.Api.Models.SecretFriend.Interface;

namespace Utilidades.Api.Services;

public class SecretFriendService(UtilDbContext dbContext) : ISecretFriendService {
    /// <inheritdoc />
    public async Task<ApiResponse<SecretFriend>> Draw(int secretFriendId, int requesterId) {
        var secretFriend = await dbContext.SecretFriends.Include(x => x.Members).WhereId(secretFriendId)
            .FirstOrDefaultAsync();
        if (secretFriend is null) {
            return new() {
                Messages = {
                    new() {
                        Message = "Amigo secreto não encontrado",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var requesterMember = secretFriend.Members.FirstOrDefault(m => m.UserId == requesterId);

        if (requesterMember is null) {
            return new() {
                Messages = {
                    new() {
                        Message = "Usuario não é membro do amigo secreto",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        if (!secretFriend.AllowPick) {
            if (!requesterMember.IsAdmin) {
                return new() {
                    Messages = {
                        new() {
                            Message = "Apenas administradores podem girar o amigo secreto",
                            Type = MessageType.warning
                        }
                    },
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }

        var picked = secretFriend.Members.Where(x => x.UserPickedId != null).Select(x => x.UserPickedId ?? 0).ToList();

        foreach (var sf in secretFriend.Members) {
            var available =
                (secretFriend.Members.Where(x => x.UserId != sf.UserId && !picked.Contains(x.UserId))).ToArray();

            if (available.Length == 0) {
                continue;
            }

            var random = new Random();
            var index = random.Next(0, available.Length);
            sf.UserPickedId = available.ElementAt(index).UserId;
            picked.Add(sf.UserPickedId.Value);
        }

        return new();
    }

    /// <inheritdoc />
    public async Task<ApiResponse<SecretFriend>> Create(CreateSecretFriendDto data, int userId) {
        var created = await dbContext.SecretFriends.AddAsync(new(data) {
            CreatedById = userId,
            Members = [
                new() {
                    UserId = userId,
                    IsAdmin = true,
                }
            ]
        });

        return new(created.Entity) {
            StatusCode = 201
        };
    }


    /// <inheritdoc />
    public async Task<ApiResponse<SecretFriend>> AddMember(int secretFriendId, AddSecretFriendMemberDto data) {
        var sf = await dbContext.SecretFriends.Include(x => x.Members).WhereId(secretFriendId).FirstOrDefaultAsync();
        var user = await dbContext.Users
            .Where(x => x.Id == data.UserId || data.Email != null && x.Email == data.Email.AsInsensitive())
            .FirstOrDefaultAsync();

        if (user is null) {
            return new() {
                Messages = {
                    new ResponseMessage() {
                        Message = "Usuario não encontrado",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        if (sf is null) {
            return new() {
                Messages = {
                    new() {
                        Message = "Amigo secreto não encontrado",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status404NotFound,
            };
        }

        if (sf.Members.Any(x => x.UserId == user.Id)) {
            return new() {
                Messages = {
                    new() {
                        Message = "Usuario já é membro do amigo secreto",
                        Type = MessageType.warning
                    }
                }
            };
        }

        sf.Members.Add(new() {
            UserId = user.Id,
            IsAdmin = data.Admin ?? false
        });

        return new(sf) {
            StatusCode = StatusCodes.Status201Created,
            Messages = {
                new ResponseMessage() {
                    Message = "Membro adicionado com sucesso",
                    Type = MessageType.success
                }
            }
        };
    }

    /// <inheritdoc />
    public async Task<bool> CanAddMember(int secretFriendId, int userId) {
        var secretFriend = await dbContext.SecretFriends.Include(x => x.Members).WhereId(secretFriendId)
            .FirstOrDefaultAsync();
        if (secretFriend is null) {
            return false;
        }

        // Apenas o criador ou um admin do grupo pode adicionar membros 
        if (secretFriend.CreatedById != userId && !secretFriend.Members.Any(x => x.IsAdmin && x.UserId == userId)) {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public IQueryable<SecretFriend> List(ListSecretFriendDto filters) {
        var query = dbContext.SecretFriends.AsQueryable();
        if (filters.SecretFriendId is { Length: > 0 } ids) {
            query = query.WhereId(ids);
        }

        if (filters.CreatorId is { Length: > 0 } creatorIds) {
            query = query.Where(x => creatorIds.Contains(x.CreatedById));
        }

        if (filters.MemberId is { Length: > 0 } memberIds) {
            query = query.Where(x => memberIds.Contains(x.CreatedById));
        }

        if (filters.Name is { Length: > 0 } name) {
            query = query.Where(x => x.Name.ToLower().Trim().Contains(name.AsInsensitive()));
        }

        if (filters.Description is { Length: > 0 } description) {
            query = query.Where(x =>
                x.Description != null && x.Description.ToLower().Trim().Contains(description.AsInsensitive()));
        }

        if (filters.DateMin is { } minDate) {
            query = query.Where(x => x.Date >= minDate);
        }

        if (filters.DateMax is { } maxDate) {
            query = query.Where(x => x.Date <= maxDate);
        }

        if (filters.CreatedMin is { } minCreated) {
            query = query.Where(x => x.CreatedAt >= minCreated);
        }

        if (filters.CreatedMax is { } maxCreated) {
            query = query.Where(x => x.CreatedAt <= maxCreated);
        }

        if (filters.MinimumPrice is { } minPrice) {
            query = query.Where(x => x.MinimumPrice <= minPrice || x.MaximumPrice <= minPrice);
        }

        if (filters.MaximumPrice is { } maxPrice) {
            query = query.Where(x => x.MinimumPrice >= maxPrice || x.MaximumPrice >= maxPrice);
        }

        if (filters.IsActive is { } isActive) {
            query = query.Where(x => x.IsActive == isActive);
        }

        return query;

    }
}