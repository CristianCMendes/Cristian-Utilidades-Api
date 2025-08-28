using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Identity.Interface;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Services;

public interface IUserService {
    Task<ApiResponse<IUser>> Invite(UserInviteDto data, int invitedBy);
}