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
using Utilidades.Api.Models.SecretFriend.Interface;
using Utilidades.Api.Services;

namespace Utilidades.Api.Controllers;

public class SecretFriendController(
    UtilDbContext dbContext,
    ISecretFriendService secretFriendService,
    IMailService mailService)
    : ApiControllerBase {
    [HttpGet(nameof(List))]
    [ProducesResponseType<ISecretFriend[]>(200)]
    public async Task<IApiResponse> List([FromQuery] ListSecretFriendDto filters, Pagination pagination) {

        ApiResponse.Links.Add(LinkRef(nameof(Get), routeData: new { id = 1 }, rel: "get"));
        ApiResponse.Links.Add(LinkRef(nameof(Members), routeData: new { id = 1 }, rel: "members"));
        ApiResponse.Links.Add(LinkRef(nameof(AddMember), routeData: new { id = 1 }, rel: "addMember"));
        ApiResponse.Links.Add(LinkRef(nameof(Draw), routeData: new { id = 1 }, rel: "draw"));

        var query = secretFriendService.List(filters);

        query = query.Where(x =>
            x.Members.Any(m => m.UserId == HttpContext.GetCurrentUserId()) ||
            x.CreatedById == HttpContext.GetCurrentUserId());

        return new ApiResponse(await query.PaginateAsync(pagination));
    }

    [HttpGet("{id}")]
    public async Task<IApiResponse> Get(int id) {
        var query = dbContext.SecretFriends.AsQueryable();
        if (!User.IsInRole(nameof(RoleType.Master))) {
            query = query.Where(x => x.Members.Any(m => m.UserId == HttpContext.GetCurrentUserId()));
        }

        var sf = await query.WhereId(id).FirstOrDefaultAsync();

        ApiResponse.Links.Add(LinkRef(nameof(Members), routeData: new { id = sf?.Id ?? 1 }, rel: "members"));
        ApiResponse.Links.Add(LinkRef(nameof(List), routeData: ListSecretFriendDto.Example(), rel: "list"));
        ApiResponse.Links.Add(LinkRef(nameof(AddMember), routeData: new { id = sf?.Id ?? 1 }, rel: "addMember"));
        ApiResponse.Links.Add(LinkRef(nameof(Draw), routeData: new { id = sf?.Id ?? 1 }, rel: "draw"));

        return new ApiResponse(sf) {
            StatusCode = sf is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
        };
    }

    [HttpGet("{id}/" + nameof(Members))]
    [ProducesResponseType<ISecretFriendMember>(200)]
    public async Task<IApiResponse> Members(int id, Pagination pagination) {
        ApiResponse.Links.Add(LinkRef(nameof(Get), routeData: new { id = 1 }, rel: "get"));
        ApiResponse.Links.Add(LinkRef(nameof(List), routeData: ListSecretFriendDto.Example(), rel: "list"));
        ApiResponse.Links.Add(LinkRef(nameof(AddMember), routeData: AddSecretFriendMemberDto.Example, rel: "addMember"));
        ApiResponse.Links.Add(LinkRef(nameof(Draw), routeData: new { id = 1 }, rel: "draw"));

        var query = dbContext.SecretFriendMembers.Where(x => x.SecretFriendId == id);
        if (!User.IsInRole(nameof(RoleType.Master))) {
            query = query.Where(x => x.UserId == HttpContext.GetCurrentUserId());
        }

        return new ApiResponse(await query.PaginateAsync(pagination));
    }

    [HttpPost(nameof(Create))]
    [ProducesResponseType<ISecretFriend>(200)]
    public async Task<IApiResponse> Create([Bind(nameof(data.Name), nameof(data.Date),
            nameof(data.Description), nameof(data.MinimumPrice), nameof(data.MaximumPrice))]
        CreateSecretFriendDto data) {
        ApiResponse = await secretFriendService.Create(data, HttpContext.GetCurrentUserId());

        await dbContext.SaveChangesAsync();
        var sfData = ApiResponse.GetData<SecretFriend>();


        ApiResponse.Links.Add(LinkRef(nameof(Get), routeData: new { id = sfData?.Id ?? 1 }, rel: "get"));
        ApiResponse.Links.Add(LinkRef(nameof(List), routeData: ListSecretFriendDto.Example(), rel: "list"));
        ApiResponse.Links.Add(LinkRef(nameof(Members), routeData: new { id = sfData?.Id ?? 1 }, rel: "members"));
        ApiResponse.Links.Add(LinkRef(nameof(AddMember), routeData: AddSecretFriendMemberDto.Example, rel: "addMember"));
        ApiResponse.Links.Add(LinkRef(nameof(Draw), routeData: new { id = sfData?.Id ?? 1 }, rel: "draw"));

        if (sfData != null) {
            ApiResponse.StatusCode = StatusCodes.Status201Created;
        }

        return ApiResponse;
    }

    [HttpPost("{id}/" + nameof(AddMember))]
    [ProducesResponseType<ISecretFriend>(200)]
    public async Task<IApiResponse> AddMember(int id, [FromBody] AddSecretFriendMemberDto data) {
        if (!await secretFriendService.CanAddMember(id, HttpContext.GetCurrentUserId())) {
            return new ApiResponse() {
                StatusCode = StatusCodes.Status401Unauthorized,
                Messages = {
                    new() {
                        Message = "Usuario não tem permissão para adicionar um membro no amigo secreto",
                        Type = MessageType.warning
                    }
                }

            };
        }

        var response = await secretFriendService.AddMember(id, data);

        await dbContext.SaveChangesAsync();

        return response;
    }

    [HttpPost("{id}/" + nameof(Draw))]
    [ProducesResponseType<ISecretFriend>(200)]
    public async Task<IApiResponse> Draw(int id) {
        var response = await secretFriendService.Draw(id, HttpContext.GetCurrentUserId());

        var sf = response.Data;
        if (sf is null) {
            return response;
        }

        // await dbContext.SaveChangesAsync();

        foreach (var member in sf.Members) {
            if (member.User is not null && member.UserPicked is not null) {
                await mailService.SendMailAsync("Seu amigo secreto foi escolhido",
                    new(
                        $"<div>Seu amigo secreto foi escolhido no amigo secreto: {sf.Name},<br/> você tirou: {member.UserPicked.Email}<div/>"),
                    member.User.Email);
            }
        }

        return response;
    }
}