using EntityEase.Models.Interfaces.Identity;
using EntityEase.Models.Interfaces.Identity.Naming;
using EntityEase.Models.Interfaces.Tracking;
using Utilidades.Api.Models.Identity;

namespace Utilidades.Api.Models.SecretFriend.Interface;

public interface ISecretFriend : IEEIdentifiable, IEENamed {
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    public DateTime Date { get; set; }
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public bool IsActive { get; set; } 

    /// <summary>
    /// Allow any user to draw friend, even if they are not a admin
    /// </summary>
    public bool AllowPick { get; set; }
}