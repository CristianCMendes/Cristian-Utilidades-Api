using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Models.SecretFriend;
using Utilidades.Api.Models.SecretFriend.Dto;

namespace Utilidades.Api.Services;

public interface ISecretFriendService {
    Task<Response<SecretFriend>> Draw(int secretFriendId, int requesterId);
    Task<Response<SecretFriend>> Create(CreateSecretFriendDto data, int userId);
    Task<Response<SecretFriend>> AddMember(int secretFriendId, AddSecretFriendMemberDto data);

    Task<bool> CanAddMember(int secretFriendId, int userId);
}