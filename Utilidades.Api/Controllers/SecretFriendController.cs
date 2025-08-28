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
    public async Task<IResponse> List(Pagination pagination) {
        var query = dbContext.SecretFriends.AsQueryable();
        query = query.Where(x => x.Members.Any(m => m.UserId == HttpContext.GetCurrentUserId()));

        return new Response(await query.PaginateAsync(pagination));
    }

    [HttpGet("{id}")]
    public async Task<IResponse> Get(int id) {
        var query = dbContext.SecretFriends.AsQueryable();
        if (!User.IsInRole(nameof(RoleType.Master))) {
            query = query.Where(x => x.Members.Any(m => m.UserId == HttpContext.GetCurrentUserId()));
        }

        var sf = await query.WhereId(id).FirstOrDefaultAsync();

        return new Response(sf) {
            StatusCode = sf is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
        };
    }

    [HttpGet("{id}/" + nameof(Members))]
    [ProducesResponseType<ISecretFriendMember>(200)]
    public async Task<IResponse> Members(int id, Pagination pagination) {
        var query = dbContext.SecretFriendMembers.Where(x => x.SecretFriendId == id);
        if (!User.IsInRole(nameof(RoleType.Master))) {
            query = query.Where(x => x.UserId == HttpContext.GetCurrentUserId());
        }

        return new Response(await query.PaginateAsync(pagination));
    }

    [HttpPost(nameof(Create))]
    [ProducesResponseType<ISecretFriend>(200)]
    public async Task<IResponse> Create([Bind(nameof(data.Name), nameof(data.Date),
            nameof(data.Description), nameof(data.MinimumPrice), nameof(data.MaximumPrice))]
        CreateSecretFriendDto data) {
        var response = await secretFriendService.Create(data, HttpContext.GetCurrentUserId());

        await dbContext.SaveChangesAsync();
        if (response.Data is not null) {
            var getUrl = Url.Action(nameof(Get), new { id = response.Data.Id });
            if (!string.IsNullOrEmpty(getUrl)) {
                Response.Headers.Location = getUrl;
            }

            var membersUrl = Url.Action(nameof(Members), new { id = response.Data.Id });
            if (!string.IsNullOrEmpty(membersUrl)) {
                response.Links.Add(new() { Href = membersUrl, Rel = "members", Method = "GET" });
            }

            var addMemberUrl = Url.Action(nameof(AddMember), new { id = response.Data.Id });
            if (!string.IsNullOrEmpty(addMemberUrl)) {
                response.Links.Add(new() { Href = addMemberUrl, Rel = "members", Method = "POST" });
            }

            var drawUrl = Url.Action(nameof(Draw), new { id = response.Data.Id });
            if (!string.IsNullOrEmpty(drawUrl)) {
                response.Links.Add(new() { Href = drawUrl, Rel = "draw", Method = "POST" });
            }

            response.StatusCode = StatusCodes.Status201Created;
        }

        return response;
    }

    [HttpPost("{id}/" + nameof(AddMember))]
    [ProducesResponseType<ISecretFriend>(200)]
    
    public async Task<IResponse> AddMember(int id, [FromBody] AddSecretFriendMemberDto data) {
        if (!await secretFriendService.CanAddMember(id, HttpContext.GetCurrentUserId())) {
            return new Response() {
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
    public async Task<IResponse> Draw(int id) {
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