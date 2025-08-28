using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Pagination;
using Utilidades.Api.Models.Response;
using Utilidades.Api.Models.SecretFriend;
using Utilidades.Api.Models.SecretFriend.Dto;
using Utilidades.Api.Models.SecretFriend.Interface;

namespace Utilidades.Api.Services;

public interface ISecretFriendService {
    Task<ApiResponse<SecretFriend>> Draw(int secretFriendId, int requesterId);
    Task<ApiResponse<SecretFriend>> Create(CreateSecretFriendDto data, int userId);
    Task<ApiResponse<SecretFriend>> AddMember(int secretFriendId, AddSecretFriendMemberDto data);

    Task<bool> CanAddMember(int secretFriendId, int userId);
    IQueryable<SecretFriend> List(ListSecretFriendDto filters);
}