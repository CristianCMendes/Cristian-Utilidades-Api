namespace Utilidades.Api.Models.SecretFriend.Dto;

public class ListSecretFriendDto {
    public int[]? SecretFriendId { get; set; }
    public int[]? CreatorId { get; set; }
    public int[]? MemberId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? DateMin { get; set; }
    public DateTime? DateMax { get; set; }
    public DateTime? CreatedMin { get; set; }
    public DateTime? CreatedMax { get; set; }
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public bool? IsActive { get; set; }

    public static ListSecretFriendDto Example() {
        return new ListSecretFriendDto() {
            SecretFriendId = [1],
            CreatorId = [1],
            MemberId = [1],
            Name = "Example",
            Description = "Example",
            DateMin = DateTime.MinValue,
            DateMax = DateTime.MaxValue,
            CreatedMin = DateTime.MinValue,
            CreatedMax = DateTime.MaxValue,
            MinimumPrice = 25.5m,
            MaximumPrice = 100.5m,
            IsActive = true,
        };
    }
}