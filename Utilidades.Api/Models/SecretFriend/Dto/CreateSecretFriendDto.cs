namespace Utilidades.Api.Models.SecretFriend.Dto;

public record CreateSecretFriendDto {
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
}