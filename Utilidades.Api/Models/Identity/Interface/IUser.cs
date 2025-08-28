using EntityEase.Models.Interfaces.Identity;
using EntityEase.Models.Interfaces.Identity.Naming;
using EntityEase.Models.Interfaces.Status;
using EntityEase.Models.Interfaces.Tracking;

namespace Utilidades.Api.Models.Identity.Interface;

public interface IUser : IEEIdentifiable, IEENamed, IEEActivable, IEECreatable {
    public string Email { get; set; }
    public bool IsEmailConfirmed { get; set; } 
    public int? InvitedById { get; set; }
}